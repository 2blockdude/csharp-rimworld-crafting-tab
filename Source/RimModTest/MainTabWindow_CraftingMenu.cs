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

        public static float outMargin = 2f;
        public static float inMargin = 5f;

        // true tab size after margins
        public Rect menuRect = Rect.zero;

        // keeps track of scroll
        internal static Vector2 _scrollPositionCategoryTab = Vector2.zero;
        internal static Vector2 _scrollPositionThingTab = Vector2.zero;
        internal static Vector2 _scrollPositionModTab = Vector2.zero;
        internal static Vector2 _scrollPositionRecipe = Vector2.zero;
        internal static Vector2 _scrollPositionDescription = Vector2.zero;
        internal static Vector2 _scrollPositionWorkBenches = Vector2.zero;

        // Lists
        public static List<ModContentPack> modsList = null;
        public static List<ThingCategoryDef> categoryList = null;
        public static List<RecipeDef> craftablesList = null;

        public List<ThingCategoryDef> categoryFilteredList = null;
        public List<RecipeDef> craftablesFilteredList = null;

        // selected things to use in filter
        public ThingCategoryDef selectedCategory = null;
        public ModContentPack selectedMod = null;
        public RecipeDef selectedCraftable = null;
        public string searchString = "";

        public ThingDef selectedWorktableType = null;
        public Building_WorkTable selectedWorktable = null;

        public MainTabWindow_CraftingMenu()
        {
            base.draggable = false;
            base.resizeable = false;

            // generate lists
            craftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def != null && def.ProducedThingDef != null).ToList();

            modsList = new List<ModContentPack>();
            modsList.InsertRange(0, craftablesList.Where(def => def != null && def.ProducedThingDef != null && def.modContentPack != null).Select(def => def.modContentPack).Distinct());
            if (modsList.Count > 1)
                modsList.Insert(0, null);

            categoryList = new List<ThingCategoryDef>();
            categoryList.Insert(0, null);
            categoryList.InsertRange(1, craftablesList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct());


            categoryFilteredList = categoryList;
            craftablesFilteredList = craftablesList;
        }

        public override void DoWindowContents(Rect inRect)
        {
            menuRect = windowRect.ContractedBy(Margin);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            DrawSearchBar();
            DrawModsTab();
            DrawCategoriesTab();
            DrawCraftablesTab();
            DrawItemDescription();
        }

        private void DrawSearchBar()
        {
            Rect searchBox = new Rect(
                menuRect.width / 2f,
                0f,
                menuRect.width / 2f,
                30f);

            searchBox = searchBox.ContractedBy(outMargin);

            if (GeneralUI.DrawSearchBar(searchBox, ref searchString))
            {
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
                categoryFilteredList = FilterThingCategoryDefs(categoryList, selectedMod, craftablesList, searchString, "");
            }
        }

        private void DrawModsTab()
        {
            // top third left
            Rect rectTab = new Rect(
                menuRect.width / 2f * 0f,            // posx
                menuRect.height / 2f * 0f,            // posy
                menuRect.width / 2f,                 // sizex
                menuRect.height / 3f);                // sizey

            Widgets.DrawBox(rectTab.ContractedBy(outMargin));
            rectTab = rectTab.ContractedBy(outMargin + inMargin);

            int selected = GeneralUI.DrawScrollTab(rectTab, DrawModButtons, modsList, ref _scrollPositionModTab);
            if (selected > -1)
            {
                ModContentPack item = modsList[selected];
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedMod = item;
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
                categoryFilteredList = FilterThingCategoryDefs(categoryList, selectedMod, craftablesList, searchString, "");
            }
        }

        private void DrawCategoriesTab()
        {
            Rect rectTab = new Rect(
                menuRect.width / 2f * 0f,            // posx
                menuRect.height / 3f * 1f,            // posy
                menuRect.width / 2f,                 // sizex
                menuRect.height / 3f);                // sizey

            Widgets.DrawBox(rectTab.ContractedBy(outMargin));
            rectTab = rectTab.ContractedBy(outMargin + inMargin);

            int selected = GeneralUI.DrawScrollTab(rectTab, DrawCategoryButtons, categoryFilteredList, ref _scrollPositionCategoryTab);
            if (selected > -1)
            {
                ThingCategoryDef item = categoryFilteredList[selected];
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCategory = item;
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
            }
        }

        private void DrawCraftablesTab()
        {
            Rect rectTab = new Rect(
                menuRect.width / 2f * 1f,             // posx
                menuRect.height / 2f * 0f + 30f,       // posy
                menuRect.width / 2f,                  // sizex
                menuRect.height / 3f * 2f - 30f);            // sizey

            Widgets.DrawBox(rectTab.ContractedBy(outMargin));
            rectTab = rectTab.ContractedBy(outMargin + inMargin);

            int selected = GeneralUI.DrawScrollTab(rectTab, DrawCraftablesButtons, craftablesFilteredList, ref _scrollPositionThingTab);
            if (selected > -1)
            {
                RecipeDef item = craftablesFilteredList[selected];
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCraftable = item;
                // reset selected work benches when clicking on new item
                selectedWorktableType = null;
                selectedWorktable = null;
            }
        }

        private void DrawItemDescription()
        {
            Rect rectGroup = new Rect(
                menuRect.width / 3f * 0f,
                menuRect.height / 3f * 2f,
                menuRect.width,
                menuRect.height / 3f);

            if (selectedCraftable == null) return;

            Widgets.DrawBox(rectGroup.ContractedBy(outMargin));
            rectGroup = rectGroup.ContractedBy(outMargin + inMargin);

            Rect rectThingLabel = new Rect(
                rectGroup.x,
                rectGroup.y,
                200f,
                30f);

            Rect rectRecipeReqLabel = new Rect(
                rectThingLabel.x,
                rectThingLabel.y + rectThingLabel.height,
                300f,
                20f);

            Rect rectModLabel = new Rect(
                rectRecipeReqLabel.x,
                rectRecipeReqLabel.y + rectRecipeReqLabel.height,
                200f,
                20f);

            Rect rectDescription = new Rect(
                rectGroup.x,
                rectGroup.y + (rectGroup.height / 3f),
                rectGroup.width / 3f,
                rectGroup.height / 3f * 2f);

            Rect rectRecipe = new Rect(
                rectGroup.x + rectGroup.width / 3f,
                rectGroup.y + rectGroup.height / 3f,
                rectGroup.width / 3f,
                rectGroup.height / 3f * 2f);

            Rect rectWorktables = new Rect(
                rectGroup.x + rectGroup.width / 3f * 2f,
                rectGroup.y + rectGroup.height / 3f,
                rectGroup.width / 3f,
                rectGroup.height / 3f * 2f);

            Rect rectInfo = new Rect(
                rectGroup.x + rectGroup.width - 30f,
                rectGroup.y,
                30f,
                30f);

            Rect rectCraft = new Rect(
                rectInfo.x - 100f,
                rectInfo.y,
                100f,
                30f);

            Widgets.DrawBox(rectDescription);
            Widgets.DrawBox(rectRecipe);
            Widgets.DrawBox(rectWorktables);

            rectDescription = rectDescription.ContractedBy(inMargin);
            rectRecipe = rectRecipe.ContractedBy(inMargin);
            rectWorktables = rectWorktables.ContractedBy(inMargin);
            rectInfo = rectInfo.ContractedBy(2f);

            Text.Font = GameFont.Medium;
            Widgets.Label(rectThingLabel, selectedCraftable.ProducedThingDef.label);
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectRecipeReqLabel, "Required Research: " + (selectedCraftable.researchPrerequisite == null ? "None" : selectedCraftable.researchPrerequisite.label));
            Widgets.Label(rectModLabel, "Mod Source: " + selectedCraftable.modContentPack.ModMetaData.Name);
            Text.Font = GameFont.Small;

            Widgets.LabelScrollable(rectDescription, selectedCraftable.ProducedThingDef.description, ref _scrollPositionDescription);
            GeneralUI.DrawScrollTab(rectRecipe, DrawRecipeButtons, selectedCraftable.ingredients, ref _scrollPositionRecipe, buttonHeight: 22f);
            WorktableButtons(rectWorktables);
            Widgets.InfoCardButton(rectInfo, selectedCraftable.ProducedThingDef);
            MakeBillButton(rectCraft, selectedCraftable);
        }

        private void WorktableButtons(Rect rectView)
        {
            List<ThingDef> allowedTables = selectedCraftable.AllRecipeUsers.Distinct().ToList();
            int selected = GeneralUI.DrawScrollTab(rectView, DrawWorktableButtons, allowedTables, ref _scrollPositionWorkBenches);
            if (selected > -1)
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedWorktableType = allowedTables[selected];

                if (Input.GetMouseButtonUp(1))
                {
                    List<Building_WorkTable> worktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
                    worktablesOnMap = worktablesOnMap.Where(def => def.def == selectedWorktableType).ToList();

                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Building_WorkTable table in worktablesOnMap)
                    {
                        options.Add(new FloatMenuOption(table.Label, default(Action)));
                    }

                    if (options.Count != 0)
                        Find.WindowStack.Add(new FloatMenu(options));
                }
            }
        }

        private void MakeBillButton(Rect button, RecipeDef recipe)
        {
            (Bill_Production bill, GeneralUI.EventCode eventVal) buttonState = GeneralUI.DrawMakeBillButton(button, recipe, selectedWorktableType);

            Bill_Production bill = buttonState.bill;
            GeneralUI.EventCode eventVal = buttonState.eventVal;

            switch (eventVal)
            {
                case GeneralUI.EventCode.BillComplete:
                    if (selectedWorktable != null)
                    {
                        selectedWorktable.BillStack.AddBill(bill.Clone());
                        Messages.Message("Bill added to worktable.", null, MessageTypeDefOf.PositiveEvent, null);
                    }
                    else if (selectedWorktableType != null)
                    {
                        List<Building_WorkTable> worktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
                        worktablesOnMap = worktablesOnMap.Where(def => def.def == selectedWorktableType).ToList();

                        // add bill to each worktable on current map
                        foreach (Building_WorkTable table in worktablesOnMap)
                            table.BillStack.AddBill(bill.Clone());
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
                    Messages.Message("Bill not available due to faction type or memes.", null, MessageTypeDefOf.CautionInput, null);
                    break;
                case GeneralUI.EventCode.ResearchIncomplete:
                    Messages.Message("Research not unlocked for this bill.", null, MessageTypeDefOf.CautionInput, null);
                    break;
                case GeneralUI.EventCode.NoSelectedWorktableType:
                    Messages.Message("Select worktable or worktable type.", null, MessageTypeDefOf.CautionInput, null);
                    break;
            }
        }

        private void DrawModButtons(ModContentPack item, Rect button)
        {
            // draw and decorate button
            if (selectedMod == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.Name;
            Widgets.Label(button, buttonTitle);
        }

        private void DrawCategoryButtons(ThingCategoryDef item, Rect button)
        {
            // draw and decorate button
            if (selectedCategory == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.label;
            Widgets.Label(button, buttonTitle.CapitalizeFirst());
        }

        private void DrawCraftablesButtons(RecipeDef item, Rect button)
        {
            // draw and decorate button
            if (selectedCraftable == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            TooltipHandler.TipRegion(button, new TipSignal(item.ProducedThingDef.description));
            Widgets.Label(button, item.ProducedThingDef.label);
        }

        private void DrawWorktableButtons(ThingDef item, Rect button)
        {
            if (selectedWorktableType == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);
            Widgets.Label(button, item.label);
        }

        private void DrawRecipeButtons(IngredientCount item, Rect button)
        {
            Widgets.Label(button, item.Summary);
        }


        private List<RecipeDef> FilterRecipeDefs(List<RecipeDef> filterFromRecipes, ModContentPack modFilter, ThingCategoryDef categoryFilter, string labelFilter)
        {
            List<RecipeDef> filteredList = filterFromRecipes;

            // filter category
            if (categoryFilter != null)
                filteredList = filteredList.Where(def => def != null && def.ProducedThingDef != null && categoryFilter.childThingDefs.Any(thingdef => def.ProducedThingDef == thingdef)).ToList();

            // filter search
            if (labelFilter != "")
                filteredList = filteredList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.label.IndexOf(labelFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            // filter mod
            if (modFilter != null)
                filteredList = filteredList.Where(def => def != null && def.modContentPack != null && def.modContentPack == modFilter).ToList();

            return filteredList;
        }

        private List<ThingCategoryDef> FilterThingCategoryDefs(List<ThingCategoryDef> filterFromCategories, ModContentPack modFilter, List<RecipeDef> whiteList, string thingDefSearch, string categoryDefSearch)
        {
            List<ThingCategoryDef> filteredList = filterFromCategories;

            // filter categories with label that include string
            if (categoryDefSearch != "")
            {
                filteredList = filteredList.Where(def => def != null && def.label.IndexOf(categoryDefSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            // filter categories with thingdefs that include string
            if (thingDefSearch != "")
            {
                filteredList = filteredList.Where(def => def != null && def.childThingDefs != null && def.childThingDefs.Any(thingdef => thingdef.label.IndexOf(thingDefSearch, StringComparison.OrdinalIgnoreCase) >= 0)).ToList();
            }

            // filter categories with thingdefs that are from mod
            if (modFilter != null)
            {
                filteredList = filteredList.Where(def => def != null && def.childThingDefs != null && def.childThingDefs.Where(thingdef => thingdef != null && thingdef.modContentPack != null).Select(thingdef => thingdef.modContentPack).Contains(modFilter)).ToList();
            }

            // note: I put at bottom because I believe it takes longer than the others and I don't know how to optimize
            // remove any categories that don't have an item within the whitelist
            if (whiteList != null)
            {
                // filter whitelist to have only selected mod items
                if (modFilter != null)
                    whiteList = whiteList.Where(def => def != null && def.modContentPack != null && def.modContentPack == modFilter).ToList();

                // filter whitelist to only contain items with thingDefSearch string
                if (thingDefSearch != null)
                    whiteList = whiteList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.label.IndexOf(thingDefSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

                // filter list with whitelist
                filteredList = whiteList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().Intersect(filteredList).ToList();
            }

            if (filteredList.Count > 1)
                filteredList.Insert(0, null);

            return filteredList;
        }

        private List<Building_WorkTable> FindWorktablesOnMap(ThingDef worktableType)
        {
            List<Building_WorkTable> worktables = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
            worktables = worktables.Where(def => def.def == worktableType).ToList();
            worktables.Insert(0, null);
            return worktables;
        }
    }
}
