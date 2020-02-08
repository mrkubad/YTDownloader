using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Diagnostics;

namespace YTDownloaderCore.Models
{
    public class VideoModel
    {
        public string Title { get; private set; }
        public int LengthSeconds { get; private set; }
        public string ThumbnailUrl { get; private set; }
        public EventHandler ParseEnd;
        private readonly string VideoId;
        private NameValueCollection VideoInfoFile;
        private JsonDocument PlayerResponse;
        private readonly List<JsonElement> VideoFormats = new List<JsonElement>();
        public VideoModel(string videoId)
        {
            if(videoId.Length == 0 || videoId.Length != 11)
            {
                throw new ArgumentException("Invalid Video Id", "video Id");
            }
            VideoId = videoId;
        }

        public async Task ParseVideoInformationAsync()
        {
            await GetPlayerResponse();
            GetAvailableFormats();
            GetVideoInformation();
            GetBestThumbnail();
            // Parsing ended we need to give information to our ViewModel
            ParseEnd?.Invoke(this, new EventArgs());
        }
        private void GetAvailableFormats()
        {
            if(PlayerResponse == null)
                throw new Exception("PlayerResponse is null, something went wrong!");

            if(PlayerResponse.RootElement.TryGetProperty("streamingData", out JsonElement streamingData))
            {
                if(streamingData.TryGetProperty("formats", out JsonElement formats))
                {
                    foreach(JsonElement elem in formats.EnumerateArray())
                    {
                        if (!VideoFormats.Contains(elem))
                            VideoFormats.Add(elem);
                    }
                }
                if (streamingData.TryGetProperty("adaptiveFormats", out JsonElement adaptiveFormats))
                {
                    foreach (JsonElement elem in adaptiveFormats.EnumerateArray())
                    {
                        if (!VideoFormats.Contains(elem))
                            VideoFormats.Add(elem);
                    }

                }
            }
            if (VideoFormats.Count == 0)
                throw new Exception("Could not find any VideoFormat");
        }
        private void GetVideoInformation()
        {
            if(PlayerResponse == null)
                throw new Exception("PlayerResponse is null, something went wrong!");

            if(PlayerResponse.RootElement.TryGetProperty("videoDetails", out JsonElement videoDetails))
            {
                Title = videoDetails.TryGetProperty("title", out JsonElement title) ? title.GetString() : "";
                LengthSeconds = videoDetails.TryGetProperty("lengthSeconds", out JsonElement lengthSeconds) ? int.Parse(lengthSeconds.GetString()) : 0;
            }
            if(string.IsNullOrEmpty(Title) && VideoInfoFile.AllKeys.Contains("title"))
            {
                string title = VideoInfoFile.Get("title");
                if (title.Length > 0)
                    Title = title;

                if(string.IsNullOrEmpty(Title))
                {
                    Title = "Unknown title :(";
                }
            }
            if(LengthSeconds == 0 && VideoInfoFile.AllKeys.Contains("length_seconds"))
            {
                string length = VideoInfoFile.Get("length_seconds");
                if(length.Length > 0)
                {
                    LengthSeconds = int.TryParse(length, out int res) ? res : 0;
                }
                if(LengthSeconds == 0)
                {
                    Debug.WriteLine("Could not find video length");
                }
            }
        }
        private void GetBestThumbnail()
        {
            if (PlayerResponse == null)
                throw new Exception("PlayerResponse is null, something went wrong!");
            if (PlayerResponse.RootElement.TryGetProperty("videoDetails", out JsonElement videoDetails))
            {
                if(videoDetails.TryGetProperty("thumbnail", out JsonElement thumbnail))
                {
                    if(thumbnail.TryGetProperty("thumbnails", out JsonElement thumbnails))
                    {
                        int bestScore = 0, bestIndex = 0;
                        try
                        {
                            for(int i = 0; i < thumbnails.GetArrayLength() ; ++i)
                            {
                                JsonElement elem = thumbnails[i];
                                int temp = elem.GetProperty("height").GetInt32() + elem.GetProperty("width").GetInt32();
                                if (temp > bestScore)
                                {
                                    bestScore = temp;
                                    bestIndex = i;
                                }
                            }
                            ThumbnailUrl = thumbnails[bestIndex].GetProperty("url").GetString();
                        }
                        catch
                        {
                            Debug.WriteLine("Problem with thumbnail url finding");
                        }
                    }
                }
                else if(VideoInfoFile.AllKeys.Contains("thumbnail_url"))
                {
                    ThumbnailUrl = VideoInfoFile.Get("thumbnail_url");
                }
            }
        }

        private async Task GetPlayerResponse()
        {
            VideoInfoFile = HttpUtility.ParseQueryString(await DownloadVideoInfoFileAsync());
            const string keyToFind = "player_response";
            if (!VideoInfoFile.AllKeys.Contains(keyToFind))
                throw new Exception("Could not find player_respone key");
            // Convert string to stream
            byte[] byteArray = Encoding.UTF8.GetBytes(VideoInfoFile.Get(keyToFind));
            PlayerResponse = await JsonDocument.ParseAsync(new MemoryStream(byteArray));
            if (PlayerResponse == null)
                throw new Exception("PlayerResponse is null, something went wrong!");
        }
        private async Task<string> DownloadVideoInfoFileAsync()
        {
            return await new YTDownloaderCore.Downloader.Downloader().DownloadStringTaskAsync($"https://www.youtube.com/get_video_info?&video_id={VideoId}&asv=3&el=detailpage&hl=pl_PL");
        }

    }
}
