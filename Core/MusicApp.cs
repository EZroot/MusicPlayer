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
    
    private float minHue = 0.7f, maxHue = 0.84f;
    private float maxHueSeperation = 0.24f;
    private float hueTransitionSpeed = 0.001f;

    private float minHueA, maxHueA;
    private float hueSeperation = 0.01f;
    
    
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
        
        EventHub.Subscribe<OnMusicFinishedPlaying>(OnMusicFinishedPlaying);
    }

    private void OnMusicFinishedPlaying(object? sender, OnMusicFinishedPlaying e)
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
            
            m_audioPlayer.StopAudio();
            m_audioPlayer.LoadAudio(filePath, AudioType.Music);
            m_audioPlayer.PlayAudio(AppHelper.GLOBAL_VOLUME);
        }
    }

    public void Update(float deltaTime)
    {
        // minHue += Time.DeltaTime * hueTransitionSpeed;
        // maxHue += Time.DeltaTime * hueTransitionSpeed;
        if (minHue >= 1.0f) minHue -= 1.0f;
        if (maxHue >= 1.0f) maxHue -= 1.0f;
        if (maxHue > minHue + maxHueSeperation) maxHue = minHue + maxHueSeperation;
        if (minHue > maxHue - maxHueSeperation) minHue = maxHue - maxHueSeperation;

        minHueA = minHue + hueSeperation;
        maxHueA = maxHue + hueSeperation;
        if (minHueA >= 1.0f) minHueA -= 1.0f;
        if (maxHueA >= 1.0f) maxHueA -= 1.0f;
        if (maxHueA > minHueA + maxHueSeperation) maxHueA = minHueA + maxHueSeperation;
        if (minHueA > maxHueA - maxHueSeperation) minHueA = maxHueA - maxHueSeperation;
    }

    public void Render()
    {
        m_audioSynth.Render(m_renderService.RenderPtr, minHue, maxHue, SynthSettings.RectSynthSmoothness);
        m_audioSynth.RenderLineSynth(m_renderService.RenderPtr, minHueA, maxHueA, SynthSettings.LineSynthSmoothness);
        m_audioSynth.RenderLineSynthOpposite(m_renderService.RenderPtr, minHueA, maxHueA, SynthSettings.LineSynthSmoothness);
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