﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace BlockdudesTabs
{
    public static class GeneralUI
    {
        // only used for crafting button thing. don't know how to do this better yet so i will keep this till further notice
        private static Bill_Production bill = null;
        private static Dialog_BillConfig billConfig = null;
        private static bool canCraft = false;
        public enum EventCode
        {
            RecipeNotAvailable = -5,
            NoAvailableWorktables,
            NoSelectedWorktableType,
            ResearchIncomplete,
            IncompatibleRecipe,
            NoEvent,
            ButtonPressed,
            BillComplete
        }

        public static int ScrollMenu<T>(Rect rectOut, Action<T, Rect> decorateButton, List<T> list, ref Vector2 scrollPosition, float buttonHeight = 30f, bool doMouseoverSound = false)
        {
            int selectedItem = -1;

            Rect rectView = new Rect(0f, 0f, rectOut.width - 16f, list.Count * buttonHeight);
            Widgets.BeginScrollView(rectOut, ref scrollPosition, rectView);

            // expected custom function for drawing buttons
            for (int i = 0; i < list.Count; i++)
            {
                Rect rectButt = new Rect(0f, i * buttonHeight, rectView.width, buttonHeight);
                decorateButton(list[i], rectButt);
                if (Widgets.ButtonText(rectButt, "", false, doMouseoverSound))
                    selectedItem = i;
            }

            Widgets.EndScrollView();

            // will return item from list if button has been clicked
            return selectedItem;
        }

        public static bool SearchBar(Rect rectView, ref string searchString)
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

        // not sure if this is stupid or not but i like it here
        public static (Bill_Production bill, EventCode eventVal) MakeBillButton(Rect button, RecipeDef recipe, ThingDef worktableType, bool doChecking = true)
        {
            if (Widgets.ButtonText(button, "Make Bill"))
            {
                if (doChecking)
                {
                    // check if research for item is done. note: don't need to check if worktable research is finished
                    if (recipe.researchPrerequisite != null && !recipe.researchPrerequisite.IsFinished)
                        return (null, EventCode.ResearchIncomplete);

                    if (!recipe.AvailableNow)
                        return (null, EventCode.RecipeNotAvailable);

                    // check if we have a worktable selected
                    if (worktableType == null)
                        return (null, EventCode.NoSelectedWorktableType);

                    // check if recipe is compatible with selected worktable
                    if (!recipe.AllRecipeUsers.Contains(worktableType))
                        return (null, EventCode.IncompatibleRecipe);

                    // find, filter, and update worktables on map
                    List<Building> worktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building>().ToList();
                    worktablesOnMap = worktablesOnMap.Where(def => def.def == worktableType).ToList();

                    // check if there are any available compatible worktables
                    if (worktablesOnMap.Count == 0)
                        return (null, EventCode.NoAvailableWorktables);
                }

                // open bill config menu
                Building_WorkTable tempTable = new Building_WorkTable();
                bill = new Bill_Production(recipe);
                billConfig = new Dialog_BillConfig(bill, tempTable.Position);
                // note: need to add bill to temp table or bill config will not open properly
                tempTable.billStack.AddBill(bill);
                Find.WindowStack.Add(billConfig);
                canCraft = true;

                return (null, EventCode.ButtonPressed);
            }

            // only returns bills after billconfig is closed
            if (recipe != null && canCraft && billConfig != null && billConfig.IsOpen == false)
            {
                canCraft = false;
                return (bill, EventCode.BillComplete);
            }

            return (null, EventCode.NoEvent);
        }

        public static Rect LabelColorAndOutLine(Rect rect, string label, Color color, TextAnchor anchor, float margin = 0f)
        {
            // color outline
            GUI.color = color;
            Widgets.DrawBox(rect);

            // reset color
            GUI.color = Color.white;

            // color in rect to what ever this sets the color to
            Widgets.DrawAltRect(rect);

            rect = rect.ContractedBy(margin);

            Text.Anchor = anchor;
            Widgets.Label(rect, label);

            // reset anchor
            Text.Anchor = TextAnchor.UpperLeft;

            // set rect size after label is in
            rect.height -= 25f;
            rect.y += 25f;

            Widgets.DrawLineHorizontal(rect.x, rect.y - 5f, rect.width);

            return rect;
        }
    }
}
