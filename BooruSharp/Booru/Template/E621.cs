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
            if (element.TryGetProperty("posts", out var posts)
                && posts.ValueKind == JsonValueKind.Array
                && posts.GetArrayLength() > 0)
                return posts.EnumerateArray().First();

            if (element.TryGetProperty("post", out var post))
                return post;

            throw new Search.InvalidTags();
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            // Enumerate all subproperties ("general", "species", etc) of
            // the "tags" property, and extract tag strings from them.
            var tags = element.GetProperty("tags").EnumerateObject().SelectMany(property =>
            {
                return property.Value.GetArrayLength() > 0
                    ? property.Value.Select(e => e.GetString())
                    : Array.Empty<string>();
            }).ToArray();

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
                sourcesElement.GetArrayLength() > 0 
                    ? sourcesElement.EnumerateArray().First().GetString()
                    : "",
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
