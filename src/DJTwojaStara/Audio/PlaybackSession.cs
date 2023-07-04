using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSCore;
using CSCore.Streams.Effects;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.Logging;
using Template;

namespace DJTwojaStara.Audio;
public class PlaybackSession
{
    public string id;
    private VoiceNextConnection _audioClient;
    private MixerSource _mixer;
    readonly WaveFormat discordFormat = new WaveFormat(48000, 16, 2, AudioEncoding.Pcm);
    private DateTime LastPlayTime;
    public ulong ChannelID;
    private ILogger _logger;
    public bool Disconnected;
    private Equalizer _equalizer;
    
    private EQPreset _eqPreset = EQPreset.Normal;
    public EQPreset EQPreset
    {
        get => _eqPreset;
        set {
            SetEQPreset(value);
        }
    }

    int trackCount = 0;
    public PlaybackSession(ulong channelID, VoiceNextConnection client, ILogger logger)
    {
        id = Guid.NewGuid().ToString();
        ChannelID = channelID;
        _audioClient = client;
        _logger = logger;
        
        _mixer = new MixerSource();
    }
    public async Task StartStreamAsync()
    {
        _logger.LogInformation("Starting playback session...");

        await WriteToAudioClient(_audioClient);
    }
    
    public void AddToQueue(IEnumerable<IStreamable> sources)
    {
        foreach (var source in sources)
        {
            source.Id = trackCount++;
            _logger.LogInformation("Adding source {source} to queue", source.Name);
            _mixer.Sources.Enqueue(source);
        }
    }
    
    public void RemoveById(int id)
    {
        _mixer.Sources = new Queue<IStreamable>(_mixer.Sources.Where(x => x.Id != id));
    }

    public void Skip()
    {
        _mixer.Skip();
    }
    public void SetEQPreset(EQPreset preset)
    {
        _eqPreset = preset;
        _equalizer.UseEQPreset(preset);
    }
    
    public void SetInterruption(IStreamable interruption)
    {
        _mixer.Interrupt=interruption;
    }

    public async Task DisconnectAsync()
    {
        _logger.LogInformation("Disconnecting...");

        Disconnected = true; 
        _audioClient.Disconnect();
    }
    
    public List<IStreamable> GetQueue()
    {
        var list = _mixer.Sources.ToList();
        list.Insert(0,_mixer.CurrentStreamable);
        return list;
    }

    private async Task WriteToAudioClient(VoiceNextConnection client)
    {
        LastPlayTime=DateTime.UtcNow;
        using (var speakStream = client.GetTransmitSink())
        using (var source = _mixer
                   .ToStereo()
                   .AppendSource(x => _equalizer = Equalizer.Create10BandEqualizer(x))
                   .ToWaveSource(discordFormat.BitsPerSample))
        {
            while (DateTime.UtcNow-LastPlayTime<TimeSpan.FromMinutes(3))
            {
                if (_mixer.Available)
                {
                    LastPlayTime=DateTime.UtcNow;
                    var bytesToSpeak = new byte[48000];
                    var read = source.Read(bytesToSpeak, 0, (int)bytesToSpeak.Length);
                    
                    await speakStream.WriteAsync(bytesToSpeak[..read]);
                }
            }
            await speakStream.FlushAsync().ConfigureAwait(false);
        }
        _logger.LogInformation("Timed out");

        await DisconnectAsync();
    }
}