resource "aws_cloudwatch_event_rule" "schedule_fetch_latest_tweets" {
  name = "ScheduleFetchLatestTweets"
  schedule_expression = "rate(1 minute)"
  is_enabled = true
}

resource "aws_cloudwatch_event_target" "fetch_latest_tweets" {
  rule = aws_cloudwatch_event_rule.schedule_fetch_latest_tweets.name
  target_id = "FetchLatestTweets"
  arn = aws_lambda_function.fetch_latest_tweets.arn
  input = "\"\""
}