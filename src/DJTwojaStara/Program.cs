using System;
using System.Reflection;
using DJTwojaStara.Services;
using DSharpPlus;
using DSharpPlus.Interactivity.Enums;
using Microsoft.Extensions.Hosting;
using Nefarius.DSharpPlus.Extensions.Hosting;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Nefarius.DSharpPlus.CommandsNext.Extensions.Hosting;
using Nefarius.DSharpPlus.Interactivity.Extensions.Hosting;
using Nefarius.DSharpPlus.SlashCommands.Extensions.Hosting;
using Nefarius.DSharpPlus.VoiceNext.Extensions.Hosting;

namespace DJTwojaStara;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddLogging();

                services.AddDiscord(options =>
                {
                    options.Token = hostContext.Configuration["Token"];
                    options.Intents = DiscordIntents.AllUnprivileged;
                });
                
                services
                    .AddSingleton<IPlaybackService, PlaybackService>()
                    .AddSingleton<IAiService, LLaMaService>()
                    .AddSingleton<YoutubeService>()
                    .AddHostedService<IStreamerService>(x => x.GetService<YoutubeService>());

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
            });
    }
}