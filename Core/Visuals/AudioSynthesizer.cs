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

    private Dictionary<string, float> previousAmplitudes = new Dictionary<string, float>();

    public AudioSynthesizer(int startWindowWidth, int startWindowHeight, IAudioService audioService)
    {
        m_windowWidth = startWindowWidth;
        m_windowHeight = startWindowHeight;
        m_audioService = audioService;

        SubscribeToEvents();
    }

    public void Initialize(int rectSpacing = 1, int rectWidth = 3, int rectMaxHeight = 40, float bandIntensity = 1.75f,
        float smoothingFactor = 0.3f)
    {
        m_bandIntensityMod = bandIntensity;
        m_rectSpacing = rectSpacing;
        m_maxRectHeight = rectMaxHeight;
        m_rectWidth = rectWidth;

        m_maxAmplitude = 0f;
        m_previousHeights = new float[m_audioService.FrequencyBands.Count];
        m_smoothingFactor = smoothingFactor;
    }

    public void RenderLineSynth(nint renderer, float minHue = 0.7f, float maxHue = 0.85f, float lerpFactor = 0.1f)
    {
        if (!SynthSettings.ShowLineSynth)
            return;

        var frequencyBands = m_audioService.FrequencyBands;
        int bandCount = frequencyBands.Count;
        if (bandCount == 0) return;

        var bandRectSize = (m_rectWidth + m_rectSpacing) * frequencyBands.Count;
        var initialY = m_windowHeight / 2;
        var initialX = m_windowWidth - m_dockWindowWidth / 2 - bandRectSize / 2;

        List<(int, int)> points = new List<(int, int)>();

        for (int i = 0; i < bandCount; i++)
        {
            string bandName = frequencyBands.Keys.ElementAt(i);
            float currentAmplitude = m_audioService.GetAmplitudeByName(bandName);

            if (!previousAmplitudes.TryGetValue(bandName, out float previousAmplitude))
            {
                previousAmplitudes[bandName] = currentAmplitude;
                previousAmplitude = currentAmplitude;
            }

            float interpolatedAmplitude = previousAmplitude + (currentAmplitude - previousAmplitude) * lerpFactor;
            int currentHeight = (int)((interpolatedAmplitude * m_bandIntensityMod / m_maxAmplitude) * m_maxRectHeight);
            currentHeight = Math.Clamp(currentHeight, 0, m_maxRectHeight);

            int currentX = initialX + i * (m_rectWidth + m_rectSpacing);
            int currentY = initialY - currentHeight / 2;

            points.Add((currentX + m_rectWidth / 2 - 2, currentY));

            SDL.SDL_Rect dot = new SDL.SDL_Rect
            {
                x = currentX + m_rectWidth / 2 - 2,
                y = currentY,
                w = 1,
                h = 1
            };
            // SDL.SDL_RenderFillRect(renderer, ref dot);

            previousAmplitudes[bandName] = interpolatedAmplitude;
        }

        if (points.Count >= 4)
        {
            var firstSplinePoints = GenerateCatmullRomSplinePoints(points[0], points[0], points[1], points[2], 20);
            for (int j = 0; j < firstSplinePoints.Count - 1; j++)
            {
                int height1 = Math.Abs(firstSplinePoints[j].Item2 - initialY);
                int height2 = Math.Abs(firstSplinePoints[j + 1].Item2 - initialY);
                int avgHeight = (height1 + height2) / 2;

                float ratio = avgHeight / (float)m_maxRectHeight;
                float hue = (minHue + (maxHue - minHue) * ratio) * 360;
                var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

                SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                SDL.SDL_RenderDrawLine(renderer, firstSplinePoints[j].Item1, firstSplinePoints[j].Item2,
                    firstSplinePoints[j + 1].Item1, firstSplinePoints[j + 1].Item2);
            }
        }

        for (int i = 1; i < points.Count - 2; i++)
        {
            var splinePoints =
                GenerateCatmullRomSplinePoints(points[i - 1], points[i], points[i + 1], points[i + 2], 20);
            for (int j = 0; j < splinePoints.Count - 1; j++)
            {
                int height1 = Math.Abs(splinePoints[j].Item2 - initialY);
                int height2 = Math.Abs(splinePoints[j + 1].Item2 - initialY);
                int avgHeight = (height1 + height2) / 2;

                float ratio = avgHeight / (float)m_maxRectHeight;
                float hue = (minHue + (maxHue - minHue) * ratio) * 360;
                var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

                SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                SDL.SDL_RenderDrawLine(renderer, splinePoints[j].Item1, splinePoints[j].Item2,
                    splinePoints[j + 1].Item1,
                    splinePoints[j + 1].Item2);
            }
        }

        if (points.Count >= 4)
        {
            var lastSplinePoints = GenerateCatmullRomSplinePoints(points[points.Count - 3], points[points.Count - 2],
                points[points.Count - 1], points[points.Count - 1], 20);
            for (int j = 0; j < lastSplinePoints.Count - 1; j++)
            {
                int height1 = Math.Abs(lastSplinePoints[j].Item2 - initialY);
                int height2 = Math.Abs(lastSplinePoints[j + 1].Item2 - initialY);
                int avgHeight = (height1 + height2) / 2;

                float ratio = avgHeight / (float)m_maxRectHeight;
                float hue = (minHue + (maxHue - minHue) * ratio) * 360;
                var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

                SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                SDL.SDL_RenderDrawLine(renderer, lastSplinePoints[j].Item1, lastSplinePoints[j].Item2,
                    lastSplinePoints[j + 1].Item1, lastSplinePoints[j + 1].Item2);
            }
        }
    }

    public void RenderLineSynthOpposite(nint renderer, float minHue = 0.7f, float maxHue = 0.85f,
        float lerpFactor = 0.1f)
    {
        if (!SynthSettings.ShowLineSynth)
            return;

        var frequencyBands = m_audioService.FrequencyBands;
        int bandCount = frequencyBands.Count;
        if (bandCount == 0) return;

        var bandRectSize = (m_rectWidth + m_rectSpacing) * frequencyBands.Count;
        var initialY = m_windowHeight / 2;
        var initialX = m_windowWidth - m_dockWindowWidth / 2 - bandRectSize / 2;

        List<(int, int)> points = new List<(int, int)>();

        for (int i = 0; i < bandCount; i++)
        {
            string bandName = frequencyBands.Keys.ElementAt(i);
            float currentAmplitude = m_audioService.GetAmplitudeByName(bandName);

            if (!previousAmplitudes.TryGetValue(bandName, out float previousAmplitude))
            {
                previousAmplitudes[bandName] = currentAmplitude;
                previousAmplitude = currentAmplitude;
            }

            float interpolatedAmplitude = previousAmplitude + (currentAmplitude - previousAmplitude) * lerpFactor;
            int currentHeight = (int)((interpolatedAmplitude * m_bandIntensityMod / m_maxAmplitude) * m_maxRectHeight);
            currentHeight = Math.Clamp(currentHeight, 0, m_maxRectHeight);

            int currentX = initialX + i * (m_rectWidth + m_rectSpacing);
            int currentY = initialY + currentHeight / 2; // Reverse direction for downward effect

            points.Add((currentX + m_rectWidth / 2 - 2, currentY));

            SDL.SDL_Rect dot = new SDL.SDL_Rect
            {
                x = currentX + m_rectWidth / 2 - 2,
                y = currentY,
                w = 1,
                h = 1
            };
            // SDL.SDL_RenderFillRect(renderer, ref dot);

            previousAmplitudes[bandName] = interpolatedAmplitude;
        }

        if (points.Count >= 4)
        {
            var firstSplinePoints = GenerateCatmullRomSplinePoints(points[0], points[0], points[1], points[2], 20);
            for (int j = 0; j < firstSplinePoints.Count - 1; j++)
            {
                int height1 = Math.Abs(firstSplinePoints[j].Item2 - initialY);
                int height2 = Math.Abs(firstSplinePoints[j + 1].Item2 - initialY);
                int avgHeight = (height1 + height2) / 2;

                float ratio = avgHeight / (float)m_maxRectHeight;
                float hue = (minHue + (maxHue - minHue) * ratio) * 360;
                var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

                SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                SDL.SDL_RenderDrawLine(renderer, firstSplinePoints[j].Item1, firstSplinePoints[j].Item2,
                    firstSplinePoints[j + 1].Item1, firstSplinePoints[j + 1].Item2);
            }
        }

        for (int i = 1; i < points.Count - 2; i++)
        {
            var splinePoints =
                GenerateCatmullRomSplinePoints(points[i - 1], points[i], points[i + 1], points[i + 2], 20);
            for (int j = 0; j < splinePoints.Count - 1; j++)
            {
                int height1 = Math.Abs(splinePoints[j].Item2 - initialY);
                int height2 = Math.Abs(splinePoints[j + 1].Item2 - initialY);
                int avgHeight = (height1 + height2) / 2;

                float ratio = avgHeight / (float)m_maxRectHeight;
                float hue = (minHue + (maxHue - minHue) * ratio) * 360;
                var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

                SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                SDL.SDL_RenderDrawLine(renderer, splinePoints[j].Item1, splinePoints[j].Item2,
                    splinePoints[j + 1].Item1,
                    splinePoints[j + 1].Item2);
            }
        }

        if (points.Count >= 4)
        {
            var lastSplinePoints = GenerateCatmullRomSplinePoints(points[points.Count - 3], points[points.Count - 2],
                points[points.Count - 1], points[points.Count - 1], 20);
            for (int j = 0; j < lastSplinePoints.Count - 1; j++)
            {
                int height1 = Math.Abs(lastSplinePoints[j].Item2 - initialY);
                int height2 = Math.Abs(lastSplinePoints[j + 1].Item2 - initialY);
                int avgHeight = (height1 + height2) / 2;

                float ratio = avgHeight / (float)m_maxRectHeight;
                float hue = (minHue + (maxHue - minHue) * ratio) * 360;
                var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);

                SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                SDL.SDL_RenderDrawLine(renderer, lastSplinePoints[j].Item1, lastSplinePoints[j].Item2,
                    lastSplinePoints[j + 1].Item1, lastSplinePoints[j + 1].Item2);
            }
        }
    }

    public void Render(nint renderer, float minHue = 0.7f, float maxHue = 0.85f, int extraBandsPerGap = 2)
    {
        var frequencyBands = m_audioService.FrequencyBands;
        var freqCount = frequencyBands.Count == 0 ? 1 : frequencyBands.Count;
        int totalVisibleBands = frequencyBands.Count + (frequencyBands.Count - 1) * extraBandsPerGap;
        int totalSpacing = (totalVisibleBands - 1) * m_rectSpacing;
        m_rectWidth = SynthSettings.RectWidthModifier; //(int)((m_dockWindowWidth - totalSpacing) / totalVisibleBands) + SynthSettings.RectWidthModifier;
        m_maxRectHeight = (int)(m_dockWindowHeight * 0.9f) + SynthSettings.RectMaxHeightModifier;
        var spacing = m_dockWindowWidth / totalVisibleBands;
        m_rectSpacing = spacing + SynthSettings.RectSpacingModifier; // * SynthSettings.RectSynthSmoothness;

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

        for (int i = 0; i < frequencyBands.Count; i++)
        {
            var currentBandKey = frequencyBands.ElementAt(i).Key;
            var currentAmplitude = m_audioService.GetAmplitudeByName(currentBandKey) *
                                   SynthSettings.RectBandIntensityModifier;
            var currentHeight = (int)((currentAmplitude / m_maxAmplitude) * m_maxRectHeight);
            currentHeight = Math.Clamp(currentHeight, 0, m_maxRectHeight);

            m_previousHeights[int.Parse(currentBandKey)] = currentHeight;

            if (i < frequencyBands.Count - 1)
            {
                var nextBandKey = frequencyBands.ElementAt(i + 1).Key;
                var nextAmplitude = m_audioService.GetAmplitudeByName(nextBandKey) *
                                    SynthSettings.RectBandIntensityModifier;
                var nextHeight = (int)((nextAmplitude / m_maxAmplitude) * m_maxRectHeight);
                nextHeight = Math.Clamp(nextHeight, 0, m_maxRectHeight);

                for (int j = 0; j <= extraBandsPerGap; j++)
                {
                    float t = j / (float)(extraBandsPerGap + 1);
                    float weightedT = (float)(1 - Math.Cos(t * Math.PI)) / 2;

                    int interpolatedHeight = (int)MathHelper.Lerp(currentHeight, nextHeight, weightedT);
                    int interpolatedX = (int)(initialRectStartX + ((i + t) * (m_rectWidth + m_rectSpacing)));

                    var ratio = interpolatedHeight / (float)m_maxRectHeight;
                    var hue = (minHue + (maxHue - minHue) * ratio) * 360;
                    var (red, green, blue) = ColorHelper.HsvToRgb(hue, 1f, 1.0f);
                    if (SynthSettings.ShowRectSynth)
                    {
                        SDL.SDL_SetRenderDrawColor(renderer, red, green, blue, 255);
                        SDL.SDL_Rect interpolatedRect = new SDL.SDL_Rect
                        {
                            x = interpolatedX,
                            y = initialRectStartY - (interpolatedHeight / 2),
                            w = m_rectWidth,
                            h = interpolatedHeight
                        };
                        SDL.SDL_RenderFillRect(renderer, ref interpolatedRect);
                    }
                }
            }

            var currentX = initialRectStartX + (i * (m_rectWidth + m_rectSpacing));
            var rectRatio = currentHeight / (float)m_maxRectHeight;
            var rectHue = (minHue + (maxHue - minHue) * rectRatio) * 360;
            var (r, g, b) = ColorHelper.HsvToRgb(rectHue, 1f, 1.0f);
            if (SynthSettings.ShowRectSynth)
            {
                SDL.SDL_SetRenderDrawColor(renderer, r, g, b, 255);
                SDL.SDL_Rect bandRect = new SDL.SDL_Rect
                {
                    x = currentX,
                    y = initialRectStartY - (currentHeight / 2),
                    w = m_rectWidth,
                    h = currentHeight
                };
                SDL.SDL_RenderFillRect(renderer, ref bandRect);
            }
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

    private List<(int, int)> GenerateCatmullRomSplinePoints((int, int) p0, (int, int) p1, (int, int) p2, (int, int) p3,
        int numPoints)
    {
        List<(int, int)> points = new List<(int, int)>();
        float t0 = 0.0f;
        for (int i = 1; i <= numPoints; i++)
        {
            float t = i / (float)numPoints;
            float t2 = t * t;
            float t3 = t2 * t;

            float f0 = -0.5f * t3 + t2 - 0.5f * t;
            float f1 = 1.5f * t3 - 2.5f * t2 + 1.0f;
            float f2 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
            float f3 = 0.5f * t3 - 0.5f * t2;

            int x = (int)(f0 * p0.Item1 + f1 * p1.Item1 + f2 * p2.Item1 + f3 * p3.Item1);
            int y = (int)(f0 * p0.Item2 + f1 * p1.Item2 + f2 * p2.Item2 + f3 * p3.Item2);

            points.Add((x, y));
        }

        return points;
    }
}