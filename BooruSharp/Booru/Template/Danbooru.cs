using BooruSharp.Extensions;
using System;
using System.Linq;
using System.Text.Json;

namespace BooruSharp.Booru.Template
{
    /// <summary>
    /// Template booru based on Danbooru. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Danbooru : ABooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Danbooru"/> template class.
        /// </summary>
        /// <param name="domain">
        /// The fully qualified domain name. Example domain
        /// name should look like <c>www.google.com</c>.
        /// </param>
        /// <param name="options">
        /// The options to use. Use <c>|</c> (bitwise OR) operator to combine multiple options.
        /// </param>
        protected Danbooru(string domain, BooruOptions options = BooruOptions.None)
            : base(domain, UrlFormat.Danbooru, options | BooruOptions.NoLastComments | BooruOptions.NoPostCount
                  | BooruOptions.NoFavorite)
        { }

        private protected override JsonElement ParseFirstPostSearchResult(in JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Array when element.GetArrayLength() > 0:
                    return element.EnumerateArray().First();

                case JsonValueKind.Object:
                    return element;

                default:
                    throw new Search.InvalidTags();
            }
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            var id = element.GetInt32("id") ?? 0;

            return new Search.Post.SearchResult(
                element.GetUri("file_url"),
                element.GetUri("preview_file_url"),
                new Uri(BaseUrl + "posts/" + id),
                GetRating(element.GetString("rating")[0]),
                element.GetString("tag_string").Split(' '),
                id,
                element.GetInt32("file_size"),
                element.GetInt32("image_height").Value,
                element.GetInt32("image_width").Value,
                null,
                null,
                element.GetDateTime("created_at"),
                element.GetString("source"),
                element.GetInt32("score"),
                element.GetString("md5"));
        }

        private protected override Search.Post.SearchResult[] GetPostsSearchResult(in JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Array when element.GetArrayLength() > 0:
                    return element.Select(e => GetPostSearchResult(e)).ToArray();

                case JsonValueKind.Object when element.TryGetProperty("post", out var post):
                    return new[] { GetPostSearchResult(post) };

                default:
                    return Array.Empty<Search.Post.SearchResult>();
            }
        }

        private protected override Search.Comment.SearchResult GetCommentSearchResult(in JsonElement element)
        {
            return new Search.Comment.SearchResult(
                element.GetInt32("id").Value,
                element.GetInt32("post_id").Value,
                element.GetInt32("creator_id"),
                element.GetDateTime("created_at").Value,
                element.GetString("creator_name"),
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
                (Search.Tag.TagType)element.GetInt32("category").Value,
                element.GetInt32("post_count").Value);
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
