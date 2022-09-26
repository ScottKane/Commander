namespace ShellProgressBar.Example
{
	public interface IProgressBarExample
	{
		Task Start(CancellationToken token);
	}
}