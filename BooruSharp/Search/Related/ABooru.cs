using BooruSharp.Extensions;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BooruSharp.Booru
{
    public abstract partial class ABooru
    {
        /// <summary>
        /// Gets the tags related to the specified <paramref name="tag"/>.
        /// </summary>
        /// <param name="tag">The tag that other tags must be related to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="Search.FeatureUnavailable"/>
        /// <exception cref="System.Net.Http.HttpRequestException"/>
        public virtual async Task<Search.Related.SearchResult[]> GetRelatedAsync(string tag)
        {
            if (!HasRelatedAPI)
                throw new Search.FeatureUnavailable();

            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            bool isDanbooruFormat = _format == UrlFormat.Danbooru;
            var url = CreateUrl(_relatedUrl, (isDanbooruFormat ? "query" : "tags") + "=" + tag);

            using (var content = await GetResponseContentAsync(url))
            using (var stream = await content.ReadAsStreamAsync())
            using (var document = await JsonDocument.ParseAsync(stream))
            {
                var element = isDanbooruFormat
                    ? document.RootElement.GetProperty("tags")
                    : document.RootElement.GetProperty(document.RootElement.EnumerateObject().First().Name);

                return element.Select(e => GetRelatedSearchResult(e)).ToArray();
            }
        }
    }
}
