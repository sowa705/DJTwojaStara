using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSCore;
using DJTwojaStara.Audio;
using Microsoft.Extensions.Hosting;

namespace DJTwojaStara.Services;

public interface IStreamerService : IHostedService
{
    public Task<IStreamable> GetStreamable(SongInfo songInfo);
}

public interface ISearchService
{
    public Task<IEnumerable<SongInfo>> Search(string query);
    public Task<IEnumerable<SongInfo>> GetRelatedVideos(SongInfo song);
}