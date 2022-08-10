using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Addons;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic;
using InventoryTools.Sections;
using OtterGui;

namespace InventoryTools.Ui
{
    public class FiltersWindow : Window
    {
        public override bool SaveState => true;

        public static string AsKey = "filters";
        public override string Name => "Allagan Tools - Filters";
        public override string Key => AsKey;
        private string _activeFilter = "";
        private string _tabLayout = "";
        private int _selectedFilterTab = 0;
        public override Vector2 Size { get; } = new(700, 700);
        public override Vector2 MaxSize { get; } = new(2000, 2000);
        public override Vector2 MinSize { get; } = new(200, 200);
        public override bool DestroyOnClose => false;
        private static TextureWrap _settingsIcon => PluginService.IconStorage.LoadIcon(66319);
        private static TextureWrap _tetrisIcon => PluginService.IconStorage.LoadIcon(76955);
        private static TextureWrap _craftIcon => PluginService.IconStorage.LoadIcon(114060);
        private static TextureWrap _csvIcon => PluginService.IconStorage.LoadIcon(47);
        private static TextureWrap _clearIcon => PluginService.IconStorage.LoadIcon(66308);
        private static TextureWrap _marketIcon => PluginService.IconStorage.LoadIcon(90003);
        private static TextureWrap _helpIcon => PluginService.IconStorage.LoadIcon(66313);
        
        private List<FilterConfiguration>? _filters;

        public FiltersWindow()
        {
            _tabLayout = Utils.GenerateRandomId();
        }

        private List<FilterConfiguration> Filters
        {
            get
            {
                if (_filters == null)
                {
                    _filters = PluginService.FilterService.FiltersList.Where(c => c.FilterType != FilterType.CraftFilter).ToList();
                }

                return _filters;
            }
        }

        public override void Draw()
        {
            if (ImGui.BeginTabBar("###InventoryTabs" + _tabLayout, ImGuiTabBarFlags.FittingPolicyScroll))
            {
                var filterConfigurations = Filters;
                for (var index = 0; index < filterConfigurations.Count; index++)
                {
                    var filterConfiguration = filterConfigurations[index];
                    var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration);
                    if (itemTable == null)
                    {
                        continue;
                    }
                    if (ImGui.BeginTabItem(itemTable.Name + "##" + filterConfiguration.Key))
                    {
                        var activeFilter = DrawFilter(itemTable, filterConfiguration, _activeFilter);
                        if (_activeFilter != activeFilter && ImGui.IsWindowFocused())
                        {
                            if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                ConfigurationManager.Config.ActiveUiFilter != null)
                            {
                                PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                            }
                        }

                        ImGui.EndTabItem();
                    }
                }
                
                if (ConfigurationManager.Config.ShowFilterTab && ImGui.BeginTabItem("Filters"))
                {
                    if (ImGui.BeginChild("###monitorLeft", new Vector2(100, -1) * ImGui.GetIO().FontGlobalScale, true))
                    {
                        for (var index = 0; index < filterConfigurations.Count; index++)
                        {
                            var filterConfiguration = filterConfigurations[index];
                            if (ImGui.Selectable(filterConfiguration.Name + "###fl" + filterConfiguration.Key, index == _selectedFilterTab))
                            {
                                if (ConfigurationManager.Config.SwitchFiltersAutomatically && ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key)
                                {
                                    PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                                }

                                _selectedFilterTab = index;
                            }
                        }

                        ImGui.EndChild();
                    }

                    ImGui.SameLine();

                    if (ImGui.BeginChild("###monitorRight", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
                    {
                        for (var index = 0; index < filterConfigurations.Count; index++)
                        {
                            if (_selectedFilterTab == index)
                            {
                                var filterConfiguration = filterConfigurations[index];
                                var table = PluginService.FilterService.GetFilterTable(filterConfiguration.Key);
                                if (table != null)
                                {
                                    var activeFilter = DrawFilter(table, filterConfiguration, _activeFilter);
                                    if (_activeFilter != activeFilter)
                                    {
                                        if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                                            ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                                            ConfigurationManager.Config.ActiveUiFilter != null)
                                        {
                                            PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                                        }
                                    }
                                }
                            }
                        }
                        ImGui.EndChild();
                    }
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }

        public static unsafe string DrawFilter(FilterTable itemTable, FilterConfiguration filterConfiguration,string activeFilter)
        {
            if (ImGui.BeginChild("TopBar", new Vector2(0, 25) * ImGui.GetIO().FontGlobalScale))
            {
                var highlightItems = itemTable.HighlightItems;
                ImGui.Checkbox("Highlight?" + "###" + itemTable.Key + "VisibilityCheckbox",
                    ref highlightItems);
                if (highlightItems != itemTable.HighlightItems)
                {
                    PluginService.FilterService.ToggleActiveUiFilter(itemTable.FilterConfiguration);
                }
                ImGui.SameLine();
                if (ImGui.ImageButton(_clearIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    itemTable.ClearFilters();
                }

                ImGuiUtil.HoverTooltip("Clear the current search.");

                ImGui.EndChild();
            }

            if (ImGui.BeginChild("Content", new Vector2(0, -30) * ImGui.GetIO().FontGlobalScale))
            {
                if (filterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration);
                    craftTable?.Draw(new Vector2(0, -400));
                    itemTable.Draw(new Vector2(0, 0));
                }
                else
                {
                    itemTable.Draw(new Vector2(0, 0));
                }

                activeFilter = filterConfiguration.Key;

                ImGui.EndChild();
            }

            //Need to have these buttons be determined dynamically or moved elsewhere
            if (ImGui.BeginChild("BottomBar", new Vector2(0, 0), false, ImGuiWindowFlags.None))
            {
                if (ImGui.ImageButton(_marketIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    foreach (var item in itemTable.RenderSortedItems)
                    {
                        Universalis.QueuePriceCheck(item.InventoryItem.ItemId);
                    }

                    foreach (var item in itemTable.RenderItems)
                    {
                        Universalis.QueuePriceCheck(item.RowId);
                    }
                }
                ImGuiUtil.HoverTooltip("Refresh Market Prices");
                ImGui.SameLine();
                if (ImGui.ImageButton(_csvIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "export.csv", ".csv",
                        (b, s) => { SaveCallback(itemTable, b, s); }, null, true);
                }

                ImGuiUtil.HoverTooltip("Export to CSV");
                ImGui.SameLine();
                if (filterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    unsafe
                    {
                        var subMarinePartsMenu = PluginService.GameUi.GetWindow("SubmarinePartsMenu");
                        if (subMarinePartsMenu != null)
                        {
                            if (ImGui.Button("Add Submarine Parts to Craft"))
                            {
                                var subAddon = (SubmarinePartsMenuAddon*)subMarinePartsMenu;
                                for (int i = 0; i < 6; i++)
                                {
                                    var itemRequired = subAddon->RequiredItemId(i);
                                    if (itemRequired != 0)
                                    {
                                        var amountHandedIn = subAddon->AmountHandedIn(i);
                                        var amountNeeded = subAddon->AmountNeeded(i);
                                        var amountLeft = Math.Max((int)amountNeeded - (int)amountHandedIn,
                                            0);
                                        if (amountLeft > 0)
                                        {
                                            filterConfiguration.CraftList.AddCraftItem(itemRequired,
                                                (uint)amountLeft, ItemFlags.None);
                                            filterConfiguration.NeedsRefresh = true;
                                            filterConfiguration.StartRefresh();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                ImGui.SameLine();
                ImGui.Text("Pending Market Requests: " + Universalis.QueuedCount);
                if (filterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    ImGui.SameLine();
                    ImGui.Text("Total Cost NQ: " + filterConfiguration.CraftList.MinimumNQCost);
                    ImGui.SameLine();
                    ImGui.Text("Total Cost HQ: " + filterConfiguration.CraftList.MinimumHQCost);
                }

                if (filterConfiguration.FilterType == FilterType.CraftFilter)
                {
                    var craftTable = PluginService.FilterService.GetCraftTable(filterConfiguration);
                    craftTable?.DrawFooterItems();
                    itemTable.DrawFooterItems();
                }
                else
                {
                    itemTable.DrawFooterItems();
                }

                var width = ImGui.GetWindowSize().X;
                width -= 28;
                ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
                ImGui.SetCursorPosY(2 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.ImageButton(_settingsIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    PluginService.WindowService.ToggleConfigurationWindow();
                }

                ImGuiUtil.HoverTooltip("Open the configuration window.");

                width -= 30;
                ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
                ImGui.SetCursorPosY(2 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.ImageButton(_craftIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    PluginService.WindowService.ToggleCraftsWindow();
                }

                ImGuiUtil.HoverTooltip("Open the craft window.");

                width -= 30;
                ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
                ImGui.SetCursorPosY(2 * ImGui.GetIO().FontGlobalScale);
                if (ImGui.ImageButton(_helpIcon.ImGuiHandle,
                        new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                        new Vector2(1, 1), 2))
                {
                    PluginService.WindowService.ToggleHelpWindow();
                }
                ImGuiUtil.HoverTooltip("Open the help window.");
                
                if (ConfigurationManager.Config.TetrisEnabled)
                {
                    width -= 30;
                    ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
                    ImGui.SetCursorPosY(2 * ImGui.GetIO().FontGlobalScale);
                    if (ImGui.ImageButton(_tetrisIcon.ImGuiHandle,
                            new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale, new Vector2(0, 0),
                            new Vector2(1, 1), 2))
                    {
                        PluginService.WindowService.ToggleTetrisWindow();
                    }

                    ImGuiUtil.HoverTooltip("Open the tetris input window.");
                }


                ImGui.EndChild();
            }

            return activeFilter;
        }
        
        public override FilterConfiguration? SelectedConfiguration
        {
            get
            {
                if (_selectedFilterTab >= 0 && _selectedFilterTab < Filters.Count) return Filters[_selectedFilterTab];
                return null;
            }
        }

        private static void SaveCallback(FilterTable filterTable, bool arg1, string arg2)
        {
            if (arg1)
            {
                filterTable.ExportToCsv(arg2);
            }
        }
        
        public override void Invalidate()
        {
            _filters = null;
            _tabLayout = Utils.GenerateRandomId();
        }
    }
}