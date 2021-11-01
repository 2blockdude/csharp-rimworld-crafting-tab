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
        public override Vector2 RequestedTabSize => new Vector2(700f, 700f);
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;
        protected override float Margin => 5f;

        // true tab size after margins
        public Vector2 TabSize;

        // keeps track of scroll
        internal static Vector2 _scrollPositionCategoryTab = Vector2.zero;
        internal static Vector2 _scrollPositionThingTab = Vector2.zero;
        internal static Vector2 _scrollPositionModTab = Vector2.zero;
        internal static Vector2 _scrollPositionRecipe = Vector2.zero;
        internal static Vector2 _scrollPositionDescription = Vector2.zero;
        internal static Vector2 _scrollPositionWorkBenches = Vector2.zero;

        // Lists
        public static List<ModMetaData> ModsList = DefDatabase<RecipeDef>.AllDefsListForReading.Where(def => def.ProducedThingDef != null).Select(def => def.modContentPack.ModMetaData).Distinct().ToList();
        public static List<ThingCategoryDef> CategoryList = DefDatabase<RecipeDef>.AllDefsListForReading.Where(def => def.ProducedThingDef != null).Select(def => def.ProducedThingDef.FirstThingCategory).Distinct().ToList();
        public static List<RecipeDef> CraftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).ToList();

        public List<ThingCategoryDef> CategoryFilteredList = CategoryList;
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

            // init lists note: need to find a better way to generate lists
            ModsList.Insert(0, null);
            CategoryList.Insert(0, null);
        }

        public override void DoWindowContents(Rect inRect)
        {
            TabSize.x = windowRect.width - Margin * 2f;
            TabSize.y = windowRect.height - Margin * 2f;
            Text.Font = GameFont.Small;

            DrawScrollTab(ModsList);
            DrawScrollTab(CategoryFilteredList);
            DrawScrollTab(CraftablesFilteredList);
            DrawSearchBar();
            DrawItemDescription();
        }

        // wrapper function for real drawscrolltab
        private void DrawScrollTab(List<ModMetaData> list)
        {
            // top third left
            Rect RectTab = new Rect(
                TabSize.x / 2f * 0f,            // posx
                TabSize.y / 2f * 0f,            // posy
                TabSize.x / 2f,                 // sizex
                TabSize.y / 3f);                // sizey

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
                TabSize.y / 3f * 1f,            // posy
                TabSize.x / 2f,                 // sizex
                TabSize.y / 3f);                // sizey

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
                TabSize.y / 3f * 2f - 30f);            // sizey

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
                FilterCategories();
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
                TabSize.x / 3f * 0f,
                TabSize.y / 3f * 2f,
                TabSize.x,
                TabSize.y / 3f);

            TabUI.CreateMargins(ref RectOut, 2f, 5f, true);

            if (SelectedThingDef != null)
            {
                Rect ThingLabel = new Rect(
                    RectOut.x,
                    RectOut.y,
                    200f,
                    30f);

                Rect RecipeReqLabel = new Rect(
                    ThingLabel.x,
                    ThingLabel.y + ThingLabel.height,
                    300f,
                    20f);

                Rect ModLabel = new Rect(
                    RecipeReqLabel.x,
                    RecipeReqLabel.y + RecipeReqLabel.height,
                    200f,
                    20f);

                Rect RectDescription = new Rect(
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

                Rect RectInfo = new Rect(
                    RectOut.x + RectOut.width - 30f,
                    RectOut.y,
                    30f,
                    30f);

                Rect RectCraft = new Rect(
                    RectInfo.x - 100f,
                    RectInfo.y,
                    100f,
                    30f);

                TabUI.CreateMargins(ref RectDescription, 0f, 5f, true);
                TabUI.CreateMargins(ref RectRecipe, 0f, 5f, true);
                TabUI.CreateMargins(ref RectWorkBenches, 0f, 5f, true);
                TabUI.CreateMargins(ref RectInfo, 0f, 2f, false);

                Text.Font = GameFont.Medium;
                Widgets.Label(ThingLabel, SelectedThingDef.ProducedThingDef.label);
                Text.Font = GameFont.Tiny;
                Widgets.Label(RecipeReqLabel, "Required Research: " + (SelectedThingDef.researchPrerequisite == null ? "None" : SelectedThingDef.researchPrerequisite.label));
                Widgets.Label(ModLabel, "Mod Source: " + SelectedThingDef.modContentPack.ModMetaData.Name);
                Text.Font = GameFont.Small;

                Widgets.LabelScrollable(RectDescription, SelectedThingDef.ProducedThingDef.description, ref _scrollPositionDescription);
                TabUI.DrawScrollTab(DrawRecipeButtons, SelectedThingDef.ingredients, ref _scrollPositionRecipe, RectRecipe, ButtonHeight: 23f);
                TabUI.DrawScrollTab(DrawWorkBenchesButtons, SelectedThingDef.AllRecipeUsers.Distinct().ToList(), ref _scrollPositionWorkBenches, RectWorkBenches);

                if (Widgets.ButtonImage(RectInfo, TexButton.Info, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
                    new Dialog_InfoCard.Hyperlink(SelectedThingDef.ProducedThingDef, -1).ActivateHyperlink();

                if (Widgets.ButtonText(RectCraft, "Craft"))
                {
                    Dialog_BillConfig s = new Dialog_BillConfig(new Bill_Production(SelectedThingDef), new IntVec3());
                    Find.WindowStack.Add(s);
                }

            }
        }


        private void FilterCraftables()
        {
            // reset
            CraftablesFilteredList = CraftablesList;
            //_scrollPositionThingTab = Vector2.zero;

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

        private void FilterCategories()
        {
            CategoryFilteredList = CategoryList;

            if (SelectedMod != null)
            {
                CategoryFilteredList = CategoryFilteredList.Where(def => def != null && def.childThingDefs.Select(thingdef => thingdef.modContentPack.ModMetaData).Contains(SelectedMod)).ToList();
                CategoryFilteredList.Insert(0, null);
            }
        }
    }
}
