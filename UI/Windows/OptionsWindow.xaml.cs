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
            Program.Options.UseHardwareAcceleration = HardwareAcc.IsChecked.Value;
            ToggleRestartText();
        }


        private void AutoOpenInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.OpenCustomIncludes = OpenIncludes.IsChecked.Value;
            OpenIncludesRecursive.IsEnabled = OpenIncludes.IsChecked.Value;
        }
        private void OpenIncludeRecursively_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.OpenIncludesRecursively = OpenIncludesRecursive.IsChecked.Value;
        }


        private void DynamicISAC_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.Options.DynamicIsac = DynamicISAC.IsChecked.Value;
		}

        private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            double size = FontSizeD.Value;
            Program.Options.FontSize = size;
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
            Program.Options.ScrollLines = ScrollSpeed.Value;
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool wrapping = WordWrap.IsChecked.Value;
            Program.Options.WordWrap = wrapping;
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
            Program.Options.AgressiveIndentation = AgressiveIndentation.IsChecked.Value;
        }

        private void LineReformat_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.Options.ReformatLineAfterSemicolon = LineReformatting.IsChecked.Value;
        }

        private void TabToSpace_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool replaceTabs = TabToSpace.IsChecked.Value;
            Program.Options.ReplaceTabsToWhitespace = replaceTabs;
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
			Program.Options.AutoCloseBrackets = AutoCloseBrackets.IsChecked ?? true;
		}

		private void AutoCloseStringChars_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.Options.AutoCloseStringChars = AutoCloseStringChars.IsChecked ?? true;
		}

		private void ShowSpaces_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			bool showSpacesValue = Program.Options.ShowSpaces = ShowSpaces.IsChecked ?? true;
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
			bool showTabsValue = Program.Options.ShowTabs = ShowTabs.IsChecked ?? true;
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
            Program.Options.FontFamily = familyString;
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
			int indentationSizeValue = Program.Options.IndentationSize = (int)Math.Round(IndentationSize.Value);
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
            Program.Options.HighlightDeprecateds = HighlightDeprecateds.IsChecked ?? true;
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
				Program.Options.AutoSave = false;
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
				Program.Options.AutoSave = true;
				switch (newIndex)
                {
                    case 1:
                        Program.Options.AutoSaveInterval = 30;
                        break;
                    case 7:
                        Program.Options.AutoSaveInterval = 600;
                        break;
                    case 8:
                        Program.Options.AutoSaveInterval = 900;
                        break;
                    default:
                        Program.Options.AutoSaveInterval = (newIndex - 1) * 60;
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
            

            HardwareAcc.IsChecked = Program.Options.UseHardwareAcceleration;
            OpenIncludes.IsChecked = Program.Options.OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.Options.OpenIncludesRecursively;
            if (!Program.Options.OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            DynamicISAC.IsChecked = Program.Options.DynamicIsac;
            
            if (Program.Options.AutoSave)
			{
				int seconds = Program.Options.AutoSaveInterval;
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
            HighlightDeprecateds.IsChecked = Program.Options.HighlightDeprecateds;
            FontSizeD.Value = Program.Options.FontSize;
            ScrollSpeed.Value = Program.Options.ScrollLines;
            WordWrap.IsChecked = Program.Options.WordWrap;
            AgressiveIndentation.IsChecked = Program.Options.AgressiveIndentation;
            LineReformatting.IsChecked = Program.Options.ReformatLineAfterSemicolon;
            TabToSpace.IsChecked = Program.Options.ReplaceTabsToWhitespace;
			AutoCloseBrackets.IsChecked = Program.Options.AutoCloseBrackets;
			AutoCloseStringChars.IsChecked = Program.Options.AutoCloseStringChars;
			ShowSpaces.IsChecked = Program.Options.ShowSpaces;
			ShowTabs.IsChecked = Program.Options.ShowTabs;
			FontFamilyTB.Content = $"{Properties.Resources.FontFamily} ({Program.Options.FontFamily}):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.Options.FontFamily);
			IndentationSize.Value = Program.Options.IndentationSize;
            LoadSH();
        }

        private void ToggleRestartText(bool fullEffect = false)
        {
            if (AllowChanging && !RestartTextIsShown)
            {
                //StatusTextBlock.Content = (FullEffect) ? Properties.Resources.RestartEdiFullEff : Properties.Resources.RestartEdiEff;
                RestartTextIsShown = true;
            }
        }

 
    }
}
