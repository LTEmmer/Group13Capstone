using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using GodotDictionary = Godot.Collections.Dictionary;

// Lightweight bridge between C# gameplay code and the Talo Godot plugin autoload.
// This stays no-op when the plugin is not installed or configured.
public static class TaloTelemetry
{
	private const string FlashcardAnsweredEventName = "flashcard_answered";
	private const string FlashcardAnswersTotalStat = "flashcard_answers_total";
	private const string FlashcardAnswersCorrectStat = "flashcard_answers_correct";
	private const string FlashcardAnswersIncorrectStat = "flashcard_answers_incorrect";
	private const string EnemiesFacedTotalStat = "enemies_faced_total";
	private const string EnemiesDefeatedTotalStat = "enemies_defeated_total";
	private const string RoomsEnteredTotalStat = "rooms_entered_total";
	private const string FloorsClearedTotalStat = "floors_cleared_total";
	private const string ChestsOpenedTotalStat = "chests_opened_total";
	private const string DamageTakenTotalStat = "damage_taken_total";
	private const string DamageDealtTotalStat = "damage_dealt_total";
	private const string ItemsPickedUpTotalStat = "items_picked_up_total";

	private static bool _missingPluginLogged;
	private static bool _identifyAttempted;
	private static bool _identifySignalConnected;
	private static readonly List<PendingAnswer> _pendingAnswers = new();
	private static readonly List<PendingStatUpdate> _pendingStats = new();
	private static readonly Dictionary<string, float> _sessionStatValues = new();
	private static int _sessionTotalAnswers;
	private static int _sessionCorrectAnswers;
	private static int _sessionIncorrectAnswers;

	public static void ResetSessionStats()
	{
		_pendingAnswers.Clear();
		_pendingStats.Clear();
		_sessionStatValues.Clear();
		_sessionTotalAnswers = 0;
		_sessionCorrectAnswers = 0;
		_sessionIncorrectAnswers = 0;
	}

	public static void TrackEnemiesFaced(int count = 1)
	{
		TrackCountStat(EnemiesFacedTotalStat, count, trackToTalo: true);
	}

	public static void TrackEnemiesDefeated(int count = 1)
	{
		TrackCountStat(EnemiesDefeatedTotalStat, count);
	}

	public static void TrackRoomsEntered(int count = 1)
	{
		TrackCountStat(RoomsEnteredTotalStat, count, trackToTalo: true);
	}

	public static void TrackFloorsCleared(int count = 1)
	{
		TrackCountStat(FloorsClearedTotalStat, count, trackToTalo: true);
	}

	public static void TrackChestsOpened(int count = 1)
	{
		TrackCountStat(ChestsOpenedTotalStat, count, trackToTalo: true);
	}

	public static void TrackDamageTaken(float amount)
	{
		TrackAmountStat(DamageTakenTotalStat, amount);
	}

	public static void TrackDamageDealt(float amount)
	{
		TrackAmountStat(DamageDealtTotalStat, amount);
	}

	public static void TrackItemsPickedUp(int count = 1)
	{
		TrackCountStat(ItemsPickedUpTotalStat, count);
	}

	public static void TrackFlashcardAnswer(bool isCorrect, string action, float difficulty)
	{
		RecordSessionAnswer(isCorrect);

		if (!TryGetTalo(out Node talo))
		{
			return;
		}

		_pendingAnswers.Add(new PendingAnswer
		{
			IsCorrect = isCorrect,
			Action = string.IsNullOrWhiteSpace(action) ? "unknown" : action,
			Difficulty = difficulty
		});

		TryIdentifyAnonymousPlayer(talo);
		if (!IsPlayerIdentified(talo))
		{
			return;
		}

		FlushPendingAnswers(talo);
	}

	public static IReadOnlyList<SessionStatEntry> GetSessionStats()
	{
		return new List<SessionStatEntry>
		{
			new(FlashcardAnswersTotalStat, "Total questions answered", FormatCount(GetSessionValue(FlashcardAnswersTotalStat))),
			new(RoomsEnteredTotalStat, "Rooms entered", FormatCount(GetSessionValue(RoomsEnteredTotalStat))),
			new(FloorsClearedTotalStat, "Floors cleared", FormatCount(GetSessionValue(FloorsClearedTotalStat))),
			new(FlashcardAnswersCorrectStat, "Questions answered correctly", FormatCount(GetSessionValue(FlashcardAnswersCorrectStat))),
			new(FlashcardAnswersIncorrectStat, "Questions answered incorrectly", FormatCount(GetSessionValue(FlashcardAnswersIncorrectStat))),
			new(EnemiesFacedTotalStat, "Enemies faced", FormatCount(GetSessionValue(EnemiesFacedTotalStat))),
			new(EnemiesDefeatedTotalStat, "Enemies defeated", FormatCount(GetSessionValue(EnemiesDefeatedTotalStat))),
			new(DamageDealtTotalStat, "Damage dealt", FormatAmount(GetSessionValue(DamageDealtTotalStat))),
			new(DamageTakenTotalStat, "Damage taken", FormatAmount(GetSessionValue(DamageTakenTotalStat))),
			new(ChestsOpenedTotalStat, "Treasure chests opened", FormatCount(GetSessionValue(ChestsOpenedTotalStat))),
			new(ItemsPickedUpTotalStat, "Items picked up", FormatCount(GetSessionValue(ItemsPickedUpTotalStat)))
		};
	}

	public static SessionStatsSnapshot GetSessionSnapshot()
	{
		return new SessionStatsSnapshot(_sessionTotalAnswers, _sessionCorrectAnswers, _sessionIncorrectAnswers);
	}

	private static void RecordSessionAnswer(bool isCorrect)
	{
		_sessionTotalAnswers++;
		IncrementSessionValue(FlashcardAnswersTotalStat, 1f);

		IncrementSessionValue(isCorrect ? FlashcardAnswersCorrectStat : FlashcardAnswersIncorrectStat, 1f);
		if (isCorrect)
		{
			_sessionCorrectAnswers++;
		}
		else
		{
			_sessionIncorrectAnswers++;
		}
	}

	private static void FlushPendingAnswers(Node talo)
	{
		if (_pendingAnswers.Count == 0)
		{
			return;
		}

		try
		{
			GodotObject eventsApi = talo.Get("events").AsGodotObject();
			GodotObject statsApi = talo.Get("stats").AsGodotObject();

			foreach (var answer in _pendingAnswers)
			{
				if (eventsApi != null)
				{
					GodotDictionary props = new GodotDictionary
					{
						{ "correct", answer.IsCorrect ? "true" : "false" },
						{ "action", answer.Action },
						{ "difficulty", answer.Difficulty.ToString("0.00", CultureInfo.InvariantCulture) }
					};

					eventsApi.Call("track", FlashcardAnsweredEventName, props);
				}

				if (statsApi != null)
				{
					string outcomeStat = answer.IsCorrect ? FlashcardAnswersCorrectStat : FlashcardAnswersIncorrectStat;
					TrackTaloStat(talo, FlashcardAnswersTotalStat, 1.0f);
					TrackTaloStat(talo, outcomeStat, 1.0f);
				}
			}

			_pendingAnswers.Clear();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"TaloTelemetry: Failed to track flashcard answer: {ex.Message}");
		}
	}

	private static void IncrementSessionValue(string statName, float delta)
	{
		if (_sessionStatValues.ContainsKey(statName))
		{
			_sessionStatValues[statName] += delta;
			return;
		}

		_sessionStatValues[statName] = delta;
	}

	private static void TrackCountStat(string statName, int count, bool trackToTalo = false)
	{
		if (count <= 0)
		{
			return;
		}

		IncrementSessionValue(statName, count);

		if (!trackToTalo)
		{
			return;
		}

		if (!TryGetTalo(out Node talo) || !IsPlayerIdentified(talo))
		{
			for (int i = 0; i < count; i++)
			{
				_pendingStats.Add(new PendingStatUpdate(statName, 1.0f));
			}
			return;
		}

		for (int i = 0; i < count; i++)
		{
			TrackTaloStat(talo, statName, 1.0f);
		}
	}

	private static void TrackAmountStat(string statName, float amount)
	{
		if (amount <= 0f)
		{
			return;
		}

		IncrementSessionValue(statName, amount);
	}

	private static float GetSessionValue(string statName)
	{
		return _sessionStatValues.TryGetValue(statName, out float value) ? value : 0f;
	}

	private static string FormatCount(float value)
	{
		return Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
	}

	private static string FormatAmount(float value)
	{
		return value.ToString("0.##", CultureInfo.InvariantCulture);
	}

	private static void TrackTaloStat(Node talo, string statName, float delta)
	{
		try
		{
			GodotObject statsApi = talo.Get("stats").AsGodotObject();
			if (statsApi != null)
			{
				statsApi.Call("track", statName, delta);
			}
		}
		catch (Exception ex)
		{
			GD.PrintErr($"TaloTelemetry: Failed to track stat '{statName}': {ex.Message}");
		}
	}

	private static bool TryGetTalo(out Node talo)
	{
		talo = null;

		if (Engine.GetMainLoop() is not SceneTree tree)
		{
			return false;
		}

		talo = tree.Root.GetNodeOrNull<Node>("/root/Talo");
		if (talo != null)
		{
			return true;
		}

		if (!_missingPluginLogged)
		{
			GD.Print("TaloTelemetry: Talo plugin autoload '/root/Talo' not found. Telemetry is disabled.");
			_missingPluginLogged = true;
		}

		return false;
	}

	private static void TryIdentifyAnonymousPlayer(Node talo)
	{
		try
		{
			GodotObject playersApi = talo.Get("players").AsGodotObject();
			if (playersApi == null)
			{
				return;
			}

			TryConnectIdentifiedSignal(playersApi);

			if (_identifyAttempted)
			{
				return;
			}

			_identifyAttempted = true;

			string identifier = OS.GetUniqueId();
			if (string.IsNullOrWhiteSpace(identifier))
			{
				identifier = Guid.NewGuid().ToString("N");
			}

			// The plugin handles this asynchronously; this call simply starts identification.
			playersApi.Call("identify", "device", identifier);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"TaloTelemetry: Failed to identify player: {ex.Message}");
		}
	}

	private static void TryConnectIdentifiedSignal(GodotObject playersApi)
	{
		if (_identifySignalConnected || playersApi is not Node playersNode)
		{
			return;
		}

		Callable onIdentified = Callable.From<GodotObject>(OnPlayerIdentified);
		if (!playersNode.IsConnected("identified", onIdentified))
		{
			playersNode.Connect("identified", onIdentified);
		}

		_identifySignalConnected = true;
	}

	private static void OnPlayerIdentified(GodotObject _player)
	{
		if (!TryGetTalo(out Node talo))
		{
			return;
		}

		FlushPendingAnswers(talo);
		FlushPendingStats(talo);
	}

	private static void FlushPendingStats(Node talo)
	{
		if (_pendingStats.Count == 0)
		{
			return;
		}

		try
		{
			foreach (PendingStatUpdate stat in _pendingStats)
			{
				TrackTaloStat(talo, stat.StatName, stat.Amount);
			}

			_pendingStats.Clear();
		}
		catch (Exception ex)
		{
			GD.PrintErr($"TaloTelemetry: Failed to track pending stats: {ex.Message}");
		}
	}

	private static bool IsPlayerIdentified(Node talo)
	{
		try
		{
			return talo.Get("current_alias").AsGodotObject() != null;
		}
		catch
		{
			return false;
		}
	}

	public sealed class SessionStatEntry
	{
		public SessionStatEntry(string key, string label, string value)
		{
			Key = key;
			Label = label;
			Value = value;
		}

		public string Key { get; }
		public string Label { get; }
		public string Value { get; }
	}

	public readonly struct SessionStatsSnapshot
	{
		public SessionStatsSnapshot(int totalAnswers, int correctAnswers, int incorrectAnswers)
		{
			TotalAnswers = totalAnswers;
			CorrectAnswers = correctAnswers;
			IncorrectAnswers = incorrectAnswers;
		}

		public int TotalAnswers { get; }
		public int CorrectAnswers { get; }
		public int IncorrectAnswers { get; }
	}

	private sealed class PendingAnswer
	{
		public bool IsCorrect { get; init; }
		public string Action { get; init; }
		public float Difficulty { get; init; }
	}

	private sealed class PendingStatUpdate
	{
		public PendingStatUpdate(string statName, float amount)
		{
			StatName = statName;
			Amount = amount;
		}

		public string StatName { get; }
		public float Amount { get; }
	}
}
