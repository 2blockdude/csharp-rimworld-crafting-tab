using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse.Sound;
using Verse;

namespace BlockdudesTabs
{
    [StaticConstructorOnStartup]
    public class CraftingTab : MainTabWindow
    {
        // Window Settings
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

        // Thing Lists
        public List<ModMetaData> ModsList = ModLister.AllInstalledMods.Where(mods => mods.Active).ToList();
        public List<ThingCategoryDef> CategoryList = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
        public List<RecipeDef> RecipesList = DefDatabase<RecipeDef>.AllDefsListForReading;
        public List<RecipeDef> CraftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).ToList();

        public List<RecipeDef> CraftablesFilteredList = null;

        // selected things to use in filter
        public ThingCategoryDef SelectedCategory = null;
        public ModMetaData SelectedMod = null;
        public RecipeDef SelectedThingDef = null;

        //public static List<ThingDef> Craftables = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).Select(def => def.ProducedThingDef).ToList();

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
            DrawScrollTab(CraftablesList);
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

        private void DrawScrollTab<T>(Func<List<T>, Rect, bool> DrawButtons, List<T> list, ref Vector2 ScrollPosition, float PositionX, float PositionY, float SizeX, float SizeY, float ButtonHeight = 30f, float OutMargin = 5f, float Gap = 2f)
        {
            // center postition based on gaps and margins
            PositionX += Gap;
            PositionY += Gap;
            SizeX -= Gap * 2f;
            SizeY -= Gap * 2f;
            Vector2 OutRectPos = new Vector2(PositionX + OutMargin, PositionY + OutMargin);
            Vector2 OutRectSize = new Vector2(SizeX - OutMargin * 2f, SizeY - OutMargin * 2f);
            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 16f, list.Count * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(PositionX, PositionY, SizeX, SizeY);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw and decorate tab
            Widgets.DrawBox(RectMargin);
            Widgets.DrawHighlight(RectMargin);

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
                Widgets.DrawHighlightIfMouseover(Button);

                string ButtonTitle = list[i] == null ? "All" : list[i].Name;
                if (Widgets.ButtonText(Button, ButtonTitle, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedMod = list[i];
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
                Widgets.DrawHighlightIfMouseover(Button);

                string ButtonTitle = list[i] == null ? "All" : list[i].label;
                if (Widgets.ButtonText(Button, ButtonTitle, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedCategory = list[i];
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
                Widgets.DrawHighlightIfMouseover(Button);

                if (Widgets.ButtonText(Button, list[i].ProducedThingDef.label, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedThingDef = list[i];
                }
            }

            return true;
        }

        private void DrawItemDescription()
        {
        }

        private void FilterCraftables(out List<RecipeDef> FilteredList, ModMetaData mod)
        {
            FilteredList = null;
        }

        private List<RecipeDef> FindRecipe(ThingDef thing)
        {
            //return DefDatabase<RecipeDef>.AllDefs.Where(def => def.products.Select(defCount => defCount.thingDef).Contains(thing)).ToList();
            // may be faster than getting from defdatabase because we are reading from a list instead of ienum
            return RecipesList.Where(def => def.products.Select(defCount => defCount.thingDef).Contains(thing)).ToList();
        }
    }
}
