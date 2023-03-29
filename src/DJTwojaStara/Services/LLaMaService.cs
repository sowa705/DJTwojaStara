using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;

namespace DJTwojaStara.Services;

public class LLaMaService: IHostedService, IAiService
{
    private readonly string _path = "llama.cpp";

    public LLaMaService()
    {
        _path = Path.GetTempPath()+"/djtwojastara-temp/";
    }
    
    private Process process;
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        return; //TODO make it actually work
        // check if the llama.cpp folder exists
        if (!Directory.Exists(_path))
        {
            // clone the repository
            if (OperatingSystem.IsWindows())
            {
                return;
            }
            else if (OperatingSystem.IsLinux())
            {
                await InstallLinux();
            }
            else if (OperatingSystem.IsMacOS())
            {
                await InstallLinux(); // macos is just a linux distro
            }
        }
        
        // check if the model exists
        if (File.Exists(_path+"llama.cpp/models/ggml-model-q4_0.bin"))
        {
            // download the model
            await DownloadModel();
        }
    }

    private async Task InstallLinux()
    {
        // get the repository off github
        await RunCommand("git clone https://github.com/tarruda/llama.cpp", _path); // we use a fork with TCP support
        
        // run make
        await RunCommand("make", _path+"/llama.cpp");
    }

    private async Task DownloadModel()
    {
        // TODO upload the model to a server and download it from there
    }

    private async Task DownloadFileTaskAsync(string uri, string fileName)
    {
        var client = new HttpClient();
        await using var s = await client.GetStreamAsync(uri);
        await using var fs = new FileStream(fileName, FileMode.CreateNew);
        await s.CopyToAsync(fs);
    }
    
    private async Task RunCommand(string command, string path)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command.Split(" ")[0],
                Arguments = command.Substring(command.IndexOf(" ")),
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = path
            }
        };
        process.Start();
        process.WaitForExit();
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        process.Kill();
        return Task.CompletedTask;
    }

    public async Task RespondToMessageAsync(string query, InteractionContext ctx)
    {
        const string serverAddress = "127.0.0.1";
        const int serverPort = 8080;
        const string rPrompt = "### Instruction:";
        const string nPredict = "4096";
        const string repeatPenalty = "1.0";
        // get the number of threads
        string nThreads = Environment.ProcessorCount.ToString();
        
        string userInput = query;
        
        // add the alpaca prompt to the user input (changing this prompt doesnt really work in my experience, YMMV)
        var alpacaPrompt = $"Write a response that appropriately completes the request.\n### Instruction:\n{userInput}\n### Response:";
        
        // Connect to the chat server
        TcpClient client = new TcpClient(serverAddress, serverPort);

        // Get the stream for reading from and writing to the server
        NetworkStream stream = client.GetStream();

        // Pass the arguments to the server
        string[] args = {
            "-t", nThreads, "-n", nPredict, "--repeat_penalty", repeatPenalty, "-i", "-r", rPrompt, "-p", alpacaPrompt
        };
        byte[] argsBytes = GetArgsBytes(args);
        stream.Write(argsBytes, 0, argsBytes.Length);

        

        // Read responses from the server
        byte[] buffer = new byte[1024];
        
        var embed = new DiscordEmbedBuilder
        {
            Title = "DJ Twoja Stara",
            Description = "Generating response...",
            Color = DiscordColor.Gold,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Responding to stupid questions since 2023",
                IconUrl = "https://cdn.discordapp.com/emojis/1076975001183469578.png?v=1"
            }
        };
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        var fullResponse = "";
        bool startEmbed = false;
        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                Console.WriteLine("(disconnected, press Enter to exit)");
                break;
            }
            string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            // wait until server sends the Instruction prompt to start sending embeds
            if (response.Contains("== Running in interactive mode. =="))
            {
                response = response.Substring(response.IndexOf("== Running in interactive mode. =="));
                startEmbed = true;
            }
            if(!startEmbed)
                continue;
            fullResponse += response;
            
            // get everything after ### instruction: so the user can see the bot working
            if (startEmbed&& fullResponse.Contains(rPrompt))
            {
                var text = fullResponse.Substring(fullResponse.IndexOf(rPrompt));
                // send the embed
                
                var embed2 = new DiscordEmbedBuilder
                {
                    Title = "DJ Twoja Stara",
                    Description = text,
                    Color = DiscordColor.Gold,
                    Footer = new DiscordEmbedBuilder.EmbedFooter
                    {
                        Text = "Responding to stupid questions since 2023",
                        IconUrl = "https://cdn.discordapp.com/emojis/1076975001183469578.png?v=1"
                    }
                };
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed2));
                
                // count the number of ### instruction: in the response
                var instructionPrompts = text.Split(rPrompt).Length-1;
                
                // if there are 2 ### instruction: then the response is complete
                if (instructionPrompts == 2)
                {
                    break;
                }
            }
        }
        
        // change the embed color to green to indicate that the response is complete
        
        var finalResponse = fullResponse.Substring(fullResponse.IndexOf("### Response:")+14);
        
        // cut the ### Instruction: off the end of the response
        var finalResponse2 = finalResponse.Substring(0, finalResponse.IndexOf(rPrompt));

        var finalEmbed = new DiscordEmbedBuilder
        {
            Title = "DJ Twoja Stara",
            Description = $"**{query}**\n\n"+finalResponse2,
            Color = DiscordColor.Green,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Responding to stupid questions since 2023",
                IconUrl = "https://cdn.discordapp.com/emojis/1076975001183469578.png?v=1"
            }
        };
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(finalEmbed));

        // Close the connection
        stream.Close();
        client.Close();
    }
    private static byte[] GetArgsBytes(string[] args)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(args.Length.ToString());
        foreach (string arg in args)
        {
            builder.Append(arg).Append('\0');
        }
        return Encoding.ASCII.GetBytes(builder.ToString());
    }
}