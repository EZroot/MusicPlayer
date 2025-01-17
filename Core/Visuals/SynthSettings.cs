using System;
using System.IO;
using System.Text.Json;
using SDL2Engine.Core.Utils;

namespace MusicPlayer.Core.Visuals
{
    public static class SynthSettings
    {
        public static bool ShowRectSynth = true;
        public static int RectSynthSmoothness = 4;
        public static int RectWidthModifier = 1;
        public static int RectMaxHeightModifier = 0;
        public static int RectSpacingModifier = 16;
        public static float RectBandIntensityModifier = 1.75f;

        public static bool ShowLineSynth = true;
        public static float LineSynthSmoothness = 0.1f;

        private static readonly string SettingsFilePath = "synth_settings.json";

        public static void SaveSettings()
        {
            try
            {
                var settings = new
                {
                    ShowRectSynth,
                    RectSynthSmoothness,
                    RectWidthModifier,
                    RectMaxHeightModifier,
                    RectSpacingModifier,
                    RectBandIntensityModifier,
                    ShowLineSynth,
                    LineSynthSmoothness
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
                Debug.Log("<color=green>Settings saved successfully.</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving settings: {ex.Message}");
            }
        }

        public static void LoadSettings()
        {
            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    Debug.Log("Warning: Settings file not found. Using default settings.");
                    return;
                }

                var json = File.ReadAllText(SettingsFilePath);
                var settings = JsonSerializer.Deserialize<dynamic>(json);

                ShowRectSynth = settings["ShowRectSynth"];
                RectSynthSmoothness = settings["RectSynthSmoothness"];
                RectWidthModifier = settings["RectWidthModifier"];
                RectMaxHeightModifier = settings["RectMaxHeightModifier"];
                RectSpacingModifier = settings["RectSpacingModifier"];
                RectBandIntensityModifier = (float)settings["RectBandIntensityModifier"];
                ShowLineSynth = settings["ShowLineSynth"];
                LineSynthSmoothness = (float)settings["LineSynthSmoothness"];

                Debug.Log("<color=green>Settings loaded successfully.</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading settings: {ex.Message}");
            }
        }
    }
}