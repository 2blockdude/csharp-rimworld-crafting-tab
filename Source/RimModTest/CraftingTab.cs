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

        // Thing Lists
        public static List<RecipeDef> RecipeList = DefDatabase<RecipeDef>.AllDefsListForReading;
        public static List<ThingCategoryDef> CategoryList = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
        public static List<ThingDef> SelectedCategoryList = null;
        public static ThingDef SelectedThingDef = null;

        internal static Vector2 _scrollPositionCateg = Vector2.zero;
        internal static Vector2 _scrollPositionThing = Vector2.zero;

        public CraftingTab()
        {
            base.draggable = false;
            base.resizeable = false;
            _instance = this;
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawCategoryTab();
            DrawItemsTab();
            DrawItemDescription();
        }

        private void DrawModsTab()
        {

        }

        private void DrawCategoryTab()
        {
            Rect rectView = new Rect(0f, 0f, 300f, RequestedTabSize.y - 10);
            Rect rectSize = new Rect(0f, 0f, 280f, CategoryList.Count * 52f + 5f);

            Widgets.DrawBox(rectView);
            Widgets.DrawHighlight(rectView);
            Widgets.BeginScrollView(rectView, ref _scrollPositionCateg, rectSize);
            // draw button for each catagory
            for (int i = 0; i < CategoryList.Count; i++)
            {
                Rect CusBut = new Rect(5f, i * 52f + 1, 280f, 51f);
                Widgets.DrawHighlightIfMouseover(CusBut);
                Texture2D previewImage = null;
                previewImage = CategoryList[i].icon;
                Widgets.DrawTextureFitted(CusBut, previewImage, 1f);
                if (Widgets.ButtonText(CusBut, $"{CategoryList[i].label}", false))
                {
                    SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                    SelectedCategoryList = CategoryList[i].childThingDefs;
                }
                Widgets.DrawLineHorizontal(0f, i * 52f, 285f);
            }
            Widgets.EndScrollView();
        }

        private void DrawItemsTab()
        {
            Rect rectView = new Rect(300f, 0f, 300f, RequestedTabSize.y - 10);
            Widgets.DrawBox(rectView);
            Widgets.DrawHighlight(rectView);
            if (SelectedCategoryList != null)
            {

                Rect rectSize = new Rect(0f, 0f, 280f, SelectedCategoryList.Count * 52f + 5f);
                Widgets.BeginScrollView(rectView, ref _scrollPositionThing, rectSize);
                // draw button for each item in selected catagory
                for (int i = 0; i < SelectedCategoryList.Count; i++)
                {
                    Rect CusBut = new Rect(5f, i * 52f + 1, 280f, 51f);
                    Widgets.DrawHighlightIfMouseover(CusBut);
                    Texture2D previewImage = null;
                    previewImage = SelectedCategoryList[i].uiIcon;
                    // give icon color
                    Color guicolor = GUI.color;
                    GUI.color = SelectedCategoryList[i].uiIconColor;
                    Widgets.DrawTextureFitted(CusBut, previewImage, 1f);
                    GUI.color = guicolor;
                    if (Widgets.ButtonText(CusBut, $"{SelectedCategoryList[i].label}", false))
                    {
                        SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                        SelectedThingDef = SelectedCategoryList[i];
                    }
                    Widgets.DrawLineHorizontal(0f, i * 52f, 285f);
                }
                Widgets.EndScrollView();
            }
        }

        private void DrawItemDescription()
        {
            if (SelectedThingDef != null)
            {
                Widgets.Label(new Rect(605f, 0f, 395f, 200f), SelectedThingDef.description);
                Widgets.Label(new Rect(605f, 200f, 395f, 100f), SelectedThingDef.modContentPack.Name);
                Widgets.Label(new Rect(605f, 220f, 395f, 100f), SelectedThingDef.label);
                Widgets.Label(new Rect(605f, 240f, 395f, 100f), SelectedThingDef.FirstThingCategory.label);
                Widgets.Label(new Rect(605f, 260f, 395f, 100f), SelectedThingDef.category.ToString());
                RecipeDef tr = FindRecipeFromThing(SelectedThingDef);
                if (tr != null)
                {
                    if (Widgets.ButtonText(new Rect(605f, 600f, 395f, 95f), "Craft"))
                    {
                        Log.Message(tr.defName + " : " + tr.label);
                        Widgets.InfoCardButton(0f, 0f, SelectedThingDef);
                        List<Building_WorkTable> worktablesOnMap =
                            Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
                        foreach (Building_WorkTable table in worktablesOnMap)
                        {
                            table.BillStack.AddBill(FindRecipeFromThing(SelectedThingDef).MakeNewBill());
                        }
                    }
                }
            }
        }

        private RecipeDef FindRecipeFromThing(ThingDef thing)
        {
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefsListForReading)
                if (ThingDef.Equals(recipe.ProducedThingDef, thing))
                    return recipe;
            return null;
        }
    }
}
