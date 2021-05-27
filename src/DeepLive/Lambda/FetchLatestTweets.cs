using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement.Model;
using DeepLive.Client;
using DeepLive.Model;
using Newtonsoft.Json;
using Tweetinvi.Models.V2;
using Tweetinvi.Parameters.V2;

namespace DeepLive.Lambda
{
    public class FetchLatestTweets
    {
        private readonly ClientFactory _clientFactory = new ClientFactory();

        public async Task<int> Handle()
        {
            var users = await GetTwitterUsers();
            var searchQueries = CreateTwitterSearchQueries(users);
            var latestFetchedTweetId = await GetLatestFetchedTweetId();

            LambdaLogger.Log($"Fetching Tweets since Tweet id {latestFetchedTweetId}\n");

            var tweets = await GetLatestTweets(searchQueries, latestFetchedTweetId);
            
            LambdaLogger.Log($"Fetched {tweets.Count} Tweets\n");

            if (tweets.Count > 0)
            {
                var deepLiveTweets = MapToDeepLiveTweets(tweets, users);
                SendToQueue(deepLiveTweets);
                var updatedLatestFetchedTweetId = await UpdateLatestFetchedTweetId(deepLiveTweets);

                LambdaLogger.Log($"Updated latest fetched Tweet id to {updatedLatestFetchedTweetId}\n");
            }

            return tweets.Count;
        }

        private async Task<ICollection<DeepLiveTwitterUser>> GetTwitterUsers()
        {
            var tableName = Environment.GetEnvironmentVariable("USERS_TABLE_NAME");
            var dynamoDbClient = _clientFactory.GetDynamoDbClient();

            var table = Table.LoadTable(dynamoDbClient, tableName);
            var userDocuments = new List<Document>();
            var search = table.Scan(new ScanFilter());

            while (!search.IsDone)
            {
                var nextUserDocuments = await search.GetNextSetAsync();
                userDocuments.AddRange(nextUserDocuments);
            }

            var users = userDocuments.Select(d => new DeepLiveTwitterUser
            {
                Id = d["Id"],
                Name = d["Name"]
            });

            return users.ToList();
        }

        private ICollection<string> CreateTwitterSearchQueries(ICollection<DeepLiveTwitterUser> users)
        {
            var excludeRetweetQuery = ") -is:retweet";
            var searchQueries = new List<string>();
            var searchQuery = "(";

            foreach (var user in users)
            {
                var fromUserQuery = $"from:{user.Name} OR ";

                if (searchQuery.Length + fromUserQuery.Length + excludeRetweetQuery.Length > 512)
                {
                    searchQuery = searchQuery.Remove(searchQuery.Length - 4) + excludeRetweetQuery;
                    searchQueries.Add(searchQuery);
                    searchQuery = "(" + fromUserQuery;
                }
                else
                {
                    searchQuery += fromUserQuery;
                }
            }

            searchQuery = searchQuery.Remove(searchQuery.Length - 4) + excludeRetweetQuery;
            searchQueries.Add(searchQuery);

            return searchQueries;
        }

        private async Task<string> GetLatestFetchedTweetId()
        {
            var parameterName = Environment.GetEnvironmentVariable("LATEST_FETCHED_TWEET_ID_PARAMETER_NAME");
            var ssmClient = _clientFactory.GetSsmClient();

            var request = new GetParameterRequest
            {
                Name = parameterName
            };

            var response = await ssmClient.GetParameterAsync(request);
            var latestFetchedTweetId = response.Parameter.Value;

            return latestFetchedTweetId;
        }

        private async Task<ICollection<TweetV2>> GetLatestTweets(ICollection<string> searchQueries, string latestFetchedTweetId)
        {
            var twitterClient = _clientFactory.GetTwitterClient();
            var tweets = new List<TweetV2>();

            foreach (var searchQuery in searchQueries)
            {
                var searchParameters = new SearchTweetsV2Parameters(searchQuery);
                searchParameters.SinceId = latestFetchedTweetId;
                var searchIterator = twitterClient.SearchV2.GetSearchTweetsV2Iterator(searchParameters);

                while (!searchIterator.Completed)
                {
                    var searchPage = await searchIterator.NextPageAsync();
                    var searchResponse = searchPage.Content;
                    var searchTweets = searchResponse.Tweets;

                    tweets.AddRange(searchTweets.ToList());
                }
            }

            return tweets;
        }

        private ICollection<DeepLiveTweet> MapToDeepLiveTweets(ICollection<TweetV2> tweets, ICollection<DeepLiveTwitterUser> users)
        {
            var deepLiveTweets = tweets.Select(t => new DeepLiveTweet
            {
                Id = t.Id,
                CreatedAt = t.CreatedAt,
                AuthorId = t.AuthorId,
                AuthorName = users.Where(u => u.Id == t.AuthorId).First().Name,
                SourceText = t.Text,
                TargetLanguage = "EN",
            });

            return deepLiveTweets.ToList();
        }

        private async void SendToQueue(ICollection<DeepLiveTweet> tweets)
        {
            var queueUrl = Environment.GetEnvironmentVariable("TWEETS_TO_TRANSLATE_QUEUE_URL");
            var sqsClient = _clientFactory.GetSqsClient();

            foreach (var tweet in tweets)
            {
                var tweetJson = JsonConvert.SerializeObject(tweet);
                LambdaLogger.Log($"Sending tweet {tweet.Id} from {tweet.AuthorName} to queue\n");
                await sqsClient.SendMessageAsync(queueUrl, tweetJson);
            }
        }

        private async Task<string> UpdateLatestFetchedTweetId(ICollection<DeepLiveTweet> tweets)
        {
            var parameterName = Environment.GetEnvironmentVariable("LATEST_FETCHED_TWEET_ID_PARAMETER_NAME");
            var ssmClient = _clientFactory.GetSsmClient();

            var latestFetchedTweetId = tweets.OrderByDescending(t => t.Id).First().Id;
            var request = new PutParameterRequest
            {
                Name = parameterName,
                Value = latestFetchedTweetId,
                Overwrite = true
            };

            var response = await ssmClient.PutParameterAsync(request);

            return latestFetchedTweetId;
        }
    }
}