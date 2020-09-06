using BooruSharp.Extensions;
using System;
using System.Linq;
using System.Text.Json;

namespace BooruSharp.Booru.Template
{
    /// <summary>
    /// Template booru based on Sankaku. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Sankaku : ABooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sankaku"/> template class.
        /// </summary>
        /// <param name="domain">
        /// The fully qualified domain name. Example domain
        /// name should look like <c>www.google.com</c>.
        /// </param>
        /// <param name="options">
        /// The options to use. Use <c>|</c> (bitwise OR) operator to combine multiple options.
        /// </param>
        protected Sankaku(string domain, BooruOptions options = BooruOptions.None)
            : base(domain, UrlFormat.Sankaku, options | BooruOptions.NoRelated | BooruOptions.NoPostByMD5 | BooruOptions.NoPostByID
                  | BooruOptions.NoPostCount | BooruOptions.NoFavorite | BooruOptions.NoTagByID)
        { }

        private protected override JsonElement ParseFirstPostSearchResult(in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
                ? element.EnumerateArray().First()
                : throw new Search.InvalidTags();
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            var id = element.GetInt32("id").Value;

            var postUriBuilder = new UriBuilder(BaseUrl)
            {
                Host = BaseUrl.Host.Replace("capi-v2", "beta"),
                Path = $"/post/show/{id}",
            };

            var tags = element.GetProperty("tags").Select(e => e.GetString("name")).ToArray();

            return new Search.Post.SearchResult(
                element.GetUri("file_url"),
                element.GetUri("preview_url"),
                postUriBuilder.Uri,
                GetRating(element.GetString("rating")[0]),
                tags,
                id,
                element.GetInt32("file_size"),
                element.GetInt32("height").Value,
                element.GetInt32("width").Value,
                element.GetInt32("preview_height"),
                element.GetInt32("preview_width"),
                _unixTime.AddSeconds(element.GetProperty("created_at").GetInt32("s").Value),
                element.GetString("source"),
                element.GetInt32("total_score"),
                element.GetString("md5"));
        }

        private protected override Search.Post.SearchResult[] GetPostsSearchResult(in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
                ? element.Select(e => GetPostSearchResult(e)).ToArray()
                : Array.Empty<Search.Post.SearchResult>();
        }

        private protected override Search.Comment.SearchResult GetCommentSearchResult(in JsonElement element)
        {
            var author = element.GetProperty("author");

            return new Search.Comment.SearchResult(
                element.GetInt32("id").Value,
                element.GetInt32("post_id").Value,
                author.GetInt32("id"),
                _unixTime.AddSeconds(element.GetProperty("created_at").GetInt32("s").Value),
                author.GetString("name"),
                element.GetString("body"));
        }

        private protected override Search.Wiki.SearchResult GetWikiSearchResult(in JsonElement element)
        {
            return new Search.Wiki.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("title"),
                _unixTime.AddSeconds(element.GetProperty("created_at").GetInt32("s").Value),
                _unixTime.AddSeconds(element.GetProperty("updated_at").GetInt32("s").Value),
                element.GetString("body"));
        }

        private protected override Search.Tag.SearchResult GetTagSearchResult(in JsonElement element) // TODO: Fix TagType values
        {
            return new Search.Tag.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("name"),
                (Search.Tag.TagType)element.GetInt32("type").Value,
                element.GetInt32("count").Value);
        }

        // GetRelatedSearchResult not available
    }
}
