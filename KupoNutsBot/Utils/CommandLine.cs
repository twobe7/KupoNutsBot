// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Utils
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Medallion.Shell;

	public static class CommandLine
	{
		public static CommandProcess Run(string cmd)
		{
			return new CommandProcess(cmd);
		}

		public static async Task<string> RunAsync(string cmd)
		{
			CommandProcess proc = new CommandProcess(cmd);
			await proc.Wait();
			return proc.Output;
		}

		public static CommandProcess DotNetRun(string file)
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				file = "../../" + file;

			return Run("dotnet " + file);
		}

		public class CommandProcess
		{
			private Command process;
			private StringBuilder output;

			public CommandProcess(string cmd)
			{
				Log.Write("> " + cmd);

				string escapedArgs = cmd.Replace("\"", "\\\"");

				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					this.process = Command.Run("CMd.exe", $"/C " + cmd);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					this.process = Command.Run("/bin/bash", $"-c \"{escapedArgs}\"");
				}

				if (this.process == null)
					throw new Exception("Failed to start command line process");

				this.output = new StringBuilder();

				Task.Run(async () => { await this.WatchOutput(); });
			}

			public string Output
			{
				get
				{
					return this.output.ToString();
				}
			}

			public async Task Kill()
			{
				if (this.process == null)
					return;

				await this.process.TrySignalAsync(CommandSignal.ControlC);
				this.process.Kill();

				await this.process.Task;
			}

			public async Task Wait()
			{
				await this.process.Task;

				CommandResult result = this.process.Result;

				if (!result.Success)
				{
					throw new Exception("Command exited with code " + result.ExitCode, new Exception(result.StandardError));
				}
			}

			private async Task WatchOutput()
			{
				try
				{
					while (this.process != null && this.process.Task != null && !this.process.Task.IsCompleted)
					{
						await Task.Yield();
						Thread.Sleep(100);

						string line = this.process.StandardOutput.ReadLine();

						if (string.IsNullOrEmpty(line))
							continue;

						Log.Write("< " + line);
						this.output.AppendLine(line);
					}
				}
				catch (Exception ex)
				{
					Log.Write(ex);
				}
			}
		}
	}
}
