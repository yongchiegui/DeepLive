resource "aws_dynamodb_table" "translated_tweets" {
  name = "TranslatedTweets"
  hash_key = "AuthorId"
  range_key = "Id"
  billing_mode = "PAY_PER_REQUEST"

  global_secondary_index {
    name = "DateAndId"
    hash_key = "CreatedAtDate"
    range_key = "Id"
    projection_type = "INCLUDE"
    non_key_attributes = ["TranslatedText"]
  }

  attribute {
    name = "AuthorId"
    type = "S"
  }

  attribute {
    name = "Id"
    type = "S"
  }

  attribute {
    name = "CreatedAtDate"
    type = "S"
  }
}

resource "aws_dynamodb_table" "twitter_users" {
  name = "TwitterUsers"
  hash_key = "Id"
  billing_mode = "PAY_PER_REQUEST"

  attribute {
    name = "Id"
    type = "S"
  }
}