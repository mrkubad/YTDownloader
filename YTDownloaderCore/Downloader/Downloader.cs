using System;
using System.Net;

namespace YTDownloaderCore.Downloader
{
    public class Downloader : WebClient
    {
        private readonly string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299";
        public Downloader()
        {
            Headers.Add("User-Agent", UserAgent);
        }
        public bool FastDashDownload { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            if (FastDashDownload)
                request.AddRange(0);
            return request;
        }

    }
}
