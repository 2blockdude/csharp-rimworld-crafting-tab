using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace BlockdudesTabs
{
    public static class TabUI
    {
        public static void DrawScrollTab<T>(Action<T, Rect> draw, List<T> list, ref Vector2 scrollPosition, Rect rectOut, float buttonHeight = 30f)
        {
            Rect rectView = new Rect(0f, 0f, rectOut.width - 16f, list.Count * buttonHeight);
            Widgets.BeginScrollView(rectOut, ref scrollPosition, rectView);

            // expected custom function for drawing buttons
            for (int i = 0; i < list.Count; i++)
                draw(list[i], new Rect(0f, i * buttonHeight, rectView.width, buttonHeight));

            Widgets.EndScrollView();
        }

        public static bool DrawSearchBar(ref string searchString, Rect rectView)
        {
            bool update = false;

            // scroll rects
            Rect textBox = new Rect(
                rectView.x + rectView.height,
                rectView.y,
                rectView.width - rectView.height * 2,
                rectView.height);
            Rect buttonClear = new Rect(
                textBox.x + textBox.width,
                rectView.y,
                rectView.height,
                rectView.height);
            Rect searchIcon = new Rect(
                rectView.x,
                rectView.y,
                rectView.height,
                rectView.height);

            CreateMargins(ref buttonClear, 3f, 0f, false);
            Widgets.DrawTextureFitted(searchIcon, TexButton.Search, 1f);
            if (Widgets.ButtonImage(buttonClear, TexButton.CloseXSmall, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
            {
                //Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                searchString = "";
                update = true;
            }

            // need to give the textbox a name inorder to do the loose focus thing below
            string textBoxName = "search";
            GUI.SetNextControlName(textBoxName);

            // draw textbox and make sure to store the string entered in the box
            searchString = Widgets.TextField(textBox, searchString);

            bool focused = GUI.GetNameOfFocusedControl() == textBoxName;

            // loose focus so the filter does not always run
            if (Input.GetMouseButtonDown(0) && !Mouse.IsOver(textBox) && focused)
                GUI.FocusControl(null);

            // only filter if focused and the user presses a keyboard key are typing
            if (focused && Event.current.isKey)
                update = true;

            return update;
        }

        private static Bill_Production bill = null;
        private static Dialog_BillConfig billConfig = null;
        private static bool canCraft = false;
        private static List<Building_WorkTable> worktablesOnMap = null;

        public static bool DrawCraftButton(Rect button, RecipeDef recipe, List<Building_WorkTable> workBenches)
        {
            //bill.DoInterface(0f, 0f, 200, _instance.ID);
            if (Widgets.ButtonText(button, "Craft"))
            {
                worktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
                if (worktablesOnMap.Count > 0)
                {
                    bill = new Bill_Production(recipe);
                    billConfig = new Dialog_BillConfig(bill, worktablesOnMap[0].Position);
                    Find.WindowStack.Add(billConfig);
                    canCraft = true;
                }
            }

            if (canCraft && billConfig != null && billConfig.IsOpen == false)
            {
                foreach (Building_WorkTable thing in worktablesOnMap)
                    thing.billStack.AddBill(bill.Clone());
                canCraft = false;
                return true;
            }

            return false;
        }

        public static void CreateMargins(ref Rect RectMain, float OutMargin, float InMargin, bool Outline = true)
        {
            if (RectMain == null) return;

            RectMain = RectMain.ContractedBy(OutMargin);
            if (Outline) Widgets.DrawBox(RectMain);
            RectMain = RectMain.ContractedBy(InMargin);
        }
    }
}
