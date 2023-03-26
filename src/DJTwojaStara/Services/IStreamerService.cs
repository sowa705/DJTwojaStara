using System.Collections.Generic;
using System.Threading.Tasks;
using CSCore;
using DJTwojaStara.Audio;
using Microsoft.Extensions.Hosting;

namespace DJTwojaStara.Services;

public interface IStreamerService : IHostedService
{
    public Task<IEnumerable<IStreamable>> StreamSongs(string searchQuery);
}