using System;
using System.Net.Http;
using System.Threading.Tasks;
using BooruSharp.Search;

namespace BooruSharp.Booru
{
    // TODO: add variants that return Stream objects.
    public abstract partial class ABooru
    {
        // Allow this many files to be downloaded at once.
        // DO NOT change this to a constant field, this was intentionally made a property.
        // TODO: we should probably also consider images that are currenly being downloaded
        // by DownloadPreviewImageBytesAsync and DownloadOriginalImageBytesAsync.
        private protected virtual int SimultaneousImageDownloadLimit => 4;

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
        // TODO: this should probably return an array of tasks instead. Returning an array of tasks
        // will let us attach continuations to the tasks from the calling method, making saving files
        // much more efficient since we won't have to wait until all of the tasks complete before
        // we can start copying bytes to files. Maybe an overload with custom continuation function
        // should be provided just for that. IAsyncEnumerable when?
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
