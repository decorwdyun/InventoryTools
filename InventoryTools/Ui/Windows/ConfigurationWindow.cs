using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.MenuItems;
using InventoryTools.Ui.Widgets;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    using Dalamud.Interface.Textures;

    public class ConfigurationWindow : GenericWindow
    {
        private readonly ConfigurationWizardService _configurationWizardService;
        private readonly IChatUtilities _chatUtilities;
        private readonly PluginLogic _pluginLogic;
        private readonly IListService _listService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Func<SettingCategory,SettingPage> _settingPageFactory;
        private readonly Func<Type, IConfigPage> _configPageFactory;
        private readonly Func<FilterConfiguration, FilterPage> _filterPageFactory;
        private readonly InventoryToolsConfiguration _configuration;

        public ConfigurationWindow(ILogger<ConfigurationWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ConfigurationWizardService configurationWizardService, IChatUtilities chatUtilities, PluginLogic pluginLogic, IListService listService,IServiceScopeFactory serviceScopeFactory, Func<SettingCategory,SettingPage> settingPageFactory, Func<Type,IConfigPage> configPageFactory, Func<FilterConfiguration,FilterPage> filterPageFactory, string name = "Configuration Window") : base(logger, mediator, imGuiService, configuration, name)
        {
            _configurationWizardService = configurationWizardService;
            _chatUtilities = chatUtilities;
            _pluginLogic = pluginLogic;
            _listService = listService;
            _serviceScopeFactory = serviceScopeFactory;
            _settingPageFactory = settingPageFactory;
            _configPageFactory = configPageFactory;
            _filterPageFactory = filterPageFactory;
            _configuration = configuration;
        }


        public override void Initialize()
        {
            WindowName = "Configuration";
            Key = "configuration";
            _configPages = new List<IConfigPage>();
            _configPages.Add(new SeparatorPageItem("Settings"));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Lists));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Windows));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.AutoSave));
            _configPages.Add(new SeparatorPageItem("Modules", true));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.MarketBoard));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.ToolTips));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.ContextMenu));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Hotkeys));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.MobSpawnTracker));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.TitleMenuButtons));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.History));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Misc));
            _configPages.Add(new SeparatorPageItem("Data", true));
            _configPages.Add(_configPageFactory.Invoke(typeof(FiltersPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(CraftFiltersPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(ImportExportPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(CharacterRetainerPage)));
            
            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectableAskName("搜索筛选器", "adf1", "新建搜索筛选器", AddSearchFilter, "这将创建一个新的筛选器，允许你在角色和雇员的库存中搜索特定物品。"),
                new PopupMenu.PopupMenuItemSelectableAskName("整理筛选器", "af2", "新建整理筛选器", AddSortFilter, "这将创建一个新的筛选器，允许你在角色和雇员的库存中搜索特定物品，并确定它们应放置的位置。"),
                new PopupMenu.PopupMenuItemSelectableAskName("游戏物品筛选器", "af3", "新建游戏物品筛选器", AddGameItemFilter, "这将创建一个筛选器，允许你搜索游戏中的所有物品。"),
                new PopupMenu.PopupMenuItemSelectableAskName("历史记录筛选器", "af4", "新建历史记录筛选器", AddHistoryFilter, "这将创建一个筛选器，允许你查看库存变化的历史数据。"),
            });

            _addSampleMenu = new PopupMenu("addSampleFilter", PopupMenu.PopupMenuButtons.LeftRight,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectableAskName("全部", "af4", "全部", AddAllFilter, "这将添加一个预配置的筛选器，显示所有库存中的物品。"),
                new PopupMenu.PopupMenuItemSelectableAskName("玩家", "af5", "玩家", AddPlayerFilter, "这将添加一个预配置的筛选器，显示所有角色库存中的物品。"),
                new PopupMenu.PopupMenuItemSelectableAskName("雇员", "af6", "雇员", AddRetainersFilter, "这将添加一个预配置的筛选器，显示所有雇员库存中的物品。"),
                new PopupMenu.PopupMenuItemSelectableAskName("部队", "af7", "部队", AddFreeCompanyFilter, "这将添加一个预配置的筛选器，显示所有部队库存中的物品。"),
                new PopupMenu.PopupMenuItemSelectableAskName("所有游戏物品", "af8", "所有游戏物品", AddAllGameItemsFilter, "这将添加一个预配置的筛选器，显示游戏中的所有物品。"),
                new PopupMenu.PopupMenuItemSeparator(),
                new PopupMenu.PopupMenuItemSelectableAskName("价格低于100金币的物品", "af9", "低于100金币", AddLessThan100GilFilter, "这将添加一个筛选器，显示可以在金币商店中以低于100金币购买的所有物品。它将检查角色和雇员库存。"),
                new PopupMenu.PopupMenuItemSelectableAskName("整理多余材料 +", "af10", "整理材料", AddPutAwayMaterialsFilter, "这将添加一个筛选器，快速整理多余的材料，并自动添加所有材料分类。它将优先考虑已有的物品堆栈。"),
                new PopupMenu.PopupMenuItemSelectableAskName("角色/雇员间重复的物品 +", "af11", "重复物品", AddDuplicatedItemsFilter, "这将添加一个筛选器，列出出现在多个库存中的所有重复堆栈。你可以用这个来确保只有一个雇员持有某类物品。")
            });

            _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectable("物品窗口", "filters", OpenFiltersWindow,"打开物品窗口。"),
                new PopupMenu.PopupMenuItemSelectable("制作窗口", "crafts", OpenCraftsWindow,"打开制作窗口。"),
                new PopupMenu.PopupMenuItemSeparator(),
                new PopupMenu.PopupMenuItemSelectable("怪物窗口", "mobs", OpenMobsWindow,"打开怪物窗口。"),
                new PopupMenu.PopupMenuItemSelectable("NPC 窗口", "npcs", OpenNpcsWindow,"打开 NPC 窗口。"),
                new PopupMenu.PopupMenuItemSelectable("副本窗口", "duties", OpenDutiesWindow,"打开副本窗口。"),
                new PopupMenu.PopupMenuItemSelectable("飞空艇窗口", "airships", OpenAirshipsWindow,"打开飞空艇窗口。"),
                new PopupMenu.PopupMenuItemSelectable("潜水艇窗口", "submarines", OpenSubmarinesWindow,"打开潜水艇窗口。"),
                new PopupMenu.PopupMenuItemSelectable("雇员探险窗口", "ventures", OpenRetainerVenturesWindow,"打开雇员探险窗口。"),
                new PopupMenu.PopupMenuItemSelectable("俄罗斯方块", "tetris", OpenTetrisWindow,"打开俄罗斯方块窗口。", () => _configuration.TetrisEnabled),
                new PopupMenu.PopupMenuItemSeparator(),
                new PopupMenu.PopupMenuItemSelectable("帮助", "help", OpenHelpWindow,"打开帮助窗口。"),
            });

            _wizardMenu = new PopupMenu("wizardMenu", PopupMenu.PopupMenuButtons.All,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectable("配置新设置", "configureNew", ConfigureNewSettings,"配置新设置。"),
                new PopupMenu.PopupMenuItemSelectable("配置所有设置", "configureAll", ConfigureAllSettings,"配置所有设置。"),
            });

            GenerateFilterPages();
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ConfigurationWindowEditFilter>(this,  message =>
            {
                Invalidate();
                SetActiveFilter(message.filter);
            });
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
        }

        private void ListInvalidated(ListInvalidatedMessage obj)
        {
            Invalidate();
        }

        private HoverButton _addIcon = new();
        private HoverButton _lightBulbIcon= new();
        private HoverButton _menuIcon = new ();
        private HoverButton _wizardStart = new();

        private PopupMenu _wizardMenu = null!;

        private void ConfigureAllSettings(string obj)
        {
            _configurationWizardService.ClearFeaturesSeen();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
        }

        private void ConfigureNewSettings(string obj)
        {
            if (_configurationWizardService.HasNewFeatures)
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
            }
            else
            {
                _chatUtilities.Print("There are no new settings available to configure.");
            }
        }

        private PopupMenu _addFilterMenu = null!;
        private PopupMenu _addSampleMenu = null!;
        private PopupMenu _settingsMenu = null!;

        private void OpenCraftsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
        }

        private void OpenFiltersWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }

        private void OpenHelpWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
        }

        private void OpenDutiesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(DutiesWindow)));
        }

        private void OpenAirshipsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(AirshipsWindow)));
        }

        private void OpenSubmarinesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarinesWindow)));
        }
        
        private void OpenRetainerVenturesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(RetainerTasksWindow)));
        }

        private void OpenMobsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(BNpcsWindow)));
        }

        private void OpenNpcsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ENpcsWindow)));
        }
        
        private void OpenTetrisWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(TetrisWindow)));
        }

        private void AddAllGameItemsFilter(string arg1, string arg2)
        {
            _pluginLogic.AddAllGameItemsFilter(arg1);
            SetNewFilterActive();
        }

        private void AddFreeCompanyFilter(string arg1, string arg2)
        {
            _pluginLogic.AddFreeCompanyFilter(arg1);
            SetNewFilterActive();
        }

        private void AddRetainersFilter(string arg1, string arg2)
        {
            _pluginLogic.AddRetainerFilter(arg1);
            SetNewFilterActive();
        }

        private void AddPlayerFilter(string arg1, string arg2)
        {
            _pluginLogic.AddPlayerFilter(arg1);
            SetNewFilterActive();
        }

        private void AddAllFilter(string arg1, string arg2)
        {
            _pluginLogic.AddAllFilter(arg1);
            SetNewFilterActive();
        }

        private Dictionary<FilterConfiguration, PopupMenu> _popupMenus = new();
        public PopupMenu GetFilterMenu(FilterConfiguration configuration)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new PopupMenu("fm" + configuration.Key, PopupMenu.PopupMenuButtons.Right,
                    new List<PopupMenu.IPopupMenuItem>()
                    {
                        new PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateFilter, "Duplicate the filter."),
                        new PopupMenu.PopupMenuItemSelectable("Move Up", "mu_" + configuration.Key, MoveFilterUp, "Move the filter up."),
                        new PopupMenu.PopupMenuItemSelectable("Move Down", "md_" + configuration.Key, MoveFilterDown, "Move the filter down."),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this filter?", RemoveFilter, "Remove the filter."),
                    }
                );
            }

            return _popupMenus[configuration];
        }

        private void RemoveFilter(string id, bool confirmed)
        {
            if (confirmed)
            {
                id = id.Replace("rf_", "");
                var existingFilter = _listService.GetListByKey(id);
                if (existingFilter != null)
                {
                    _listService.RemoveList(existingFilter);
                    ConfigSelectedConfigurationPage--;
                }
            }
        }

        private void MoveFilterDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListDown(existingFilter);
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListUp(existingFilter);
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.DuplicateList(existingFilter, filterName);
                SetNewFilterActive();
            }
        }

        private void AddDuplicatedItemsFilter(string newName, string id)
        {
            _pluginLogic.AddSampleFilterDuplicatedItems(newName);
            SetNewFilterActive();
        }

        private void AddPutAwayMaterialsFilter(string newName, string id)
        {
            _pluginLogic.AddSampleFilterMaterials(newName);
            SetNewFilterActive();
        }

        private void AddLessThan100GilFilter(string newName, string id)
        {
            _pluginLogic.AddSampleFilter100Gil(newName);
            SetNewFilterActive();
        }

        private void AddSearchFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.SearchFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }

        private void AddHistoryFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.HistoryFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }

        private void AddGameItemFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.GameItemFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }

        private void AddSortFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.SortingFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }


        private int ConfigSelectedConfigurationPage
        {
            get => _configuration.SelectedConfigurationPage;
            set => _configuration.SelectedConfigurationPage = value;
        }

        public void SetActiveFilter(FilterConfiguration configuration)
        {
            var filterIndex = _filterPages.ContainsKey(configuration.Key) ? _filterPages.Where(c => !c.Value.IsMenuItem).Select(c => c.Key).IndexOf(configuration.Key) - 2 : -1;
            if (filterIndex != -1)
            {
                ConfigSelectedConfigurationPage = _configPages.Count + filterIndex;
            }
        }

        public void GenerateFilterPages()
        {
            var filterConfigurations = _listService.Lists.Where(c => c.FilterType != FilterType.CraftFilter);
            var filterPages = new Dictionary<string, IConfigPage>(); 
            foreach (var filter in filterConfigurations)
            {
                if (!filterPages.ContainsKey(filter.Key))
                {
                    filterPages.Add(filter.Key, _filterPageFactory.Invoke(filter));
                }
            }

            _filterPages = filterPages;
        }
        
        public override bool SaveState => true;
        public override Vector2? DefaultSize { get; } = new(700, 700);
        public override Vector2? MaxSize { get; } = new(2000, 2000);
        public override Vector2? MinSize { get; } = new(200, 200);
        public override string GenericKey => "configuration";
        public override string GenericName => "Configuration";
        public override bool DestroyOnClose => true;
        private List<IConfigPage> _configPages = null!;
        public Dictionary<string, IConfigPage> _filterPages = new Dictionary<string,IConfigPage>();
        

        private void SetNewFilterActive()
        {
            ConfigSelectedConfigurationPage = _configPages.Count + _filterPages.Count - 2;
        }

        public override void Draw()
        {

            using (var sideBarChild =
                   ImRaii.Child("SideBar", new Vector2(180, 0) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideBarChild.Success)
                {
                    using (var menuChild = ImRaii.Child("Menu", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale,
                               false, ImGuiWindowFlags.NoSavedSettings))
                    {
                        if (menuChild.Success)
                        {

                            var count = 0;
                            for (var index = 0; index < _configPages.Count; index++)
                            {
                                var configPage = _configPages[index];
                                if (configPage.IsMenuItem)
                                {
                                    MediatorService.Publish(configPage.Draw());
                                }
                                else
                                {
                                    if (ImGui.Selectable(configPage.Name, ConfigSelectedConfigurationPage == count))
                                    {
                                        ConfigSelectedConfigurationPage = count;
                                    }

                                    count++;
                                }
                            }

                            ImGui.NewLine();
                            ImGui.TextUnformatted("Item Lists");
                            ImGui.Separator();

                            var filterIndex = count;
                            foreach (var item in _filterPages)
                            {
                                filterIndex++;
                                if (ImGui.Selectable(item.Value.Name + "##" + item.Key,
                                        ConfigSelectedConfigurationPage == filterIndex))
                                {
                                    ConfigSelectedConfigurationPage = filterIndex;
                                }

                                var filter = _listService.GetListByKey(item.Key);
                                if (filter != null)
                                {
                                    GetFilterMenu(filter).Draw();
                                }

                            }
                        }
                    }

                    using (var commandBarChild = ImRaii.Child("CommandBar",
                               new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
                    {
                        if (commandBarChild.Success)
                        {

                            float height = ImGui.GetWindowSize().Y;
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);

                            if(_addIcon.Draw(ImGuiService.GetIconTexture(66315).ImGuiHandle, "addFilter"))
                            {

                            }

                            _addFilterMenu.Draw();
                            ImGuiUtil.HoverTooltip("Add a new filter");

                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(26 * ImGui.GetIO().FontGlobalScale);

                            if (_lightBulbIcon.Draw(ImGuiService.GetIconTexture(66318).ImGuiHandle,"addSample"))
                            {

                            }

                            _addSampleMenu.Draw();
                            ImGuiUtil.HoverTooltip("Add a sample filter");

                            var width = ImGui.GetWindowSize().X;
                            width -= 24 * ImGui.GetIO().FontGlobalScale;
                            
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(width);

                            if (_menuIcon.Draw(ImGuiService.GetImageTexture("menu").ImGuiHandle, "openMenu"))
                            {

                            }

                            _settingsMenu.Draw();
                            
                            
                            width -= 26 * ImGui.GetIO().FontGlobalScale;
                            
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(width);

                            if (_wizardStart.Draw(ImGuiService.GetImageTexture("wizard").ImGuiHandle, "openMenu"))
                            {
                                _wizardMenu.Open();
                            }
                            _wizardMenu.Draw();

                            
                            ImGuiUtil.HoverTooltip("Start configuration wizard.");
                        }
                    }
                }
            }
            
            

            ImGui.SameLine();

            IConfigPage? currentConfigPage = null;

            {
                var count = 0;
                for (var index = 0; index < _configPages.Count; index++)
                {
                    if (_configPages[index].IsMenuItem)
                    {
                        count++;
                        continue;
                    }

                    if (ConfigSelectedConfigurationPage == index - count)
                    {
                        currentConfigPage = _configPages[index];
                    }
                }

                var filterIndex2 = _configPages.Count - count;
                foreach (var filter in _filterPages)
                {
                    filterIndex2++;
                    if (ConfigSelectedConfigurationPage == filterIndex2)
                    {
                        currentConfigPage = filter.Value;
                    }
                }
            }

            using (var mainChild =
                   ImRaii.Child("Main", new Vector2(-1, -1), currentConfigPage?.DrawBorder ?? false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (mainChild.Success)
                {
                    if (currentConfigPage != null)
                    {
                        MediatorService.Publish(currentConfigPage.Draw());
                    }
                }
            }
        }

        public override void Invalidate()
        {
            GenerateFilterPages();
        }

        public override FilterConfiguration? SelectedConfiguration => null;
    }
}