resource "aws_ssm_parameter" "latest_fetched_tweet_id" {
  name = "LatestFetchedTweetId"
  type = "String"
  value = " "
  overwrite = false
  
  lifecycle { 
    ignore_changes = [value]
  }
}