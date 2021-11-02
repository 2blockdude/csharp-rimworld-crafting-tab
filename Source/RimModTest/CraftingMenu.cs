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
    public class CraftingMenu : MainTabWindow
    {
        // Tab Settings
        public static MainTabWindow _instance { get; private set; }
        public override Vector2 RequestedTabSize => new Vector2(700f, 700f);
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;
        protected override float Margin => 5f;

        public static float outMargin = 2f;
        public static float inMargin = 5f;

        // true tab size after margins
        public Vector2 tabSize;

        // keeps track of scroll
        internal static Vector2 _scrollPositionCategoryTab = Vector2.zero;
        internal static Vector2 _scrollPositionThingTab = Vector2.zero;
        internal static Vector2 _scrollPositionModTab = Vector2.zero;
        internal static Vector2 _scrollPositionRecipe = Vector2.zero;
        internal static Vector2 _scrollPositionDescription = Vector2.zero;
        internal static Vector2 _scrollPositionWorkBenches = Vector2.zero;

        // Lists
        public static List<ModMetaData> modsList = DefDatabase<RecipeDef>.AllDefsListForReading.Where(def => def.ProducedThingDef != null).Select(def => def.modContentPack.ModMetaData).Distinct().ToList();
        public static List<ThingCategoryDef> categoryList = DefDatabase<RecipeDef>.AllDefsListForReading.Where(def => def.ProducedThingDef != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().ToList();
        public static List<RecipeDef> craftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).ToList();

        public List<ThingCategoryDef> categoryFilteredList = categoryList;
        public List<RecipeDef> craftablesFilteredList = craftablesList;

        // selected things to use in filter
        public ThingCategoryDef selectedCategory = null;
        public ModMetaData selectedMod = null;
        public RecipeDef selectedCraftable = null;
        public string searchString = "";

        public ThingDef selectedWorktableType = null;
        public Building_WorkTable selectedWorktable = null;

        public CraftingMenu()
        {
            base.draggable = false;
            base.resizeable = false;
            _instance = this;

            // init lists note: find a better way to include null
            modsList.Insert(0, null);
            categoryList.Insert(0, null);
        }

        public override void DoWindowContents(Rect inRect)
        {
            tabSize.x = windowRect.width - Margin * 2f;
            tabSize.y = windowRect.height - Margin * 2f;
            Text.Font = GameFont.Small;

            DrawSearchBar();
            DrawModsTab();
            DrawCategoriesTab();
            DrawCraftablesTab();
            DrawItemDescription();
        }

        private void DrawSearchBar()
        {
            Rect searchBox = new Rect(
                tabSize.x / 2f,
                0f,
                tabSize.x / 2f,
                30f);

            searchBox = searchBox.ContractedBy(outMargin);

            if (GeneralUI.DrawSearchBar(searchBox, ref searchString))
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
        }

        private void DrawModsTab()
        {
            // top third left
            Rect rectTab = new Rect(
                tabSize.x / 2f * 0f,            // posx
                tabSize.y / 2f * 0f,            // posy
                tabSize.x / 2f,                 // sizex
                tabSize.y / 3f);                // sizey

            Widgets.DrawBox(rectTab.ContractedBy(outMargin));
            rectTab = rectTab.ContractedBy(outMargin + inMargin);

            GeneralUI.DrawScrollTab(rectTab, DrawModButtons, modsList, ref _scrollPositionModTab);
        }

        private void DrawCategoriesTab()
        {
            Rect rectTab = new Rect(
                tabSize.x / 2f * 0f,            // posx
                tabSize.y / 3f * 1f,            // posy
                tabSize.x / 2f,                 // sizex
                tabSize.y / 3f);                // sizey

            Widgets.DrawBox(rectTab.ContractedBy(outMargin));
            rectTab = rectTab.ContractedBy(outMargin + inMargin);

            GeneralUI.DrawScrollTab(rectTab, DrawCategoryButtons, categoryFilteredList, ref _scrollPositionCategoryTab);
        }

        private void DrawCraftablesTab()
        {
            Rect rectTab = new Rect(
                tabSize.x / 2f * 1f,             // posx
                tabSize.y / 2f * 0f + 30f,       // posy
                tabSize.x / 2f,                  // sizex
                tabSize.y / 3f * 2f - 30f);            // sizey

            Widgets.DrawBox(rectTab.ContractedBy(outMargin));
            rectTab = rectTab.ContractedBy(outMargin + inMargin);

            GeneralUI.DrawScrollTab(rectTab, DrawCraftablesButtons, craftablesFilteredList, ref _scrollPositionThingTab);
        }

        private void DrawItemDescription()
        {
            Rect rectGroup = new Rect(
                tabSize.x / 3f * 0f,
                tabSize.y / 3f * 2f,
                tabSize.x,
                tabSize.y / 3f);

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
            GeneralUI.DrawScrollTab(rectWorktables, DrawWorktableButtons, selectedCraftable.AllRecipeUsers.Distinct().ToList(), ref _scrollPositionWorkBenches);
            Widgets.InfoCardButton(rectInfo, selectedCraftable.ProducedThingDef);
            List<Building_WorkTable> worktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
            GeneralUI.DrawMakeBillButton(rectCraft, selectedCraftable, worktablesOnMap);
        }

        private void DrawModButtons(ModMetaData item, Rect button)
        {
            // draw and decorate button
            if (selectedMod == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.Name;
            if (Widgets.ButtonText(button, buttonTitle, false))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedMod = item;
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
                categoryFilteredList = FilterThingCategoryDefs(categoryList, selectedMod);
            }
        }

        private void DrawCategoryButtons(ThingCategoryDef item, Rect button)
        {
            // draw and decorate button
            if (selectedCategory == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            string buttonTitle = item == null ? "All" : item.label;
            if (Widgets.ButtonText(button, buttonTitle, false))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCategory = item;
                craftablesFilteredList = FilterRecipeDefs(craftablesList, selectedMod, selectedCategory, searchString);
            }
        }

        private void DrawCraftablesButtons(RecipeDef item, Rect button)
        {
            // draw and decorate button
            if (selectedCraftable == item) Widgets.DrawHighlight(button);
            Widgets.DrawHighlightIfMouseover(button);

            TooltipHandler.TipRegion(button, new TipSignal(item.ProducedThingDef.description));
            if (Widgets.ButtonText(button, item.ProducedThingDef.label, false))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                selectedCraftable = item;
            }
        }

        private void DrawWorktableButtons(ThingDef item, Rect button)
        {
            Widgets.Label(button, item.label);
        }

        private void DrawRecipeButtons(IngredientCount item, Rect button)
        {
            Widgets.Label(button, item.Summary);
        }


        private List<RecipeDef> FilterRecipeDefs(List<RecipeDef> filterFrom, ModMetaData modFilter, ThingCategoryDef categoryFilter, string labelFilter)
        {
            List<RecipeDef> filteredList = filterFrom;

            // filter mod
            if (modFilter != null)
                filteredList = filteredList.Where(def => def.modContentPack.ModMetaData == modFilter).ToList();

            // filter category
            if (categoryFilter != null)
                filteredList = filteredList.Where(def => def.ProducedThingDef.FirstThingCategory == categoryFilter).ToList();

            // filter search
            if (labelFilter != "")
                filteredList = filteredList.Where(def => def.ProducedThingDef.label.IndexOf(labelFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            return filteredList;
        }

        private List<ThingCategoryDef> FilterThingCategoryDefs(List<ThingCategoryDef> filterFrom, ModMetaData modFilter, string labelFilter = "")
        {
            List<ThingCategoryDef> filteredList = filterFrom;

            if (modFilter != null)
            {
                filteredList = filteredList.Where(def => def != null && def.childThingDefs.Select(thingdef => thingdef.modContentPack.ModMetaData).Contains(modFilter)).ToList();
                filteredList.Insert(0, null);
            }

            return filteredList;
        }
    }
}
