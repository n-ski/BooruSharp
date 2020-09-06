using BooruSharp.Extensions;
using System;
using System.Linq;
using System.Text.Json;

namespace BooruSharp.Booru.Template
{
    /// <summary>
    /// Template booru based on Moebooru. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Moebooru : ABooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Moebooru"/> template class.
        /// </summary>
        /// <param name="domain">
        /// The fully qualified domain name. Example domain
        /// name should look like <c>www.google.com</c>.
        /// </param>
        /// <param name="options">
        /// The options to use. Use <c>|</c> (bitwise OR) operator to combine multiple options.
        /// </param>
        protected Moebooru(string domain, BooruOptions options = BooruOptions.None)
            : base(domain, UrlFormat.PostIndexJson, options | BooruOptions.NoPostByMD5 | BooruOptions.NoPostByID
                  | BooruOptions.NoFavorite)
        { }

        private protected override JsonElement ParseFirstPostSearchResult(in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
                ? element.EnumerateArray().First()
                : throw new Search.InvalidTags();
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            int id = element.GetInt32("id").Value;

            return new Search.Post.SearchResult(
                element.GetUri("file_url"),
                element.GetUri("preview_url"),
                new Uri(BaseUrl + "post/show/" + id),
                GetRating(element.GetString("rating")[0]),
                element.GetString("tags").Split(' '),
                id,
                element.GetInt32("file_size"),
                element.GetInt32("height").Value,
                element.GetInt32("width").Value,
                element.GetInt32("preview_height"),
                element.GetInt32("preview_width"),
                _unixTime.AddSeconds(element.GetInt32("created_at").Value),
                element.GetString("source"),
                element.GetInt32("score"),
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
            return new Search.Comment.SearchResult(
                element.GetInt32("id").Value,
                element.GetInt32("post_id").Value,
                element.GetInt32("creator_id"),
                element.GetDateTime("created_at").Value,
                element.GetString("creator"),
                element.GetString("body"));
        }

        private protected override Search.Wiki.SearchResult GetWikiSearchResult(in JsonElement element)
        {
            return new Search.Wiki.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("title"),
                element.GetDateTime("created_at").Value,
                element.GetDateTime("updated_at").Value,
                element.GetString("body"));
        }

        private protected override Search.Tag.SearchResult GetTagSearchResult(in JsonElement element)
        {
            return new Search.Tag.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("name"),
                (Search.Tag.TagType)element.GetInt32("type").Value,
                element.GetInt32("count").Value);
        }

        private protected override Search.Related.SearchResult GetRelatedSearchResult(in JsonElement element)
        {
            var childElements = element.EnumerateArray().Take(2).ToArray();

            return new Search.Related.SearchResult(
                childElements[0].GetString(),
                childElements[1].GetInt32());
        }
    }
}
