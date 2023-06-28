using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DJTwojaStara;
using DJTwojaStara.Repositories;
using DJTwojaStara.Services;
using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Hosting;
using Nefarius.DSharpPlus.Extensions.Hosting;
using DSharpPlus.SlashCommands;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nefarius.DSharpPlus.CommandsNext.Extensions.Hosting;
using Nefarius.DSharpPlus.Interactivity.Extensions.Hosting;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting;
using Nefarius.DSharpPlus.VoiceNext.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MainDbContext>(options =>
{
    options.UseSqlite("Data Source=database.db");
});

var services = builder.Services;
services.AddLogging();

services.AddDiscord(options =>
{
    options.Token = builder.Configuration["Token"];
    options.Intents = DiscordIntents.AllUnprivileged;
});
                
services
    .AddSingleton<IPlaybackService, PlaybackService>()
    .AddSingleton<IAiService, LLaMaService>()
    .AddSingleton<YoutubeService>()
    .AddScoped<PerformanceSnapshotRepository>()
    .AddHostedService<PerformanceSnapshotService>()
    .AddSingleton<IStreamerService,YoutubeService>()
    .AddHostedService(x => x.GetService<IStreamerService>());
builder.Services.AddDirectoryBrowser();

services.AddDiscordSlashCommands();
                    
services.AddDiscordCommandsNext(options =>
{
    options.StringPrefixes = new[] { ">" };
    options.EnableDms = false;
    options.EnableMentionPrefix = true;
}, extension =>
{
    var cmd = extension.Client.GetSlashCommands();
    cmd.RegisterCommands(Assembly.GetExecutingAssembly());
});

services.AddDiscordInteractivity(options =>
{
    options.PaginationBehaviour = PaginationBehaviour.WrapAround;
    options.ResponseBehavior = InteractionResponseBehavior.Ack;
    options.ResponseMessage = "That's not a valid button";
    options.Timeout = TimeSpan.FromMinutes(2);
});
                    
//add voicenext
services.AddDiscordVoiceNext();

services.AddDiscordHostedService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseDefaultFiles();

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();