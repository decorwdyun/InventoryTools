using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Internal;
using ImGuiNET;
using InventoryTools.Logic;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    public class IntroWindow : GenericWindow
    {
        public IntroWindow(ILogger<IntroWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "Intro Window") : base(logger, mediator, imGuiService, configuration, name)
        {
        }
        public override void Initialize()
        {
            WindowName = "Allagan Tools";
            Flags =
                ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar;
            Key = "intro";
        }
        

        public override void Invalidate()
        {
        }

        public override FilterConfiguration? SelectedConfiguration => null;
        public override string GenericKey { get; } = "intro";
        public override string GenericName { get; } = "Intro";
        public override bool DestroyOnClose => true;

        public override void Draw()
        {
            using (var leftChild = ImRaii.Child("Left", new Vector2(200, 0)))
            {
                if (leftChild.Success)
                {
                    ImGui.SetCursorPosY(40);
                    ImGui.Image(ImGuiService.GetImageTexture("icon-hor").ImGuiHandle, new Vector2(200, 200) * ImGui.GetIO().FontGlobalScale);
                }
            }
            ImGui.SameLine();
            using (var rightChild = ImRaii.Child("Right", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollbar))
            {
                if (rightChild.Success)
                {
                    using (var textChild = ImRaii.Child("Text", new Vector2(0, -32)))
                    {
                        if (textChild.Success)
                        {
                            ImGui.TextWrapped("欢迎使用 Allagan Tools.");
                            ImGui.TextWrapped(
                                "Allagan Tools 是《最终幻想14》的一款插件，提供以下功能：");
                            using (ImRaii.PushIndent())
                            {
                                ImGui.Bullet();
                                ImGui.Text("追踪你的库存");
                                ImGui.Bullet();
                                ImGui.Text("规划你的制作");
                                ImGui.Bullet();
                                ImGui.Text("提供物品、怪物、副本等详细信息");
                            }
                            
                            ImGui.TextWrapped(
                                "你可以使用命令快捷键或从主窗口打开各种新窗口。");
                            ImGui.TextWrapped(
                                "如果不确定，可以右键点击物品或表格行以查看更多选项！");
                            ImGui.TextWrapped(
                                "想了解不同功能，建议前往设置部分，并查看?图标提供的信息。");
                        }
                    }

                    using (var buttonsChild = ImRaii.Child("Buttons", new Vector2(0, 32)))
                    {
                        if (buttonsChild.Success)
                        {
                            if (ImGui.Button("Close"))
                            {
                                Close();
                            }

                            ImGui.SameLine(0, 4);
                            if (ImGui.Button("Close & Open Main Window"))
                            {
                                Close();
                                MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
                            }
                        }
                    }
                }
            }
        }

        public override Vector2? DefaultSize { get; } = new Vector2(800, 300);
        public override Vector2? MaxSize { get; } = new Vector2(800, 300);
        public override Vector2? MinSize { get; } = new Vector2(800, 300);
        public override bool SaveState => false;
    }
}