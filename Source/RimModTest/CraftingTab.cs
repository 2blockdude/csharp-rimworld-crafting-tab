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

        // keeps track of scroll
        internal static Vector2 _scrollPositionCategoryTab = Vector2.zero;
        internal static Vector2 _scrollPositionThingTab = Vector2.zero;
        internal static Vector2 _scrollPositionModTab = Vector2.zero;

        // Thing Lists
        public static List<ModMetaData> ModsList = ModLister.AllInstalledMods.Where(mods => mods.Active).ToList();
        public static List<RecipeDef> RecipesList = DefDatabase<RecipeDef>.AllDefsListForReading;
        public static List<RecipeDef> CraftablesList = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).Where(def => !CraftablesList.Contains(def)).ToList();
        public static List<RecipeDef> CraftablesFilteredList = null;
        //public static List<ThingDef> Craftables = DefDatabase<RecipeDef>.AllDefs.Where(def => def.ProducedThingDef != null).Select(def => def.ProducedThingDef).ToList();
        public ThingDef SelectedThingDef = null;
        public ModMetaData SelectedMod = null;

        public CraftingTab()
        {
            base.draggable = false;
            base.resizeable = false;
            _instance = this;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Vector2 TabSize = new Vector2(RequestedTabSize.x - Margin * 2f, RequestedTabSize.y - Margin * 2f);
            DrawScrollTab(DrawModButtons, ModsList, ref _scrollPositionModTab, TabSize.x / 2f * 0f, TabSize.y / 2f * 0f, TabSize.x / 2f, TabSize.y / 2f);
            //DrawModsTab(ModsList);
            DrawCategoryTab();
            DrawScrollTab(DrawThingButtons, CraftablesList, ref _scrollPositionThingTab, TabSize.x / 2f * 1f, TabSize.y / 2f * 0f, TabSize.x / 2f, TabSize.y / 3f * 2);
            DrawItemDescription();
        }

        private void DrawScrollTab<T>(Func<List<T>, Rect, bool> DrawButtons, List<T> list, ref Vector2 ScrollPosition, float PositionX, float PositionY, float SizeX, float SizeY)
        {
            // constants
            float ButtonHeight = 30f;
            float OutMargin = 5f;

            Vector2 OutRectPos = new Vector2(PositionX + OutMargin, PositionY + OutMargin);
            Vector2 OutRectSize = new Vector2(SizeX - OutMargin * 2f, SizeY - OutMargin * 2f);

            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 18f, list.Count * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(PositionX, PositionY, SizeX, SizeY);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw and decorate tab
            Widgets.DrawBox(RectMargin);
            Widgets.DrawHighlight(RectMargin);

            // begin scroll
            Widgets.BeginScrollView(RectOut, ref ScrollPosition, RectView);

            DrawButtons(list, RectView);

            Widgets.EndScrollView();
        }

        private void DrawScrollTab<T>(Func<List<T>, Rect, bool> DrawButtons, List<T> list, ref Vector2 ScrollPosition, Vector2 Position, Vector2 Size)
        {
            // constants
            float ButtonHeight = 30f;
            float OutMargin = 5f;

            Vector2 OutRectPos = new Vector2(Position.x + OutMargin, Position.y + OutMargin);
            Vector2 OutRectSize = new Vector2(Size.x - OutMargin * 2f, Size.y - OutMargin * 2f);

            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 18f, list.Count * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(Position, Size);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw and decorate tab
            Widgets.DrawBox(RectMargin);
            Widgets.DrawHighlight(RectMargin);

            // begin scroll
            Widgets.BeginScrollView(RectOut, ref ScrollPosition, RectView);

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

                if (Widgets.ButtonText(Button, list[i].Name, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedMod = list[i];
                }
            }

            return true;
        }

        private bool DrawCategoryButtons()
        {
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
                    SelectedThingDef = list[i].ProducedThingDef;
                }
            }

            return true;
        }

        private void DrawModsTab(List<ModMetaData> mods)
        {
            // constants
            float ButtonHeight = 30f;
            float OutMargin = 5f;

            Vector2 TabSize = new Vector2(RequestedTabSize.x - Margin * 2f, RequestedTabSize.y - Margin * 2f);

            Vector2 MarginRectSize = new Vector2(TabSize.x / 2f, TabSize.y / 2f);
            Vector2 MarginRectPos = new Vector2(TabSize.x / 2f * 0f, TabSize.y / 2f * 0f);

            Vector2 OutRectSize = new Vector2(MarginRectSize.x - OutMargin * 2f, MarginRectSize.y - OutMargin * 2f);
            Vector2 OutRectPos = new Vector2(MarginRectPos.x + OutMargin, MarginRectPos.y + OutMargin);

            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 18f, mods.Count * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(MarginRectPos, MarginRectSize);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw and decorate tab
            Widgets.DrawBox(RectMargin);
            Widgets.DrawHighlight(RectMargin);

            // begin scroll
            Widgets.BeginScrollView(RectOut, ref _scrollPositionModTab, RectView);

            for (int i = 0; i < mods.Count; i++)
            {
                Rect Button = new Rect(0f, i * ButtonHeight, ViewRectSize.x, ButtonHeight);

                // draw and decorate button
                Widgets.DrawHighlightIfMouseover(Button);

                if (Widgets.ButtonText(Button, mods[i].Name, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedMod = mods[i];
                }
            }

            Widgets.EndScrollView();
        }

        private void DrawCategoryTab()
        {
            // constants
            float ButtonHeight = 30f;
            float OutMargin = 5f;

            Vector2 TabSize = new Vector2(RequestedTabSize.x - Margin * 2f, RequestedTabSize.y - Margin * 2f);

            Vector2 MarginRectSize = new Vector2(TabSize.x / 2f, TabSize.y / 2f);
            Vector2 MarginRectPos = new Vector2(TabSize.x / 2f * 0f, TabSize.y / 2f * 1f);

            Vector2 OutRectSize = new Vector2(MarginRectSize.x - OutMargin * 2f, MarginRectSize.y - OutMargin * 2f);
            Vector2 OutRectPos = new Vector2(MarginRectPos.x + OutMargin, MarginRectPos.y + OutMargin);

            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 18f, 0f * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(MarginRectPos, MarginRectSize);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw and decorate tab
            Widgets.DrawBox(RectMargin);
            Widgets.DrawHighlight(RectMargin);

            // begin scroll
            Widgets.BeginScrollView(RectOut, ref _scrollPositionModTab, RectView);

            //for (int i = 0; i < mods.Count; i++)
            //{
            //    Rect Button = new Rect(0f, i * ButtonHeight, ViewRectSize.x, ButtonHeight);

            //    // draw and decorate button
            //    Widgets.DrawHighlightIfMouseover(Button);

            //    if (Widgets.ButtonText(Button, mods[i].Name, false))
            //    {
            //        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
            //        SelectedMod = mods[i];
            //    }
            //}

            Widgets.EndScrollView();
        }

        private void DrawItemsTab(List<RecipeDef> craftables)
        {
            // constants
            float ButtonHeight = 30f;
            float OutMargin = 5f;

            Vector2 TabSize = new Vector2(RequestedTabSize.x - Margin * 2f, RequestedTabSize.y - Margin * 2f);

            Vector2 MarginRectSize = new Vector2(TabSize.x / 2f, TabSize.y / 3f * 2);
            Vector2 MarginRectPos = new Vector2(TabSize.x / 2f * 1f, TabSize.y / 2f * 0f);

            Vector2 OutRectSize = new Vector2(MarginRectSize.x - OutMargin * 2f, MarginRectSize.y - OutMargin * 2f);
            Vector2 OutRectPos = new Vector2(MarginRectPos.x + OutMargin, MarginRectPos.y + OutMargin);

            Vector2 ViewRectSize = new Vector2(OutRectSize.x - 18f, craftables.Count * ButtonHeight);

            // scroll rects
            Rect RectMargin = new Rect(MarginRectPos, MarginRectSize);
            Rect RectOut = new Rect(OutRectPos, OutRectSize);
            Rect RectView = new Rect(Vector2.zero, ViewRectSize);

            // draw and decorate tab
            Widgets.DrawBox(RectMargin);
            Widgets.DrawHighlight(RectMargin);

            // begin scroll
            Widgets.BeginScrollView(RectOut, ref _scrollPositionThingTab, RectView);

            for (int i = 0; i < craftables.Count; i++)
            {
                Rect Button = new Rect(0f, i * ButtonHeight, ViewRectSize.x, ButtonHeight);

                // draw and decorate button
                Widgets.DrawHighlightIfMouseover(Button);

                if (Widgets.ButtonText(Button, craftables[i].ProducedThingDef.label, false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedThingDef = craftables[i].ProducedThingDef;
                }
            }

            Widgets.EndScrollView();
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
