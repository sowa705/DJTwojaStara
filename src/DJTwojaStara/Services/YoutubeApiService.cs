using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CSCore;

namespace DJTwojaStara.Services;

public class YoutubeApiService: ISearchService
{
    private readonly Google.Apis.YouTube.v3.YouTubeService _youtubeService;
    public YoutubeApiService(string apiKey)
    {
        _youtubeService = new Google.Apis.YouTube.v3.YouTubeService(new Google.Apis.Services.BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = "DJTwojaStara"
        });
    }
    
    public async Task<IEnumerable<SongInfo>> Search(string query)
    {
        // determine if query is a link or a search term
        if (query.StartsWith("https://"))
        {
            // playlist or single video?
            if (query.Contains("&list="))
            {
                Uri uri = new Uri(query);
                string getQuery = uri.Query;
                var playlistId = HttpUtility.ParseQueryString(getQuery).Get("list");
                
                var playlistItemsListRequest = _youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = playlistId;
                playlistItemsListRequest.MaxResults = 50;
                
                var playlistItemsListResponse = await playlistItemsListRequest.ExecuteAsync();
                
                return playlistItemsListResponse.Items.Select(item => new SongInfo
                {
                    Name = item.Snippet.Title,
                    Author = item.Snippet.ChannelTitle,
                    Length = 0, //TODO: fix this
                    Url = "https://www.youtube.com/watch?v=" + item.Snippet.ResourceId.VideoId,
                    CoverUrl = item.Snippet.Thumbnails.High.Url,
                    Id = item.Snippet.ResourceId.VideoId
                });
            }
            else
            {
                Uri uri = new Uri(query);
                string getQuery = uri.Query;
                var videoId = HttpUtility.ParseQueryString(getQuery).Get("v");
                var info = await GetInfoByVideoId(videoId);
                return new List<SongInfo> { info };
            }
        }
        else
        {
            var searchListRequest = _youtubeService.Search.List("snippet");
            searchListRequest.Q = query;
            searchListRequest.MaxResults = 1;
            searchListRequest.Type = "video";
            
            var searchListResponse = await searchListRequest.ExecuteAsync();
            
            return searchListResponse.Items.Select(item => new SongInfo
            {
                Name = item.Snippet.Title,
                Author = item.Snippet.ChannelTitle,
                Length = 0, //TODO: fix this
                Url = "https://www.youtube.com/watch?v=" + item.Id.VideoId,
                CoverUrl = item.Snippet.Thumbnails.High.Url,
                Id = item.Id.VideoId
            });
        }
    }

    public async Task<IEnumerable<SongInfo>> GetRelatedVideos(SongInfo song)
    {
        var relatedVideosRequest = _youtubeService.Search.List("snippet");
        relatedVideosRequest.RelatedToVideoId = song.Id;
        relatedVideosRequest.MaxResults = 1;

        var response = await relatedVideosRequest.ExecuteAsync();
        
        return response.Items.Select(item => new SongInfo
        {
            Name = item.Snippet.Title,
            Author = item.Snippet.ChannelTitle,
            Length = 0, //TODO: fix this
            Url = "https://www.youtube.com/watch?v=" + item.Id.VideoId,
            CoverUrl = item.Snippet.Thumbnails.High.Url,
            Id = item.Id.VideoId
        });
    }

    private async Task<SongInfo> GetInfoByVideoId(string videoId)
    {
        var videoListRequest = _youtubeService.Videos.List("snippet,contentDetails");
        videoListRequest.Id = videoId;
        
        var videoListResponse = await videoListRequest.ExecuteAsync();
        
        var video = videoListResponse.Items.First();
        
        return new SongInfo
        {
            Name = video.Snippet.Title,
            Author = video.Snippet.ChannelTitle,
            Length = ParseDuration(video.ContentDetails.Duration),
            Url = "https://www.youtube.com/watch?v=" + video.Id,
            CoverUrl = video.Snippet.Thumbnails.High.Url,
            Id = video.Id
        };
    }
    
    private int ParseDuration(string duration)
    {
        var time = System.Xml.XmlConvert.ToTimeSpan(duration);
        return (int)time.TotalSeconds;
    }
}