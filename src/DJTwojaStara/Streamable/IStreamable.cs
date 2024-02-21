using System;
using System.Threading.Tasks;
using CSCore;

namespace DJTwojaStara.Audio;

public interface IStreamable: IDisposable
{
    /// <summary>
    /// Tells the streamable to prepare itself for reading soon
    /// </summary>
    Task Preheat();
    Task<ISampleSource> GetSampleSource();
    public int Id { get; set; }
}