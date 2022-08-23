﻿using System;
using System.Diagnostics;
using System.IO;
using OsuDiffCalc.OsuMemoryReader.OsuMemoryModels;
using OsuDiffCalc.Utility;

namespace OsuDiffCalc.OsuMemoryReader;

internal readonly record struct OsuMemoryState(
	OsuStatus Status,
	OsuGameMode GameMode,
	int MapId,
	int SetId,
	string MapString,
	string FolderName,
	string OsuFileName,
	string MD5FileHash
) {
	public OsuMemoryState()
		: this(OsuStatus.Unknown, OsuGameMode.Unknown, 0, 0, null, null, null, null) {
	}
	public static OsuMemoryState Invalid => new();

	public bool IsInGame => Status is OsuStatus.Playing or OsuStatus.ResultsScreen or OsuStatus.MultiplayerResultsScreen;
	public bool IsInEditor => Status is OsuStatus.EditingMap;
}

internal class OsuStateReader {
	private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();
	private static readonly char[] _invalidFileNameChars = Path.GetInvalidFileNameChars();

	// Can find needle in https://github.com/Piotrekol/ProcessMemoryDataFinder/blob/95030bba9c5e2de0667c4ae95e6e6b1fbbde3d5c/OsuMemoryDataProvider/StructuredOsuMemoryReader.cs#L53
	private const string _osuBaseAddressNeedleHexString = "F80174048365";
	private static readonly ProcessPropertyReader _memoryReader = new(_osuBaseAddressNeedleHexString);
	
	private static OsuStatus _prevStatus = OsuStatus.Unknown;
	private static OsuGameMode _prevGameMode = OsuGameMode.Unknown;

	public static bool TryReadCurrentOsuState(Process osuProcess, out OsuMemoryState currentState) {
		if (osuProcess is not null && !osuProcess.HasExitedSafe()) {
			try {
				_memoryReader.TargetProcess = osuProcess;

				currentState = new OsuMemoryState {
					Status      = _memoryReader.ReadProperty<GeneralData, OsuStatus>(nameof(GeneralData.RawStatus), OsuStatus.Unknown),
					GameMode    = _memoryReader.ReadProperty<GeneralData, OsuGameMode>(nameof(GeneralData.GameMode), OsuGameMode.Unknown),
					MapId       = _memoryReader.ReadProperty<GeneralData, int>(nameof(CurrentBeatmap.Id)),
					SetId       = _memoryReader.ReadProperty<GeneralData, int>(nameof(CurrentBeatmap.SetId)),
					MapString   = _memoryReader.ReadProperty<CurrentBeatmap, string>(nameof(CurrentBeatmap.MapString)),
					FolderName  = _memoryReader.ReadProperty<CurrentBeatmap, string>(nameof(CurrentBeatmap.FolderName)),
					OsuFileName = _memoryReader.ReadProperty<CurrentBeatmap, string>(nameof(CurrentBeatmap.OsuFileName)),
					MD5FileHash = _memoryReader.ReadProperty<CurrentBeatmap, string>(nameof(CurrentBeatmap.MD5FileHash)),
				};

				if (currentState.Status != _prevStatus) {
					Console.WriteLine($"   memory current status: {currentState.Status}");
					_prevStatus = currentState.Status;
				}
				if (currentState.GameMode != _prevGameMode) {
					Console.WriteLine($"   memory current game mode: {currentState.GameMode}");
					_prevGameMode = currentState.GameMode;
				}

				// under rare circumstances, can end up with broken strings if user is changing state while we are reading memory
				// so we check for invalid path chars to try to hedge against this
				return !string.IsNullOrEmpty(currentState.MapString) 
						&& !string.IsNullOrEmpty(currentState.FolderName)
						&& !string.IsNullOrEmpty(currentState.OsuFileName)
						&& currentState.FolderName.IndexOfAny(_invalidPathChars) == -1
						&& currentState.OsuFileName.IndexOfAny(_invalidFileNameChars) == -1;
			}
			catch (Exception ex) {
				Console.WriteLine("Failed to find current beatmap for osu process");
				Console.WriteLine(ex);
#if DEBUG
				System.Diagnostics.Debugger.Break();
#endif
			}
		}
		currentState = OsuMemoryState.Invalid;
		return false;
	}
}
