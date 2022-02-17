﻿namespace OsuDiffCalc {
	using System;
	using System.Windows.Forms;
	using System.Threading;
	using UserInterface;
	using System.Diagnostics;

	class Program {
		/// <summary>
		/// Program-wide settings
		/// </summary>
		internal static Properties.Settings Settings { get; private set; } = Properties.Settings.Default;

		/// <summary> System-wide pid for the console window thread </summary>
		internal static int ConsolePid { get; private set; }

		/// <summary> Win32 window HANDLE for the console thread which called Main() </summary>
		internal static IntPtr ConsoleWindowHandle { get; private set; }

		//the STAThread is needed to call Ux.getFileFromDialog()->.openFileDialog()
		[STAThread]
		static void Main(string[] args) {
			if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
				Thread.CurrentThread.Name = "Main.Thread";

			// ensure fonts look OK on monitors with non-default scaling set
			if (Environment.OSVersion.Version.Major >= 6)
				NativeMethods.SetProcessDPIAware();

			// configure settings
			Settings = Properties.Settings.Default;
#if DEBUG
			Settings.Reset();
#endif
			Settings.Upgrade();

			var hWndConsole = NativeMethods.GetConsoleWindow();
			int consolePid = -1;
			if (hWndConsole != IntPtr.Zero) {
				NativeMethods.GetWindowThreadProcessId(hWndConsole, out uint nativePid);
				consolePid = (int)nativePid;
			}
			ConsoleWindowHandle = hWndConsole;
			ConsolePid = consolePid != -1 ? consolePid : (int)NativeMethods.GetCurrentThreadId();

			// TODO: use argument as initialization point for map analysis
			//       check if argument is a valid path to .osu file or directory containing osu files

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new GUI());

			//Finder.debugAllProcesses();

			/*
			Beatmap test = new Beatmap(Ux.getFilenameFromDialog());
			MapsetManager.analyzeMap(test, false);
			test.printDebug();
			*/
			//MapsetManager.analyzeCurrentMapset();
#if DEBUG
			try {
				Console.WriteLine("-----------program finished-----------");
				Console.ReadKey();
			}
			catch { }
#endif
		}
	}
}