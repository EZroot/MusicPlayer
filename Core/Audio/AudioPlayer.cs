using MusicPlayer.Core.Helper;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;

namespace MusicPlayer.Core.Audio;

public class AudioPlayer
{
    private IAudioService m_audioService;
    private IntPtr m_soundPtr;  // Changed to IntPtr to match typical usage
    private int m_channel;

    public AudioPlayer(IAudioService audioService)
    {
        m_audioService = audioService;
        m_soundPtr = IntPtr.Zero;  // Ensure pointer is initialized to zero
    }

    public void LoadAudio(string filePath)
    {
        //FreeAudio();  // Ensure any previously loaded audio is freed
        m_soundPtr = m_audioService.LoadSound(filePath);
    }
    
    public void PlayAudio()
    {
        if (m_soundPtr != IntPtr.Zero)
        {
            m_channel = m_audioService.PlaySound((int)m_soundPtr, AppHelper.GLOBAL_VOLUME);
        }
    }

    public void StopAudio()
    {
        try
        {
            SDL_mixer.Mix_HaltChannel(m_channel); // Stop the specific channel playing the sound
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void FreeAudio()
    {
        StopAudio();
        if (m_soundPtr != IntPtr.Zero)
        {
            SDL_mixer.Mix_FreeChunk(m_soundPtr);  // Free the sound chunk
            m_soundPtr = IntPtr.Zero;  // Reset the pointer to zero
        }
    }
}