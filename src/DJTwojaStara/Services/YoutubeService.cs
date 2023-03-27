using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ATL;
using CSCore;
using CSCore.Codecs.OPUS;
using CSCore.Streams.Effects;
using DJTwojaStara.Audio;
using DJTwojaStara.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DJTwojaStara.Services;

public class YoutubeService : IStreamerService
{
    private readonly string _path;

    public YoutubeService()
    {
        _path = Path.GetTempPath()+"/djtwojastara-temp";
        Directory.CreateDirectory(_path);
        Directory.CreateDirectory(_path + "/ytdlp-cache");
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await YoutubeDL.DownloadYtDlpBinary(_path);
        await YoutubeDL.DownloadFFmpegBinary(_path);
        
        // run chmod +x on the binaries if on linux
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = "+x " + _path + "/yt-dlp",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
            
            process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = "+x " + _path + "/ffmpeg",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<IStreamable>> StreamSongs(string searchQuery)
    {
        searchQuery = searchQuery.Trim();
        var ytdlp = new YoutubeDL();
        
        ytdlp.FFmpegPath = _path+"/ffmpeg";
        ytdlp.YoutubeDLPath = _path+"/yt-dlp";
        ytdlp.OutputFolder = _path + "/ytdlp-cache";
        ytdlp.OutputFileTemplate = "%(id)s.%(ext)s";
        var targetIDs = new[] { "" };

        if (searchQuery.StartsWith("http"))
        {
            var uri = new Uri(searchQuery);
            var parsed = HttpUtility.ParseQueryString(uri.Query);
            
            if (parsed["list"] is not null)
            {
                var searchResults = await ytdlp.RunWithOptions(
                    new string[] { $"{searchQuery}" },
                    OptionSet.FromString(new [] { "--flat-playlist", "--get-id" }),
                    CancellationToken.None);
                
                if (searchResults.Data.Length>0)
                {
                    targetIDs = searchResults.Data.ToArray();
                }
            }
            else
            {
                targetIDs[0] = parsed["v"];
            }
        }
        else
        {
            var searchResults = await ytdlp.RunWithOptions(
                new string[] { $"ytsearch1:\"{searchQuery}\"" },
                OptionSet.FromString(new [] { "--get-id" }),
                CancellationToken.None);
            
            if (searchResults.Data.Length>0)
            {
                targetIDs[0] = searchResults.Data[0];
            }
        }

        return targetIDs.Select(x => new YoutubeStreamable(x, _path, ytdlp));
    }
}