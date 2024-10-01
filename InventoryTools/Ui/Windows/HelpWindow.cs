using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui
{
    public class HelpWindow : GenericWindow
    {
        private readonly InventoryToolsConfiguration _configuration;

        public HelpWindow(ILogger<HelpWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "Help Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _configuration = configuration;
        }
        public override void Initialize()
        {
            WindowName = "Help";
            Key = "help";
        }
        
        public override bool SaveState => false;
        public override Vector2? DefaultSize { get; } = new Vector2(700, 700);
        public override  Vector2? MaxSize { get; } = new Vector2(2000, 2000);
        public override  Vector2? MinSize { get; } = new Vector2(200, 200);
        public override string GenericKey { get; } = "help";
        public override string GenericName { get; } = "Help;";
        public override bool DestroyOnClose => true;

        
        public override void Draw()
        {
            using (var sideBarChild =
                   ImRaii.Child("SideBar", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideBarChild.Success)
                {
                    if (ImGui.Selectable("1. 常规", _configuration.SelectedHelpPage == 0))
                    {
                        _configuration.SelectedHelpPage = 0;
                    }

                    if (ImGui.Selectable("2. 筛选基础", _configuration.SelectedHelpPage == 1))
                    {
                        _configuration.SelectedHelpPage = 1;
                    }

                    if (ImGui.Selectable("3. 筛选", _configuration.SelectedHelpPage == 2))
                    {
                        _configuration.SelectedHelpPage = 2;
                    }

                    if (ImGui.Selectable("4. 关于", _configuration.SelectedHelpPage == 3))
                    {
                        _configuration.SelectedHelpPage = 3;
                    }
                }
            }

            ImGui.SameLine();

            using (var mainChild = ImRaii.Child("###ivHelpView", new Vector2(-1, -1), true))
            {
                if (mainChild.Success)
                {
                    if (_configuration.SelectedHelpPage == 0)
                    {
                        ImGui.TextWrapped(
                            "Allagan Tools 是一个多功能插件，提供三大主要功能：追踪/显示你的库存数据，帮助你规划制作，并提供物品相关信息。其他功能可以在‘功能’部分中找到。");
                        ImGui.TextWrapped(
                            "如果你使用过 Teamcraft 或 Garland Tools，本插件从它们汲取了一些灵感。");
                        ImGui.NewLine();
                        ImGui.TextUnformatted("库存追踪：");
                        ImGui.Separator();
                        ImGui.TextWrapped("插件会尽力追踪你的库存。有些库存只有在首次访问时才会被缓存。如果你没有看到雇员、部队箱、幻化箱等，请确保先查看它们，否则插件将无法缓存它们。");
                        ImGui.TextWrapped("一旦插件记录了物品，你可以创建列表来缩小搜索范围，帮助你整理物品及其他功能。");
                        ImGui.NewLine();
                        
                        ImGui.TextUnformatted("制作规划：");
                        ImGui.Separator();
                        ImGui.TextWrapped("插件有一个专门的制作窗口，让你创建需要制作的物品列表。它会为每个物品生成一个分解计划，告诉你缺少哪些物品，哪里可以找到所需的物品。");
                        ImGui.TextWrapped("如果你使用过 Teamcraft，应该会很熟悉。");
                        ImGui.NewLine();
                        
                        ImGui.TextUnformatted("物品信息：");
                        ImGui.Separator();
                        ImGui.TextWrapped("插件有一个相当全面的数据库，提供每个物品的详细信息。如果你使用过 Garland Tools，信息非常类似。点击插件内的物品图标即可打开物品信息窗口。");
                        ImGui.NewLine();
                        
                        ImGui.TextUnformatted("高亮：");
                        ImGui.Separator();
                        ImGui.TextWrapped("在使用物品列表或制作列表时，你可以开启高亮功能。这会在游戏中高亮显示物品位置。当插件窗口处于活动状态时，可以点击‘高亮’复选框启用该列表的高亮功能。如果你想通过宏触发此功能，请查看帮助的命令部分，了解如何切换‘背景’高亮。");
                        ImGui.NewLine();
                        
                        ImGui.TextUnformatted("这只是一个非常基础的指南，更多信息请参考 wiki。");
                        if (ImGui.Button("打开 Wiki"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }
                    }
                    else if (_configuration.SelectedHelpPage == 1)
                    {
                        ImGui.PushTextWrapPos();
                        ImGui.Text("列表是插件提供查看或整理物品的核心方式。");
                        ImGui.Text("当前可以创建三种类型的列表。");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("搜索列表");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("此类列表允许你在所有库存中搜索特定物品。如果你只需找到物品而不需要整理，这是你想要的列表类型。");
                        ImGui.TextUnformatted("示例用法：");
                        ImGui.BulletText("寻找制作材料。");
                        ImGui.BulletText("找到放在某处的家具物品。");
                        ImGui.BulletText("查看刚捡到的物品价值。");
                        ImGui.BulletText("查看特定物品是否已在幻化箱或衣柜中。");
                        ImGui.BulletText("无需跑到雇员铃即可查看随从装备。");
                        ImGui.BulletText("检查是否有物品可以放入衣柜。");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("整理筛选器");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("此类列表基于‘搜索列表’，并允许你选择将物品整理到指定位置。它会为你提供最优化的物品存储计划。");
                        ImGui.TextUnformatted("示例用法：");
                        ImGui.BulletText("在制作后存放材料，避免重复。");
                        ImGui.BulletText("将特定物品存放在陆行鸟鞍包中，以备后用。");
                        ImGui.BulletText("找到专属于部队箱的物品并放入其中。");
                        ImGui.PopTextWrapPos();
                        ImGui.NewLine();

                        ImGui.Text("游戏物品筛选器");
                        ImGui.Separator();
                        ImGui.PushTextWrapPos();
                        ImGui.TextUnformatted("此筛选器允许你在游戏的所有物品目录中搜索物品。");
                        ImGui.TextUnformatted("示例用法：");
                        ImGui.BulletText("搜索幻化。");
                        ImGui.BulletText("查看尚未获得的坐骑/宠物。");
                        ImGui.BulletText("跟踪游戏内所有物品的价格。");
                        ImGui.PopTextWrapPos();
                    }
                    else if (_configuration.SelectedHelpPage == 2)
                    {
                        ImGui.TextUnformatted("高级搜索/筛选语法：");
                        ImGui.Separator();
                        ImGui.TextWrapped(
                            "在创建筛选器或搜索列表结果时，可以使用一系列运算符来使搜索更加具体。可用的运算符取决于搜索内容，目前支持 !、<、>、>=、<=、= 等运算符。");
                        ImGui.TextWrapped(
                            "! - 显示不包含输入内容的任何结果 - 可用于文本和数字。");
                        ImGui.TextWrapped(
                            "< - 显示值小于输入内容的任何结果 - 可用于数字。");
                        ImGui.TextWrapped(
                            "> - 显示值大于输入内容的任何结果 - 可用于数字。");
                        ImGui.TextWrapped(
                            ">= - 显示值大于或等于输入内容的任何结果 - 可用于数字。");
                        ImGui.TextWrapped(
                            "<= - 显示值小于或等于输入内容的任何结果 - 可用于数字。");
                        ImGui.TextWrapped(
                            "= - 显示值等于输入内容的任何结果 - 可用于文本和数字。");
                        ImGui.TextWrapped(
                            "&& 和 || 分别表示 AND 和 OR - 可以用来连接多个运算符。");
                    }
                    else if (_configuration.SelectedHelpPage == 3)
                    {
                        ImGui.TextUnformatted("关于：");
                        ImGui.TextUnformatted(
                            "这个插件是我在空闲时间写的，它是一项充满热情的工作，我希望可以长期发布更新。");
                        ImGui.TextUnformatted(
                            "如果遇到任何问题，请通过插件安装程序的反馈按钮提交反馈。");
                        ImGui.TextUnformatted("插件 Wiki: ");
                        ImGui.SameLine();
                        if (ImGui.Button("打开##WikiBtn"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/wiki/1.-Overview".OpenBrowser();
                        }

                        ImGui.TextUnformatted("发现漏洞？");
                        ImGui.SameLine();
                        if (ImGui.Button("打开##BugBtn"))
                        {
                            "https://github.com/Critical-Impact/InventoryTools/issues".OpenBrowser();
                        }
                    }
                }
            }
        }
        
        public override FilterConfiguration? SelectedConfiguration => null;

        public override void Invalidate()
        {
            
        }
    }
}