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

        // selected worktables to add bill to
        public ThingDef selectedWorktableType = null;
        public Building selectedWorktable = null;

        public MainTabWindow_CraftingMenu()
        {
            base.draggable = false;
            base.resizeable = false;

            // generate lists
            craftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def != null && def.ProducedThingDef != null && def.AllRecipeUsers != null && def.AllRecipeUsers.Count() > 0).ToList();

            // note: I insert a null value into list to indicate a selection of all items in list
            modsList = new List<ModContentPack>();
            modsList.Insert(0, null);
            modsList.InsertRange(1, craftablesList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.modContentPack != null).Select(def => def.ProducedThingDef.modContentPack).Distinct());
            // remove all indicator if there is only one mod
            if (modsList.Count <= 2)
                modsList.RemoveAt(0);

            categoryList = new List<ThingCategoryDef>();
            categoryList.Insert(0, null);
            categoryList.InsertRange(1, craftablesList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.FirstThingCategory != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct());

            categoryFilteredList = categoryList;
            craftablesFilteredList = craftablesList;
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

            DoCraftablesTab(new Rect(
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
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
                categoryFilteredList = FilterThingCategoryDefs(categoryList, selectedMod, craftablesList, searchString, "");
            }
        }

        public void DoModsTab(Rect rectTab)
        {
            rectTab = GeneralUI.LabelColorAndOutLine(rectTab, "Mods", Color.gray, TextAnchor.UpperCenter, inMargin);

            int selected = GeneralUI.ScrollMenu(rectTab, DecorateModButtons, modsList, ref _scrollPositionModTab);
            if (selected > -1)
            {
                ModContentPack item = modsList[selected];
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedMod = item;
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
                categoryFilteredList = FilterThingCategoryDefs(categoryList, selectedMod, craftablesList, searchString, "");
            }
        }

        public void DoCategoriesTab(Rect rectTab)
        {
            rectTab = GeneralUI.LabelColorAndOutLine(rectTab, "Categories", Color.gray, TextAnchor.UpperCenter, inMargin);

            int selected = GeneralUI.ScrollMenu(rectTab, DecorateCategoryButtons, categoryFilteredList, ref _scrollPositionCategoryTab);
            if (selected > -1)
            {
                ThingCategoryDef item = categoryFilteredList[selected];
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCategory = item;
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
            }
        }

        public void DoCraftablesTab(Rect rectTab)
        {
            rectTab = GeneralUI.LabelColorAndOutLine(rectTab, "Items", Color.gray, TextAnchor.UpperCenter, inMargin);

            int selected = GeneralUI.ScrollMenu(rectTab, DecorateCraftablesButtons, craftablesFilteredList, ref _scrollPositionThingTab);
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

        public void DoItemDescription(Rect rectTab)
        {
            GUI.color = Color.gray;
            Widgets.DrawBox(rectTab);
            GUI.color = Color.white;
            Widgets.DrawAltRect(rectTab);

            rectTab = rectTab.ContractedBy(inMargin);

            if (selectedCraftable == null) return;

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
            Widgets.Label(rectThingLabel, selectedCraftable.ProducedThingDef.label.CapitalizeFirst());
            Text.Font = GameFont.Tiny;
            Widgets.Label(rectModLabel, "Source: " + (selectedCraftable.ProducedThingDef == null || selectedCraftable.ProducedThingDef.modContentPack == null || selectedCraftable.ProducedThingDef.modContentPack.Name == null ? "None" : selectedCraftable.ProducedThingDef.modContentPack.Name));
            Text.Font = GameFont.Small;

            Widgets.LabelScrollable(rectDescription, selectedCraftable.ProducedThingDef.description, ref _scrollPositionDescription);
            GeneralUI.ScrollMenu(rectRecipe, DecorateRecipeButtons, selectedCraftable.ingredients, ref _scrollPositionRecipe, buttonHeight: 22f);
            Decription_WorktableButtons(rectWorktables);
            Widgets.InfoCardButton(rectInfo, selectedCraftable.ProducedThingDef);
            Description_MakeBillButton(rectCraft, selectedCraftable);
        }
        // ------------------------
        // end of menu ui functions

        // start of description helper functions
        // -------------------------------------
        private void Decription_WorktableButtons(Rect rectView)
        {
            List<ThingDef> tableDefs = selectedCraftable.AllRecipeUsers.Distinct().ToList();
            int selected = GeneralUI.ScrollMenu(rectView, DecorateWorktableButtons, tableDefs, ref _scrollPositionWorkBenches);
            if (selected > -1)
            {
                selectedWorktableType = tableDefs[selected];

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
                        Action select = delegate()
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
            (Bill_Production bill, GeneralUI.EventCode eventVal) buttonState = GeneralUI.MakeBillButton(button, recipe, selectedWorktableType);

            Bill_Production bill = buttonState.bill;
            GeneralUI.EventCode eventVal = buttonState.eventVal;

            switch (eventVal)
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
        private void DecorateModButtons(ModContentPack item, Rect button)
        {
            // draw and decorate button
            if (selectedMod == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.Name;
            Widgets.Label(button, buttonTitle);
        }

        private void DecorateCategoryButtons(ThingCategoryDef item, Rect button)
        {
            // draw and decorate button
            if (selectedCategory == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.label.CapitalizeFirst();
            Widgets.Label(button, buttonTitle);
        }

        private void DecorateCraftablesButtons(RecipeDef item, Rect button)
        {
            // draw and decorate button
            if (selectedCraftable == item) Widgets.DrawHighlight(button);
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

        private void DecorateWorktableButtons(ThingDef item, Rect button)
        {
            if (selectedWorktableType == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);
            Widgets.Label(button, item.label.CapitalizeFirst() + (selectedWorktable != null && selectedWorktable.def == item? ": " + selectedWorktable.thingIDNumber : ""));
        }

        private void DecorateRecipeButtons(IngredientCount item, Rect button)
        {
            Widgets.Label(button, item.Summary);
        }
        // ------------------------
        // end of button decoration

        // start of helper funcions
        // ------------------------
        public static List<RecipeDef> FilterRecipeDefs(List<RecipeDef> filterFromRecipes, ModContentPack modFilter, ThingCategoryDef categoryFilter, string labelFilter)
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
                filteredList = filteredList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.modContentPack != null && def.ProducedThingDef.modContentPack == modFilter).ToList();

            return filteredList;
        }

        public static List<ThingCategoryDef> FilterThingCategoryDefs(List<ThingCategoryDef> filterFromCategories, ModContentPack modFilter, List<RecipeDef> whiteList, string thingDefSearch, string categoryDefSearch)
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
                    whiteList = whiteList.Where(def => def != null && def.ProducedThingDef != null && def.ProducedThingDef.modContentPack != null && def.ProducedThingDef.modContentPack == modFilter).ToList();

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
