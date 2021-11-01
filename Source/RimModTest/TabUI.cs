using System;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;

namespace BlockdudesTabs
{
    public static class TabUI
    {
        public static void DrawScrollTab<T>(Action<T, Rect> Draw, List<T> list, ref Vector2 ScrollPosition, Rect RectOut, float ButtonHeight = 30f)
        {
            Rect RectView = new Rect(0f, 0f, RectOut.width - 16f, list == null ? 1f : list.Count * ButtonHeight);
            Widgets.BeginScrollView(RectOut, ref ScrollPosition, RectView);

            // expected custom function for drawing buttons
            for (int i = 0; i < list.Count; i++)
                Draw(list[i], new Rect(0f, i * ButtonHeight, RectView.width, ButtonHeight));

            Widgets.EndScrollView();
        }

        public static void DrawSearchBar(Action UpdateFunction, ref string SearchString, Rect RectView)
        {
            // scroll rects
            Rect TextBox = new Rect(
                RectView.x + RectView.height,
                RectView.y,
                RectView.width - RectView.height * 2,
                RectView.height);
            Rect ButtonClear = new Rect(
                TextBox.x + TextBox.width,
                RectView.y,
                RectView.height,
                RectView.height);
            Rect SearchIcon = new Rect(
                RectView.x,
                RectView.y,
                RectView.height,
                RectView.height);

            CreateMargins(ref ButtonClear, 3f, 0f, false);
            Widgets.DrawTextureFitted(SearchIcon, TexButton.Search, 1f);
            if (Widgets.ButtonImage(ButtonClear, TexButton.CloseXSmall, Color.white, Color.white * GenUI.SubtleMouseoverColor, true))
            {
                //Verse.Sound.SoundStarter.PlayOneShotOnCamera(SoundDefOf.Click);
                SearchString = "";
                UpdateFunction();
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
                UpdateFunction();
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
