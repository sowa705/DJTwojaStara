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
    Stream stream;
    
    public OpusFileStreamable(string path)
    {
        _path = path;
    }

    public Task Preheat() // not implemented and not needed
    {
        return Task.CompletedTask;
    }

    public Task<ISampleSource> GetSampleSource()
    {
        if (openSource != null)
        {
            return Task.FromResult(openSource);
        }
        
        stream = File.OpenRead(_path);
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
    public int Id { get; set; }

    public void Dispose()
    {
        if (openSource != null)
        {
            openSource.Dispose();
        }
        stream.Dispose();
    }
}