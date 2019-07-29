// This document is intended for use by Kupo Nut Brigade developers.

namespace KupoNutsBot.Utils
{
	using System;
	using System.Diagnostics;
	using System.Threading.Tasks;

	public static class BashUtils
	{
		public static async Task<string> Run(string cmd, bool waitforExit = true)
		{
			Log.Write("> " + cmd);

			string escapedArgs = cmd.Replace("\"", "\\\"");

			ProcessStartInfo info = new ProcessStartInfo();
			info.FileName = "/bin/bash";
			info.Arguments = $"-c \"{escapedArgs}\"";
			info.RedirectStandardOutput = waitforExit;
			info.UseShellExecute = false;
			info.CreateNoWindow = true;

			Process process = new Process();
			process.StartInfo = info;
			process.Start();

			if (!waitforExit)
				return null;

			string result = process.StandardOutput.ReadToEnd();

			while (!process.HasExited)
				await Task.Yield();

			process.WaitForExit();

			if (process.ExitCode != 0)
				throw new Exception(result);

			Log.Write("< " + result);

			return result;
		}
	}
}
