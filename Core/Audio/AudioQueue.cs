using SDL2Engine.Core.Utils;

namespace MusicPlayer.Core.Audio;

public class AudioQueue
{
    private Queue<string> m_audioQueue = new Queue<string>();
    public List<string> QueuedAudio => m_audioQueue.ToList();
    public bool IsSongPlaying;
    public string CurrentSong;
    
    public void Enqueue(string audioPath)
    {
        m_audioQueue.Enqueue(audioPath);
        Debug.Log($"Enqueuing {audioPath}");
    }

    public string Dequeue()
    {
        var path = m_audioQueue.Dequeue();
        Debug.Log($"Dequeuing {path}");
        return path;
    }
}