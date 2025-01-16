using System.Numerics;
using ImGuiNET;
using MusicPlayer.Core.Audio;
using MusicPlayer.Core.Gui.Bindings;
using MusicPlayer.Core.Gui.Interfaces;
using MusicPlayer.Core.Helper;
using SDL2Engine.Core.Addressables.Interfaces;

namespace MusicPlayer.Core.Gui;

public class MusicPlayerGui : IGui
{
    private IAudioService m_audioService;
    private AudioPlayer m_audioPlayer;
    private AudioQueue m_audioQueue;
    private MusicPlayerBindings m_bindings;
    private string m_currentDirectory = AppHelper.SOUND_FOLDER;
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
            BuildConsole();
            ImGui.End();
        }
    }

    private void BuildConsole()
    {
        Vector2 availableSize = ImGui.GetContentRegionAvail();
        float childWidth = (availableSize.X) - ImGui.GetStyle().ItemSpacing.X;
        float childHeight = availableSize.Y;
        var currentSong = m_audioQueue.IsSongPlaying ? m_audioQueue.CurrentSong : "N/A";
        ImGui.SeparatorText($"Current: {currentSong}");
        if (m_audioQueue.IsSongPlaying)
        {
            if (ImGui.Button("Stop"))
            {
                m_audioPlayer.FreeAudio();
            }
        }

        ImGui.BeginChild("##MusicList", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders);
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
                                    //need a way to tell when audio is already playing
                                    // m_audioService.UnregisterEffects(0);
                                    
                                    m_audioPlayer.LoadAudio(file);
                                    m_audioPlayer.PlayAudio();
                                    // var soundId = m_audioService.LoadSound(file, AudioType.Wave);
                                    // m_audioService.PlaySound(soundId, AppHelper.GLOBAL_VOLUME);
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
            
            ImGui.Separator();
            ImGui.Text("Current Queue");
            ImGui.Separator();
            
            foreach (var queue in m_audioQueue.QueuedAudio)
            {
                    ImGui.SameLine();
                    ImGui.Text(queue);
                    ImGui.Separator();
            }
        }
        ImGui.EndChild();
        
    }
}