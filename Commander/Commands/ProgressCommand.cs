using System.Text;
using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using ShellProgressBar.Example;
using ShellProgressBar.Example.Examples;
using ShellProgressBar.Example.TestCases;

namespace Commander.Commands;

[Command("progress")]
public class ProgressCommand : ICommand
{
    private static readonly IList<IProgressBarExample> TestCases = new List<IProgressBarExample>
    {
        new PersistMessageExample(),
        new FixedDurationExample(),
        new DeeplyNestedProgressBarTreeExample(),
        new NestedProgressBarPerStepProgress(),
        new DrawsOnlyOnTickExample(),
        new ThreadedTicksOverflowExample(),
        new TicksOverflowExample(),
        new NegativeMaxTicksExample(),
        new ZeroMaxTicksExample(),
        new LongRunningExample(),
        new NeverCompletesExample(),
        new UpdatesMaxTicksExample(),
        new NeverTicksExample(),
        new EstimatedDurationExample(),
        new IndeterminateProgressExample(),
    };

    private static readonly IList<IProgressBarExample> Examples = new List<IProgressBarExample>
    {
        new DontDisplayInRealTimeExample(),
        new StylingExample(),
        new ProgressBarOnBottomExample(),
        new ChildrenExample(),
        new ChildrenNoCollapseExample(),
        new IntegrationWithIProgressExample(),
        new IntegrationWithIProgressPercentageExample(),
        new MessageBeforeAndAfterExample(),
        new DeeplyNestedProgressBarTreeExample(),
        new EstimatedDurationExample(),
        new DownloadProgressExample()
    };

    public async ValueTask ExecuteAsync(IConsole console)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var token = new CancellationTokenSource();
        // Console.CancelKeyPress += (s, e) => { cts.Cancel(); };

        var command = "test";
        switch (command)
        {
            case "test":
                await RunTestCases(console, token.Token).ConfigureAwait(false);
                break;
            // case "example":
            //     var nth = args.Length > 1 ? int.Parse(args[1]) : 0;
            //     await RunExample(token, nth);
            //     return;
            default:
                await console.Error.WriteLineAsync($"Unknown command:{command}");
                break;
        }
    }
    
    private static async ValueTask RunExample(CancellationToken token, int nth)
    {
        if (nth > Examples.Count - 1 || nth < 0)
            await Console.Error.WriteLineAsync($"There are only {Examples.Count} examples, {nth} is not valid");

        var example = Examples[nth];

        await example.Start(token);
    }

    private static async ValueTask RunTestCases(IConsole console, CancellationToken token)
    {
        // var i = 0;
        foreach (var example in TestCases)
        {
            // if (i > 0) Console.Clear(); //not necessary but for demo/recording purposes.
            await example.Start(token);
            // i++;
        }
        await console.Output.WriteAsync("Shown all examples!");
    }

    public static void BusyWait(int milliseconds)
    {
        Thread.Sleep(milliseconds);
    }
}