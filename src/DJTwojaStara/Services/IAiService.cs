using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace DJTwojaStara.Services;

public interface IAiService
{
    Task RespondToMessageAsync(string query, InteractionContext ctx);
    Task AbortAsync();
}