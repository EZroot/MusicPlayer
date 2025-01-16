namespace MusicPlayer.Core.Events;

public class OnMainDockResized : EventArgs
{
    public int WindowWidth;
    public int WindowHeight;

    public OnMainDockResized(int windowWidth, int windowHeight)
    {
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
    }
}