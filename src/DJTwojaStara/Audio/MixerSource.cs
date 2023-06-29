using System;
using System.Collections.Generic;
using CSCore;

namespace DJTwojaStara.Audio;
class MixerSource : ISampleSource
{
    public MixerSource()
    {
        WaveFormat = new WaveFormat(48000, 32, 2);
    }
    public Queue<IStreamable> Sources = new Queue<IStreamable>();
    public IStreamable CurrentStreamable { get; private set; }
    private ISampleSource? _currentSource;
    
    public IStreamable? Interrupt { get; set; }
    private ISampleSource? _interruptSource;
    public int Read(float[] buffer, int offset, int count)
    {
        int readSamples = 0;
        while (readSamples != count)
        {
            if (_currentSource == null)
            {
                if (!Sources.TryPeek(out _))
                {
                    return 0;
                }

                CurrentStreamable = Sources.Dequeue();
                _currentSource = CurrentStreamable.GetSampleSource().Result;
                
                if (Sources.TryPeek(out _))
                {
                    Sources.Peek().Preheat();
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
                        return readSamples;
                    }
                }
                
                return readSamples;
            }
        }

        return readSamples;
    }

    public void Dispose()
    {
        Console.WriteLine("dispose called");
    }

    public void Skip()
    {
        Console.WriteLine("skip called");

        if (_currentSource != null)
        {
            CurrentStreamable.Dispose();
        }
        _currentSource = null;
    }

    public bool Available
    {
        get => Sources.Count > 0 || _currentSource!=null;
    }
    public bool CanSeek { get=>false; }
    public WaveFormat WaveFormat { get; }
    public long Position { get; set; }
    public long Length { get; }
}