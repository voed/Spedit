using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Spedit.UI.Components;
using Spedit.Utils;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        
		string[] AvailableAccents = { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber",
			"Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };



        bool RestartTextIsShown;
        bool AllowChanging;
        private MainWindow mainWindow;
        public OptionsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            
			LoadSettings();
            AllowChanging = true;
            this.mainWindow = mainWindow;
        }

        private void RestoreButton_Clicked(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(Properties.Resources.ResetOptions, Properties.Resources.ResetOptQues, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                Program.Options = new OptionsControl();
                Program.MainWindow.OptionMenuEntry.IsEnabled = false;
                MessageBox.Show(Properties.Resources.RestartEditor, Properties.Resources.YRestartEditor);
                Close();
            }
        }

        private void HardwareAcc_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Program_UseHardwareAcceleration = HardwareAcc.IsChecked.Value;
            ToggleRestartText();
        }


        private void AutoOpenInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Program_OpenCustomIncludes = OpenIncludes.IsChecked.Value;
            OpenIncludesRecursive.IsEnabled = OpenIncludes.IsChecked.Value;
        }
        private void OpenIncludeRecursively_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Program_OpenIncludesRecursively = OpenIncludesRecursive.IsChecked.Value;
        }

        private void AutoUpdate_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Program_CheckForUpdates = AutoUpdate.IsChecked.Value;
        }

        private void DynamicISAC_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.Options.Program_DynamicISAC = DynamicISAC.IsChecked.Value;
		}

        private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            double size = FontSizeD.Value;
            Program.Options.Editor_FontSize = size;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
            {
                foreach (var editor in editors)
                {
                    editor.UpdateFontSize(size);
                }
            }
        }

        private void ScrollSpeed_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Editor_ScrollLines = ScrollSpeed.Value;
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool wrapping = WordWrap.IsChecked.Value;
            Program.Options.Editor_WordWrap = wrapping;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
            {
                foreach (var editor in editors)
                {
                    editor.editor.WordWrap = wrapping;
                }
            }
        }

        private void AIndentation_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Editor_AgressiveIndentation = AgressiveIndentation.IsChecked.Value;
        }

        private void LineReformat_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.Editor_ReformatLineAfterSemicolon = LineReformatting.IsChecked.Value;
        }

        private void TabToSpace_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool replaceTabs = TabToSpace.IsChecked.Value;
            Program.Options.Editor_ReplaceTabsToWhitespace = replaceTabs;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
            if (editors != null)
            {
                foreach (EditorElement t in editors)
                {
                    t.editor.Options.ConvertTabsToSpaces = replaceTabs;
                }
            }
        }

		private void AutoCloseBrackets_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.Options.Editor_AutoCloseBrackets = AutoCloseBrackets.IsChecked ?? true;
		}

		private void AutoCloseStringChars_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.Options.Editor_AutoCloseStringChars = AutoCloseStringChars.IsChecked ?? true;
		}

		private void ShowSpaces_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			bool showSpacesValue = Program.Options.Editor_ShowSpaces = ShowSpaces.IsChecked ?? true;
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
            {
                foreach (EditorElement t in editors)
                {
                    t.editor.Options.ShowSpaces = showSpacesValue;
                }
            }
		}

		private void ShowTabs_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			bool showTabsValue = Program.Options.Editor_ShowTabs = ShowTabs.IsChecked ?? true;
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
			{
				for (int i = 0; i < editors.Length; ++i)
				{
					editors[i].editor.Options.ShowTabs = showTabsValue;
				}
			}
		}

		private void FontFamily_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            FontFamily family = (FontFamily)FontFamilyCB.SelectedItem;
            string familyString = family.Source;
            Program.Options.Editor_FontFamily = familyString;
            FontFamilyTB.Content = "Font (" + familyString + "):";
            EditorElement[] editors = mainWindow.GetAllEditorElements();
			if (editors != null)
            {
                foreach (var editor in editors)
                {
                    editor.editor.FontFamily = family;
                }
            }
        }

		private void IndentationSize_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			int indentationSizeValue = Program.Options.Editor_IndentationSize = (int)Math.Round(IndentationSize.Value);
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (editors != null)
            {
                foreach (var editor in editors)
                {
                    editor.editor.Options.IndentationSize = indentationSizeValue;
                }
            }
		}


		private void HighlightDeprecateds_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.SH_HighlightDeprecateds = HighlightDeprecateds.IsChecked ?? true;
            ToggleRestartText();
        }

		private void LanguageBox_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }

            Program.Options.Language = Culture.cultures[LanguageBox.SelectedIndex].Name;
            ToggleRestartText(true);
        }


		private void AutoSave_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			int newIndex = AutoSave.SelectedIndex;
			EditorElement[] editors = mainWindow.GetAllEditorElements();
			if (newIndex == 0)
			{
				Program.Options.Editor_AutoSave = false;
                foreach (EditorElement t in editors)
                {
                    if (t.AutoSaveTimer.Enabled)
                    {
                        t.AutoSaveTimer.Stop();
                    }
                }
            }
			else
			{
				Program.Options.Editor_AutoSave = true;
				switch (newIndex)
                {
                    case 1:
                        Program.Options.Editor_AutoSaveInterval = 30;
                        break;
                    case 7:
                        Program.Options.Editor_AutoSaveInterval = 600;
                        break;
                    case 8:
                        Program.Options.Editor_AutoSaveInterval = 900;
                        break;
                    default:
                        Program.Options.Editor_AutoSaveInterval = (newIndex - 1) * 60;
                        break;
                }

                foreach (var editor in editors)
                {
                    editor.StartAutoSaveTimer();
                }
            }
		}

		private void LoadSettings()
        {
            
            foreach (CultureInfo culture in Culture.cultures)
            {
                LanguageBox.Items.Add(culture.NativeName);
            }

            LanguageBox.SelectedItem = CultureInfo.CurrentUICulture.NativeName;
            

            HardwareAcc.IsChecked = Program.Options.Program_UseHardwareAcceleration;
            OpenIncludes.IsChecked = Program.Options.Program_OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.Options.Program_OpenIncludesRecursively;
            AutoUpdate.IsChecked = Program.Options.Program_CheckForUpdates;
            if (!Program.Options.Program_OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            DynamicISAC.IsChecked = Program.Options.Program_DynamicISAC;
            
            if (Program.Options.Editor_AutoSave)
			{
				int seconds = Program.Options.Editor_AutoSaveInterval;
				if (seconds < 60)
					AutoSave.SelectedIndex = 1;
				else if (seconds <= 300)
					AutoSave.SelectedIndex = Math.Max(1, Math.Min(seconds / 60, 5)) + 1;
				else if (seconds == 600)
					AutoSave.SelectedIndex = 7;
				else
					AutoSave.SelectedIndex = 8;
			}
			else
			{
				AutoSave.SelectedIndex = 0;
			}
            HighlightDeprecateds.IsChecked = Program.Options.SH_HighlightDeprecateds;
            FontSizeD.Value = Program.Options.Editor_FontSize;
            ScrollSpeed.Value = Program.Options.Editor_ScrollLines;
            WordWrap.IsChecked = Program.Options.Editor_WordWrap;
            AgressiveIndentation.IsChecked = Program.Options.Editor_AgressiveIndentation;
            LineReformatting.IsChecked = Program.Options.Editor_ReformatLineAfterSemicolon;
            TabToSpace.IsChecked = Program.Options.Editor_ReplaceTabsToWhitespace;
			AutoCloseBrackets.IsChecked = Program.Options.Editor_AutoCloseBrackets;
			AutoCloseStringChars.IsChecked = Program.Options.Editor_AutoCloseStringChars;
			ShowSpaces.IsChecked = Program.Options.Editor_ShowSpaces;
			ShowTabs.IsChecked = Program.Options.Editor_ShowTabs;
			FontFamilyTB.Content = $"{Properties.Resources.FontFamily} ({Program.Options.Editor_FontFamily}):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.Options.Editor_FontFamily);
			IndentationSize.Value = Program.Options.Editor_IndentationSize;
            LoadSH();
        }

        private void ToggleRestartText(bool FullEffect = false)
        {
            if (AllowChanging && !RestartTextIsShown)
            {
                //StatusTextBlock.Content = (FullEffect) ? Properties.Resources.RestartEdiFullEff : Properties.Resources.RestartEdiEff;
                RestartTextIsShown = true;
            }
        }

 
    }
}
