using System;
using System.Net.Http;
using System.Threading.Tasks;
using BooruSharp.Search;

namespace BooruSharp.Booru
{
    // TODO: add variants that return Stream objects.
    public abstract partial class ABooru
    {
        /// <summary>
        /// Downloads the <paramref name="post"/>'s preview image as an array of bytes.
        /// </summary>
        /// <param name="post">The post to get the preview image from.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="HttpRequestException"/>
        public virtual Task<byte[]> DownloadPreviewImageBytesAsync(Search.Post.SearchResult post)
        {
            return DownloadBytesFromUrlWithReferer(post.PreviewUrl, post.PostUrl);
        }

        /// <summary>
        /// Downloads the <paramref name="post"/>'s original image as an array of bytes.
        /// </summary>
        /// <param name="post">The post to get the image from.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="HttpRequestException"/>
        public virtual Task<byte[]> DownloadOriginalImageBytesAsync(Search.Post.SearchResult post)
        {
            return DownloadBytesFromUrlWithReferer(post.FileUrl, post.PostUrl);
        }

        /// <summary>
        /// Downloads all of the <paramref name="post"/>'s images as an array of <see cref="byte"/> arrays.
        /// </summary>
        /// <param name="post">The post to get the image from.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="HttpRequestException"/>
        public virtual Task<byte[][]> DownloadAllImagesAsync(Search.Post.SearchResult post)
        {
            // I'm not sure if this should return an array with one element by default.
            throw new FeatureUnavailable();
        }

        // TODO: move to a separate extension methods/utilities class.
        private protected async Task<byte[]> DownloadBytesFromUrlWithReferer(Uri uriToDownload, Uri refererUri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uriToDownload);
            request.Headers.Add("Referer", refererUri.AbsoluteUri);

            using (var response = await GetResponseAsync(request))
            {
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsByteArrayAsync();
            }
        }
    }
}
