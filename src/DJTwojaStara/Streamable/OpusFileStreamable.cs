using System;
using System.IO;
using System.Threading.Tasks;
using ATL;
using CSCore;
using CSCore.Codecs.OPUS;

namespace DJTwojaStara.Audio;
public class OpusFileStreamable : IStreamable
{
    private readonly string _path;
    ISampleSource? openSource;
    
    public OpusFileStreamable(string path, string name)
    {
        _path = path;
        Name = name;
    }

    public void Preheat() // not implemented and not needed
    {
    }

    public Task<ISampleSource> GetSampleSource()
    {
        if (openSource != null)
        {
            return Task.FromResult(openSource);
        }
        
        var stream = File.OpenRead(_path);
        var sampleRate = 48000;
        try
        {
            sampleRate = (int)new Track(stream, ".opus").SampleRate;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        var waveSource = new OpusSource(stream, sampleRate, 2);

        openSource = waveSource.ToSampleSource().ChangeSampleRate(48000);

        return Task.FromResult(openSource);
    }

    public Task DownloadMetadataAsync()
    {
        return Task.CompletedTask;
    }

    public string Name { get; set; }
    public string CoverUrl { get; }

    public void Dispose()
    {
        if (openSource != null)
        {
            openSource.Dispose();
        }
    }
}