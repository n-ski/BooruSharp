﻿using BooruSharp.Extensions;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace BooruSharp.Booru
{
    public abstract partial class ABooru
    {
        /// <summary>
        /// Gets the wiki page of a tag.
        /// </summary>
        /// <param name="query">The tag to get the wiki page for.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="Search.FeatureUnavailable"/>
        /// <exception cref="System.Net.Http.HttpRequestException"/>
        /// <exception cref="Search.InvalidTags"/>
        public virtual async Task<Search.Wiki.SearchResult> GetWikiAsync(string query)
        {
            if (!HasWikiAPI)
                throw new Search.FeatureUnavailable();

            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var url = CreateUrl(_wikiUrl, SearchArg(_format == UrlFormat.Danbooru ? "title" : "query") + query);

            using (var content = await GetResponseContentAsync(url))
            using (var stream = await content.ReadAsStreamAsync())
            using (var document = await JsonDocument.ParseAsync(stream))
            {
                foreach (var element in document.RootElement.EnumerateArray())
                    if (element.GetString("title") == query)
                        return GetWikiSearchResult(element);
            }

            throw new Search.InvalidTags();
        }
    }
}
