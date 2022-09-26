using System.Text;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Commander.Converters;
using Pty.Net;

namespace Commander.Commands;

public abstract class SteamCmdCommand
{
    private readonly TaskCompletionSource<uint> _process = new();
    
    [CommandOption(
        "steamcmd-directory",
        's',
        Description = "Path to directory containing steamcmd executable.",
        EnvironmentVariable = "STEAMCMD_LOCATION",
        Converter = typeof(DirectoryConverter))]
    
    public string? SteamCmdPath { get; set; }
    [CommandOption(
        "username",
        'u',
        Description = "Username for logging in with steamcmd.",
        EnvironmentVariable = "STEAMCMD_USERNAME",
        Converter = typeof(DirectoryConverter),
        IsRequired = true)]
    
    public string? Username { get; set; }
    [CommandOption(
        "password",
        'p',
        Description = "Password for logging in with steamcmd.",
        EnvironmentVariable = "STEAMCMD_PASSWORD",
        Converter = typeof(DirectoryConverter))]
    
    public string? Password { get; set; }
    protected IEnumerable<string> Command { get; set; } = Array.Empty<string>();

    protected abstract ValueTask ParseAsync(IConsole console, IPtyConnection connection, string? line);

    private async ValueTask ParseAsyncCore(IConsole console, IPtyConnection connection, string? line)
    {
        if (!string.IsNullOrWhiteSpace(line))
            await ParseAsync(console, connection, line);
    }
    
    public virtual async ValueTask ExecuteAsync(IConsole console)
    {
        // try
        // {
            var tokenSource = new CancellationTokenSource();
            var connection = await PtyProvider.SpawnAsync(new PtyOptions()
            {
                Cwd = SteamCmdPath!,
                App = "steamcmd",
                CommandLine = Command.Append("+quit").ToArray(),
            }, tokenSource.Token);
    
            connection.ProcessExited += (_, e) => _process.TrySetResult((uint) e.ExitCode);
    
            var buffer = new byte[1024];
            var output = string.Empty;
            var read = 1;
            
            while (!tokenSource.Token.IsCancellationRequested && !_process.Task.IsCompleted && read > 0)
            {
                try
                {
                    read = await connection.ReaderStream.ReadAsync(buffer, tokenSource.Token);
                    output += Encoding.ASCII.GetString(buffer, 0, read);
                    await connection.ReaderStream.FlushAsync(tokenSource.Token);

                    foreach (var line in output.Split('\n'))
                    {
                        await ParseAsyncCore(console, connection, line);
                        output = output[line.Length..].TrimStart('\r', '\n');
                    }
                }
                catch (IOException e)
                {
                    console.Output.WriteLine(e.HResult & 0x0000FFFF);
                    console.Output.WriteLine(_process.Task.Result);

                    if ( /*(e.HResult & 0x0000FFFF) != 5 ||*/ _process.Task.Result is not (0xC000013A or 1 or 0))
                        throw;
                }
            }
        // }
        // catch (IOException e)
        // {
        //     console.Output.WriteLine(e.HResult & 0x0000FFFF);
        //     
        //     if (/*(e.HResult & 0x0000FFFF) != 5 ||*/ _process.Task.Result is not (0xC000013A or 1 or 0))
        //         throw;
        // }
    }
}