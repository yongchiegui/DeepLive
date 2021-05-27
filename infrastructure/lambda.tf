resource "aws_lambda_function" "fetch_latest_tweets" {
  filename = "./DeepLive.zip"
  function_name = "FetchLatestTweets"
  role = aws_iam_role.fetch_latest_tweets.arn
  handler = "DeepLive::DeepLive.Lambda.FetchLatestTweets::Handle"
  runtime = "dotnetcore3.1"
  memory_size = 256
  timeout = 90

  environment {
    variables = {
      TWITTER_ACCESS_TOKEN = ""
      TWITTER_ACCESS_TOKEN_SECRET = ""
      TWITTER_BEARER_TOKEN = ""
      TWITTER_CONSUMER_API_KEY = ""
      TWITTER_CONSUMER_API_SECRET_KEY = ""
      TWEETS_TO_TRANSLATE_QUEUE_URL = "${aws_sqs_queue.tweets_to_translate.id}"
      LATEST_FETCHED_TWEET_ID_PARAMETER_NAME = "${aws_ssm_parameter.latest_fetched_tweet_id.name}"
      USERS_TABLE_NAME = "${aws_dynamodb_table.twitter_users.id}"
    }
  }
}

resource "aws_lambda_function" "translate_tweets" {
  filename = "./DeepLive.zip"
  function_name = "TranslateTweets"
  role = aws_iam_role.translate_tweets.arn
  handler = "DeepLive::DeepLive.Lambda.TranslateTweets::Handle"
  runtime = "dotnetcore3.1"
  memory_size = 256
  timeout = 90

  environment {
    variables = {
      DEEPL_API_URL = "https://api.deepl.com/v2/translate"
      DEEPL_API_KEY = ""
      TWEETS_TO_TRANSLATE_QUEUE_URL = "${aws_sqs_queue.tweets_to_translate.id}"
      TRANSLATED_TWEETS_TABLE_NAME = "${aws_dynamodb_table.translated_tweets.id}"
    }
  }
}

resource "aws_lambda_event_source_mapping" "translate_tweets_sqs_trigger" {
  event_source_arn = aws_sqs_queue.tweets_to_translate.arn
  function_name    = aws_lambda_function.translate_tweets.arn
  depends_on = [aws_iam_role_policy.translate_tweets_sqs]
}