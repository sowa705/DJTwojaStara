using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CSCore;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace DJTwojaStara.Audio;

public class YoutubeStreamable : IStreamable
{
    private readonly string _path;
    private readonly string _id;
    private readonly YoutubeDL _ytdlp;
    public YoutubeStreamable(string youtubeID, string cachePath, YoutubeDL ytdlp)
    {
        _id = youtubeID;
        _path = cachePath;
        _ytdlp = ytdlp;
    }
    
    private OpusFileStreamable? _streamable;
    bool metadataFetched = false;

    private Task? _downloadTask;
    public async Task DownloadFile()
    {
        var opusPath="";
        if (File.Exists(_path + "/ytdlp-cache/"+_id+".opus"))
        {
            opusPath = _path + "/ytdlp-cache/" + _id + ".opus";
        }
        else
        {
            var res = await _ytdlp.RunAudioDownload(
                $"https://www.youtube.com/watch?v={_id}",
                AudioConversionFormat.Opus
            );
            opusPath = res.Data;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await Process.Start(
                    new ProcessStartInfo(
                        "ffmpeg",
                        arguments: $"-y -i \"{res.Data}\" -vn -acodec copy {Path.GetDirectoryName(res.Data)}/{_id}.opus"
                    )
                )!.WaitForExitAsync();
                opusPath = $"{Path.GetDirectoryName(res.Data)}/{_id}.opus";
            }

        }
        _streamable = new OpusFileStreamable(opusPath);
    }
    public void Dispose()
    {
        if (_streamable!= null)
        {
            _streamable.Dispose();
        }
    }

    public async Task Preheat()
    {
        Console.WriteLine($"Preheating {_id}");
        if (_streamable != null)
        {
            return;
        }
        if (_downloadTask!=null)
        {
            return;
        }
        
        _downloadTask = DownloadFile();
        
        await _downloadTask;
    }

    public async Task<ISampleSource> GetSampleSource()
    {
        if (_streamable != null)
        {
            return await _streamable.GetSampleSource();
        }

        await Preheat(); // preheat if someone didn't call it earlier
    
        return await _streamable.GetSampleSource();
    }
    public int Id { get; set; }
}