using BooruSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BooruSharp.Booru
{
    public abstract partial class ABooru
    {
        /// <summary>
        /// Get the comments posted on a post.
        /// </summary>
        /// <param name="postId">The ID of the post to get information about.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Search.FeatureUnavailable"/>
        /// <exception cref="System.Net.Http.HttpRequestException"/>
        public virtual async Task<Search.Comment.SearchResult[]> GetCommentsAsync(int postId)
        {
            if (!HasCommentAPI)
                throw new Search.FeatureUnavailable();

            var url = CreateUrl(_commentUrl, SearchArg("post_id") + postId);
            var results = new List<Search.Comment.SearchResult>();

            if (CommentsUseXml)
            {
                var xml = await GetXmlAsync(url);

                foreach (var node in xml.LastChild)
                {
                    var result = GetCommentSearchResult(node);

                    if (result.PostID == postId)
                        results.Add(result);
                }
            }
            else
            {
                using (var content = await GetResponseContentAsync(url))
                using (var stream = await content.ReadAsStreamAsync())
                using (var document = await JsonDocument.ParseAsync(stream))
                {
                    foreach (var element in document.RootElement.EnumerateArray())
                    {
                        var result = GetCommentSearchResult(element);

                        if (result.PostID == postId)
                            results.Add(result);
                    }
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// Get the last comments posted on the website.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="Search.FeatureUnavailable"/>
        /// <exception cref="System.Net.Http.HttpRequestException"/>
        public virtual async Task<Search.Comment.SearchResult[]> GetLastCommentsAsync()
        {
            if (!HasSearchLastComment)
                throw new Search.FeatureUnavailable();

            var url = CreateUrl(_commentUrl);

            if (CommentsUseXml)
            {
                var xml = await GetXmlAsync(url);
                var results = new List<Search.Comment.SearchResult>(xml.LastChild.ChildNodes.Count);

                foreach (var node in xml.LastChild)
                    results.Add(GetCommentSearchResult(node));

                return results.ToArray();
            }
            else
            {
                using (var content = await GetResponseContentAsync(url))
                using (var stream = await content.ReadAsStreamAsync())
                using (var document = await JsonDocument.ParseAsync(stream))
                {
                    return document.RootElement.Select(e => GetCommentSearchResult(e)).ToArray();
                }
            }
        }
    }
}
