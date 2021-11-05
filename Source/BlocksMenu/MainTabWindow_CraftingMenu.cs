using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse;

namespace BlocksMenu
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
        public static List<RecipeDef> recipeList = null;

        // note for future: this could be what i was looking for
        //public static List<IGrouping<ThingDef, RecipeDef>> recipeListCompact = null;

        // changing lists
        public List<ModContentPack> modFilteredList = null;
        public List<ThingCategoryDef> categoryFilteredList = null;
        public List<RecipeDef> recipeFilteredList = null;

        // selected things to use in filter
        public ModContentPack selectedModContentPack = null;
        public ThingCategoryDef selectedCategoryDef = null;
        public RecipeDef selectedRecipeDef = null;
        public string searchString = "";

        // bill making stuff
        public Bill_Production bill = null;
        public Dialog_BillConfig billConfig = null;

        // options
        public bool isResearchOnly = false;
        public bool showProducedThingLabel = false;
        public bool searchByProducedThing = true;
        public bool categorizeByProducedThingSource = true;

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

            modFilteredList = FilterModContentPacks(recipeList, string.Empty, string.Empty, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            categoryFilteredList = FilterThingCategoryDefs(recipeList, null, string.Empty, string.Empty, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            recipeFilteredList = FilterRecipeDefs(recipeList, null, null, string.Empty, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect menuRect = windowRect.ContractedBy(Margin);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // figure out how to update list when a research project has been completed either ai thing, debug, or naturally
            //if (isResearchOnly && Find.ResearchManager.currentProj != null && Find.ResearchManager.GetProgress(Find.ResearchManager.currentProj) == Find.ResearchManager.currentProj.baseCost)
            //{
            //    modFilteredList = FilterModContentPacks(recipeList, searchString, string.Empty, isResearchOnly);
            //    categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, string.Empty, isResearchOnly);
            //    recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly);
            //}

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

            DoItemDescription(new Rect(
                0f,
                menuRect.height / 3f * 2f,
                menuRect.width,
                menuRect.height / 3f)
                .ContractedBy(outMargin));

            Rect rectItemTab = new Rect(
                menuRect.width / 2f,
                30f,
                menuRect.width / 2f,
                menuRect.height / 3f * 2f - 30f)
                .ContractedBy(outMargin);

            Rect rectResearchOnlyCheckBox = new Rect(
                rectItemTab.x + inMargin,
                rectItemTab.y + inMargin,
                15f,
                15f);

            Rect rectProducedThingLabelCheckBox = new Rect(
                rectResearchOnlyCheckBox.x + inMargin + 15f,
                rectResearchOnlyCheckBox.y,
                15f,
                15f);

            Rect rectProducedThingSearchCheckBox = new Rect(
                rectProducedThingLabelCheckBox.x + inMargin + 15f,
                rectProducedThingLabelCheckBox.y,
                15f,
                15f);

            Rect rectCategorizeProducedThingSourceCheckBox = new Rect(
                rectProducedThingSearchCheckBox.x + inMargin + 15f,
                rectProducedThingSearchCheckBox.y,
                15f,
                15f);

            DoItemsTab(rectItemTab);

            DoResearchOnlyCheckBox(rectResearchOnlyCheckBox);
            DoShowProducedThingNameCheckBox(rectProducedThingLabelCheckBox);
            DoSearchProducedThingCheckBox(rectProducedThingSearchCheckBox);
            DoCategorizeProducedThingSourceCheckBox(rectCategorizeProducedThingSourceCheckBox);
        }

        // start of menu ui functions
        // --------------------------
        public void DoSearchBox(Rect rect)
        {
            if (GeneralUI.SearchBar(rect, ref searchString))
            {
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, "", isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            }
        }

        public void DoModsTab(Rect rect)
        {
            rect = GeneralUI.LabelColorAndOutLine(rect, "Mods", Color.gray, TextAnchor.UpperCenter, inMargin);

            ModContentPack item = null;
            if (GeneralUI.ScrollMenu(rect, DecorateModButton, modFilteredList, ref item, ref _scrollPositionModTab))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedModContentPack = item;
                selectedCategoryDef = null;
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, "", isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            }
        }

        public void DoCategoriesTab(Rect rect)
        {
            rect = GeneralUI.LabelColorAndOutLine(rect, "Categories", Color.gray, TextAnchor.UpperCenter, inMargin);

            ThingCategoryDef item = null;
            if (GeneralUI.ScrollMenu(rect, DecorateCategoryButton, categoryFilteredList, ref item, ref _scrollPositionCategoryTab))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCategoryDef = item;
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            }
        }

        public void DoItemsTab(Rect rect)
        {
            rect = GeneralUI.LabelColorAndOutLine(rect, "Items", Color.gray, TextAnchor.UpperCenter, inMargin);

            RecipeDef item = null;
            if (GeneralUI.ScrollMenu(rect, DecorateItemButton, recipeFilteredList, ref item, ref _scrollPositionThingTab))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedRecipeDef = item;
            }
        }

        // start of checkboxes
        // -------------------
        // names get a little bit uhhh long here cause i am stupid
        public void DoResearchOnlyCheckBox(Rect rect)
        {
            if (GeneralUI.CheckboxMinimal(rect, !isResearchOnly ? "Show Available Items" : "Show All Items", Color.gray, ref isResearchOnly))
            {
                modFilteredList = FilterModContentPacks(recipeList, string.Empty, string.Empty, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, string.Empty, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            }
        }

        public void DoShowProducedThingNameCheckBox(Rect rect)
        {
            GeneralUI.CheckboxMinimal(rect, !showProducedThingLabel ? "Show Item Name" : "Show Bill Name", Color.gray, ref showProducedThingLabel);
        }

        public void DoSearchProducedThingCheckBox(Rect rect)
        {
            if (GeneralUI.CheckboxMinimal(rect, !searchByProducedThing ? "Search by Produced Item Name" : "Search by Bill Name", Color.gray, ref searchByProducedThing, false))
            {
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, "", isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            }
        }

        public void DoCategorizeProducedThingSourceCheckBox(Rect rect)
        {
            if (GeneralUI.CheckboxMinimal(rect, !categorizeByProducedThingSource ? "Show All Items From Selected Mod" : "Show All Bills From Selected Mod", Color.gray, ref categorizeByProducedThingSource, false))
            {
                recipeFilteredList = FilterRecipeDefs(recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
                categoryFilteredList = FilterThingCategoryDefs(recipeList, selectedModContentPack, searchString, "", isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);
            }
        }
        // -----------------
        // end of checkboxes

        public void DoItemDescription(Rect rect)
        {
            GUI.color = Color.gray;
            Widgets.DrawBox(rect);
            GUI.color = Color.white;
            Widgets.DrawAltRect(rect);

            rect = rect.ContractedBy(inMargin);

            if (selectedRecipeDef == null) return;

            Rect rectThingLabel = new Rect(
                rect.x,
                rect.y,
                200f,
                30f);

            Rect rectModLabel = new Rect(
                rectThingLabel.x,
                rectThingLabel.y + rectThingLabel.height,
                300f,
                20f);

            Rect rectDescription = new Rect(
                rect.x,
                rect.y + (rect.height / 3f),
                rect.width / 3f,
                rect.height / 3f * 2f);

            Rect rectRecipe = new Rect(
                rect.x + rect.width / 3f,
                rect.y + rect.height / 3f,
                rect.width / 3f,
                rect.height / 3f * 2f);

            Rect rectWorktables = new Rect(
                rect.x + rect.width / 3f * 2f,
                rect.y + rect.height / 3f,
                rect.width / 3f,
                rect.height / 3f * 2f);

            Rect rectInfo = new Rect(
                rect.x + rect.width - 30f,
                rect.y,
                30f,
                30f);

            Rect rectCraft = new Rect(
                rectInfo.x - 100f,
                rectInfo.y,
                100f,
                30f);

            rectRecipe = GeneralUI.LabelColorAndOutLine(rectRecipe, "Info", Color.gray, TextAnchor.UpperCenter, inMargin);
            rectWorktables = GeneralUI.LabelColorAndOutLine(rectWorktables, "Compatible worktable(s)", Color.gray, TextAnchor.UpperCenter, inMargin);
            rectDescription = GeneralUI.LabelColorAndOutLine(rectDescription, "Description", Color.gray, TextAnchor.UpperCenter, inMargin);
            rectInfo = rectInfo.ContractedBy(2f);

            Text.Font = GameFont.Medium;
            Widgets.Label(rectThingLabel, selectedRecipeDef.label.CapitalizeFirst());
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectModLabel, "Bill source: " + (selectedRecipeDef == null || selectedRecipeDef.modContentPack == null || selectedRecipeDef.modContentPack.Name == null ? "None" : selectedRecipeDef.modContentPack.Name));
            Text.Font = GameFont.Small;

            Widgets.LabelScrollable(rectDescription, selectedRecipeDef.description, ref _scrollPositionDescription);
            GeneralUI.ScrollMenu(rectRecipe, DecorateRecipeButton, selectedRecipeDef.ingredients, ref _scrollPositionRecipe, buttonHeight: 22f);
            Decription_WorktableButtons(rectWorktables);
            Widgets.InfoCardButton(rectInfo, selectedRecipeDef);
            Description_MakeBillButton(rectCraft, selectedRecipeDef, "Make Bill");
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
                if (!Input.GetKey(KeyCode.LeftShift))
                    Find.Selector.ClearSelection();

                foreach (Building table in FindWorktablesOnMap(selectedItem))
                    Find.Selector.Select(table);
            }
        }

        private void Description_MakeBillButton(Rect button, RecipeDef recipe, string buttonLabel)
        {
            if (Widgets.ButtonText(button, buttonLabel))
            {
                // check if research for item is done. note: don't need to check if worktable research is finished
                if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished)
                {
                    Messages.Message("Research not unlocked for this bill.", null, MessageTypeDefOf.CautionInput, null);
                    goto Exit;
                }

                if (!recipe.AvailableNow)
                {
                    Messages.Message("Bill not available for you right now.", null, MessageTypeDefOf.CautionInput, null);
                    goto Exit;
                }

                List<Building> selectedWorktablesOnMap = Find.Selector.SelectedObjectsListForReading.OfType<Building>().Where(building => building != null && building.def != null && selectedRecipeDef != null && selectedRecipeDef.AllRecipeUsers != null && recipe.AllRecipeUsers.Any(def => building.def == def)).ToList();
                if (selectedWorktablesOnMap.Count < 1)
                {
                    Messages.Message("No selected and/or compatable worktables to make bill with.", null, MessageTypeDefOf.CautionInput, null);
                    goto Exit;
                }

                billConfig = GeneralUI.OpenDialogBillConfig(recipe, ref bill);
            Exit:;
            }

            // checks if user is done making bill before adding bill to worktable(s)
            if (billConfig != null && billConfig.IsOpen == false)
            {
                List<Building> selectedWorktablesOnMap = Find.Selector.SelectedObjectsListForReading.OfType<Building>().Where(building => building != null && building.def != null && selectedRecipeDef != null && selectedRecipeDef.AllRecipeUsers != null && recipe.AllRecipeUsers.Any(def => building.def == def)).ToList();
                // note: i converted it to a building_worktable. won't work for mods like rimfactory where there buildings are a different type so this will throw and error
                foreach (Building_WorkTable table in selectedWorktablesOnMap)
                {
                    table.BillStack.AddBill(bill.Clone());
                    Messages.Message("Bill added to selected worktable(s).", null, MessageTypeDefOf.PositiveEvent, null);
                }

                // reset bill stuff
                billConfig = null;
                bill = null;
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

            string buttonLabel = showProducedThingLabel ? item.ProducedThingDef.label.CapitalizeFirst() : item.label.CapitalizeFirst();
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLabel, buttonLabel);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DecorateWorktableButton(Rect button, ThingDef item)
        {
            Widgets.DrawHighlightIfMouseover(button);
            TooltipHandler.TipRegion(button, new TipSignal("Select all " + item.label.CapitalizeFirst()));
            Rect rectPreviewImage = new Rect(button.x, button.y, button.height, button.height);
            if (item.uiIconColor != null)
                GUI.color = item.uiIconColor;
            Widgets.DrawTextureFitted(rectPreviewImage.ContractedBy(2f), item.uiIcon, 1f); ;
            GUI.color = Color.white;

            Rect rectLabel = new Rect(button.x + rectPreviewImage.width + 5f, button.y, button.width - rectPreviewImage.width - 5f, button.height);

            string buttonLabel = item.label.CapitalizeFirst();
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectLabel, buttonLabel);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DecorateRecipeButton(Rect button, IngredientCount item)
        {
            Widgets.Label(button, item.Summary);
        }
        // ------------------------
        // end of button decoration

        // start of helper funcions
        // ------------------------
        public static List<RecipeDef> FilterRecipeDefs(List<RecipeDef> filterFrom, ModContentPack modFilter, ThingCategoryDef categoryFilter, string filterString = "", bool filterAvailable = false, bool searchProducedThingDef = false, bool filterProducedThingSource = false)
        {
            // filter category
            if (categoryFilter != null)
                filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && categoryFilter.childThingDefs.Any(thingdef => def.ProducedThingDef == thingdef)).ToList();

            // filter search
            if (filterString != "")
            {
                if (!searchProducedThingDef)
                {
                    filterFrom = filterFrom.Where(def => def != null && def.label != null && def.label.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
                else
                {
                    filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.label != null && def.ProducedThingDef.label.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }
            }

            // filter mod
            if (modFilter != null)
            {
                if (!filterProducedThingSource)
                {
                    filterFrom = filterFrom.Where(def => def != null && def.modContentPack != null && def.modContentPack == modFilter).ToList();
                }
                else
                {
                    filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.modContentPack != null && def.ProducedThingDef.modContentPack == modFilter).ToList();
                }
            }

            // filters if it has been researched etc
            if (filterAvailable)
                filterFrom = filterFrom.Where(def => def != null && def.AllRecipeUsers != null && def.AvailableNow && def.AllRecipeUsers.Any(table => table.IsResearchFinished)).ToList();

            return filterFrom;
        }

        public static List<ThingCategoryDef> FilterThingCategoryDefs(List<RecipeDef> filterFrom, ModContentPack modFilter, string thingDefSearch = "", string categoryDefSearch = "", bool filterAvailable = false, bool searchProducedThingDef = false, bool filterProducedThingSource = false)
        {
            filterFrom = FilterRecipeDefs(filterFrom, modFilter, null, thingDefSearch, filterAvailable, searchProducedThingDef, filterProducedThingSource);

            List<ThingCategoryDef> filteredCategoryList;
            filteredCategoryList = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().ToList();

            if (filteredCategoryList.Count > 1)
                filteredCategoryList.Insert(0, null);

            return filteredCategoryList;
        }

        public static List<ModContentPack> FilterModContentPacks(List<RecipeDef> filterFrom, string thingDefSearch = "", string modContentPackSearch = "", bool filterAvailable = false, bool searchProducedThingDef = false, bool filterProducedThingSource = false)
        {
            filterFrom = FilterRecipeDefs(filterFrom, null, null, thingDefSearch, filterAvailable, searchProducedThingDef, filterProducedThingSource);

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
