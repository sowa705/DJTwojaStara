using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DJTwojaStara.Models;
using Microsoft.EntityFrameworkCore;

namespace DJTwojaStara.Repositories;

public class PerformanceSnapshotRepository
{
    private readonly MainDbContext _dbContext;
    
    public PerformanceSnapshotRepository(MainDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AddAsync(PerformanceSnapshot snapshot, CancellationToken cancellationToken)
    {
        await _dbContext.PerformanceSnapshots.AddAsync(snapshot, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<PerformanceSnapshot>> GetLastHourAsync(CancellationToken cancellationToken)
    {
        var list = await _dbContext.PerformanceSnapshots.ToListAsync(cancellationToken: cancellationToken);
        return list.Where(x => x.Timestamp > DateTimeOffset.Now.AddHours(-1));
    }
}