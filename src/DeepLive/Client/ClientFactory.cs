using System.Net.Http;
using Amazon.DynamoDBv2;
using Amazon.SimpleSystemsManagement;
using Amazon.SQS;
using Tweetinvi;

namespace DeepLive.Client
{
    public class ClientFactory
    {
        private ITwitterClient _twitterClient;
        private IAmazonSQS _sqsClient;
        private IAmazonSimpleSystemsManagement _ssmClient;
        private IAmazonDynamoDB _dynamoDbClient;
        private HttpClient _httpClient;
        

        public ITwitterClient GetTwitterClient()
        {
            if (_twitterClient == null)
            {
                InitializeTwitterClient();
            }

            return _twitterClient;
        }

        public IAmazonSQS GetSqsClient()
        {
            if (_sqsClient == null)
            {
                InitializeSqsClient();
            }

            return _sqsClient;
        }

        public IAmazonSimpleSystemsManagement GetSsmClient()
        {
            if (_ssmClient == null)
            {
                InitializeSsmClient();
            }

            return _ssmClient;
        }

        public IAmazonDynamoDB GetDynamoDbClient()
        {
            if (_dynamoDbClient == null)
            {
                InitializeDynamoDbClient();
            }

            return _dynamoDbClient;
        }

        public HttpClient GetHttpClient()
        {
            if (_httpClient == null)
            {
                InitializeHttpClient();
            }

            return _httpClient;
        }

        private void InitializeTwitterClient()
        {
            var accessToken = System.Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");
            var accessTokenSecret = System.Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN_SECRET");
            var consumerApiKey = System.Environment.GetEnvironmentVariable("TWITTER_CONSUMER_API_KEY");
            var consumerApiSecretKey = System.Environment.GetEnvironmentVariable("TWITTER_CONSUMER_API_SECRET_KEY");

            _twitterClient = new TwitterClient(consumerApiKey, consumerApiSecretKey, accessToken, accessTokenSecret);
        }

        private void InitializeSqsClient()
        {
            _sqsClient = new AmazonSQSClient();
        }

        private void InitializeSsmClient()
        {
            _ssmClient = new AmazonSimpleSystemsManagementClient();
        }

        private void InitializeDynamoDbClient()
        {
            _dynamoDbClient = new AmazonDynamoDBClient();
        }

        private void InitializeHttpClient()
        {
            _httpClient = new HttpClient();
        }
    }
}
