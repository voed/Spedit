using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using Spedit.UI.Components;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        bool IsSearchFieldOpen;

        public void ToggleSearchField()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (IsSearchFieldOpen)
            {
                if (ee != null)
                {
                    if (ee.IsKeyboardFocusWithin)
                    {
                        if (ee.editor.SelectionLength > 0)
                        {
                            FindBox.Text = ee.editor.SelectedText;
                        }
                        FindBox.SelectAll();
                        FindBox.Focus();
                        return;
                    }
                }
                IsSearchFieldOpen = false;
                FindReplaceGrid.IsHitTestVisible = false;
                FindReplaceGrid.Height = 0;
                if (ee == null)
                {
                    return;
                }
                ee.editor.Focus();
            }
            else
            {
                IsSearchFieldOpen = true;
                FindReplaceGrid.IsHitTestVisible = true;
                if (ee == null)
                {
                    return;
                }
                if (ee.editor.SelectionLength > 0)
                {
                    FindBox.Text = ee.editor.SelectedText;
                }
                FindBox.SelectAll();
                FindReplaceGrid.Height = double.NaN;
                
                FindBox.Focus();
            }
        }

        private void CloseFindReplaceGrid(object sender, RoutedEventArgs e)
        {
            ToggleSearchField();
        }
        private void SearchButtonClicked(object sender, RoutedEventArgs e)
        {
            Search();
        }
        private void ReplaceButtonClicked(object sender, RoutedEventArgs e)
        {
            /*if ( == 1)
            {
                ReplaceAll();
            }
            else
            {
                Replace();
            }*/
        }
        private void CountButtonClicked(object sender, RoutedEventArgs e)
        {
            Count();
        }
        private void SearchBoxTextChanged(object sender, RoutedEventArgs e)
        {
            FindResultBlock.Text = string.Empty;
        }
        private void SearchBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
        private void ReplaceBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Replace();
            }
        }
        private void FindReplaceGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ToggleSearchField();
            }
        }

        private void Button_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void Search()
        {
            EditorElement[] editors = GetEditorElementsForFRAction(out var editorIndex);
            if (editors == null) { return; }
            if (editors.Length < 1) { return; }
            if (editors[0] == null) { return; }
            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            int startFileCaretOffset = 0;
            bool foundOccurence = false;
            for (int i = editorIndex; i < (editors.Length + editorIndex + 1); ++i)
            {
                int index = ValueUnderMap(i, editors.Length);
                string searchText;
                int addToOffset = 0;
                if (i == editorIndex)
                {
                    startFileCaretOffset = editors[index].editor.CaretOffset;
                    addToOffset = startFileCaretOffset;
                    if (startFileCaretOffset < 0) { startFileCaretOffset = 0; }
                    searchText = editors[index].editor.Text.Substring(startFileCaretOffset);
                }
                else if (i == (editors.Length + editorIndex))
                {
                    searchText = startFileCaretOffset == 0 ? string.Empty : editors[index].editor.Text.Substring(0, startFileCaretOffset);
                }
                else
                {
                    searchText = editors[index].editor.Text;
                }
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    Match m = regex.Match(searchText);
                     if (m.Success)
                     {
                         foundOccurence = true;
                         editors[index].Parent.IsSelected = true;
                         editors[index].editor.CaretOffset = m.Index + addToOffset + m.Length;
                         editors[index].editor.Select(m.Index + addToOffset, m.Length);
                         TextLocation location = editors[index].editor.Document.GetLocation(m.Index + addToOffset);
                         editors[index].editor.ScrollTo(location.Line, location.Column);
                         //FindResultBlock.Text = "Found in offset " + (m.Index + addToOffset).ToString() + " with length " + m.Length.ToString();
                         FindResultBlock.Text = string.Format(Properties.Resources.FoundInOff, m.Index + addToOffset,
                             m.Length);
                         break;
                     }
                    
                }
            }
            if (!foundOccurence)
            {
                FindResultBlock.Text = Properties.Resources.FoundNothing;
            }
        }

        private void Replace()
        {
            EditorElement[] editors = GetEditorElementsForFRAction(out var editorIndex);
            if (editors == null) { return; }
            if (editors.Length < 1) { return; }
            if (editors[0] == null) { return; }
            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            string replaceString = ReplaceBox.Text;
            int startFileCaretOffset = 0;
            bool foundOccurence = false;
            for (int i = editorIndex; i < (editors.Length + editorIndex + 1); ++i)
            {
                int index = ValueUnderMap(i, editors.Length);
                string searchText;
                int addToOffset = 0;
                if (i == editorIndex)
                {
                    startFileCaretOffset = editors[index].editor.CaretOffset;
                    addToOffset = startFileCaretOffset;
                    if (startFileCaretOffset < 0) { startFileCaretOffset = 0; }
                    searchText = editors[index].editor.Text.Substring(startFileCaretOffset);
                }
                else if (i == (editors.Length + editorIndex))
                {
                    searchText = startFileCaretOffset == 0 ? string.Empty : editors[index].editor.Text.Substring(0, startFileCaretOffset);
                }
                else
                {
                    searchText = editors[index].editor.Text;
                }

                if (string.IsNullOrWhiteSpace(searchText))
                    continue;

                Match m = regex.Match(searchText);

                if (!m.Success)
                    continue;

                foundOccurence = true;
                editors[index].Parent.IsSelected = true;
                string result = m.Result(replaceString);
                editors[index].editor.Document.Replace(m.Index + addToOffset, m.Length, result);
                editors[index].editor.CaretOffset = m.Index + addToOffset + result.Length;
                editors[index].editor.Select(m.Index + addToOffset, result.Length);
                var location = editors[index].editor.Document.GetLocation(m.Index + addToOffset);
                editors[index].editor.ScrollTo(location.Line, location.Column);
                FindResultBlock.Text = string.Format(Properties.Resources.ReplacedOff, MinHeight + addToOffset);
                break;
            }
            if (!foundOccurence)
            {
                FindResultBlock.Text = Properties.Resources.FoundNothing;
            }
        }

        private void ReplaceAll()
        {

            int editorIndex = 0;
            EditorElement[] editors = GetEditorElementsForFRAction(out editorIndex);
            if (editors?.Length < 1)
                return; 

            if (editors?[0] == null)
                return;

            Regex regex = GetSearchRegex();
            if (regex == null)
                return;
            int count = 0;
            int fileCount = 0;
            string replaceString = ReplaceBox.Text;
            foreach (var editor in editors)
            {
                MatchCollection mc = regex.Matches(editor.editor.Text);

                if (mc.Count <= 0)
                    continue;

                fileCount++;
                count += mc.Count;
                editor.editor.BeginChange();
                for (int j = mc.Count - 1; j >= 0; --j)
                {
                    string replace = mc[j].Result(replaceString);
                    editor.editor.Document.Replace(mc[j].Index, mc[j].Length, replace);
                }
                editor.editor.EndChange();
                editor.NeedsSave = true;
            }
			//FindResultBlock.Text = "Replaced " + count.ToString() + " occurences in " + fileCount.ToString() + " documents";
			FindResultBlock.Text = string.Format(Properties.Resources.ReplacedOcc, count, fileCount);
        }

        private void Count()
        {
            int editorIndex = 0;
            EditorElement[] editors = GetEditorElementsForFRAction(out editorIndex);

            if (editors?.Length < 1)
                return;

            if (editors?[0] == null)
                return;

            Regex regex = GetSearchRegex();
            if (regex == null) { return; }
            int count = editors.Select(editor => regex.Matches(editor.editor.Text)).Select(mc => mc.Count).Sum();
            FindResultBlock.Text = count + Properties.Resources.OccFound;
        }

        private Regex GetSearchRegex()
        {
            string findString = FindBox.Text;
            if (string.IsNullOrEmpty(findString))
            {
                FindResultBlock.Text = Properties.Resources.EmptyPatt;
                return null;
            }
            Regex regex;
            RegexOptions regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (CCBox.IsChecked != null && !CCBox.IsChecked.Value)
            { regexOptions |= RegexOptions.IgnoreCase; }


            string searchType = SearchTypeBox.SelectedValue.ToString();
            if (searchType.Equals(Properties.Resources.NormalSearch))
            {
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else if (searchType.Equals(Properties.Resources.MatchWholeWords))
            {
                regex = new Regex("\\b" + Regex.Escape(findString) + "\\b", regexOptions);
            }
            else if (searchType.Equals(Properties.Resources.AdvancSearch))
            {
                findString = findString.Replace("\\t", "\t").Replace("\\r", "\r").Replace("\\n", "\n");
                Regex rx = new Regex(@"\\[uUxX]([0-9A-F]{4})");
                findString = rx.Replace(findString, delegate(Match match) { return ((char)Int32.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString(); });
                regex = new Regex(Regex.Escape(findString), regexOptions);
            }
            else
            {
                regexOptions |= RegexOptions.Multiline;
                if (MLRBox.IsChecked != null && MLRBox.IsChecked.Value)
                {
                    regexOptions |= RegexOptions.Singleline;
                } //paradox, isn't it? ^^
                try
                {
                    regex = new Regex(findString, regexOptions);
                }
                catch (Exception) { FindResultBlock.Text = Properties.Resources.NoValidRegex; return null; }
            }
            return regex;
        }

        private EditorElement[] GetEditorElementsForFRAction(out int editorIndex)
        {
            int editorStartIndex = 0;
            EditorElement[] editors;
            if (FindDestinies.SelectedIndex == 0)
                editors = new[] { GetCurrentEditorElement() };
            else
            {
                editors = GetAllEditorElements();
                object checkElement = DockingPane.SelectedContent?.Content;
                if (checkElement is EditorElement)
                {
                    for (int i = 0; i < editors.Length; ++i)
                    {
                        if (editors[i] == checkElement)
                        {
                            editorStartIndex = i;
                        }
                    }
                }
            }
            editorIndex = editorStartIndex;
            return editors;
        }

        private static int ValueUnderMap(int value, int map)
        {
            while (value >= map)
            {
                value -= map;
            }
            return value;
        }
    }
}
