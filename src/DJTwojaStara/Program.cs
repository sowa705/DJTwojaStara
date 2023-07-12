using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DJTwojaStara;
using DJTwojaStara.Repositories;
using DJTwojaStara.Services;
using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Nefarius.DSharpPlus.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nefarius.DSharpPlus.CommandsNext.Extensions.Hosting;
using Nefarius.DSharpPlus.Interactivity.Extensions.Hosting;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting;
using Nefarius.DSharpPlus.VoiceNext.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder);
ConfigureHosting(builder);

var app = builder.Build();

ConfigurePipeline(app);

app.UseAuthorization();
app.MapControllers();

app.Run();

void ConfigureServices(WebApplicationBuilder builder)
{
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddDbContext<MainDbContext>(options => options.UseSqlite("Data Source=database.db"));
    builder.Services.AddDirectoryBrowser();

    builder.Services
        .AddSingleton<IPlaybackService, PlaybackService>()
        .AddSingleton<IAiService, LLaMaService>()
        .AddSingleton<YoutubeService>()
        .AddScoped<PerformanceSnapshotRepository>()
        .AddHostedService<PerformanceSnapshotService>()
        .AddSingleton<IStreamerService,YoutubeService>()
        .AddHostedService(x => x.GetService<IStreamerService>());
}

void ConfigureHosting(WebApplicationBuilder builder)
{
    var services = builder.Services;
    services.AddLogging();

    services.AddDiscord(options =>
    {
        options.Token = builder.Configuration["Token"];
        options.Intents = DiscordIntents.AllUnprivileged;
    });
    
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
}

void ConfigurePipeline(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    var defaultFilesOptions = new DefaultFilesOptions 
    {
        DefaultFileNames = new List<string> { "index.html" }
    };
    if (app.Environment.IsDevelopment())
    {
        app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
    }

    app.UseDefaultFiles(defaultFilesOptions);
    app.UseStaticFiles(new StaticFileOptions { FileProvider = defaultFilesOptions.FileProvider });
    
    app.UseRouting();
    app.UseAuthorization();
    app.MapControllers();
    
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
    
    app.UseSpa(spa =>
    {
        spa.Options.SourcePath = "";
        spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions
        {
            FileProvider = defaultFilesOptions.FileProvider
        };
    });
}