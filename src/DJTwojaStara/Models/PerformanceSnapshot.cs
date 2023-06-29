using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DJTwojaStara.Models;
[PrimaryKey(nameof(Timestamp), nameof(Host))]
public record PerformanceSnapshot
{
    public string Host { get; init; } = "";
    public float CPU { get; init; }
    public float RAM { get; init; }
    public float CacheSize { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}