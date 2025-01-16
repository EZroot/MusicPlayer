using Microsoft.Extensions.DependencyInjection;
using MusicPlayer.Core.Audio;
using MusicPlayer.Core.Gui;
using MusicPlayer.Core.Gui.Interfaces;
using MusicPlayer.Core.Helper;
using MusicPlayer.Core.Visuals;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;
using SDL2Engine.Core.Windowing.Interfaces;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData.Audio;

namespace MusicPlayer.Core;

public class MusicApp : IGame
{
    private IWindowService m_windowService;
    private IRenderService m_renderService;
    private IGuiRenderService m_guiRenderService;
    private IAudioService m_audioService;
    private AudioQueue m_audioQueue;
    
    private AudioSynthesizer m_audioSynth;
    private AudioPlayer m_audioPlayer;
    
    private IGui m_dockerGui;
    private IGui m_musicPlayerGui;

    private bool m_debugConsole = true;

    public void Initialize(IServiceProvider serviceProvider)
    {
        m_windowService = serviceProvider.GetService<IWindowService>() ?? throw new InvalidOperationException("IWindowService is required but not registered.");
        m_renderService = serviceProvider.GetService<IRenderService>() ?? throw new InvalidOperationException("IRenderService is required but not registered.");
        m_guiRenderService = serviceProvider.GetService<IGuiRenderService>() ?? throw new InvalidOperationException("IGuiRenderService is required but not registered.");
        m_audioService = serviceProvider.GetService<IAudioService>() ?? throw new InvalidOperationException("IAudioService is required but not registered.");

        SDL.SDL_GetWindowSize(m_windowService.WindowPtr, out var width, out var height);
        
        m_audioSynth = new AudioSynthesizer(width, height, m_audioService);
        m_audioSynth.Initialize();

        m_audioQueue = new AudioQueue();
        m_audioPlayer = new AudioPlayer(m_audioService);
        
        m_dockerGui = new DockerGui(m_guiRenderService);
        m_dockerGui.Initialize();
        
        m_musicPlayerGui = new MusicPlayerGui(m_audioService, m_audioQueue, m_audioPlayer);
        m_musicPlayerGui.Initialize();
        
        // var soundId = m_audioService.LoadSound(AppHelper.SOUND_FOLDER + "/skidrow.wav");
        // m_audioService.PlaySound(soundId, AppHelper.GLOBAL_VOLUME);
        
        EventHub.Subscribe<OnAudioProcessFinished>(OnAudioProcessFinished);
        //todo: yt-dlp downloader if its unavailable
        //browse and download vids
        //file explorerr gui window to filter music
        //buttons to play or queue music
    }

    private void OnAudioProcessFinished(object? sender, OnAudioProcessFinished e)
    {
        if (m_audioQueue.QueuedAudio.Count == 0)
        {
            m_audioQueue.IsSongPlaying = false;
            m_audioQueue.CurrentSong = "";
            return;
        }
        var filePath = m_audioQueue.Dequeue();
        if (!string.IsNullOrEmpty(filePath))
        {
            m_audioQueue.IsSongPlaying = true;
            m_audioQueue.CurrentSong = filePath.Replace(AppHelper.SOUND_FOLDER, "");
            
            // m_audioService.UnregisterEffects(0);
            m_audioPlayer.LoadAudio(filePath);
            m_audioPlayer.PlayAudio();
        }
    }

    public void Update(float deltaTime)
    {
    }

    public void Render()
    {
        m_audioSynth.Render(m_renderService.RenderPtr);
    }

    public void RenderGui()
    {
        m_dockerGui.RenderGui();
        m_musicPlayerGui.RenderGui();
        Debug.RenderDebugConsole(ref m_debugConsole);
    }

    public void Shutdown()
    {
    }
}