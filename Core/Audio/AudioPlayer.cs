using MusicPlayer.Core.Helper;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;

namespace MusicPlayer.Core.Audio;

public class AudioPlayer
{
    private IAudioService m_audioService;
    private nint m_soundPtr;  
    private nint m_channel;

    public AudioPlayer(IAudioService audioService)
    {
        m_audioService = audioService;
        m_soundPtr = IntPtr.Zero;  
    }

    public void LoadAudio(string filePath, AudioType audioType)
    {
        m_soundPtr = SDL_mixer.Mix_LoadMUS(filePath);
    }
    
    public void PlayAudio(int volume)
    {
        if (m_soundPtr != IntPtr.Zero)
        {
            m_audioService.PlayMusic(m_soundPtr, volume);
        }
        else
        {
            Debug.LogError("No sound available in memory ptr!");
        }
    }

    public void StopAudio()
    {
        try
        {
            SDL_mixer.Mix_HaltMusic();
            SDL_mixer.Mix_FreeMusic(m_soundPtr);

        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }
}