using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace BlockdudesTabs
{
    public static class GeneralUI
    {
        // used for crafting button thing. don't know how to do this better yet so i will keep this till further notice
        private static Bill_Production bill = null;
        private static Dialog_BillConfig billConfig = null;
        private static bool canCraft = false;

        public static void DrawScrollTab<T>(Rect rectOut, Action<T, Rect> draw, List<T> list, ref Vector2 scrollPosition, float buttonHeight = 30f)
        {
            Rect rectView = new Rect(0f, 0f, rectOut.width - 16f, list.Count * buttonHeight);
            Widgets.BeginScrollView(rectOut, ref scrollPosition, rectView);

            // expected custom function for drawing buttons
            for (int i = 0; i < list.Count; i++)
                draw(list[i], new Rect(0f, i * buttonHeight, rectView.width, buttonHeight));

            Widgets.EndScrollView();
        }
        //public static T DrawScrollTab<T>(Rect rectOut, Action<T, Rect> decorateButton, List<T> list, ref Vector2 scrollPosition, float buttonHeight = 30f, bool doMouseoverSound = false)
        //{
        //    T selectedItem = default(T);

        //    Rect rectView = new Rect(0f, 0f, rectOut.width - 16f, list.Count * buttonHeight);
        //    Widgets.BeginScrollView(rectOut, ref scrollPosition, rectView);

        //    // expected custom function for drawing buttons
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        Rect rectButt = new Rect(0f, i * buttonHeight, rectView.width, buttonHeight);
        //        decorateButton(list[i], rectButt);
        //        if (Widgets.ButtonText(rectButt, "", false, doMouseoverSound))
        //            selectedItem = list[i];
        //    }

        //    Widgets.EndScrollView();

        //    // will return item from list if button has been clicked
        //    return selectedItem;
        //}

        public static bool DrawSearchBar(Rect rectView, ref string searchString)
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

            Widgets.DrawTextureFitted(searchIcon, TexButton.Search, 1f);
            if (Widgets.ButtonImage(buttonClear.ContractedBy(3f), TexButton.CloseXSmall, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
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

            // only update if focused and the user presses a keyboard key
            if (focused && Event.current.isKey)
                update = true;

            // gives you info on if the textbox has been updated
            return update;
        }

        public static void DrawMakeBillButton(Rect button, RecipeDef recipe, List<Building_WorkTable> workTables)
        {
            if (Widgets.ButtonText(button, "Make Bill"))
            {
                if (workTables.Count > 0)
                {
                    Building_WorkTable tempTable = new Building_WorkTable();
                    bill = new Bill_Production(recipe);
                    billConfig = new Dialog_BillConfig(bill, tempTable.Position);
                    tempTable.billStack.AddBill(bill);
                    Find.WindowStack.Add(billConfig);

                    canCraft = true;
                }
            }

            // only apply bills after billconfig is closed
            if (canCraft && billConfig != null && billConfig.IsOpen == false)
            {
                // give bill to all listed workTables
                for (int i = 0; i < workTables.Count; i++)
                    workTables[i].billStack.AddBill(bill.Clone());
                canCraft = false;
            }
        }
    }
}
