using System.Reflection;
using CliFx;

namespace Commander;

internal static class Program
{
    public static async Task<int> Main() =>
        await new CliApplicationBuilder()
            .SetTitle(nameof(Commander).ToLower())
            .SetExecutableName(nameof(Commander).ToLower())
            .SetVersion("0.1.0")
            .AddCommandsFrom(Assembly.GetExecutingAssembly())
            .Build()
            .RunAsync();
}