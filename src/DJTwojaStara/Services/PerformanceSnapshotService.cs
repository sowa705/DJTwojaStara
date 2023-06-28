using System;
using System.Threading;
using System.Threading.Tasks;
using DJTwojaStara.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Universe.CpuUsage;

namespace DJTwojaStara.Services;

public class PerformanceSnapshotService : IHostedService
{
    private readonly MainDbContext _dbContext;
    private Timer _timer;
    private readonly ILogger<PerformanceSnapshotService> _logger;

    public PerformanceSnapshotService(MainDbContext dbContext, ILogger<PerformanceSnapshotService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var lastCpuTime = CpuUsage.Get(CpuUsageScope.Process);
        // schedule a task to run every 5 seconds
        _timer = new Timer(async _ =>
        {
            var currentCpuTime = CpuUsage.Get(CpuUsageScope.Process);
            // get performance snapshot
            var snapshot = new PerformanceSnapshot
            {
                Host = Environment.MachineName,
                CPU = (float) (currentCpuTime - lastCpuTime).Value.TotalMicroSeconds / 1000,
                RAM = GC.GetTotalMemory(false) / 1024f / 1024f,
                CacheSize = Helpers.GetUsedCacheSpace(),
                Timestamp = DateTimeOffset.Now
            };
            _logger.LogInformation("Performance snapshot: {Snapshot}", snapshot);
            // save snapshot to database
            await _dbContext.PerformanceSnapshots.AddAsync(snapshot, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            lastCpuTime = currentCpuTime;
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Dispose();
        return Task.CompletedTask;
    }
}