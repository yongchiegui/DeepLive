resource "aws_sqs_queue" "tweets_to_translate" {
  name = "TweetsToTranslate"
  visibility_timeout_seconds = 90
}