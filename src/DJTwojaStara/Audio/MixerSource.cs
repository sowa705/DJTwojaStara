using System;
using System.Collections.Generic;
using System.Linq;
using CSCore;
using DJTwojaStara.Models;
using DJTwojaStara.Services;

namespace DJTwojaStara.Audio;
class MixerSource : ISampleSource
{
    private readonly PlayList _playList;
    private readonly IStreamerService _streamerService;
    public IStreamable CurrentStreamable { get; private set; }
    private ISampleSource? _currentSource;
    
    public IStreamable? Interrupt { get; set; }
    private ISampleSource? _interruptSource;
    
    public MixerSource(PlayList playList, IStreamerService streamerService)
    {
        _playList = playList;
        _streamerService = streamerService;
        
        WaveFormat = new WaveFormat(48000, 32, 2);
    }
    
    public int Read(float[] buffer, int offset, int count)
    {
        try
        {
            int readSamples = 0;
            while (readSamples != count)
            {
                if (_currentSource == null)
                {
                    // can we get a new song?
                    if (_playList.CurrentSong + 1 > _playList.SongOrder.Count)
                    {
                        // no more songs in the queue
                        return readSamples;
                    }

                    Console.WriteLine("reading next song");
                    
                    // lets get the next song in the queue
                    var nextSong = _playList.Songs.First(x => x.Id == _playList.SongOrder[_playList.CurrentSong]);
                    CurrentStreamable = _streamerService.GetStreamable(nextSong).Result;
                    
                    _currentSource = CurrentStreamable.GetSampleSource().Result;
                    _playList.CurrentPosition = 0;
                    
                    // preheat next song
                    if (_playList.CurrentSong + 1 < _playList.SongOrder.Count)
                    {
                        var nextNextSong = _playList.Songs.First(x => x.Id == _playList.SongOrder[_playList.CurrentSong + 1]);
                        _streamerService.GetStreamable(nextNextSong).Result.Preheat().ConfigureAwait(false);
                    }
                }

                if (_interruptSource == null)
                {
                    if (Interrupt is not null)
                    {
                        _interruptSource = Interrupt.GetSampleSource().Result;
                    }
                }

                var samples = 0;
                if (_interruptSource is not null)
                {
                    samples = _interruptSource.Read(buffer,readSamples, count-readSamples);
                }
                else
                {
                    samples = _currentSource.Read(buffer,readSamples, count-readSamples);
                }
                readSamples += samples;
                

                if (readSamples!=count) // no more data in the stream, dequeue to the next
                {
                    if (_interruptSource is not null)
                    {
                        if (samples==0) //reasonably close to the end
                        {
                            Interrupt = null;
                            _interruptSource = null;
                            return readSamples;
                        }
                    }
                    else
                    {
                        if (samples==0) //reasonably close to the end
                        {
                            _currentSource = null;
                            _playList.CurrentSong++;
                            return readSamples;
                        }
                    }
                    
                    return readSamples;
                }
            }

            return readSamples;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void Dispose()
    {
        var stackTrace = new System.Diagnostics.StackTrace();
        Console.WriteLine("dispose called by " + stackTrace.ToString());
    }

    public void ReloadSong()
    {
        _currentSource = null;
        
        if (CurrentStreamable is not null)
            CurrentStreamable.Dispose();
        CurrentStreamable = null;
    }
    public bool Available
    {
        get => _playList.Songs.Count > 0;
    }
    public bool CanSeek { get=>false; }
    public WaveFormat WaveFormat { get; }
    public long Position { get; set; }
    public long Length { get; }
}