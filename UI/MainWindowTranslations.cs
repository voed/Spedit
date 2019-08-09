﻿using MahApps.Metro.Controls;
using Spedit.UI.Components;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace Spedit.UI
{
	public partial class MainWindow : MetroWindow
	{
		public void Language_Translate(bool Initial = false)
		{
			if (Initial && Program.Translations.IsDefault)
			{
				return;
			}
			if (!Initial)
			{
				compileButtonDict = new ObservableCollection<string>() { Program.Translations.CompileAll, Program.Translations.CompileCurr };
				actionButtonDict = new ObservableCollection<string>() { Program.Translations.Copy, Program.Translations.FTPUp, Program.Translations.StartServer };
				findReplaceButtonDict = new ObservableCollection<string>() { Program.Translations.Replace, Program.Translations.ReplaceAll };
				((MenuItem)ConfigMenu.Items[ConfigMenu.Items.Count - 1]).Header = Program.Translations.EditConfig;
				EditorElement[] ee = GetAllEditorElements();
				if (ee != null)
                {
                    foreach (var editor in ee)
                    {
                        editor?.Language_Translate();
                    }
                }
			}
			MenuI_File.Header = Program.Translations.FileStr;
			MenuI_New.Header = Program.Translations.New;
			MenuI_Open.Header = Program.Translations.Open;
			MenuI_Save.Header = Program.Translations.Save;
			MenuI_SaveAll.Header = Program.Translations.SaveAll;
			MenuI_SaveAs.Header = Program.Translations.SaveAs;
			MenuI_Close.Header = Program.Translations.Close;
			MenuI_CloseAll.Header = Program.Translations.CloseAll;

			MenuI_Edit.Header = Program.Translations.Edit;
			MenuI_Undo.Header = Program.Translations.Undo;
			MenuI_Redo.Header = Program.Translations.Redo;
			MenuI_Cut.Header = Program.Translations.Cut;
			MenuI_Copy.Header = Program.Translations.Copy;
			MenuI_Paste.Header = Program.Translations.Paste;
			MenuI_Folding.Header = Program.Translations.Folding;
			MenuI_ExpandAll.Header = Program.Translations.ExpandAll;
			MenuI_CollapseAll.Header = Program.Translations.CollapseAll;
			MenuI_JumpTo.Header = Program.Translations.JumpTo;
			MenuI_ToggleComment.Header = Program.Translations.TogglComment;
			MenuI_SelectAll.Header = Program.Translations.SelectAll;
			MenuI_FindReplace.Header = Program.Translations.FindReplace;

			MenuI_Build.Header = Program.Translations.Build;
			MenuI_CompileAll.Header = Program.Translations.CompileAll;
			MenuI_Compile.Header = Program.Translations.CompileCurr;
			MenuI_CopyPlugin.Header = Program.Translations.CopyPlugin;
			MenuI_FTPUpload.Header = Program.Translations.FTPUp;
			MenuI_StartServer.Header = Program.Translations.StartServer;
			MenuI_SendRCon.Header = Program.Translations.SendRCon;

			ConfigMenu.Header = Program.Translations.Config;

			MenuI_Tools.Header = Program.Translations.Tools;
			OptionMenuEntry.Header = Program.Translations.Options;
			MenuI_ParsedIncDir.Header = Program.Translations.ParsedIncDir;
			MenuI_OldApiWeb.Header = Program.Translations.OldAPIWeb;
			MenuI_NewApiWeb.Header = Program.Translations.NewAPIWeb;
			MenuI_Reformatter.Header = Program.Translations.Reformatter;
			MenuI_ReformattCurr.Header = Program.Translations.ReformatCurr;
			MenuI_ReformattAll.Header = Program.Translations.ReformatAll;
            MenuI_ReportBugGit.Header = Program.Translations.ReportBugGit;
            MenuI_About.Header = Program.Translations.About;

			MenuC_FileName.Header = Program.Translations.FileName;
			MenuC_Line.Header = Program.Translations.Line;
			MenuC_Type.Header = Program.Translations.TypeStr;
			MenuC_Details.Header = Program.Translations.Details;

			NSearch_RButton.Content = Program.Translations.NormalSearch;
			WSearch_RButton.Content = Program.Translations.MatchWholeWords;
			ASearch_RButton.Content = $"{Program.Translations.AdvancSearch} (\\r, \\n, \\t, ...)";
			RSearch_RButton.Content = Program.Translations.RegexSearch;
			MenuFR_CurrDoc.Content = Program.Translations.CurrDoc;
			MenuFR_AllDoc.Content = Program.Translations.AllDoc;

			Find_Button.Content = $"{Program.Translations.Find} (F3)";
			Count_Button.Content = Program.Translations.Count;
			CCBox.Content = Program.Translations.CaseSen;
			MLRBox.Content = Program.Translations.MultilineRegex;

            OBItemText_File.Text = Program.Translations.OBTextFile;
            OBItemText_Config.Text = Program.Translations.OBTextConfig;
            OBItemText_Item.Text = Program.Translations.OBTextItem;
		}
	}
}
