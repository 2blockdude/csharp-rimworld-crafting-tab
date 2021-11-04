using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse;

namespace BlockdudesTabs
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_CraftingMenu : MainTabWindow
    {
        // Tab Settings
        public override Vector2 RequestedTabSize => new Vector2(700f, 700f);
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;
        protected override float Margin => 5f;

        public float outMargin = 2f;
        public float inMargin = 5f;

        // keeps track of scroll
        internal static Vector2 _scrollPositionCategoryTab = Vector2.zero;
        internal static Vector2 _scrollPositionThingTab = Vector2.zero;
        internal static Vector2 _scrollPositionModTab = Vector2.zero;
        internal static Vector2 _scrollPositionRecipe = Vector2.zero;
        internal static Vector2 _scrollPositionDescription = Vector2.zero;
        internal static Vector2 _scrollPositionWorkBenches = Vector2.zero;

        // Lists
        public static HashSet<Tuple<ThingDef, List<RecipeDef>>> thingListCompact = null;
        public static List<ModContentPack> modList = null;
        public static List<ThingCategoryDef> categoryList = null;
        public static List<RecipeDef> recipeList = null;

        public List<ModContentPack> modFilteredList = null;
        public List<ThingCategoryDef> categoryFilteredList = null;
        public List<RecipeDef> recipeFilteredList = null;

        // selected things to use in filter
        public ModContentPack selectedModContentPack = null;
        public ThingCategoryDef selectedCategoryDef = null;
        public RecipeDef selectedRecipeDef = null;
        public string searchString = "";

        // selected worktables to add bill to
        public ThingDef selectedWorktableType = null;
        public Building selectedWorktable = null;

        public bool isResearchOnly = false;

        public MainTabWindow_CraftingMenu()
        {
            base.draggable = false;
            base.resizeable = false;
            GenerateLists();
        }

        private void GenerateLists()
        {
            // generate lists
            recipeList = DefDatabase<RecipeDef>.AllDefs.Where(def => def != null && def.ProducedThingDef != null && def.AllRecipeUsers != null && def.AllRecipeUsers.Count() > 0).ToList();
            modList = FilterModContentPacks(recipeList, string.Empty, string.Empty, false);
            categoryList = FilterThingCategoryDefs(recipeList, null, string.Empty, string.Empty, false);

            modFilteredList = FilterModContentPacks(recipeList, string.Empty, string.Empty, isResearchOnly);
            categoryFilteredList = FilterThingCategoryDefs(recipeList, null, string.Empty, string.Empty, isResearchOnly);
            recipeFilteredList = FilterRecipeDefs(recipeList, null, null, string.Empty, isResearchOnly);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect menuRect = windowRect.ContractedBy(Margin);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            DoSearchBox(new Rect(
                menuRect.width / 2f,
                0f,
                menuRect.width / 2f,
                30f).ContractedBy(outMargin));

            DoModsTab(new Rect(
                0f,
                0f,
                menuRect.width / 2f,
                menuRect.height / 3f)
                .ContractedBy(outMargin));

            DoCategoriesTab(new Rect(
                0f,
                menuRect.height / 3f,
                menuRect.width / 2f,
                menuRect.height / 3f)
                .ContractedBy(outMargin));

            DoItemsTab(new Rect(
                menuRect.width / 2f,
                30f,
                menuRect.width / 2f,
                menuRect.height / 3f * 2f - 30f)
                .ContractedBy(outMargin));

            DoItemDescription(new Rect(
                0f,
                menuRect.height / 3f * 2f,
                menuRect.width,
                menuRect.height / 3f)
                .ContractedBy(outMargin));

        }

        // start of menu ui functions
        // --------------------------
        public void DoSearchBox(Rect rectSearchBox)
        {
            if (GeneralUI.SearchBar(rectSearchBox, ref searchString))
            {
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, "", isResearchOnly);
            }
        }

        public void DoModsTab(Rect rectTab)
        {
            rectTab = GeneralUI.LabelColorAndOutLine(rectTab, "Mods", Color.gray, TextAnchor.UpperCenter, inMargin);

            ModContentPack item = null;
            if (GeneralUI.ScrollMenu(rectTab, DecorateModButton, modFilteredList, ref item, ref _scrollPositionModTab))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedModContentPack = item;
                selectedCategoryDef = null;
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, "", isResearchOnly);
            }
        }

        public void DoCategoriesTab(Rect rectTab)
        {
            rectTab = GeneralUI.LabelColorAndOutLine(rectTab, "Categories", Color.gray, TextAnchor.UpperCenter, inMargin);

            ThingCategoryDef item = null;
            if (GeneralUI.ScrollMenu(rectTab, DecorateCategoryButton, categoryFilteredList, ref item, ref _scrollPositionCategoryTab))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCategoryDef = item;
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly);
            }
        }

        public void DoItemsTab(Rect rectTab)
        {
            Rect checkbox = new Rect(rectTab.x + inMargin, rectTab.y + inMargin, 15f, 15f);
            if (GeneralUI.CheckboxMinimal(checkbox, "Show Available Only", Color.gray, ref isResearchOnly))
            {
                modFilteredList = FilterModContentPacks(recipeList, searchString, string.Empty, isResearchOnly);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, string.Empty, isResearchOnly);
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly);
            }

            rectTab = GeneralUI.LabelColorAndOutLine(rectTab, "Items", Color.gray, TextAnchor.UpperCenter, inMargin);

            RecipeDef item = null;
            if (GeneralUI.ScrollMenu(rectTab, DecorateItemButton, recipeFilteredList, ref item, ref _scrollPositionThingTab))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedRecipeDef = item;
                // reset selected work benches when clicking on new item
                selectedWorktableType = null;
                selectedWorktable = null;
            }
        }

        public void DoItemDescription(Rect rectTab)
        {
            GUI.color = Color.gray;
            Widgets.DrawBox(rectTab);
            GUI.color = Color.white;
            Widgets.DrawAltRect(rectTab);

            rectTab = rectTab.ContractedBy(inMargin);

            if (selectedRecipeDef == null) return;

            Rect rectThingLabel = new Rect(
                rectTab.x,
                rectTab.y,
                200f,
                30f);

            Rect rectModLabel = new Rect(
                rectThingLabel.x,
                rectThingLabel.y + rectThingLabel.height,
                300f,
                20f);

            Rect rectDescription = new Rect(
                rectTab.x,
                rectTab.y + (rectTab.height / 3f),
                rectTab.width / 3f,
                rectTab.height / 3f * 2f);

            Rect rectRecipe = new Rect(
                rectTab.x + rectTab.width / 3f,
                rectTab.y + rectTab.height / 3f,
                rectTab.width / 3f,
                rectTab.height / 3f * 2f);

            Rect rectWorktables = new Rect(
                rectTab.x + rectTab.width / 3f * 2f,
                rectTab.y + rectTab.height / 3f,
                rectTab.width / 3f,
                rectTab.height / 3f * 2f);

            Rect rectInfo = new Rect(
                rectTab.x + rectTab.width - 30f,
                rectTab.y,
                30f,
                30f);

            Rect rectCraft = new Rect(
                rectInfo.x - 100f,
                rectInfo.y,
                100f,
                30f);

            rectRecipe = GeneralUI.LabelColorAndOutLine(rectRecipe, "Info", Color.gray, TextAnchor.UpperCenter, inMargin);
            rectWorktables = GeneralUI.LabelColorAndOutLine(rectWorktables, "Select worktable", Color.gray, TextAnchor.UpperCenter, inMargin);
            rectDescription = GeneralUI.LabelColorAndOutLine(rectDescription, "Description", Color.gray, TextAnchor.UpperCenter, inMargin);
            rectInfo = rectInfo.ContractedBy(2f);

            Text.Font = GameFont.Medium;
            Widgets.Label(rectThingLabel, selectedRecipeDef.ProducedThingDef.label.CapitalizeFirst());
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectModLabel, "Source: " + (selectedRecipeDef.ProducedThingDef == null || selectedRecipeDef.ProducedThingDef.modContentPack == null || selectedRecipeDef.ProducedThingDef.modContentPack.Name == null ? "None" : selectedRecipeDef.ProducedThingDef.modContentPack.Name));
            Text.Font = GameFont.Small;

            Widgets.LabelScrollable(rectDescription, selectedRecipeDef.ProducedThingDef.description, ref _scrollPositionDescription);
            GeneralUI.ScrollMenu(rectRecipe, DecorateRecipeButton, selectedRecipeDef.ingredients, ref _scrollPositionRecipe, buttonHeight: 22f);
            Decription_WorktableButtons(rectWorktables);
            Widgets.InfoCardButton(rectInfo, selectedRecipeDef.ProducedThingDef);
            Description_MakeBillButton(rectCraft, selectedRecipeDef);
        }
        // ------------------------
        // end of menu ui functions

        // start of description helper functions
        // -------------------------------------
        private void Decription_WorktableButtons(Rect rectView)
        {
            ThingDef selectedItem = null;
            if (GeneralUI.ScrollMenu(rectView, DecorateWorktableButton, selectedRecipeDef.AllRecipeUsers.Distinct().ToList(), ref selectedItem, ref _scrollPositionWorkBenches))
            {
                selectedWorktableType = selectedItem;
                if (selectedWorktable != null && selectedWorktable.def != selectedWorktableType)
                    selectedWorktable = null;

                // only if button is left clicked
                if (Input.GetMouseButtonUp(0))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);

                    if (selectedWorktable != null && selectedWorktable.def == selectedWorktableType)
                        CameraJumper.TryJumpAndSelect(selectedWorktable);
                    else
                    {
                        Find.Selector.ClearSelection();
                        foreach (Building table in FindWorktablesOnMap(selectedWorktableType))
                            Find.Selector.Select(table);
                    }
                }

                // only show float menu if button is right clicked
                if (Input.GetMouseButtonUp(1))
                {
                    List<Building> worktablesOnMap = FindWorktablesOnMap(selectedWorktableType);
                    if (worktablesOnMap.Count() > 1)
                        worktablesOnMap.Insert(0, null);
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach (Building table in worktablesOnMap)
                    {
                        Action select = delegate ()
                        {
                            selectedWorktable = table;
                            CameraJumper.TryJumpAndSelect(table);

                            if (selectedWorktable == null)
                            {
                                Find.Selector.ClearSelection();
                                foreach (Building all in FindWorktablesOnMap(selectedWorktableType))
                                    Find.Selector.Select(all);
                            }
                        };
                        options.Add(new FloatMenuOption(table != null ? table.Label.CapitalizeFirst() + $": {table.thingIDNumber}" : "All", select));
                    }

                    if (options.Count != 0)
                    {
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                    else
                    {
                        Messages.Message("Cannot find any worktables of this type.", null, MessageTypeDefOf.CautionInput, null);
                    }
                }
            }
        }

        private void Description_MakeBillButton(Rect button, RecipeDef recipe)
        {
            Bill_Production bill = null;
            switch (GeneralUI.MakeBillButton(button, recipe, selectedWorktableType, ref bill))
            {
                case GeneralUI.EventCode.BillComplete:
                    if (selectedWorktable != null)
                    {
                        ((Building_WorkTable)selectedWorktable).BillStack.AddBill(bill.Clone());
                        Messages.Message("Bill added to worktable.", null, MessageTypeDefOf.PositiveEvent, null);
                    }
                    else if (selectedWorktableType != null)
                    {
                        List<Building> worktablesOnMap = FindWorktablesOnMap(selectedWorktableType);

                        // add bill to each worktable on current map
                        foreach (Building table in worktablesOnMap)
                            ((Building_WorkTable)table).BillStack.AddBill(bill.Clone());
                        Messages.Message("Bill added to all worktables.", null, MessageTypeDefOf.PositiveEvent, null);
                    }
                    break;

                case GeneralUI.EventCode.IncompatibleRecipe:
                    Messages.Message("Bill not compatiable with worktable.", null, MessageTypeDefOf.CautionInput, null);
                    break;
                case GeneralUI.EventCode.NoAvailableWorktables:
                    Messages.Message("No available worktables for bill.", null, MessageTypeDefOf.CautionInput, null);
                    break;
                case GeneralUI.EventCode.RecipeNotAvailable:
                    Messages.Message("Bill not available.", null, MessageTypeDefOf.CautionInput, null);
                    break;
                case GeneralUI.EventCode.ResearchIncomplete:
                    Messages.Message("Research not unlocked for this bill.", null, MessageTypeDefOf.CautionInput, null);
                    break;
                case GeneralUI.EventCode.NoSelectedWorktableType:
                    Messages.Message("Select worktable or worktable type.", null, MessageTypeDefOf.CautionInput, null);
                    break;
            }
        }
        // -----------------------------------
        // end of description helper functions

        // Start of button decorations for scroll view
        // --------------------------
        private void DecorateModButton(Rect button, ModContentPack item)
        {
            // draw and decorate button
            if (selectedModContentPack == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.Name;
            Widgets.Label(button, buttonTitle);
        }

        private void DecorateCategoryButton(Rect button, ThingCategoryDef item)
        {
            // draw and decorate button
            if (selectedCategoryDef == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.label.CapitalizeFirst();
            Widgets.Label(button, buttonTitle);
        }

        private void DecorateItemButton(Rect button, RecipeDef item)
        {
            // draw and decorate button
            if (selectedRecipeDef == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            TooltipHandler.TipRegion(button, new TipSignal(item.ProducedThingDef.description));

            Rect rectPreviewImage = new Rect(button.x, button.y, button.height, button.height);
            if (item.ProducedThingDef.uiIconColor != null)
                GUI.color = item.ProducedThingDef.uiIconColor;
            Widgets.DrawTextureFitted(rectPreviewImage.ContractedBy(2f), item.ProducedThingDef.uiIcon, 1f); ;
            GUI.color = Color.white;

            Rect rectLabel = new Rect(button.x + rectPreviewImage.width + 5f, button.y, button.width - rectPreviewImage.width - 5f, button.height);

            string buttonLabel = item.ProducedThingDef.label.CapitalizeFirst();
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLabel, buttonLabel);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DecorateWorktableButton(Rect button, ThingDef item)
        {
            if (selectedWorktableType == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);
            Widgets.Label(button, item.label.CapitalizeFirst() + (selectedWorktable != null && selectedWorktable.def == item ? ": " + selectedWorktable.thingIDNumber : ""));
        }

        private void DecorateRecipeButton(Rect button, IngredientCount item)
        {
            Widgets.Label(button, item.Summary);
        }
        // ------------------------
        // end of button decoration

        // start of helper funcions
        // ------------------------
        public static List<RecipeDef> FilterRecipeDefs(List<RecipeDef> filterFrom, ModContentPack modFilter, ThingCategoryDef categoryFilter, string thingDefSearch = "", bool filterAvailable = false)
        {
            // filter category
            if (categoryFilter != null)
                filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && categoryFilter.childThingDefs.Any(thingdef => def.ProducedThingDef == thingdef)).ToList();

            // filter search
            if (thingDefSearch != "")
                filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.label.IndexOf(thingDefSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            // filter mod
            if (modFilter != null)
                filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.modContentPack != null && def.ProducedThingDef.modContentPack == modFilter).ToList();

            // filters if it has been researched etc
            if (filterAvailable)
                filterFrom = filterFrom.Where(def => def != null && def.AllRecipeUsers != null && def.AvailableNow && def.AllRecipeUsers.Any(table => table.IsResearchFinished)).ToList();

            return filterFrom;
        }

        public static List<ThingCategoryDef> FilterThingCategoryDefs(List<RecipeDef> filterFrom, ModContentPack modFilter, string thingDefSearch = "", string categoryDefSearch = "", bool filterAvailable = false)
        {
            filterFrom = FilterRecipeDefs(filterFrom, modFilter, null, thingDefSearch, filterAvailable);

            List<ThingCategoryDef> filteredCategoryList;
            filteredCategoryList = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().ToList();

            if (filteredCategoryList.Count > 1)
                filteredCategoryList.Insert(0, null);

            return filteredCategoryList;
        }

        public static List<ModContentPack> FilterModContentPacks(List<RecipeDef> filterFrom, string thingDefSearch = "", string modContentPackSearch = "", bool filterAvailable = false)
        {
            filterFrom = FilterRecipeDefs(filterFrom, null, null, thingDefSearch, filterAvailable);

            List<ModContentPack> filteredModContentPack;
            filteredModContentPack = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.modContentPack != null).Select(def => def.ProducedThingDef.modContentPack).Distinct().ToList();

            if (filteredModContentPack.Count > 1)
                filteredModContentPack.Insert(0, null);

            return filteredModContentPack;
        }

        public static List<Building> FindWorktablesOnMap(ThingDef worktableType)
        {
            List<Building> worktables = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building>().ToList();
            worktables = worktables.Where(def => def.def == worktableType).ToList();
            return worktables;
        }
        // -----------------------
        // end of helper functions
    }
}
