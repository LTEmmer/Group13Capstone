using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

// Lightweight bridge between C# gameplay code and the Talo Godot plugin autoload.
// This stays no-op when the plugin is not installed or configured.
public static class TaloTelemetry
{
    private const string FlashcardAnsweredEventName = "flashcard_answered";
    private const string FlashcardAnswersTotalStat = "flashcard_answers_total";
    private const string FlashcardAnswersCorrectStat = "flashcard_answers_correct";
    private const string FlashcardAnswersIncorrectStat = "flashcard_answers_incorrect";

    private static bool _missingPluginLogged;
    private static bool _identifyAttempted;
    private static bool _identifySignalConnected;
    private static readonly List<PendingAnswer> _pendingAnswers = new();

    public static void TrackFlashcardAnswer(bool isCorrect, string action, float difficulty)
    {
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
                    var props = new Dictionary
                    {
                        { "correct", answer.IsCorrect ? "true" : "false" },
                        { "action", answer.Action },
                        { "difficulty", answer.Difficulty.ToString("0.00") }
                    };

                    eventsApi.Call("track", FlashcardAnsweredEventName, props);
                }

                if (statsApi != null)
                {
                    string outcomeStat = answer.IsCorrect ? FlashcardAnswersCorrectStat : FlashcardAnswersIncorrectStat;
                    statsApi.Call("track", FlashcardAnswersTotalStat, 1.0f);
                    statsApi.Call("track", outcomeStat, 1.0f);
                }
            }

            _pendingAnswers.Clear();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"TaloTelemetry: Failed to track flashcard answer: {ex.Message}");
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

    private sealed class PendingAnswer
    {
        public bool IsCorrect { get; init; }
        public string Action { get; init; }
        public float Difficulty { get; init; }
    }
}
