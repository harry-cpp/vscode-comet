﻿using System;
using System.IO;
using System.Linq;
using VsCodeMobileUtil;

namespace VSCodeDebug {
	public class MSBuild {
		static string exePath = "/bin/zsh";
		static MSBuild ()
		{
			if (Util.IsWindows)
			{
				exePath = GetWindowsMSBuildPath();
			}
			else
			{
				if (!File.Exists(exePath))
					exePath = "/bin/bash";
			}
		}
		static string GetWindowsMSBuildPath ()
		{
			//%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
			// -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe

			var workingDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft Visual Studio", "Installer");

			var p = new System.Diagnostics.Process {
				StartInfo = {
					CreateNoWindow = true,
					FileName = Path.Combine(workingDir, "vswhere.exe"),
					Arguments = "-latest -requires Microsoft.Component.MSBuild -find MSBuild\\**\\Bin\\MSBuild.exe",
					UseShellExecute = false,
					RedirectStandardOutput = true,
				}
			};
			p.Start ();
			p.WaitForExit ();
			return p.StandardOutput.ReadToEnd ()?.Trim();
		}
		public static (bool Success, string Output) Run (string workingDirectory, params string [] args)
		{
			//On non windows platforms, we run this through bash
			var newArgs = Util.IsWindows ? args :
				new [] { "msbuild" }.Union (args).ToArray ();
			var p = new System.Diagnostics.Process ();

			try {
				p.StartInfo.CreateNoWindow = false;
				p.StartInfo.FileName = exePath;
				p.StartInfo.WorkingDirectory = workingDirectory;
				//p.StartInfo.RedirectStandardOutput = true;
				if (Util.IsWindows)
					p.StartInfo.Arguments = Utilities.ConcatArgs (newArgs);
				else
					p.StartInfo.Arguments = "-c \"" + Utilities.ConcatArgs(newArgs) + "\"";

				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				p.Start ();
				p.WaitForExit ();
				return (true, p.StandardOutput.ReadToEnd());

			} catch (Exception ex) {

				Console.WriteLine (ex);
			}

			return (false, "");
		}

		
	}
}
