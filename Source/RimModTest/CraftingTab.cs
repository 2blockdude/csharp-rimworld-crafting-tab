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
    public class CraftingTab : MainTabWindow
    {
        // Tab Settings
        public static MainTabWindow _instance { get; private set; }
        public override Vector2 RequestedTabSize => new Vector2(1000f, 700f);
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;
        protected override float Margin => 5f;

        // true tab size after margins
        public static Vector2 TabSize;

        // keeps track of scroll
        internal static Vector2 _scrollPositionCategoryTab = Vector2.zero;
        internal static Vector2 _scrollPositionThingTab = Vector2.zero;
        internal static Vector2 _scrollPositionModTab = Vector2.zero;
        internal static Vector2 _scrollPositionRecipe = Vector2.zero;
        internal static Vector2 _scrollPositionDescription = Vector2.zero;
        internal static Vector2 _scrollPositionWorkBenches = Vector2.zero;

        // Lists
        public static List<ModMetaData> ModsList = ModLister.AllInstalledMods.Where(mods => mods.Active).ToList();
        public static List<ThingCategoryDef> CategoryList = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
        public static List<RecipeDef> RecipesList = DefDatabase<RecipeDef>.AllDefsListForReading;
        public static List<RecipeDef> CraftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).ToList();

        public List<RecipeDef> CraftablesFilteredList = CraftablesList;

        // selected things to use in filter
        public ThingCategoryDef SelectedCategory = null;
        public ModMetaData SelectedMod = null;
        public RecipeDef SelectedThingDef = null;
        public string SearchString = "";

        public CraftingTab()
        {
            base.draggable = false;
            base.resizeable = false;
            _instance = this;

            // gives me the tab size after margin
            TabSize = new Vector2(RequestedTabSize.x - Margin * 2f, RequestedTabSize.y - Margin * 2f);
            ModsList.Insert(0, null);
            CategoryList.Insert(0, null);
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawScrollTab(ModsList);
            DrawScrollTab(CategoryList);
            DrawScrollTab(CraftablesFilteredList);
            DrawSearchBar();
            DrawItemDescription();
        }

        // wrapper function for real drawscrolltab
        private void DrawScrollTab(List<ModMetaData> list)
        {
            Rect RectTab = new Rect(
                TabSize.x / 2f * 0f,            // posx
                TabSize.y / 2f * 0f,            // posy
                TabSize.x / 2f,                 // sizex
                TabSize.y / 2f);                // sizey

            TabUI.CreateMargins(ref RectTab, 2f, 5f, true);

            TabUI.DrawScrollTab(
                DrawModButtons,                 // custom function for drawing buttons
                list,                           // list of course
                ref _scrollPositionModTab,      // scroll reference
                RectTab);
        }

        private void DrawScrollTab(List<ThingCategoryDef> list)
        {
            Rect RectTab = new Rect(
                TabSize.x / 2f * 0f,            // posx
                TabSize.y / 2f * 1f,            // posy
                TabSize.x / 2f,                 // sizex
                TabSize.y / 2f);                // sizey

            TabUI.CreateMargins(ref RectTab, 2f, 5f, true);

            TabUI.DrawScrollTab(
                DrawCategoryButtons,
                list,
                ref _scrollPositionCategoryTab,
                RectTab);
        }

        private void DrawScrollTab(List<RecipeDef> list)
        {
            Rect RectTab = new Rect(
                TabSize.x / 2f * 1f,             // posx
                TabSize.y / 2f * 0f + 30f,       // posy
                TabSize.x / 2f,                  // sizex
                TabSize.y / 3f * 2f);            // sizey

            TabUI.CreateMargins(ref RectTab, 2f, 5f, true);

            TabUI.DrawScrollTab(
                DrawThingButtons,
                list,
                ref _scrollPositionThingTab,
                RectTab);
        }

        private void DrawSearchBar()
        {
            Rect SearchBar = new Rect(
                TabSize.x / 2f,
                0f,
                TabSize.x / 2f,
                30f);

            TabUI.CreateMargins(ref SearchBar, 2f, 0f, false);

            TabUI.DrawSearchBar(
                FilterCraftables,
                ref SearchString,
                SearchBar);
        }

        private void DrawModButtons(ModMetaData Item, Rect Button)
        {
            // draw and decorate button
            if (SelectedMod == Item) Widgets.DrawHighlight(Button);
            Widgets.DrawHighlightIfMouseover(Button);

            string ButtonTitle = Item == null ? "All" : Item.Name;
            if (Widgets.ButtonText(Button, ButtonTitle, false))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                SelectedMod = Item;
                FilterCraftables();
            }
        }

        private void DrawCategoryButtons(ThingCategoryDef Item, Rect Button)
        {
            // draw and decorate button
            if (SelectedCategory == Item) Widgets.DrawHighlight(Button);
            Widgets.DrawHighlightIfMouseover(Button);

            string ButtonTitle = Item == null ? "All" : Item.label;
            if (Widgets.ButtonText(Button, ButtonTitle, false))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                SelectedCategory = Item;
                FilterCraftables();
            }
        }

        private void DrawThingButtons(RecipeDef Item, Rect Button)
        {
            // draw and decorate button
            if (SelectedThingDef == Item) Widgets.DrawHighlight(Button);
            Widgets.DrawHighlightIfMouseover(Button);

            TooltipHandler.TipRegion(Button, new TipSignal(Item.ProducedThingDef.description));
            if (Widgets.ButtonText(Button, Item.ProducedThingDef.label, false))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                SelectedThingDef = Item;
            }
        }

        private void DrawWorkBenchesButtons(ThingDef Item, Rect Button)
        {
            Widgets.Label(Button, Item.label);
        }

        private void DrawRecipeButtons(IngredientCount Item, Rect Button)
        {
            Widgets.Label(Button, Item.Summary);
        }

        private void DrawItemDescription()
        {
            Rect RectOut = new Rect(
                TabSize.x / 2f,
                TabSize.y / 3f * 2f + 30f,
                TabSize.x / 2f,
                TabSize.y / 3f - 30f);

            TabUI.CreateMargins(ref RectOut, 2f, 5f, true);

            if (SelectedThingDef != null)
            {
                Rect ThingLabel = new Rect(
                    RectOut.x,
                    RectOut.y,
                    RectOut.width / 2f,
                    RectOut.height / 3f);

                Rect RectDescption = new Rect(
                    RectOut.x,
                    RectOut.y + (RectOut.height / 3f),
                    RectOut.width / 3f,
                    RectOut.height / 3f * 2f);

                Rect RectRecipe = new Rect(
                    RectOut.x + RectOut.width / 3f,
                    RectOut.y + RectOut.height / 3f,
                    RectOut.width / 3f,
                    RectOut.height / 3f * 2f);

                Rect RectWorkBenches = new Rect(
                    RectOut.x + RectOut.width / 3f * 2f,
                    RectOut.y + RectOut.height / 3f,
                    RectOut.width / 3f,
                    RectOut.height / 3f * 2f);

                TabUI.CreateMargins(ref RectDescption, 0f, 5f, true);
                TabUI.CreateMargins(ref RectRecipe, 0f, 5f, true);
                TabUI.CreateMargins(ref RectWorkBenches, 0f, 5f, true);

                Widgets.Label(ThingLabel, SelectedThingDef.ProducedThingDef.label);
                Widgets.LabelScrollable(RectDescption, SelectedThingDef.ProducedThingDef.description, ref _scrollPositionDescription);
                TabUI.DrawScrollTab(DrawRecipeButtons, SelectedThingDef.ingredients, ref _scrollPositionRecipe, RectRecipe);
                TabUI.DrawScrollTab(DrawWorkBenchesButtons, SelectedThingDef.AllRecipeUsers.ToList(), ref _scrollPositionWorkBenches, RectWorkBenches);
            }
        }


        private void FilterCraftables()
        {
            // reset
            CraftablesFilteredList = CraftablesList;

            // filter mod
            if (SelectedMod != null)
                CraftablesFilteredList = CraftablesFilteredList.Where(def => def.modContentPack.ModMetaData == SelectedMod).ToList();

            // filter category
            if (SelectedCategory != null)
                CraftablesFilteredList = CraftablesFilteredList.Where(def => def.ProducedThingDef.FirstThingCategory == SelectedCategory).ToList();

            // filter search
            if (SearchString != "")
                CraftablesFilteredList = CraftablesFilteredList.Where(def => def.ProducedThingDef.label.IndexOf(SearchString, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        private List<RecipeDef> FindRecipe(ThingDef thing)
        {
            return RecipesList.Where(def => def.products.Select(defCount => defCount.thingDef).Contains(thing)).ToList();
        }
    }
}
