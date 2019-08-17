﻿using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using SourcepawnCondenser.SourcemodDefinition;
using static Spedit.Program;

namespace Spedit.UI.Components
{
    public partial class EditorElement
    {

        Regex ISFindRegex = new Regex(@"\b(?<name>[a-zA-Z_][a-zA-Z0-9_]+)\(", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
        Regex multilineCommentRegex = new Regex(@"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);

		public ulong LastSMDefUpdateUID = 0;
        public bool ISAC_Open;
        private bool AC_Open;
        private bool IS_Open;

        private bool AC_IsFuncC = true;


        private int LastShowedLine = -1;

        private string[] funcNames;
        private SMFunction[] funcs;
        private ACNode[] acEntrys;
        private ACNode[] isEntrys; //todo find out whats difference
        //private string[] methodNames;
        public void LoadAutoCompletes()
        {

            if (ISAC_Open)
			{
				HideISAC();
			}
			SMDefinition def = ConfigList.Current.GetSMDef();
            funcNames = def.FunctionStrings;
			funcs = def.Functions.ToArray();
            acEntrys = def.ProduceACNodes();
            isEntrys = def.ProduceISNodes();
            //AutoCompleteBox.Items.Clear();
            //MethodAutoCompleteBox.Items.Clear();
			AutoCompleteBox.ItemsSource = acEntrys;
			MethodAutoCompleteBox.ItemsSource = isEntrys;
            /*for (int i = 0; i < acEntrys.Length; ++i)
            {
                AutoCompleteBox.Items.Add(acEntrys[i].Name);
            }
            for (int i = 0; i < isEntrys.Length; ++i)
            {
                MethodAutoCompleteBox.Items.Add(isEntrys[i].Name);
            }*/
        }

		public void InterruptLoadAutoCompletes(string[] FunctionStrings, SMFunction[] FunctionArray, ACNode[] acNodes, ACNode[] isNodes)
		{
			Dispatcher?.Invoke(() => {
				funcNames = FunctionStrings;
				funcs = FunctionArray;
				acEntrys = acNodes;
				isEntrys = isNodes;
				//AutoCompleteBox.Items.Clear();
				//MethodAutoCompleteBox.Items.Clear();
				AutoCompleteBox.ItemsSource = acEntrys;
				MethodAutoCompleteBox.ItemsSource = isEntrys;
				/*for (int i = 0; i < acEntrys.Length; ++i)
				{
					AutoCompleteBox.Items.Add(acEntrys[i].Name);
				}
				for (int i = 0; i < isEntrys.Length; ++i)
				{
					MethodAutoCompleteBox.Items.Add(isEntrys[i].Name);
				}*/
			});
		}

        private void EvaluateIntelliSense()
        {
            if (editor.SelectionLength > 0)
            {
                HideISAC();
                return;
            }
            int currentLineIndex = editor.TextArea.Caret.Line - 1;
            var line = editor.Document.Lines[currentLineIndex];
            string text = editor.Document.GetText(line.Offset, line.Length);
            int lineOffset = editor.TextArea.Caret.Column - 1;
            int caretOffset = editor.CaretOffset;
            bool ForwardShowAC = false;
            bool ForwardShowIS = false;
            string ISFuncNameStr = string.Empty;
            string ISFuncDescriptionStr = string.Empty;
            bool ForceReSet = (currentLineIndex != LastShowedLine);
            bool ForceISKeepsClosed = ForceReSet;
            int xPos = int.MaxValue;
            LastShowedLine = currentLineIndex;
            int quotationCount = 0;
            bool MethodAC = false;
            for (int i = 0; i < lineOffset; ++i)
            {
                if (text[i] == '"')
                {
                    if (i != 0)
                    {
                        if (text[i - 1] != '\\')
                        {
                            quotationCount++;
                        }
                    }
                }
                if ((quotationCount % 2) == 0)
                {
                    if (text[i] == '/')
                    {
                        if (i != 0)
                        {
                            if (text[i - 1] == '/')
                            {
                                HideISAC();
                                return;
                            }
                        }
                    }
                }
            }
            foreach (var c in text)
            {
                if (c == '#')
                {
                    HideISAC();
                    return;
                }
                if (!char.IsWhiteSpace(c))
                {
                    break;
                }
            }
            MatchCollection mc = multilineCommentRegex.Matches(editor.Text, 0); //it hurts me to do it here..but i have no other choice...
            int mlcCount = mc.Count;
            for (int i = 0; i < mlcCount; ++i)
            {
                if (caretOffset >= mc[i].Index)
                {
                    if (caretOffset <= (mc[i].Index + mc[i].Length))
                    {
                        HideISAC();
                        return;
                    }
                }
                else
                {
                    break;
                }
            }
            if (lineOffset > 0)
            {
                #region IS
                MatchCollection ISMatches = ISFindRegex.Matches(text);
                int scopeLevel = 0;
                for (int i = lineOffset - 1; i >= 0; --i)
                {
                    if (text[i] == ')')
                    {
                        scopeLevel++;
                    }
                    else if (text[i] == '(')
                    {
                        scopeLevel--;
                        if (scopeLevel < 0)
                        {
                            bool FoundMatch = false;
                            int searchIndex = i;
                            for (int j = 0; j < ISMatches.Count; ++j)
                            {
                                if ((searchIndex >= ISMatches[j].Index) && (searchIndex <= (ISMatches[j].Index + ISMatches[j].Length)))
                                {
                                    FoundMatch = true;
                                    string testString = ISMatches[j].Groups["name"].Value;
                                    foreach (var func in funcs)
                                    {
                                        if (testString == func.Name)
                                        {
                                            xPos = ISMatches[j].Groups["name"].Index + ISMatches[j].Groups["name"].Length;
                                            ForwardShowIS = true;
                                            ISFuncNameStr = func.FullName;
                                            ISFuncDescriptionStr = func.CommentString;
                                            ForceReSet = true;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            if (FoundMatch)
                            {
                                scopeLevel--; //i have no idea why this works...
                                break;
                            }
                        }
                    }
                }
                #endregion
                #region AC
                if (IsValidFunctionChar(text[lineOffset - 1]) && (quotationCount % 2) == 0)
                {
                    bool IsNextCharValid = true;
                    if (text.Length > lineOffset)
                    {
                        if (IsValidFunctionChar(text[lineOffset]) || text[lineOffset] == '(')
                        {
                            IsNextCharValid = false;
                        }
                    }
                    if (IsNextCharValid)
                    {
                        int endOffset = lineOffset - 1;
                        for (int i = endOffset; i >= 0; --i)
                        {
                            if (!IsValidFunctionChar(text[i]))
                            {
                                if (text[i] == '.')
                                {
                                    MethodAC = true;
                                }
                                break;
                            }
                            endOffset = i;
                        }
                        string testString = text.Substring(endOffset, ((lineOffset - 1) - endOffset) + 1);
                        if (testString.Length > 0)
                        {
                            if (MethodAC)
                            {
                                for (int i = 0; i < isEntrys.Length; ++i)
                                {
                                    if (isEntrys[i].EntryName.StartsWith(testString, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (testString != isEntrys[i].EntryName)
                                        {
                                            ForwardShowAC = true;
                                            MethodAutoCompleteBox.SelectedIndex = i;
                                            MethodAutoCompleteBox.ScrollIntoView(MethodAutoCompleteBox.SelectedItem);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < acEntrys.Length; ++i)
                                {
                                    if (acEntrys[i].EntryName.StartsWith(testString, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        if (testString != acEntrys[i].EntryName)
                                        {
                                            ForwardShowAC = true;
                                            AutoCompleteBox.SelectedIndex = i;
                                            AutoCompleteBox.ScrollIntoView(AutoCompleteBox.SelectedItem);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
            }
            if (!ForwardShowAC)
            {
                if (ForceISKeepsClosed)
                {
                    ForwardShowIS = false;
                }
            }
            if (ForwardShowAC | ForwardShowIS)
            {
                if (ForwardShowAC)
                { ShowAC(!MethodAC); }
                else
                { HideAC(); }
                if (ForwardShowIS)
                { ShowIS(ISFuncNameStr, ISFuncDescriptionStr); }
                else
                { HideIS(); }
                if (ForceReSet && ISAC_Open)
                {
                    SetISACPosition(xPos);
                }
                ShowISAC(xPos);
            }
            else
            {
                HideISAC();
            }
        }

        private bool ISAC_EvaluateKeyDownEvent(Key k)
        {
            if (ISAC_Open && AC_Open)
            {
                switch (k)
                {
                    case Key.Enter:
                        {
                            int startOffset = editor.CaretOffset - 1;
                            int endOffset = startOffset;
                            for (int i = startOffset; i >= 0; --i)
                            {
                                if (!IsValidFunctionChar(editor.Document.GetCharAt(i)))
                                {
                                    break;
                                }
                                endOffset = i;
                            }
                            int length = (startOffset - endOffset) + 1;
                            string replaceString;
                            if (AC_IsFuncC)
                            {
                                replaceString = ((ACNode)AutoCompleteBox.SelectedItem).EntryName;
                                if (acEntrys[AutoCompleteBox.SelectedIndex].IsExecuteable)
                                {
                                    replaceString += "(";
                                }
                            }
                            else
                            {
                                replaceString = ((ACNode)MethodAutoCompleteBox.SelectedItem).EntryName;
                                if (isEntrys[MethodAutoCompleteBox.SelectedIndex].IsExecuteable)
                                {
                                    replaceString += "(";
                                }
                            }
                            editor.Document.Replace(endOffset, length, replaceString);
                            return true;
                        }
                    case Key.Up:
                        {
                            if (AC_IsFuncC)
                            {
                                AutoCompleteBox.SelectedIndex = Math.Max(0, AutoCompleteBox.SelectedIndex - 1);
                                AutoCompleteBox.ScrollIntoView(AutoCompleteBox.SelectedItem);
                            }
                            else
                            {
                                MethodAutoCompleteBox.SelectedIndex = Math.Max(0, MethodAutoCompleteBox.SelectedIndex - 1);
                                MethodAutoCompleteBox.ScrollIntoView(MethodAutoCompleteBox.SelectedItem);
                            }
                            return true;
                        }
                    case Key.Down:
                        {
                            if (AC_IsFuncC)
                            {
                                AutoCompleteBox.SelectedIndex = Math.Min(AutoCompleteBox.Items.Count - 1, AutoCompleteBox.SelectedIndex + 1);
                                AutoCompleteBox.ScrollIntoView(AutoCompleteBox.SelectedItem);
                            }
                            else
                            {
                                MethodAutoCompleteBox.SelectedIndex = Math.Min(MethodAutoCompleteBox.Items.Count - 1, MethodAutoCompleteBox.SelectedIndex + 1);
                                MethodAutoCompleteBox.ScrollIntoView(MethodAutoCompleteBox.SelectedItem);
                            }
                            return true;
                        }
                    case Key.Escape:
                        {
                            HideISAC();
                            return true;
                        }
                }
            }
            return false;
        }

        private void ShowISAC(int forcedXPos = int.MaxValue)
        {
            if (!ISAC_Open)
            {
                ISAC_Open = true;
                ISAC_Grid.Visibility = Visibility.Visible;
                SetISACPosition(forcedXPos);
                ISAC_Grid.Opacity = 1.0;
            }
        }
        private void HideISAC()
        {
            if (ISAC_Open)
            {
                ISAC_Open = false;
                ISAC_Grid.Opacity = 0.0;
                ISAC_Grid.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowAC(bool IsFunc)
        {
            if (!AC_Open)
            {
                AC_Open = true;
                ACBorder.Height = 175.0;

                    AutoCompleteBox.Width = 260.0;
                    MethodAutoCompleteBox.Width = 260.0;
                
            }
            if ((!(IsFunc && AC_IsFuncC)) && AC_Open)
            {
                if (IsFunc)
                {
                    if (!AC_IsFuncC)
                    {
                        AC_IsFuncC = true;

                            AutoCompleteBox.Opacity = 1.0;
                            MethodAutoCompleteBox.Opacity = 0.0;
                        
                    }
                }
                else
                {
                    if (AC_IsFuncC)
                    {
                        AC_IsFuncC = false;

                        AutoCompleteBox.Opacity = 0.0;
                        MethodAutoCompleteBox.Opacity = 1.0;
                        
                    }
                }
            }
        }
        private void HideAC()
        {
            if (AC_Open)
            {
                AC_Open = false;
                AutoCompleteBox.Width = 0.0;
                MethodAutoCompleteBox.Width = 0.0;
                ACBorder.Height = 0.0;
                
            }
        }

        private void ShowIS(string FuncName, string FuncDescription)
        {
            if (!IS_Open)
            {
                IS_Open = true;
                ISenseColumn.Width = new GridLength(0.0, GridUnitType.Auto);
            }
            IS_FuncName.Text = FuncName;
            IS_FuncDescription.Text = FuncDescription;
        }
        private void HideIS()
        {
            if (IS_Open)
            {
                IS_Open = false;
                ISenseColumn.Width = new GridLength(0.0);
            }
            IS_FuncDescription.Text = string.Empty;
            IS_FuncName.Text = string.Empty;
        }

        private void SetISACPosition(int forcedXPos = int.MaxValue)
        {
            Point p;
            if (forcedXPos != int.MaxValue)
            {
                TextViewPosition tvp = new TextViewPosition(editor.TextArea.Caret.Position.Line, forcedXPos + 1);
                p = editor.TextArea.TextView.GetVisualPosition(tvp, VisualYPosition.LineBottom) - editor.TextArea.TextView.ScrollOffset;
            }
            else
            {
                p = editor.TextArea.TextView.GetVisualPosition(editor.TextArea.Caret.Position, VisualYPosition.LineBottom) - editor.TextArea.TextView.ScrollOffset;
            }
            IS_FuncDescription.Measure(new Size(double.MaxValue, double.MaxValue));
            double y = p.Y;
            double ISACHeight = 0.0;
            if (AC_Open && IS_Open)
            {
                double ISHeight = IS_FuncDescription.DesiredSize.Height;
                ISACHeight = Math.Max(175.0, ISHeight);
            }
            else if (AC_Open)
            {
                ISACHeight = 175.0;
            }
            else if (IS_Open)
            {
                ISACHeight = IS_FuncDescription.DesiredSize.Height;
            }
            if ((y + ISACHeight) > editor.ActualHeight)
            {
                y = (editor.TextArea.TextView.GetVisualPosition(editor.TextArea.Caret.Position, VisualYPosition.LineTop) - editor.TextArea.TextView.ScrollOffset).Y;
                y -= ISACHeight;
            }
            ISAC_Grid.Margin = new Thickness(p.X + ((LineNumberMargin)editor.TextArea.LeftMargins[0]).ActualWidth + 20.0, y, 0.0, 0.0);
        }


        private bool IsValidFunctionChar(char c)
        {
            if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '_')) { return true; }
            return false;
        }
    }
}
