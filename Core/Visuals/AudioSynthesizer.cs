using MusicPlayer.Core.Events;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Events;

namespace MusicPlayer.Core.Visuals;

public class AudioSynthesizer
{
    private float m_bandIntensityMod = 1.75f;
    private int m_rectSpacing = 1;
    private int m_maxRectHeight = 40;
    private int m_rectWidth = 3;

    private float m_maxAmplitude;
    private float[] m_previousHeights;
    private float m_smoothingFactor; // Smoothing factor between 0.1 and 0.3
    
    private int m_dockWindowWidth, m_dockWindowHeight;
    private int m_windowWidth, m_windowHeight;
    private IAudioService m_audioService;
    
    public AudioSynthesizer(int startWindowWidth, int startWindowHeight, IAudioService audioService)
    {
        m_windowWidth = startWindowWidth;
        m_windowHeight = startWindowHeight;
        m_audioService = audioService;
        
        SubscribeToEvents();
    }

    public void Initialize(int rectSpacing = 1, int rectWidth = 3, int rectMaxHeight = 40, float bandIntensity = 1.75f, float smoothingFactor = 0.3f)
    {
        m_bandIntensityMod = bandIntensity;
        m_rectSpacing = rectSpacing;
        m_maxRectHeight = rectMaxHeight;
        m_rectWidth = rectWidth;

        m_maxAmplitude = 0f;
        m_previousHeights = new float[m_audioService.FrequencyBands.Count];
        m_smoothingFactor = smoothingFactor;
    }

    public void Render(nint renderer, float minHue = 0.7f, float maxHue = 0.85f)
    {
        var frequencyBands = m_audioService.FrequencyBands;
        var freqCount = frequencyBands.Count == 0 ? 1 : frequencyBands.Count;
        m_rectWidth = (int)((m_dockWindowWidth / freqCount) - freqCount * 0.1f);
        m_maxRectHeight = (int)(m_dockWindowHeight * 0.9f);
        
        var bandRectSize = (m_rectWidth + m_rectSpacing) * frequencyBands.Count;
        var initialRectStartY = m_windowHeight / 2;
        var initialRectStartX = m_windowWidth - m_dockWindowWidth / 2 - bandRectSize / 2;

        foreach (var bandPair in m_audioService.FrequencyBands)
        {
            var bandAmplitude = m_audioService.GetAmplitudeByName(bandPair.Key);
            if (bandAmplitude > m_maxAmplitude)
            {
                m_maxAmplitude = bandAmplitude;
            }
        }

        foreach (var bandPair in frequencyBands)
        {
            var amp = m_audioService.GetAmplitudeByName(bandPair.Key);
            var bandIndex = int.Parse(bandPair.Key);
            var bandAmplitude = amp * m_bandIntensityMod;
            var targetHeight = (int)((bandAmplitude / m_maxAmplitude) * m_maxRectHeight);
            targetHeight = Math.Clamp(targetHeight, 0, m_maxRectHeight);

            var smoothedHeight = (int)(m_previousHeights[bandIndex] * (1 - m_smoothingFactor) +
                                       targetHeight * m_smoothingFactor);
            m_previousHeights[bandIndex] = smoothedHeight;
            var currentRectX = initialRectStartX + (bandIndex * (m_rectWidth + m_rectSpacing));

            var ratio = smoothedHeight / (float)m_maxRectHeight;
           
            var hue = (minHue + (maxHue - minHue) * ratio) * 360;
            var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);
            SDL.SDL_Rect backgroundRect = new SDL.SDL_Rect
            {
                x = currentRectX - m_rectSpacing / 2,
                y = initialRectStartY - (smoothedHeight / 2) -
                    (m_rectSpacing / 2),
                w = m_rectWidth + m_rectSpacing,
                h = smoothedHeight + m_rectSpacing
            };
            SDL.SDL_RenderFillRect(renderer, ref backgroundRect);

            SDL.SDL_Rect rect = new SDL.SDL_Rect
            {
                x = currentRectX,
                y = initialRectStartY - (smoothedHeight / 2),
                w = m_rectWidth,
                h = smoothedHeight
            };
            SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }
    }

    private void SubscribeToEvents()
    {
        EventHub.Subscribe<OnWindowResized>(OnWindowResized);
        EventHub.Subscribe<OnMainDockResized>(OnMainDockResized);
    }

    private void OnMainDockResized(object? sender, OnMainDockResized e)
    {
        m_dockWindowWidth = e.WindowWidth;
        m_dockWindowHeight = e.WindowHeight;
    }

    private void OnWindowResized(object? sender, OnWindowResized e)
    {
        m_windowWidth = e.WindowSettings.Width;
        m_windowHeight = e.WindowSettings.Height;
    }
}