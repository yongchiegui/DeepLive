resource "aws_iam_role" "fetch_latest_tweets" {
  name = "FetchLatestTweets"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "fetch_latest_tweets_ssm" {
  name = "FetchLatestTweetsSsm"
  role = aws_iam_role.fetch_latest_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "ssm:GetParameter",
        "ssm:PutParameter"
      ],
      "Effect": "Allow",
      "Resource": "${aws_ssm_parameter.latest_fetched_tweet_id.arn}"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "fetch_latest_tweets_sqs" {
  name = "FetchLatestTweetsSqs"
  role = aws_iam_role.fetch_latest_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sqs:SendMessage",
      "Effect": "Allow",
      "Resource": "${aws_sqs_queue.tweets_to_translate.arn}"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "fetch_latest_tweets_dynamo" {
  name = "FetchLatestTweetsDynamo"
  role = aws_iam_role.fetch_latest_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "dynamodb:DescribeTable",
        "dynamodb:Scan"
      ],
      "Effect": "Allow",
      "Resource": "${aws_dynamodb_table.twitter_users.arn}"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "fetch_latest_tweets_logs" {
  name = "FetchLatestTweetsLogs"
  role = aws_iam_role.fetch_latest_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:DescribeLogStreams",
        "logs:PutLogEvents"
      ],
      "Effect": "Allow",
      "Resource": "arn:aws:logs:${var.region}:${var.account}:*"
    }
  ]
}
EOF
}

resource "aws_iam_role" "translate_tweets" {
  name = "TranslateTweets"

  assume_role_policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": "sts:AssumeRole",
      "Principal": {
        "Service": "lambda.amazonaws.com"
      },
      "Effect": "Allow",
      "Sid": ""
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "translate_tweets_sqs" {
  name = "TranslateTweetsSqs"
  role = aws_iam_role.translate_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "sqs:ReceiveMessage",
        "sqs:DeleteMessage",
        "sqs:GetQueueAttributes"
      ],
      "Effect": "Allow",
      "Resource": "${aws_sqs_queue.tweets_to_translate.arn}"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "translate_tweets_dynamo" {
  name = "TranslateTweetsDynamo"
  role = aws_iam_role.translate_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "dynamodb:PutItem",
        "dynamodb:DescribeTable"
      ],
      "Effect": "Allow",
      "Resource": "${aws_dynamodb_table.translated_tweets.arn}"
    }
  ]
}
EOF
}

resource "aws_iam_role_policy" "translate_tweets_logs" {
  name = "TranslateTweetsLogs"
  role = aws_iam_role.translate_tweets.id

  policy = <<EOF
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:DescribeLogStreams",
        "logs:PutLogEvents"
      ],
      "Effect": "Allow",
      "Resource": "arn:aws:logs:${var.region}:${var.account}:*"
    }
  ]
}
EOF
}