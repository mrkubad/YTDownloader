using System.Text.RegularExpressions;

namespace YTDownloaderCore.Regexes
{
    public static class RegexLibrary
    {
        public static Regex YoutubeUrl { get; } = new Regex("http(?:s?):\\/\\/(?:www\\.)?youtu(?:be\\.com\\/watch\\?v=)(?<VideoId>[^&^\\s]+)(?:.*?&list=(?<ListId>[^&^\\s]+))?", System.Text.RegularExpressions.RegexOptions.Compiled);
    }
}
