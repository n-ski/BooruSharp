using BooruSharp.Extensions;
using System.Text.Json;

namespace BooruSharp.Booru
{
    /// <summary>
    /// Lolibooru.
    /// <para>https://lolibooru.moe/</para>
    /// </summary>
    public sealed class Lolibooru : Template.Moebooru
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lolibooru"/> class.
        /// </summary>
        public Lolibooru()
            : base("lolibooru.moe")
        { }

        /// <inheritdoc/>
        public override bool IsSafe => false;

        private protected override Search.Tag.SearchResult GetTagSearchResult(in JsonElement element)
        {
            return new Search.Tag.SearchResult(
                element.GetInt32("id").Value,
                element.GetString("name"),
                (Search.Tag.TagType)element.GetInt32("tag_type").Value,
                element.GetInt32("post_count").Value);
        }
    }
}
