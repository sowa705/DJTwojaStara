using System;
using System.Collections.Generic;
using CSCore;

namespace DJTwojaStara.Audio
{
    class MixerSource : ISampleSource
    {
        public Queue<IStreamable> Sources = new Queue<IStreamable>();
        public IStreamable CurrentStreamable { get; private set; }
        private ISampleSource _currentSource;
        public IStreamable Interrupt { get; set; }
        private ISampleSource _interruptSource;

        public int Read(float[] buffer, int offset, int count)
        {
            int readSamples = 0;
            while (readSamples < count)
            {
                EnsureCurrentSource();
                EnsureInterruptSource();

                int samplesRead = ReadFromCurrentOrInterruptSource(buffer, readSamples, count);
                readSamples += samplesRead;

                if (readSamples < count)
                {
                    DropSourceIfEnded(samplesRead);
                }
            }

            return readSamples;
        }

        private void EnsureCurrentSource()
        {
            if (_currentSource is null && Sources.TryPeek(out _))
            {
                CurrentStreamable = Sources.Dequeue();
                _currentSource = CurrentStreamable.GetSampleSource().Result;

                if (Sources.TryPeek(out _))
                {
                    Sources.Peek().Preheat();
                }
            }
        }

        private void EnsureInterruptSource()
        {
            if (_interruptSource is null && Interrupt is not null)
            {
                _interruptSource = Interrupt.GetSampleSource().Result;
            }
        }

        private int ReadFromCurrentOrInterruptSource(float[] buffer, int readSamples, int totalSamples)
        {
            if (_interruptSource is not null)
            {
                return _interruptSource.Read(buffer, readSamples, totalSamples - readSamples);
            }

            return _currentSource.Read(buffer, readSamples, totalSamples - readSamples);
        }

        private void DropSourceIfEnded(int samplesRead)
        {
            if (_interruptSource is not null && samplesRead == 0)
            {
                Interrupt = null;
                _interruptSource = null;
            }
            else if (_currentSource is not null && samplesRead == 0)
            {
                _currentSource = null;
            }
        }

        public void Dispose()
        {
            Console.WriteLine("dispose called");
        }

        public void Skip()
        {
            Console.WriteLine("skip called");

            if (_currentSource is not null)
            {
                CurrentStreamable.Dispose();
            }

            _currentSource = null;
        }

        public bool Available => Sources.Count > 0 || _currentSource is not null;
        public bool CanSeek => false;
        public WaveFormat WaveFormat { get; } = new(48000, 32, 2);
        public long Position { get; set; }
        public long Length { get; }
    }
}