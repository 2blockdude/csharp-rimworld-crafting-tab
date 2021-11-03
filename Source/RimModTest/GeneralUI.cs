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
        // only used for crafting button thing. don't know how to do this better yet so i will keep this till further notice
        private static Bill_Production bill = null;
        private static Dialog_BillConfig billConfig = null;
        private static bool canCraft = false;
        public enum EventCode
        {
            NoAvailableWorktables = -4,
            ResearchIncomplete,
            IncompatibleRecipe,
            NoEvent,
            ButtonPressed,
            BillComplete
        }

        public static int DrawScrollTab<T>(Rect rectOut, Action<T, Rect> decorateButton, List<T> list, ref Vector2 scrollPosition, float buttonHeight = 30f, bool doMouseoverSound = false)
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

        public static (Bill_Production bill, EventCode eventVal) DrawMakeBillButton(Rect button, RecipeDef recipe, ThingDef worktableType = null, bool doChecking = true)
        {
            if (Widgets.ButtonText(button, "Make Bill"))
            {
                if (doChecking)
                {
                    // check if recipe is compatible with selected worktable
                    if (worktableType != null && !recipe.AllRecipeUsers.Contains(worktableType))
                        return (null, EventCode.IncompatibleRecipe);

                    // check if research for item is done. note: don't need to check if worktable research is finished
                    if (recipe.researchPrerequisite != null && !(recipe.researchPrerequisite.IsFinished && recipe.researchPrerequisite.PrerequisitesCompleted))
                        return (null, EventCode.ResearchIncomplete);

                    // find, filter, and update worktables on map
                    List<Building_WorkTable> worktablesOnMap = Find.CurrentMap.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.PotentialBillGiver)).OfType<Building_WorkTable>().ToList();
                    if (worktableType != null)
                        worktablesOnMap = worktablesOnMap.Where(def => def.def == worktableType).ToList();

                    // check if there are any available compatible worktables
                    if (worktablesOnMap.Count == 0)
                        return (null, EventCode.NoAvailableWorktables);
                }

                Building_WorkTable tempTable = new Building_WorkTable();
                bill = new Bill_Production(recipe);
                billConfig = new Dialog_BillConfig(bill, tempTable.Position);
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
    }
}
