using BooruSharp.Extensions;
using System;
using System.Linq;
using System.Text.Json;

namespace BooruSharp.Booru.Template
{
    /// <summary>
    /// Template booru based on E621. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class E621 : ABooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="E621"/> template class.
        /// </summary>
        /// <param name="domain">
        /// The fully qualified domain name. Example domain
        /// name should look like <c>www.google.com</c>.
        /// </param>
        /// <param name="options">
        /// The options to use. Use <c>|</c> (bitwise OR) operator to combine multiple options.
        /// </param>
        protected E621(string domain, BooruOptions options = BooruOptions.None)
            : base(domain, UrlFormat.Danbooru, options | BooruOptions.NoWiki | BooruOptions.NoRelated | BooruOptions.NoComment 
                  | BooruOptions.NoTagByID | BooruOptions.NoPostByID | BooruOptions.NoPostCount | BooruOptions.NoFavorite)
        { }

        private protected override JsonElement ParseFirstPostSearchResult(in JsonElement element)
        {
            JsonElement? firstPost;

            if (element.TryGetProperty("posts", out var posts)
                && posts.ValueKind == JsonValueKind.Array)
            {
                firstPost = posts.GetArrayLength() > 0
                    ? posts.EnumerateArray().First()
                    : (JsonElement?)null;
            }
            else
            {
                firstPost = element.HasProperty("post")
                    ? element.GetProperty("post")
                    : (JsonElement?)null;
            }

            return firstPost ?? throw new Search.InvalidTags();
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            // TODO: Check others tags
            string[] categories =
            {
                "general",
                "species",
                "character",
                "copyright",
                "artist",
                "meta",
            };

            // TODO FIXME ASAP
            var tags = Array.Empty<string>();

            var fileElement = element.GetProperty("file");
            var previewElement = element.GetProperty("preview");
            int id = element.GetInt32("id").Value;
            var sourcesElement = element.GetProperty("sources");

            return new Search.Post.SearchResult(
                fileElement.GetUri("url"),
                previewElement.GetUri("url"),
                new Uri(BaseUrl + "posts/" + id),
                GetRating(element.GetString("rating")[0]),
                tags,
                id,
                fileElement.GetInt32("size"),
                fileElement.GetInt32("height").Value,
                fileElement.GetInt32("width").Value,
                previewElement.GetInt32("height"),
                previewElement.GetInt32("width"),
                element.GetDateTime("created_at"),
                sourcesElement.EnumerateArray().FirstOrDefault().GetString(),
                element.GetProperty("score").GetInt32("total"),
                fileElement.GetString("md5"));
        }

        private protected override Search.Post.SearchResult[] GetPostsSearchResult(in JsonElement element)
        {
            var childElement = element.GetProperty("posts");

            if (element.TryGetProperty("posts", out childElement)
                && childElement.ValueKind == JsonValueKind.Array
                && childElement.GetArrayLength() > 0)
                return childElement.Select(e => GetPostSearchResult(e)).ToArray();

            if (element.TryGetProperty("post", out childElement)
                && childElement.ValueKind == JsonValueKind.Object)
                return new[] { GetPostSearchResult(childElement) };

            return Array.Empty<Search.Post.SearchResult>();
        }

        // GetCommentSearchResult not available

        // GetWikiSearchResult not available

        private protected override Search.Tag.SearchResult GetTagSearchResult(in JsonElement element)
        {
            return new Search.Tag.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("name"),
                (Search.Tag.TagType)element.GetInt32("category").Value,
                element.GetInt32("post_count").Value);
        }

        // GetRelatedSearchResult not available // TODO: Available with credentials?
    }
}
