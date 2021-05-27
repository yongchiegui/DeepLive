using System;
using System.Collections.Generic;
using System.Net.Http;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using static Amazon.Lambda.SQSEvents.SQSEvent;
using DeepLive.Model;
using DeepLive.Client;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Linq;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace DeepLive.Lambda
{
    public class TranslateTweets
    {
        private readonly ClientFactory _clientFactory = new ClientFactory();

        public async Task<int> Handle(SQSEvent sqsEvent)
        {
            var records = sqsEvent.Records;

            foreach (var record in records)
            {
                var tweet = JsonConvert.DeserializeObject<DeepLiveTweet>(record.Body);
                
                LambdaLogger.Log($"Processing Tweet {tweet.Id} from {tweet.AuthorName}\n");

                var translations = await GetTranslations(tweet);
                UpdateWithTranslations(tweet, translations);
                SaveToDatabase(tweet);
                DeleteFromQueue(record);
            }

            LambdaLogger.Log($"Processed {records.Count} tweets\n");

            return records.Count;
        }

        private async Task<DeepLTranslations> GetTranslations(DeepLiveTweet tweet)
        {
            LambdaLogger.Log($"Getting translations for Tweet {tweet.Id} from {tweet.AuthorName}\n");

            var deepLUrl = Environment.GetEnvironmentVariable("DEEPL_API_URL");
            var deepLApiKey = Environment.GetEnvironmentVariable("DEEPL_API_KEY");
            var httpClient = _clientFactory.GetHttpClient();

            var parameters = new Dictionary<string, string>
            {
                { "auth_key", deepLApiKey },
                { "text", tweet.SourceText },
                { "source_lang", tweet.SourceLanguage },
                { "target_lang", tweet.TargetLanguage }
            };

            var requestContent = new FormUrlEncodedContent(parameters);
            var response = await httpClient.PostAsync(deepLUrl, requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var deepLTranslations = JsonConvert.DeserializeObject<DeepLTranslations>(responseContent);

            return deepLTranslations;
        }

        private void UpdateWithTranslations(DeepLiveTweet tweet, DeepLTranslations translations)
        {
            tweet.SourceLanguage = translations.Translations.First().DetectedSourceLanguage;
            tweet.TranslatedText = "";

            foreach (var translation in translations.Translations)
            {
                tweet.TranslatedText += translation.Text + "\n";
            }

            tweet.TranslatedText = tweet.TranslatedText.TrimEnd();
        }

        private async void SaveToDatabase(DeepLiveTweet tweet)
        {
            LambdaLogger.Log($"Saving Tweet {tweet.Id} from {tweet.AuthorName} to database\n");

            var tableName = Environment.GetEnvironmentVariable("TRANSLATED_TWEETS_TABLE_NAME");
            var dynamoDbClient = _clientFactory.GetDynamoDbClient();
            var table = Table.LoadTable(dynamoDbClient, tableName);

            var isoDateTime = tweet.CreatedAt.UtcDateTime.ToString("s", System.Globalization.CultureInfo.InvariantCulture) + "Z";

            var tweetDocument = new Document();
            tweetDocument["Id"] = tweet.Id;
            tweetDocument["CreatedAtDate"] = isoDateTime.Substring(0, 10);
            tweetDocument["CreatedAtDateTime"] = isoDateTime;
            tweetDocument["AuthorId"] = tweet.AuthorId;
            tweetDocument["AuthorName"] = tweet.AuthorName;
            tweetDocument["SourceText"] = tweet.SourceText;
            tweetDocument["SourceLanguage"] = tweet.SourceLanguage;
            tweetDocument["TranslatedText"] = tweet.TranslatedText;
            tweetDocument["TargetLanguage"] = tweet.TargetLanguage;

            await table.PutItemAsync(tweetDocument);
        }

        private async void DeleteFromQueue(SQSMessage record)
        {
            LambdaLogger.Log($"Removing record with receipt handle {record.ReceiptHandle} from queue\n");

            var queueUrl = Environment.GetEnvironmentVariable("TWEETS_TO_TRANSLATE_QUEUE_URL");
            var sqsClient = _clientFactory.GetSqsClient();

            await sqsClient.DeleteMessageAsync(queueUrl, record.ReceiptHandle);
        }
    }
}