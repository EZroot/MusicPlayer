using System.Numerics;
using ImGuiNET;
using MusicPlayer.Core.Audio;
using MusicPlayer.Core.Events;
using MusicPlayer.Core.Gui.Bindings;
using MusicPlayer.Core.Gui.Interfaces;
using MusicPlayer.Core.Helper;
using MusicPlayer.Core.Visuals;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Events;

namespace MusicPlayer.Core.Gui;

public class MusicPlayerGui : IGui
{
    private IAudioService m_audioService;
    private AudioPlayer m_audioPlayer;
    private AudioQueue m_audioQueue;
    private MusicPlayerBindings m_bindings;
    private string m_currentDirectory = AppHelper.SOUND_FOLDER;
    private string m_ytdlpSearchBuffer = "";
    private List<VideoInfo> m_ytdlpSearchResults = new();

    private string m_downloadingPercent;
    private string m_downloadSpeed;
    
    public MusicPlayerGui(IAudioService audioService, AudioQueue audioQueue, AudioPlayer audioPlayer)
    {
        m_audioPlayer = audioPlayer;
        m_audioService = audioService;
        m_audioQueue = audioQueue;
        m_bindings = new MusicPlayerBindings();
    }
    
    public void Initialize()
    {
        
    }

    public void RenderGui()
    {
        if (ImGui.Begin("Left Dock"))
        {
            BuildDirectory();
            ImGui.End();
        }

        if (ImGui.Begin("Right Dock"))
        {
            var winSize = ImGui.GetWindowSize();
            EventHub.Raise(this, new OnMainDockResized((int)winSize.X, (int)winSize.Y));
            ImGui.End();
        }
    }

    private bool m_isSongPaused = false;

    private void BuildDirectory()
    {
        Vector2 availableSize = ImGui.GetContentRegionAvail();
        float childWidth = (availableSize.X) - ImGui.GetStyle().ItemSpacing.X;
        float childHeight = availableSize.Y;
        var currentSong = m_audioQueue.IsSongPlaying ? m_audioQueue.CurrentSong : "";
        var songStatus = m_audioQueue.IsSongPlaying ? m_audioQueue.IsSongPlaying && m_isSongPaused ? "Paused:" : "Playing:" : "No song playing";
        ImGui.SeparatorText($"Vol ({AppHelper.GLOBAL_VOLUME}/128) | {songStatus} {currentSong}");
        ImGui.Text("Volume:");
        ImGui.SameLine();
        if (ImGui.SmallButton("+"))
        {
            var prev = AppHelper.GLOBAL_VOLUME;
            prev = prev + 5;
            if (prev >= 128)
                prev = 128;

            AppHelper.GLOBAL_VOLUME = prev;
            SDL_mixer.Mix_VolumeMusic(AppHelper.GLOBAL_VOLUME);
        }
        ImGui.SameLine();
        if (ImGui.SmallButton("-"))
        {
            var prev = AppHelper.GLOBAL_VOLUME;
            prev = prev - 5;
            if (prev <= 0)
                prev = 0;
            AppHelper.GLOBAL_VOLUME = prev;
            SDL_mixer.Mix_VolumeMusic(AppHelper.GLOBAL_VOLUME);
        }
        ImGui.Separator();
        if (m_audioQueue.IsSongPlaying)
        {
            if (m_isSongPaused)
            {
                if (ImGui.Button("Resume"))
                {
                    m_isSongPaused = false;
                    SDL_mixer.Mix_ResumeMusic();
                }
            }
            else
            {
                if (ImGui.Button("Pause"))
                {
                    m_isSongPaused = true;
                    SDL_mixer.Mix_PauseMusic();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Skip"))
            {
                m_audioPlayer.StopAudio();
            }
        }
        ImGui.Separator();
        if(ImGui.BeginTabBar("TabBar"))
        {
            if (ImGui.BeginTabItem("Music"))
            {
                ImGui.BeginChild("##MusicList", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders);
                {
                    BuildDirectorySearch();
                }
                ImGui.EndChild();

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Yt-dlp"))
            {
                ImGui.BeginChild("##YtDlp", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders);
                {
                    BuildYtDlpSearch();
                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Queue"))
            {
                ImGui.BeginChild("##QueueList", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders);
                {
                    BuildQueue();
                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.BeginChild("##Settings", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders);
                {
                    BuildSettings();
                }
                ImGui.EndChild();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        
        
        
    }

    private void BuildSettings()
    {
        ImGui.SeparatorText("Line Synth");
        if (ImGui.Checkbox("Show LineSynth", ref SynthSettings.ShowLineSynth))
        {
            
        }

        if (ImGui.InputFloat("Smoothing", ref SynthSettings.LineSynthSmoothness, 0.025f))
        {
            
        }
        
        ImGui.SeparatorText("Rect Synth");
        if (ImGui.Checkbox("Show RectSynth", ref SynthSettings.ShowRectSynth))
        {
            
        }
        ImGui.Separator();

        if (ImGui.InputInt("RectMultiplier", ref SynthSettings.RectSynthSmoothness, 1))
        {
            
        }

        if (ImGui.InputFloat("Intensity", ref SynthSettings.RectBandIntensityModifier, 0.1f))
        {
            
        }
        ImGui.Separator();
        if (ImGui.InputInt("RectWidthMod", ref SynthSettings.RectWidthModifier, 1))
        {
            
        }
        if (ImGui.InputInt("RectMaxHeightMod", ref SynthSettings.RectMaxHeightModifier, 1))
        {
            
        }
        if (ImGui.InputInt("RectSpacingMod", ref SynthSettings.RectSpacingModifier, 1))
        {
            
        }
    }

    private void BuildYtDlpSearch()
    {
        if (ImGui.InputText("##Search:", ref m_ytdlpSearchBuffer, 1024))
        {
            
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Search"))
        {
            m_ytdlpSearchResults.Clear();
            Task.Run(() =>
            {
                Debug.Log($"Searching results... {m_ytdlpSearchBuffer}");
                var results = YouTubeSearcher.SearchYouTube(m_ytdlpSearchBuffer);
                foreach (var video in results)
                {
                    m_ytdlpSearchResults.Add(video);
                    Debug.Log($"Title: {video.Title}, URL: {video.Url}, Duration: {video.Duration}");
                }
            });
        }
        ImGui.SeparatorText($"Search Results - Download Percent: {m_downloadingPercent} - {m_downloadSpeed}");
        foreach (var result in m_ytdlpSearchResults)
        {
            var title = result.Title;
            if (result.Title.Length > 64)
            {
                title = result.Title.Substring(0, 64);
            }
            
            ImGui.Separator();
            if (ImGui.SmallButton($"Download##{result.Title}"))
            {
                Task.Run(() => YouTubeSearcher.DownloadAudio(result.Url, $"{m_currentDirectory}/{title}.mp3",
                    (x, y) => { 
                        m_downloadingPercent = x;
                        m_downloadSpeed = y;
                    }));
                m_ytdlpSearchResults.Clear();
                break;
            }
            ImGui.SameLine();
            ImGui.Text($"{result.Duration} {title} {result.Url}");
        }
    }

    private void BuildQueue()
    {
        ImGui.Text("Current Queue");
        ImGui.Separator();
            
        foreach (var queue in m_audioQueue.QueuedAudio)
        {
            ImGui.SameLine();
            ImGui.Text(queue);
            ImGui.Separator();
        }
    }
    private void BuildDirectorySearch()
    {
        if (ImGui.InputText("Directory", ref m_currentDirectory, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
        {
                
        }
            
        ImGui.Separator();
        if (Directory.Exists(m_currentDirectory))
        {
            var files = Directory.GetFiles(m_currentDirectory);
            foreach (var file in files)
            {
                var filePath = file.Replace(AppHelper.SOUND_FOLDER, "");
                if (filePath.EndsWith(".wav") || filePath.EndsWith(".mp3") || filePath.EndsWith(".ogg") || filePath.EndsWith(".mp4"))
                {
                    if (ImGui.Button($"Queue##{filePath}"))
                    {
                        //queue audio
                        if (m_audioQueue.QueuedAudio.Count == 0)
                        {
                            if (m_audioQueue.IsSongPlaying)
                            {
                                m_audioQueue.Enqueue(file);
                            }
                            else
                            {
                                m_audioQueue.IsSongPlaying = true;
                                m_audioQueue.CurrentSong = filePath;
                                m_audioPlayer.LoadAudio(file, AudioType.Music);
                                m_audioPlayer.PlayAudio(AppHelper.GLOBAL_VOLUME);
                            }
                        }
                        else
                        {
                            m_audioQueue.Enqueue(file);
                        }
                    }

                    ImGui.SameLine();
                    ImGui.Text(filePath);
                    ImGui.Separator();
                }

            }
        }

    }
}