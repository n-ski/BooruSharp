using BooruSharp.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace BooruSharp.Booru.Template
{
    /// <summary>
    /// Template booru based on Gelbooru. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Gelbooru : ABooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gelbooru"/> template class.
        /// </summary>
        /// <param name="domain">
        /// The fully qualified domain name. Example domain
        /// name should look like <c>www.google.com</c>.
        /// </param>
        /// <param name="options">
        /// The options to use. Use <c>|</c> (bitwise OR) operator to combine multiple options.
        /// </param>
        protected Gelbooru(string domain, BooruOptions options = BooruOptions.None)
            : base(domain, UrlFormat.IndexPhp, options | BooruOptions.NoWiki | BooruOptions.NoRelated | BooruOptions.LimitOf20000
                  | BooruOptions.CommentApiXml)
        { }

        /// <inheritdoc/>
        public async override Task<Search.Post.SearchResult> GetPostByMd5Async(string md5)
        {
            if (md5 == null)
                throw new ArgumentNullException(nameof(md5));

            // Create a URL that will redirect us to Gelbooru post URL containing post ID.
            string url = $"{BaseUrl}index.php?page=post&s=list&md5={md5}";

            using (HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Head, url))
            using (HttpResponseMessage response = await HttpClient.SendAsync(message))
            {
                response.EnsureSuccessStatusCode();

                // If HEAD message doesn't actually redirect us then ID here will be null...
                Uri redirectUri = response.RequestMessage.RequestUri;
                string id = HttpUtility.ParseQueryString(redirectUri.Query).Get("id");

                // ...which will then throw NullReferenceException here.
                // Danbooru does the same when it doesn't find a post with matching MD5,
                // though I suppose throwing exception with more meaningful message
                // would be better.
                return await GetPostByIdAsync(int.Parse(id));
            }
        }

        private protected override JsonElement ParseFirstPostSearchResult(in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
                ? element.EnumerateArray().First()
                : throw new Search.InvalidTags();
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            const string gelbooruTimeFormat = "ddd MMM dd HH:mm:ss zzz yyyy";

            var directory = element.GetString("directory");
            var image = element.GetString("image");
            var id = element.GetInt32("id").Value;

            return new Search.Post.SearchResult(
                element.GetUri("file_url"),
                new Uri("https://gelbooru.com/thumbnails/" + directory + "/thumbnail_" + image),
                new Uri(BaseUrl + "/index.php?page=post&s=view&id=" + id),
                GetRating(element.GetString("rating")[0]),
                element.GetString("tags").Split(' '),
                id,
                null,
                element.GetInt32("height").Value,
                element.GetInt32("width").Value,
                null,
                null,
                DateTime.ParseExact(element.GetString("created_at"), gelbooruTimeFormat, CultureInfo.InvariantCulture),
                element.GetString("source"),
                element.GetInt32("score"),
                element.GetString("hash"));
        }

        private protected override Search.Post.SearchResult[] GetPostsSearchResult(in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
                ? element.Select(e => GetPostSearchResult(e)).ToArray()
                : Array.Empty<Search.Post.SearchResult>();
        }

        private protected override Search.Comment.SearchResult GetCommentSearchResult(object json)
        {
            var elem = (XmlNode)json;
            XmlNode creatorId = elem.Attributes.GetNamedItem("creator_id");

            return new Search.Comment.SearchResult(
                int.Parse(elem.Attributes.GetNamedItem("id").Value),
                int.Parse(elem.Attributes.GetNamedItem("post_id").Value),
                creatorId.InnerText.Length > 0 ? int.Parse(creatorId.Value) : (int?)null,
                DateTime.ParseExact(elem.Attributes.GetNamedItem("created_at").Value, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
                elem.Attributes.GetNamedItem("creator").Value,
                elem.Attributes.GetNamedItem("body").Value);
        }

        // GetWikiSearchResult not available

        private protected override Search.Tag.SearchResult GetTagSearchResult(in JsonElement element)
        {
            return new Search.Tag.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("tag"),
                StringToTagType(element.GetString("type")),
                element.GetInt32("count").Value);
        }

        // GetRelatedSearchResult not available
    }
}
