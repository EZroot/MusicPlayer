using SDL2Engine.Core;
using MusicPlayer.Core;

public static class Program
{
    public static void Main()
    {
        var app = new GameApp(RendererType.SDLRenderer);
        app.Run(new MusicApp());
    }
}