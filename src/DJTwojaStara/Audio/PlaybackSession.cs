using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSCore;
using CSCore.Streams.Effects;
using DJTwojaStara.Models;
using DJTwojaStara.Services;
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
    private IStreamerService _streamerService;
    
    private EQPreset _eqPreset = EQPreset.Normal;
    public EQPreset EQPreset
    {
        get => _eqPreset;
        set {
            SetEQPreset(value);
        }
    }

    public PlayList PlayList { get => _playList; }

    int trackCount = 0;
    
    private PlayList _playList = new PlayList();
    public PlaybackSession(ulong channelID, VoiceNextConnection client, ILogger logger, IStreamerService streamerService)
    {
        id = Guid.NewGuid().ToString();
        ChannelID = channelID;
        _audioClient = client;
        _logger = logger;
        _streamerService = streamerService;
        
        
        _mixer = new MixerSource(_playList, _streamerService);
    }
    public async Task StartStreamAsync()
    {
        _logger.LogInformation("Starting playback session...");

        await WriteToAudioClient(_audioClient);
    }
    
    public async Task AddToQueue(IEnumerable<SongInfo> songs)
    {
        foreach (var source in songs)
        {
            _logger.LogInformation("Adding source {source} to queue", source.Name);
            _playList.Songs.Add(source);
            _playList.SongOrder.Add(source.Id);
        }
    }
    
    public void RemoveById(int id)
    {
        //are we removing the current song?
        if (_mixer.CurrentStreamable.Id == id)
        {
            NextSong();
        }
        else
        {
            _playList.SongOrder.Remove(id.ToString());
        }
    }
    
    public void NextSong()
    {
        if(_playList.CurrentSong < _playList.Songs.Count - 1)
        {
            _logger.LogInformation("Skipping to next song");
            _playList.CurrentSong++;
            _mixer.ReloadSong();
            _playList.CurrentPosition = 0;
        }
        else
        {
            throw new InvalidOperationException("Cannot skip to next song, no more songs in queue");
        }
    }
    
    public void PreviousSong()
    {
        if(_playList.CurrentSong > 0)
        {
            _logger.LogInformation("Skipping to previous song");
            _playList.CurrentSong--;
            _mixer.ReloadSong();
            _playList.CurrentPosition = 0;
        }
        else
        {
            throw new InvalidOperationException("Cannot skip to previous song, no more songs in queue");
        }
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

    private async Task WriteToAudioClient(VoiceNextConnection client)
    {
        LastPlayTime=DateTime.UtcNow;
        using (var speakStream = client.GetTransmitSink(60))
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
                    var bytesToSpeak = new byte[48000*8]; // 48000 samples * 2 channels * 2 bytes per sample = 2 seconds of audio
                    var read = source.Read(bytesToSpeak, 0, (int)bytesToSpeak.Length);
                    _playList.CurrentPosition += read / (double)discordFormat.BytesPerSecond;
                    await speakStream.WriteAsync(bytesToSpeak[..read]);
                }
                else
                {
                    await Task.Delay(500); // throttle cpu usage
                }
            }
            await speakStream.FlushAsync().ConfigureAwait(false);
        }
        _logger.LogInformation("Timed out");

        await DisconnectAsync();
    }
}