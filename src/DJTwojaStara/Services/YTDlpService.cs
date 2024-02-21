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

public class YTDlpService : IStreamerService
{
    private readonly string _path;

    public YTDlpService()
    {
        _path = Path.GetTempPath()+"/djtwojastara-temp";
        Directory.CreateDirectory(_path);
        Directory.CreateDirectory(_path + "/ytdlp-cache");
    }

    private YoutubeDL CreateYTDL()
    {
        return new YoutubeDL()
        {
            YoutubeDLPath = OperatingSystem.IsLinux() ? "yt-dlp" : "yt-dlp.exe",
            FFmpegPath = OperatingSystem.IsLinux() ? "ffmpeg" : "ffmpeg.exe"
        };
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
        return Task.CompletedTask;
    }

    public Task<IStreamable> GetStreamable(SongInfo songInfo)
    {
        return Task.FromResult<IStreamable>(new YoutubeStreamable(songInfo.Id, _path, CreateYTDL()));
    }

    public IStreamable GetStreamable(string id)
    {
        return new YoutubeStreamable(id, _path, CreateYTDL());
    }
}