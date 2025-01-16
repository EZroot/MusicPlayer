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
        if (ImGui.Begin("Left Dock"))
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
            if (ImGui.InputText("Directory", ref m_currentDirectory, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                
            }
            
            ImGui.Separator();
            if (Directory.Exists(m_currentDirectory))
            {
                var files = Directory.GetFiles(m_currentDirectory);
                foreach (var file in files)
                {
                    var filePath = file.Replace(AppHelper.SOUND_FOLDER, "");
                    if (ImGui.Button("Queue"))
                    {
                        //queue audio
                    }

                    ImGui.SameLine();
                    ImGui.Text(filePath);
                    ImGui.Separator();
                }
            }
        }
        ImGui.EndChild();
        
    }
}