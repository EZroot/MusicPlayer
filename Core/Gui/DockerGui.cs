using MusicPlayer.Core.Gui.Interfaces;
using SDL2Engine.Core.GuiRenderer;
using SDL2Engine.Core.GuiRenderer.Helpers;

namespace MusicPlayer.Core.Gui;

public class DockerGui : IGui
{
    private readonly IGuiRenderService m_renderService;
    private ImGuiDockData m_guiDockerData;

    public DockerGui(IGuiRenderService renderService)
    {
        m_renderService = renderService;
        m_guiDockerData = new ImGuiDockData(
            new DockPanelData("Main Dock", true),
            new DockPanelData("Left Dock", true),
            new DockPanelData("Top Dock", false),
            new DockPanelData("Right Dock", true),
            new DockPanelData("Bottom Dock", false),
            hasFileMenu: true);
    }
    public void Initialize()
    {
    }

    public void RenderGui()
    {
        if (m_guiDockerData.IsDockInitialized == false)
        {
            m_guiDockerData = m_renderService.InitializeDockSpace(m_guiDockerData);
        }
        m_renderService.RenderFullScreenDockSpace(m_guiDockerData);
    }
}