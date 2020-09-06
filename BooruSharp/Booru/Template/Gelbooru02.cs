using BooruSharp.Extensions;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace BooruSharp.Booru.Template
{
    /// <summary>
    /// Template booru based on Gelbooru 0.2. This class is <see langword="abstract"/>.
    /// </summary>
    public abstract class Gelbooru02 : ABooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Gelbooru02"/> template class.
        /// </summary>
        /// <param name="domain">
        /// The fully qualified domain name. Example domain
        /// name should look like <c>www.google.com</c>.
        /// </param>
        /// <param name="options">
        /// The options to use. Use <c>|</c> (bitwise OR) operator to combine multiple options.
        /// </param>
        protected Gelbooru02(string domain, BooruOptions options = BooruOptions.None) 
            : base(domain, UrlFormat.IndexPhp, options | BooruOptions.NoRelated | BooruOptions.NoWiki | BooruOptions.NoPostByMD5
                  | BooruOptions.CommentApiXml | BooruOptions.TagApiXml | BooruOptions.NoMultipleRandom)
        {
            _url = domain;
        }

        private protected override JsonElement ParseFirstPostSearchResult(in JsonElement element)
        {
            return element.ValueKind == JsonValueKind.Array && element.GetArrayLength() > 0
                ? element.EnumerateArray().First()
                : throw new Search.InvalidTags();
        }

        private protected override Search.Post.SearchResult GetPostSearchResult(in JsonElement element)
        {
            var baseUrl = BaseUrl.Scheme + "://" + _url;
            var directory = element.GetString("directory");
            var image = element.GetString("image");
            var id = element.GetInt32("id").Value;

            return new Search.Post.SearchResult(
                new Uri(baseUrl + "//images/" + directory + "/" + image),
                new Uri(baseUrl + "//thumbnails/" + directory + "/thumbnails_" + image),
                new Uri(BaseUrl + "index.php?page=post&s=view&id=" + id),
                GetRating(element.GetString("rating")[0]),
                element.GetString("tags").Split(' '),
                id,
                null,
                element.GetInt32("height").Value,
                element.GetInt32("width").Value,
                null,
                null,
                null,
                null,
                element.GetInt32("score"),
                null);
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

        private protected override Search.Tag.SearchResult GetTagSearchResult(object json)
        {
            var elem = (XmlNode)json;

            return new Search.Tag.SearchResult(
                int.Parse(elem.Attributes.GetNamedItem("id").Value),
                elem.Attributes.GetNamedItem("name").Value,
                (Search.Tag.TagType)int.Parse(elem.Attributes.GetNamedItem("type").Value),
                int.Parse(elem.Attributes.GetNamedItem("count").Value));
        }

        // GetRelatedSearchResult not available

        private readonly string _url;
    }
}
