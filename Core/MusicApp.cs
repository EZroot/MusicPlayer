using Microsoft.Extensions.DependencyInjection;
using MusicPlayer.Core.Helper;
using MusicPlayer.Core.Visuals;
using SDL2;
using SDL2Engine.Core;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Windowing.Interfaces;

namespace MusicPlayer.Core;

public class MusicApp : IGame
{
    private IWindowService m_windowService;
    private IRenderService m_renderService;
    
    private IAudioService m_audioService;
    
    private AudioSynthesizer m_audioSynth;
    
    public void Initialize(IServiceProvider serviceProvider)
    {
        m_windowService = serviceProvider.GetService<IWindowService>() ?? throw new InvalidOperationException("IWindowService is required but not registered.");
        m_renderService = serviceProvider.GetService<IRenderService>() ?? throw new InvalidOperationException("IRenderService is required but not registered.");
        m_audioService = serviceProvider.GetService<IAudioService>() ?? throw new InvalidOperationException("IAudioService is required but not registered.");

        SDL.SDL_GetWindowSize(m_windowService.WindowPtr, out var width, out var height);
        
        m_audioSynth = new AudioSynthesizer(width, height, m_audioService);
        m_audioSynth.Initialize();
        
        var soundId = m_audioService.LoadSound(AppHelper.SOUND_FOLDER + "/skidrow.wav");
        m_audioService.PlaySound(soundId, AppHelper.GLOBAL_VOLUME);
        
        
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
    }

    public void Shutdown()
    {
    }
}