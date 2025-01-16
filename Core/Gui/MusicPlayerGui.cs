using System.Numerics;
using ImGuiNET;
using MusicPlayer.Core.Gui.Bindings;
using MusicPlayer.Core.Gui.Interfaces;
using MusicPlayer.Core.Helper;

namespace MusicPlayer.Core.Gui;

public class MusicPlayerGui : IGui
{
    private MusicPlayerBindings m_bindings;
    private string m_currentDirectory = AppHelper.SOUND_FOLDER;
    public MusicPlayerGui()
    {
        m_bindings = new MusicPlayerBindings();
    }
    
    public void Initialize()
    {
        
    }

    public void RenderGui()
    {
        if (ImGui.Begin("Bottom Dock"))
        {
            BuildConsole();
            ImGui.End();
        }
    }

    private void BuildConsole()
    {
        Vector2 availableSize = ImGui.GetContentRegionAvail();
        float childWidth = (availableSize.X) - ImGui.GetStyle().ItemSpacing.X;
        float childHeight = availableSize.Y;
        
        ImGui.BeginChild("##MusicList", new Vector2(childWidth, childHeight), ImGuiChildFlags.Borders);
        {
            ImGui.Text($"Directory");
            ImGui.Separator();
            var files = Directory.GetFiles(AppHelper.SOUND_FOLDER);
            foreach (var file in files)
            {
                if (ImGui.Button("Queue"))
                {
                    
                }
                ImGui.SameLine();
                ImGui.Text(file);
                ImGui.Separator();
            }
        }
        ImGui.EndChild();
        
    }
}