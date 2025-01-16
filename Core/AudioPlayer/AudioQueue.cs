namespace MusicPlayer.Core.AudioPlayer;

public class AudioQueue
{
    public Queue<string> m_audioQueue = new Queue<string>();
    
    public void Enqueue(string audioPath)
    {
        m_audioQueue.Enqueue(audioPath);
    }

    public string Dequeue()
    {
        return m_audioQueue.Dequeue();
    }
}