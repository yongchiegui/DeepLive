using System;

namespace DeepLive.Model
{
    public class DeepLiveTweet
    {
        public string Id { get; set; }

        public DateTimeOffset CreatedAt { get; set; }

        public string AuthorId { get; set; }

        public string AuthorName { get; set; }

        public string SourceText { get; set; }

        public string SourceLanguage { get; set; }

        public string TranslatedText { get; set; }

        public string TargetLanguage { get; set; }
    }
}
