using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
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

        // wrapper function for reall drawscrolltab
        private void DrawScrollTab(List<ModMetaData> list)
        {
                        DrawScrollTab(
                            DrawModButtons,                 // custom function for drawing buttons
                            list,                           // list of course
                            ref _scrollPositionModTab,      // scroll reference
                            TabSize.x / 2f * 0f,            // posx
                            TabSize.y / 2f * 0f,            // posy
                            TabSize.x / 2f,                 // sizex
                            TabSize.y / 2f);                // sizey
        }

        private void DrawScrollTab(List<ThingCategoryDef> list)
        {
                        DrawScrollTab(
                            DrawCategoryButtons,
                            list,
                            ref _scrollPositionCategoryTab,
                            TabSize.x / 2f * 0f,            // posx
                            TabSize.y / 2f * 1f,            // posy
                            TabSize.x / 2f,                 // sizex
                            TabSize.y / 2f);                // sizey
        }

        private void DrawScrollTab(List<RecipeDef> list)
        {
                        DrawScrollTab(
                            DrawThingButtons,
                            list,
                            ref _scrollPositionThingTab,
                            TabSize.x / 2f * 1f,             // posx
                            TabSize.y / 2f * 0f + 30f,       // posy
                            TabSize.x / 2f,                  // sizex
                            TabSize.y / 3f * 2f);            // sizey
        }

        private void DrawScrollTab<T>(Func<List<T>, Rect, bool> DrawButtons, List<T> list, ref Vector2 ScrollPosition, float PositionX, float PositionY, float SizeX, float SizeY, float ButtonHeight = 30f, float InMargin = 5f, float OutMargin = 2f)
        {
            // center postition based on gaps and margins
            PositionX += OutMargin;
            PositionY += OutMargin;
            SizeX -= OutMargin * 2f;
            SizeY -= OutMargin * 2f;
            Vector2 OutRectPos = new Vector2(PositionX + InMargin, PositionY + InMargin);
            Vector2 OutRectSize = new Vector2(SizeX - InMargin * 2f, SizeY - InMargin * 2f);
            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 16f, list.Count * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(PositionX, PositionY, SizeX, SizeY);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw, decorate, and color tab
            Widgets.DrawBox(RectMargin);
            //Widgets.DrawHighlight(RectMargin);

            // begin scroll
            Widgets.BeginScrollView(RectOut, ref ScrollPosition, RectView);

            // expected custom function for drawing buttons
            DrawButtons(list, RectView);

            Widgets.EndScrollView();
        }

        private bool DrawModButtons(List<ModMetaData> list, Rect ViewRect)
        {
            float ButtonHeight = ViewRect.height / list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                Rect Button = new Rect(0f, i * ButtonHeight, ViewRect.width, ButtonHeight);

                // draw and decorate button
                if (SelectedMod == list[i]) Widgets.DrawHighlight(Button);
                Widgets.DrawHighlightIfMouseover(Button);

                string ButtonTitle = list[i] == null ? "All" : list[i].Name;
                if (Widgets.ButtonText(Button, ButtonTitle, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedMod = list[i];
                    FilterCraftables();
                }
            }

            return true;
        }

        private bool DrawCategoryButtons(List<ThingCategoryDef> list, Rect ViewRect)
        {
            float ButtonHeight = ViewRect.height / list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                Rect Button = new Rect(0f, i * ButtonHeight, ViewRect.width, ButtonHeight);

                // draw and decorate button
                if (SelectedCategory == list[i]) Widgets.DrawHighlight(Button);
                Widgets.DrawHighlightIfMouseover(Button);

                string ButtonTitle = list[i] == null ? "All" : list[i].label;
                if (Widgets.ButtonText(Button, ButtonTitle, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedCategory = list[i];
                    FilterCraftables();
                }
            }

            return true;
        }

        private bool DrawThingButtons(List<RecipeDef> list, Rect ViewRect)
        {
            float ButtonHeight = ViewRect.height / list.Count;

            for (int i = 0; i < list.Count; i++)
            {
                Rect Button = new Rect(0f, i * ButtonHeight, ViewRect.width, ButtonHeight);

                // draw and decorate button
                if (SelectedThingDef == list[i]) Widgets.DrawHighlight(Button);
                Widgets.DrawHighlightIfMouseover(Button);

                if (Widgets.ButtonText(Button, list[i].ProducedThingDef.label, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedThingDef = list[i];
                }
            }

            return true;
        }

        private void DrawSearchBar()
        {
            // constants
            float PositionX = TabSize.x / 2f;
            float PositionY = 0f;
            float SizeX = TabSize.x / 2;
            float SizeY = 30f;

            float OutMargin = 2f;

            // center postition based on gaps and margins
            PositionX += OutMargin;
            PositionY += OutMargin;
            SizeX -= OutMargin * 2f;
            SizeY -= OutMargin * 2f;

            // scroll rects
            Rect TextBox = new Rect(PositionX + SizeY, PositionY, SizeX - SizeY * 2, SizeY);
            Rect ButtonClear = new Rect(TextBox.x + TextBox.width, PositionY, SizeY, SizeY);
            Rect SearchIcon = new Rect(TextBox.x - SizeY, PositionY, SizeY, SizeY);

            Widgets.DrawTextureFitted(SearchIcon, TexButton.Search, 1f);

            if (Widgets.ButtonImage(ButtonClear, TexButton.DeleteX, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
            {
                SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                SearchString = "";
                FilterCraftables();
            }

            // need to give the textbox a name inorder to do the loose focus thing below
            string TextBoxName = "search";
            GUI.SetNextControlName(TextBoxName);

            // draw textbox and make sure to store the string entered in the box
            SearchString = Widgets.TextField(TextBox, SearchString);

            bool Focused = GUI.GetNameOfFocusedControl() == TextBoxName;

            // loose focus so the filter does not always run
			if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(TextBox) && Focused)
				GUI.FocusControl(null);

            // only filter if focused and the user presses a keyboard key are typing
            if (Focused && Event.current.isKey)
                FilterCraftables();
        }

        private void DrawItemDescription()
        {
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
