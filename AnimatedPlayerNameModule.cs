using AliceInCradleHack.config;
using AliceInCradleHack.module;
using AliceInCradleHack.utils.client;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AnimatedPlayerName
{
    /// <summary>
    /// Registers an animated leet / ASCII-split name as the player's Kaleidoscopic multiplayer name.
    /// A Harmony postfix on <c>Kaleidoscopic.Syncs.SyncPatcherPlayers::createPlayerInfo</c> rewrites
    /// <c>PlayerInfo.playerName</c> every frame, so both the local head-up display and the name that
    /// gets broadcast to other players animate: split stages build up character by character, then the
    /// name breaks back down, repeating forever (e.g. "ASM" => / -> /- -> /-\ -> A -> A5 -> AS ->
    /// AS/ -> AS/\ -> AS/\/ -> AS/\/\ -> ASM -> AS/\/\ -> AS/\/ -> ...). Middle characters can use
    /// leet forms (S -> 5, E -> 3, ...).
    /// </summary>
    public sealed class AnimatedPlayerNameModule : Module
    {
        public AnimatedPlayerNameModule() : base("AnimatedPlayerName", "Animated leet/split name for the Kaleidoscopic multiplayer display.", "Misc")
        {
        }

        public override bool IsEnabled { get; set; } = true;

        public readonly Value<string> DisplayName = new(defaultValue: "ASM", description: "Base name that will be animated.");
        public readonly RangedValue<int> FrameMs = new(defaultValue: 150, min: 10, max: 2000, suffix: "ms", description: "Milliseconds per animation frame.");
        public readonly RangedValue<int> HoldMs = new(defaultValue: 400, min: 0, max: 5000, suffix: "ms", description: "How long the full name stays visible before breaking down (0 = no hold).");
        public readonly Value<bool> UseLeet = new(defaultValue: true, description: "Use leet forms for middle characters.");

        private static readonly Harmony _harmony = new("animatedplayername.module");
        private static readonly Stopwatch _clock = new();
        private static readonly object _sync = new();

        private static string _baseName = "ASM";
        private static int _frameMs = 150;
        private static int _holdMs = 400;
        private static bool _useLeet = true;
        private static string[] _frames = { "" };
        private static int _frameIndex;
        private static int _holdIndex;
        private static long _nextStepMs;

        public override void Initialize()
        {
            ConfigSystem.Load(Settings);

            DisplayName.OnChanged(v => ApplyName(v));
            FrameMs.OnChanged(v => _frameMs = Math.Max(10, v));
            HoldMs.OnChanged(v => _holdMs = Math.Max(0, v));
            UseLeet.OnChanged(v => { _useLeet = v; Rebuild(); });

            ApplyName(DisplayName.Get());
            _frameMs = Math.Max(10, FrameMs.Get());
            _holdMs = Math.Max(0, HoldMs.Get());
            _useLeet = UseLeet.Get();
            Rebuild();

            var persisted = Settings.GetNodeByPath("__IsEnabled") as Value<bool>;
            bool shouldEnable = persisted?.Get() ?? IsEnabled;
            if (shouldEnable)
            {
                Enable();
                IsEnabled = true;
            }
        }

        public override void Enable()
        {
            _clock.Restart();
            Rebuild();

            var original = AccessTools.Method("Kaleidoscopic.Syncs.SyncPatcherPlayers:createPlayerInfo");
            if (original == null)
            {
                Log.Error("AnimatedPlayerName: Kaleidoscopic.Syncs.SyncPatcherPlayers.createPlayerInfo not found.");
                return;
            }

            _harmony.Patch(original, postfix: new HarmonyMethod(typeof(AnimatedPlayerNameModule), nameof(CreatePlayerInfoPostfix)));
            Log.Info("AnimatedPlayerName: animated name patch applied.");
        }

        public override void Disable()
        {
            _harmony.UnpatchAll(_harmony.Id);
        }

        // Rewrites the playerName that Kaleidoscopic is about to display / broadcast.
        private static void CreatePlayerInfoPostfix(object __result)
        {
            if (__result == null) return;

            string name;
            lock (_sync)
            {
                long now = _clock.ElapsedMilliseconds;
                if (_nextStepMs == 0) _nextStepMs = now + StepMs(_frameIndex);
                while (_nextStepMs <= now)
                {
                    _frameIndex++;
                    if (_frameIndex >= _frames.Length) _frameIndex = 0;
                    _nextStepMs += StepMs(_frameIndex);
                }
                name = _frames[_frameIndex];
            }

            if (name == null) return;
            var field = __result.GetType().GetField("playerName");
            field?.SetValue(__result, name);
        }

        /// <summary>Dwell time of a frame; the full-name frame is held for <see cref="_holdMs"/>.</summary>
        private static int StepMs(int index)
        {
            return index == _holdIndex && _holdMs > 0 ? _holdMs : _frameMs;
        }

        private static void ApplyName(string value)
        {
            _baseName = string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
            Rebuild();
        }

        /// <summary>
        /// Precomputes the animation frame list for the current base name:
        /// build-up per character (stages then final form), then full name, then breakdown in reverse.
        /// </summary>
        private static void Rebuild()
        {
            string name = _baseName;
            if (string.IsNullOrEmpty(name))
            {
                lock (_sync)
                {
                    _frames = new[] { "" };
                    _frameIndex = 0;
                    _nextStepMs = 0;
                }
                return;
            }

            char[] chars = name.ToCharArray();
            int n = chars.Length;

            var stageList = new List<string[]>(n);
            for (int i = 0; i < n; i++)
            {
                char c = chars[i];
                char upper = char.ToUpperInvariant(c);
                string[] charStages;
                if (_useLeet && i > 0 && i < n - 1 && LeetForms.TryGetValue(upper, out string leet))
                {
                    charStages = new[] { leet, c.ToString() };
                }
                else if (SplitStages.TryGetValue(upper, out string[] split))
                {
                    charStages = new string[split.Length + 1];
                    Array.Copy(split, charStages, split.Length);
                    charStages[split.Length] = c.ToString();
                }
                else
                {
                    charStages = new[] { c.ToString() };
                }
                stageList.Add(charStages);
            }

            var stages = stageList.ToArray();
            var frames = new List<string>(n * 8);
            var prefix = new char[n];

            for (int i = 0; i < n; i++)
            {
                string p = Prefix(prefix, i);
                for (int j = 0; j < stages[i].Length; j++)
                    frames.Add(p + stages[i][j]);
                prefix[i] = chars[i];
            }

            // The frame the full target string is first assembled: hold it for _holdMs.
            int holdIndex = frames.Count - 1;

            for (int i = n - 1; i >= 0; i--)
            {
                string p = Prefix(prefix, i);
                for (int j = stages[i].Length - 2; j >= 0; j--)
                    frames.Add(p + stages[i][j]);
                prefix[i] = '\0';
            }

            lock (_sync)
            {
                _frames = frames.ToArray();
                _frameIndex = 0;
                _holdIndex = holdIndex;
                _nextStepMs = 0;
            }
        }

        private static string Prefix(char[] prefix, int upTo)
        {
            var sb = new StringBuilder(upTo);
            for (int k = 0; k < upTo; k++)
                if (prefix[k] != '\0') sb.Append(prefix[k]);
            return sb.ToString();
        }

        /// <summary>Progressive ASCII breakdown of each character (final normal form is appended at runtime).</summary>
        private static readonly Dictionary<char, string[]> SplitStages = new()
        {
            ['A'] = new[] { "/", "/-", "/-\\" },
            ['B'] = new[] { "|3" },
            ['C'] = new[] { "(" },
            ['D'] = new[] { "|)" },
            ['E'] = new[] { "|-", "3" },
            ['F'] = new[] { "|=" },
            ['G'] = new[] { "6" },
            ['H'] = new[] { "|-|", "#" },
            ['I'] = new[] { "|", "1" },
            ['J'] = new[] { "_|" },
            ['K'] = new[] { "|<" },
            ['L'] = new[] { "|_" },
            ['M'] = new[] { "/", "/\\", "/\\/", "/\\/\\" },
            ['N'] = new[] { "|\\|" },
            ['O'] = new[] { "0" },
            ['P'] = new[] { "|D" },
            ['Q'] = new[] { "0_" },
            ['R'] = new[] { "|2" },
            ['S'] = new[] { "5" },
            ['T'] = new[] { "|-", "7" },
            ['U'] = new[] { "|_|" },
            ['V'] = new[] { "\\/" },
            ['W'] = new[] { "\\/", "\\/\\/" },
            ['X'] = new[] { ">", "><" },
            ['Y'] = new[] { "`/" },
            ['Z'] = new[] { "2" },
            ['0'] = new[] { "()" },
            ['1'] = new[] { "1" },
            ['2'] = new[] { "2" },
            ['3'] = new[] { "3" },
            ['4'] = new[] { "4" },
            ['5'] = new[] { "5" },
            ['6'] = new[] { "6" },
            ['7'] = new[] { "7" },
            ['8'] = new[] { "8" },
            ['9'] = new[] { "9" }
        };

        /// <summary>Leet variants used for the middle characters of a name (e.g. S -> 5).</summary>
        private static readonly Dictionary<char, string> LeetForms = new()
        {
            ['A'] = "4", ['B'] = "8", ['C'] = "(", ['D'] = "|)", ['E'] = "3",
            ['F'] = "|=", ['G'] = "6", ['H'] = "#", ['I'] = "1", ['J'] = "_|",
            ['K'] = "|<", ['L'] = "|_", ['M'] = "/\\/\\", ['N'] = "|\\|", ['O'] = "0",
            ['P'] = "|D", ['Q'] = "0_", ['R'] = "|2", ['S'] = "5", ['T'] = "7",
            ['U'] = "|_|", ['V'] = "\\/", ['W'] = "\\/\\/", ['X'] = "><", ['Y'] = "`/",
            ['Z'] = "2"
        };
    }
}
