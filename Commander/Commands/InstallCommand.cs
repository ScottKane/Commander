using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Pty.Net;

namespace Commander.Commands;

[Command("install", Description = "Installs a game server using steamcmd.")]
public class InstallCommand : SteamCmdCommand, ICommand
{
    private readonly ConcurrentDictionary<string, bool> _output = new();
    [CommandParameter(
        0,
        Name = "game-id",
        Description = "Id of the game to be installed.",
        IsRequired = true)]
    public string? GameId { get; init; }
    
    private async ValueTask OutputAsync(IConsole console, IPtyConnection connection, string line)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            if (!_output.ContainsKey(line))
                _output.TryAdd(line, false);

            if (!_output[line])
            {
                if (line.Contains("Steam Guard code:"))
                {
                    await console.Output.WriteAsync(line + " ");
                    var input = await console.Input.ReadLineAsync();
                    var buffer = Encoding.ASCII.GetBytes(input + Environment.NewLine);
                    // _output.TryAdd(input!, true);

                    await connection.WriterStream.WriteAsync(buffer);
                    _ = await connection.ReaderStream.ReadAsync(buffer);
                    await connection.WriterStream.FlushAsync();
                    _output[line] = true;
                    
                    return;
                }
                
                // await console.Output.WriteLineAsync("Line: " + line);
                await console.Output.WriteLineAsync(line);
                _output[line] = true;
            }
        }
    }

    protected override async ValueTask ParseAsync(IConsole console, IPtyConnection connection, string? line)
    {
        line = line switch
        {
            _ when line!.Contains("stderr") => null,
            _ when line.Contains("(c)") => null,
            _ when line.Contains("exit --") => null,
            _ when line.Contains("Warning:") => null,
            _ when line.Contains("Work thread") => null,
            _ => line
        };
        if (line is null) return;
        
        const string pattern = @"^(?:[[\[\]%\- 0-9]{6}).";
        if (Regex.Match(line, pattern).Success)
        {
            line = Regex.Replace(line, pattern, m => m.Groups[1].Value).Replace(".", string.Empty);
            if (line.Contains(','))
            {
                foreach (var l in line.Split(',').Select(x => x.Trim()))
                    await OutputAsync(console, connection, string.Concat(l[0].ToString().ToUpper(), l.AsSpan(1)));
        
                return;
            }
            await OutputAsync(console, connection, line);
            
            return;
        }
        
        if (line?.Split("...").Length > 1)
        {
            foreach (var l in line.Split("...").Select(x => x.Trim()))
                await OutputAsync(console, connection, l);
            
            return;
        }
        
        if (line!.Contains("Update state"))
        {
            var half = line.Split(',');
            var quarter = half[1].Split(':')[1].Split('(');
            var eighth = quarter[1].Replace(")", string.Empty).Split('/');
            
            var state = half[0].Split(')')[1].Trim().Split(' ')[0];
            state = string.Concat(state[0].ToString().ToUpper(), state.AsSpan(1));
            var percent = quarter[0].Trim();
            var current = eighth[0].Trim();
            var total = eighth[1].Trim();
            
            await OutputAsync(console, connection, $"{state} {percent}% ({current}/{total})");
        
            return;
        }
        
        await OutputAsync(console, connection, line);
    }

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        Command = new[] { $"+force_install_dir /app/{GameId}", $"+login {Username}{(Password is null ? string.Empty : $" {Password}")}", $"+app_update {GameId} validate" };
        // Command = Array.Empty<string>();
        await base.ExecuteAsync(console);
    }
}