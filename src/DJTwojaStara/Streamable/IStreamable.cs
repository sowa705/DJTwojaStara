using System;
using System.Threading.Tasks;
using CSCore;

namespace DJTwojaStara.Audio;

public interface IStreamable: IDisposable
{
    /// <summary>
    /// Tells the streamable to prepare itself for reading soon
    /// </summary>
    void Preheat();
    Task<ISampleSource> GetSampleSource();
    public Task DownloadMetadataAsync();
    public string Name { get; }
    public string CoverUrl { get; }
    public int Id { get; set; }
}