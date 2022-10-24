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

        // Base Lists
        public static List<RecipeDef> recipeList = DefDatabase<RecipeDef>.AllDefs.Where(def => def != null && def.ProducedThingDef != null && def.AllRecipeUsers != null && def.AllRecipeUsers.Count() > 0).ToList();
        public static List<ModContentPack> modList = recipeList.Where(def => def.modContentPack != null).Select(def => def.modContentPack).Distinct().ToList();
        public static List<ModContentPack> producedThingModList = recipeList.Where(def => def.ProducedThingDef.modContentPack != null).Select(def => def.ProducedThingDef.modContentPack).Distinct().ToList();
        public static List<ThingCategoryDef> categoryList = recipeList.Where(def => def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().ToList();

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
        public bool categorizeByProducedThingSource = true;
        public bool searchByProducedThing = true;
        public bool showProducedThingName = false;
        public bool showProducedThingDescription = false;
        public bool keepModsStatic = false;
        public bool keepCategoriesStatic = false;

        public MainTabWindow_CraftingMenu()
        {
            base.draggable = false;
            base.resizeable = false;

            // input null if not done already
            if (categoryList[0] != null && categoryList.Count > 1) categoryList.Insert(0, null);
            if (modList[0] != null && modList.Count > 1) modList.Insert(0, null);
            if (producedThingModList[0] != null && producedThingModList.Count > 1) producedThingModList.Insert(0, null);

            updateLists(true, true, true);
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

            Rect rectPos1 = new Rect(
                rectItemTab.x + inMargin,
                rectItemTab.y + inMargin,
                15f,
                15f);

            Rect rectPos2 = new Rect(
                rectPos1.x + inMargin + 15f,
                rectPos1.y,
                15f,
                15f);

            Rect rectPos3 = new Rect(
                rectPos2.x + inMargin + 15f,
                rectPos2.y,
                15f,
                15f);

            Rect rectPos4 = new Rect(
                rectPos3.x + inMargin + 15f,
                rectPos3.y,
                15f,
                15f);

            Rect rectPos5 = new Rect(
                rectPos4.x + inMargin + 15f,
                rectPos4.y,
                15f,
                15f);

            Rect rectPos6 = new Rect(
                rectPos5.x + inMargin + 15f,
                rectPos5.y,
                15f,
                15f);

            DoItemsTab(rectItemTab);

            DoResearchOnlyCheckBox(rectPos1);
            DoCategorizeProducedThingSourceCheckBox(rectPos2);
            DoSearchProducedThingCheckBox(rectPos3);
            DoShowProducedThingNameCheckBox(rectPos4);
            DoShowProducedThingDescriptionCheckBox(rectPos5);
            DoKeepModsAndCategoriesStaticCheckBox(rectPos6);
        }

        // start of update functions
        // -------------------------
        public void updateLists(bool updateMods, bool updateCategories, bool updateRecipes)
        {
            List<RecipeDef> recipes = null;
            List<ThingCategoryDef> categories = null;
            List<ModContentPack> mods = null;

            FilterDefs(out recipes, out categories, out mods, recipeList, selectedModContentPack, selectedCategoryDef, searchString, isResearchOnly, searchByProducedThing, categorizeByProducedThingSource);

            if (keepModsStatic)
            {
                if (updateMods) modFilteredList = categorizeByProducedThingSource ? producedThingModList : modList;
                if (updateCategories) categoryFilteredList = categoryList;
            }
            else
            {
                if (updateMods) modFilteredList = mods;
                if (updateCategories) categoryFilteredList = categories;
            }

            if (updateRecipes) recipeFilteredList = recipes;
        }
        // -----------------------
        // end of update functions

        // start of menu ui functions
        // --------------------------
        public void DoSearchBox(Rect rect)
        {
            if (GeneralUI.SearchBar(rect, ref searchString))
            {
                updateLists(true, true, true);
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
                updateLists(false, true, true);
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
                updateLists(false, false, true);
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
            if (GeneralUI.CheckboxMinimal(rect, Color.gray, ref isResearchOnly, !isResearchOnly ? "Show Researched / Available Items" : "Show All Items"))
            {
                updateLists(true, true, true);
            }
        }

        public void DoCategorizeProducedThingSourceCheckBox(Rect rect)
        {
            if (GeneralUI.CheckboxMinimal(rect, Color.gray, ref categorizeByProducedThingSource, !categorizeByProducedThingSource ? "Categorize by Item" : "Categorize by Bill", false))
            {
                updateLists(true, true, true);
            }
        }

        public void DoSearchProducedThingCheckBox(Rect rect)
        {
            if (GeneralUI.CheckboxMinimal(rect, Color.gray, ref searchByProducedThing, !searchByProducedThing ? "Search by Item Name" : "Search by Bill Name", false))
            {
                updateLists(false, true, true);
            }
        }

        public void DoShowProducedThingNameCheckBox(Rect rect)
        {
            GeneralUI.CheckboxMinimal(rect, Color.gray, ref showProducedThingName, !showProducedThingName ? "List Item Names" : "List Bill Names");
        }

        public void DoShowProducedThingDescriptionCheckBox(Rect rect)
        {
            GeneralUI.CheckboxMinimal(rect, Color.gray, ref showProducedThingDescription, !showProducedThingDescription ? "Show Item Details" : "Show Bill Details");
        }

        public void DoKeepModsAndCategoriesStaticCheckBox(Rect rect)
        {
            GeneralUI.CheckboxMinimal(rect, Color.gray, ref keepModsStatic, !keepModsStatic ? "Keep Mods And Categories Static" : "Do Mods And Categories Filtering");
            {
                keepCategoriesStatic = keepModsStatic;
                updateLists(true, true, false);
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

            Def selectedDef;
            if (showProducedThingDescription && selectedRecipeDef.ProducedThingDef != null)
                selectedDef = selectedRecipeDef.ProducedThingDef;
            else
                selectedDef = selectedRecipeDef;

            Text.Font = GameFont.Medium;
            Widgets.Label(rectThingLabel, selectedDef.label.CapitalizeFirst());
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectModLabel, "Source: " +
                (selectedDef == null || selectedDef.modContentPack == null || selectedDef.modContentPack.Name == null ? "None" : selectedDef.modContentPack.Name));
            Text.Font = GameFont.Small;

            Widgets.LabelScrollable(rectDescription, selectedDef.description, ref _scrollPositionDescription);
            Decription_WorktableButtons(rectWorktables);
            Widgets.InfoCardButton(rectInfo, selectedDef);

            GeneralUI.ScrollMenu(rectRecipe, DecorateRecipeButton, selectedRecipeDef.ingredients, ref _scrollPositionRecipe, buttonHeight: 22f);
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
                    Messages.Message("No selected and / or compatable worktables to make bill with.", null, MessageTypeDefOf.CautionInput, null);
                    goto Exit;
                }

                billConfig = GeneralUI.OpenDialogBillConfig(recipe, ref bill);
            Exit:;
            }

            // checks if user is done making bill before adding bill to worktable(s)
            if (billConfig != null && billConfig.IsOpen == false)
            {
                List<Building> selectedWorktablesOnMap = Find.Selector.SelectedObjectsListForReading.OfType<Building>().Where(building => building != null && building.def != null && selectedRecipeDef != null && selectedRecipeDef.AllRecipeUsers != null && recipe.AllRecipeUsers.Any(def => building.def == def)).ToList();
                foreach (Building table in selectedWorktablesOnMap)
                {
                    // attempt to cast. won't work for custom workbenches
                    try
                    {
                        ((Building_WorkTable)table).BillStack.AddBill(bill.Clone());
                    }
                    catch (InvalidCastException e)
                    {
                        Log.Message(e.Message);
                        Messages.Message($"{table.Label} : {table.thingIDNumber} is not supported. Sorry.", new LookTargets(table), MessageTypeDefOf.NegativeEvent, null); ;
                    }
                }
                Messages.Message("Bill added to selected worktable(s).", new LookTargets(selectedWorktablesOnMap), MessageTypeDefOf.PositiveEvent, null);

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

            string buttonLabel = showProducedThingName ? item.ProducedThingDef.label.CapitalizeFirst() : item.label.CapitalizeFirst();
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

        public static void FilterDefs(
            out List<RecipeDef> filteredRecipeDefs, out List<ThingCategoryDef> filteredCategoryDefs, out List<ModContentPack> filteredModPacks,
            List<RecipeDef> filterFrom, ModContentPack modFilter, ThingCategoryDef categoryFilter,
            string filterString = "", bool filterAvailable = false, bool filterByProducedThingName = false, bool filterByProducedThingMod = false)
        {
            List<RecipeDef> recipes = new List<RecipeDef>(filterFrom.Count);
            HashSet<ThingCategoryDef> categories = new HashSet<ThingCategoryDef>();
            HashSet<ModContentPack> mods = new HashSet<ModContentPack>();

            foreach (RecipeDef item in filterFrom)
            {
                bool addToRecipes = true;
                bool addToCategories = true;
                bool addToMods = true;

                // checking
                if (item == null || item.label == null || item.AllRecipeUsers == null || item.modContentPack == null || item.ProducedThingDef == null ||
                    item.ProducedThingDef.label == null || item.ProducedThingDef.modContentPack == null || item.ProducedThingDef.FirstThingCategory == null)
                    continue;

                // check category
                if (categoryFilter != null && item.ProducedThingDef.FirstThingCategory != categoryFilter)
                    addToRecipes = false;

                // check mod
                if (modFilter != null)
                {
                    if (filterByProducedThingMod && item.ProducedThingDef.modContentPack != modFilter)
                        addToRecipes = addToCategories = false;
                    if (!filterByProducedThingMod && item.modContentPack != modFilter)
                        addToRecipes = addToCategories = false;
                }

                if (filterString != "")
                {
                    if (filterByProducedThingName && item.ProducedThingDef.label.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) < 0)
                        addToRecipes = addToMods = addToCategories = false;
                    if (!filterByProducedThingName && item.label.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) < 0)
                        addToRecipes = addToMods = addToCategories = false;
                }

                // check available
                if (filterAvailable && !item.AvailableNow)
                    addToRecipes = false;

                if (addToRecipes) recipes.Add(item);
                if (addToCategories) categories.Add(item.ProducedThingDef.FirstThingCategory);
                if (addToMods) mods.Add(filterByProducedThingMod ? item.ProducedThingDef.modContentPack : item.modContentPack);
            }

            filteredRecipeDefs = recipes;
            filteredCategoryDefs = categories.ToList();
            filteredModPacks = mods.ToList();

            if (filteredCategoryDefs.Count > 1) filteredCategoryDefs.Insert(0, null);
            if (filteredModPacks.Count > 1) filteredModPacks.Insert(0, null);
        }

        public static List<RecipeDef> FilterRecipeDefs(List<RecipeDef> filterFrom, ModContentPack modFilter, ThingCategoryDef categoryFilter, string filterString = "", bool filterAvailable = false, bool filterByProducedThingName = false, bool filterByProducedThingMod = false)
        {
            // filter category
            if (categoryFilter != null)
                filterFrom = filterFrom.Where(def => def != null && def.ProducedThingDef != null && categoryFilter.childThingDefs.Any(thingdef => def.ProducedThingDef == thingdef)).ToList();

            // filter search
            if (filterString != "")
            {
                if (!filterByProducedThingName)
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
                if (!filterByProducedThingMod)
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

        public static List<ThingCategoryDef> FilterThingCategoryDefs(List<RecipeDef> filterFrom, ModContentPack modFilter, string filterSearch = "", string categoryDefSearch = "", bool filterAvailable = false, bool filterByProducedThingName = false, bool filterByProducedThingMod = false)
        {
            filterFrom = FilterRecipeDefs(filterFrom, modFilter, null, filterSearch, filterAvailable, filterByProducedThingName, filterByProducedThingMod);

            List<ThingCategoryDef> filteredCategoryList;
            filteredCategoryList = filterFrom.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().ToList();

            if (filteredCategoryList.Count > 1)
                filteredCategoryList.Insert(0, null);

            return filteredCategoryList;
        }

        public static List<ModContentPack> FilterModContentPacks(List<RecipeDef> filterFrom, string filterSearch = "", string modContentPackSearch = "", bool filterAvailable = false, bool filterByProducedThingName = false, bool filterByProducedThingMod = false)
        {
            filterFrom = FilterRecipeDefs(filterFrom, null, null, filterSearch, filterAvailable, filterByProducedThingName, filterByProducedThingMod);

            List<ModContentPack> filteredModContentPack;

            if (filterByProducedThingMod)
            {
                filteredModContentPack = filterFrom.Where(
                    def => def != null &&
                    def.ProducedThingDef != null &&
                    def.ProducedThingDef.modContentPack != null)
                    .Select(def => def.ProducedThingDef.modContentPack).Distinct().ToList();
            }
            else
            {
                filteredModContentPack = filterFrom.Where(
                    def => def != null &&
                    def != null &&
                    def.modContentPack != null)
                    .Select(def => def.modContentPack).Distinct().ToList();
            }

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
