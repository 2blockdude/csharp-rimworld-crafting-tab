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

            // init lists note: find a better way to include null
            ModsList.Insert(0, null);
            CategoryList.Insert(0, null);
        }

        public override void DoWindowContents(Rect inRect)
        {
            TabSize.x = windowRect.width - Margin * 2f;
            TabSize.y = windowRect.height - Margin * 2f;
            Text.Font = GameFont.Small;

            DrawModsTab();
            DrawCategoriesTab();
            DrawCraftablesTab();
            DrawSearchBar();
            DrawItemDescription();
        }

        // wrapper function for real drawscrolltab
        private void DrawModsTab()
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
                ModsList,                           // list of course
                ref _scrollPositionModTab,      // scroll reference
                RectTab);
        }

        private void DrawCategoriesTab()
        {
            Rect RectTab = new Rect(
                TabSize.x / 2f * 0f,            // posx
                TabSize.y / 3f * 1f,            // posy
                TabSize.x / 2f,                 // sizex
                TabSize.y / 3f);                // sizey

            TabUI.CreateMargins(ref RectTab, 2f, 5f, true);

            TabUI.DrawScrollTab(
                DrawCategoryButtons,
                CategoryList,
                ref _scrollPositionCategoryTab,
                RectTab);
        }

        private void DrawCraftablesTab()
        {
            Rect RectTab = new Rect(
                TabSize.x / 2f * 1f,             // posx
                TabSize.y / 2f * 0f + 30f,       // posy
                TabSize.x / 2f,                  // sizex
                TabSize.y / 3f * 2f - 30f);            // sizey

            TabUI.CreateMargins(ref RectTab, 2f, 5f, true);

            TabUI.DrawScrollTab(
                DrawThingButtons,
                CraftablesList,
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

            if (TabUI.DrawSearchBar(ref SearchString, SearchBar))
                CraftablesFilteredList = FilterRecipeDefs(CraftablesList, SelectedMod, SelectedCategory, SearchString);
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
                CraftablesFilteredList = FilterRecipeDefs(CraftablesList, SelectedMod, SelectedCategory, SearchString);
                CategoryFilteredList = FilterThingCategoryDefs(CategoryList, SelectedMod);
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
                CraftablesFilteredList = FilterRecipeDefs(CraftablesList, SelectedMod, SelectedCategory, SearchString);
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
                TabUI.DrawScrollTab(DrawRecipeButtons, SelectedThingDef.ingredients, ref _scrollPositionRecipe, RectRecipe, ButtonHeight: 22f);
                TabUI.DrawScrollTab(DrawWorkBenchesButtons, SelectedThingDef.AllRecipeUsers.Distinct().ToList(), ref _scrollPositionWorkBenches, RectWorkBenches);
                Widgets.InfoCardButton(RectInfo, SelectedThingDef.ProducedThingDef);
                //if (Widgets.ButtonImage(RectInfo, TexButton.Info, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
                    //Find.WindowStack.Add(new Dialog_InfoCard(SelectedThingDef, null));
                    //new Dialog_InfoCard.Hyperlink(SelectedThingDef.ProducedThingDef, -1).ActivateHyperlink();

                if (Widgets.ButtonText(RectCraft, "Craft"))
                {
                    List<Building_WorkTable> WorktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();

                    //bill.DoInterface(0f, 0f, 200, _instance.ID);
                    Bill_Production bill = new Bill_Production(SelectedThingDef, null);
                    Find.WindowStack.Add(new Dialog_BillConfig(bill, WorktablesOnMap[0].BillInteractionCell));
                    WorktablesOnMap[0].BillStack.AddBill(bill);

                    BillUtility.Clipboard = (Bill_Production)bill.Clone();
                    foreach (Building_WorkTable table in WorktablesOnMap)
                    {
                        table.billStack.AddBill(WorktablesOnMap[0].billStack.Bills[0]);
                        //BillUtility.
                    }
                }

            }
        }


        private List<RecipeDef> FilterRecipeDefs(List<RecipeDef> FilterFrom, ModMetaData ModFilter, ThingCategoryDef CategoryFilter, string LabelFilter)
        {
            List<RecipeDef> FilteredList = FilterFrom;

            // filter mod
            if (ModFilter != null)
                FilteredList = FilteredList.Where(def => def.modContentPack.ModMetaData == ModFilter).ToList();

            // filter category
            if (CategoryFilter != null)
                FilteredList = FilteredList.Where(def => def.ProducedThingDef.FirstThingCategory == CategoryFilter).ToList();

            // filter search
            if (LabelFilter != "")
                FilteredList = FilteredList.Where(def => def.ProducedThingDef.label.IndexOf(LabelFilter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            return FilteredList;
        }

        private List<ThingCategoryDef> FilterThingCategoryDefs(List<ThingCategoryDef> FilterFrom, ModMetaData ModFilter, string LabelFilter = "")
        {
            List<ThingCategoryDef> FilteredList = FilterFrom;

            if (ModFilter != null)
            {
                FilteredList = FilteredList.Where(def => def != null && def.childThingDefs.Select(thingdef => thingdef.modContentPack.ModMetaData).Contains(ModFilter)).ToList();
                FilteredList.Insert(0, null);
            }

            return FilteredList;
        }
    }
}
