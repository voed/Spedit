﻿using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.UI.Components;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class OptionsWindow : MetroWindow
    {
		string[] AvailableAccents = { "Red", "Green", "Blue", "Purple", "Orange", "Lime", "Emerald", "Teal", "Cobalt", "Indigo", "Violet", "Pink", "Magenta", "Crimson", "Amber",
			"Yellow", "Brown", "Olive", "Steel", "Mauve", "Taupe", "Sienna" };
        bool RestartTextIsShown;
        bool AllowChanging;
        public OptionsWindow()
        {
            InitializeComponent();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			LoadSettings();
            AllowChanging = true;
        }

        private async void RestoreButton_Clicked(object sender, RoutedEventArgs e)
        {
            var result = await this.ShowMessageAsync(Properties.Resources.ResetOptions, Properties.Resources.ResetOptQues, MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                Program.OptionsObject = new OptionsControl();
                Program.MainWindow.OptionMenuEntry.IsEnabled = false;
                await this.ShowMessageAsync(Properties.Resources.RestartEditor, Properties.Resources.YRestartEditor, MessageDialogStyle.Affirmative, MetroDialogOptions);
                Close();
            }
        }

        private void HardwareAcc_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_UseHardwareAcceleration = HardwareAcc.IsChecked.Value;
            ToggleRestartText();
        }

        private void UIAnimation_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.UI_Animations = UIAnimation.IsChecked.Value;
            ToggleRestartText();
        }

        private void AutoOpenInclude_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_OpenCustomIncludes = OpenIncludes.IsChecked.Value;
            OpenIncludesRecursive.IsEnabled = OpenIncludes.IsChecked.Value;
        }
        private void OpenIncludeRecursively_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_OpenIncludesRecursively = OpenIncludesRecursive.IsChecked.Value;
        }

        private void AutoUpdate_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Program_CheckForUpdates = AutoUpdate.IsChecked.Value;
        }

        private void ShowToolbar_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }

            if (ShowToolBar.IsChecked != null)
                Program.OptionsObject.UI_ShowToolBar = ShowToolBar.IsChecked.Value;
            Program.MainWindow.Win_ToolBar.Height = Program.OptionsObject.UI_ShowToolBar ? double.NaN : 0.0;
        }

		private void DynamicISAC_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_DynamicISAC = DynamicISAC.IsChecked.Value;
		}

		private void DarkTheme_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			if (DarkTheme.IsChecked.Value)
			{ Program.OptionsObject.Program_Theme = "BaseDark"; }
			else
			{ Program.OptionsObject.Program_Theme = "BaseLight"; }
			ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ThemeManager.ChangeAppStyle(Program.MainWindow, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ToggleRestartText(true);
		}

		private void AccentColor_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Program_AccentColor = (string)AccentColor.SelectedItem;
			ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
			ThemeManager.ChangeAppStyle(Program.MainWindow, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme));
		}

		private void FontSize_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            double size = FontSizeD.Value;
            Program.OptionsObject.Editor_FontSize = size;
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
            Program.OptionsObject.Editor_ScrollLines = ScrollSpeed.Value;
        }

        private void WordWrap_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool wrapping = WordWrap.IsChecked.Value;
            Program.OptionsObject.Editor_WordWrap = wrapping;
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
            Program.OptionsObject.Editor_AgressiveIndentation = AgressiveIndentation.IsChecked.Value;
        }

        private void LineReformat_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            Program.OptionsObject.Editor_ReformatLineAfterSemicolon = LineReformatting.IsChecked.Value;
        }

        private void TabToSpace_Changed(object sender, RoutedEventArgs e)
        {
            if (!AllowChanging) { return; }
            bool replaceTabs = TabToSpace.IsChecked.Value;
            Program.OptionsObject.Editor_ReplaceTabsToWhitespace = replaceTabs;
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
            if (editors != null)
            {
                for (int i = 0; i < editors.Length; ++i)
                {
                    editors[i].editor.Options.ConvertTabsToSpaces = replaceTabs;
                }
            }
        }

		private void AutoCloseBrackets_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Editor_AutoCloseBrackets = AutoCloseBrackets.IsChecked ?? true;
		}

		private void AutoCloseStringChars_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			Program.OptionsObject.Editor_AutoCloseStringChars = AutoCloseStringChars.IsChecked ?? true;
		}

		private void ShowSpaces_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			bool showSpacesValue = Program.OptionsObject.Editor_ShowSpaces = ShowSpaces.IsChecked ?? true;
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
			bool showTabsValue = Program.OptionsObject.Editor_ShowTabs = ShowTabs.IsChecked ?? true;
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
            Program.OptionsObject.Editor_FontFamily = familyString;
            FontFamilyTB.Content = "Font (" + familyString + "):";
            EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
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
			int indentationSizeValue = Program.OptionsObject.Editor_IndentationSize = (int)Math.Round(IndentationSize.Value);
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
            Program.OptionsObject.SH_HighlightDeprecateds = HighlightDeprecateds.IsChecked ?? true;
            ToggleRestartText();
        }

		private void LanguageBox_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			string selectedString = (string)LanguageBox.SelectedItem;
            ToggleRestartText(true);
		}


		private void AutoSave_Changed(object sender, RoutedEventArgs e)
		{
			if (!AllowChanging) { return; }
			int newIndex = AutoSave.SelectedIndex;
			EditorElement[] editors = Program.MainWindow.GetAllEditorElements();
			if (newIndex == 0)
			{
				Program.OptionsObject.Editor_AutoSave = false;
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
				Program.OptionsObject.Editor_AutoSave = true;
				switch (newIndex)
                {
                    case 1:
                        Program.OptionsObject.Editor_AutoSaveInterval = 30;
                        break;
                    case 7:
                        Program.OptionsObject.Editor_AutoSaveInterval = 600;
                        break;
                    case 8:
                        Program.OptionsObject.Editor_AutoSaveInterval = 900;
                        break;
                    default:
                        Program.OptionsObject.Editor_AutoSaveInterval = (newIndex - 1) * 60;
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
			foreach (string accent in AvailableAccents)
            {
                AccentColor.Items.Add(accent);
            }
            HardwareAcc.IsChecked = Program.OptionsObject.Program_UseHardwareAcceleration;
            UIAnimation.IsChecked = Program.OptionsObject.UI_Animations;
            OpenIncludes.IsChecked = Program.OptionsObject.Program_OpenCustomIncludes;
            OpenIncludesRecursive.IsChecked = Program.OptionsObject.Program_OpenIncludesRecursively;
            AutoUpdate.IsChecked = Program.OptionsObject.Program_CheckForUpdates;
            if (!Program.OptionsObject.Program_OpenCustomIncludes)
            {
                OpenIncludesRecursive.IsEnabled = false;
            }
            ShowToolBar.IsChecked = Program.OptionsObject.UI_ShowToolBar;
			DynamicISAC.IsChecked = Program.OptionsObject.Program_DynamicISAC;
			DarkTheme.IsChecked = (Program.OptionsObject.Program_Theme == "BaseDark");
			for (int i = 0; i < AvailableAccents.Length; ++i)
			{
				if (AvailableAccents[i] == Program.OptionsObject.Program_AccentColor)
				{
					AccentColor.SelectedIndex = i;
					break;
				}
			}
            if (Program.OptionsObject.Editor_AutoSave)
			{
				int seconds = Program.OptionsObject.Editor_AutoSaveInterval;
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
            HighlightDeprecateds.IsChecked = Program.OptionsObject.SH_HighlightDeprecateds;
            FontSizeD.Value = Program.OptionsObject.Editor_FontSize;
            ScrollSpeed.Value = Program.OptionsObject.Editor_ScrollLines;
            WordWrap.IsChecked = Program.OptionsObject.Editor_WordWrap;
            AgressiveIndentation.IsChecked = Program.OptionsObject.Editor_AgressiveIndentation;
            LineReformatting.IsChecked = Program.OptionsObject.Editor_ReformatLineAfterSemicolon;
            TabToSpace.IsChecked = Program.OptionsObject.Editor_ReplaceTabsToWhitespace;
			AutoCloseBrackets.IsChecked = Program.OptionsObject.Editor_AutoCloseBrackets;
			AutoCloseStringChars.IsChecked = Program.OptionsObject.Editor_AutoCloseStringChars;
			ShowSpaces.IsChecked = Program.OptionsObject.Editor_ShowSpaces;
			ShowTabs.IsChecked = Program.OptionsObject.Editor_ShowTabs;
			FontFamilyTB.Content = $"{Properties.Resources.FontFamily} ({Program.OptionsObject.Editor_FontFamily}):";
            FontFamilyCB.SelectedValue = new FontFamily(Program.OptionsObject.Editor_FontFamily);
			IndentationSize.Value = Program.OptionsObject.Editor_IndentationSize;
            LoadSH();
        }

        private void ToggleRestartText(bool FullEffect = false)
        {
            if (AllowChanging && !RestartTextIsShown)
            {
                StatusTextBlock.Content = (FullEffect) ? Properties.Resources.RestartEdiFullEff : Properties.Resources.RestartEdiEff;
                RestartTextIsShown = true;
            }
        }


    }
}
