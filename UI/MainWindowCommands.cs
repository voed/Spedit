using System;
using System.Linq;
using System.Text;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Spedit.UI.Components;
using Spedit.UI.Windows;
using Spedit.Utils.SPSyntaxTidy;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public EditorElement GetCurrentEditorElement()
        {
            EditorElement outElement = null;
            if (DockingPane.SelectedContent?.Content != null)
            {
                var possElement = DockingManager.ActiveContent;
                if (possElement is EditorElement element)
                {
                    outElement = element;
                }
            }
            return outElement;
        }

        public EditorElement[] GetAllEditorElements()
        {
            return EditorsReferences.ToArray();
        }

        private void Command_New()
        {
            NewFileWindow nfWindow = new NewFileWindow { Owner = this, ShowInTaskbar = false };
            nfWindow.ShowDialog();
        }

        private void Command_Open()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog
                {
                    AddExtension = true, CheckFileExists = true, CheckPathExists = true,
                    Filter = "AMXXPawn Files|*.sma;*.inc;*.cfg;*.json;*.txt;*.ini|All Files (*.*)|*.*",
                    Multiselect = true, Title = Program.Translations.OpenNewFile
                };
                var result = ofd.ShowDialog();//this);
                if (result.Value)
                {
                    bool AnyFileLoaded = false;
                    if (ofd.FileNames.Length > 0)
                    {
                        for (int i = 0; i < ofd.FileNames.Length; ++i)
                        {
                            AnyFileLoaded |= TryLoadSourceFile(ofd.FileNames[i], (i == 0), true, (i == 0));
                        }

                        if (!AnyFileLoaded)
                        {
                            MetroDialogOptions.ColorScheme = MetroDialogColorScheme.Theme;
                            this.ShowMessageAsync(Program.Translations.NoFileOpened,
                                Program.Translations.NoFileOpenedCap, MessageDialogStyle.Affirmative,
                                MetroDialogOptions);
                        }
                    }
                }

                Activate();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void Command_Save()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                ee.Save(true);
                BlendOverEffect.Begin();
            }
        }

        private void Command_SaveAs()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee != null)
            {
                SaveFileDialog sfd = new SaveFileDialog
                {
                    AddExtension = true,
                    Filter = @"AMXXPawn Files|*.sma;*.inc;*.cfg;*.json;*.txt;*.ini|All Files (*.*)|*.*",
                    OverwritePrompt = true,
                    Title = Program.Translations.SaveFileAs,
                    FileName = ee.Parent.Title.Trim('*')
                };
                var result = sfd.ShowDialog(this);
                if (result.Value && !string.IsNullOrWhiteSpace(sfd.FileName))
                {
                    ee.FullFilePath = sfd.FileName;
                    ee.Save(true);
                    BlendOverEffect.Begin();
                }
            }
        }

        private void Command_SaveAll()
        {
            EditorElement[] editors = GetAllEditorElements();
            if (editors == null)
            {
                return;
            }
            if (editors.Length > 0)
            {
                foreach (var editor in editors)
                {
                    editor.Save();
                }

                BlendOverEffect.Begin();
            }
        }

        private void Command_Close()
        {
            EditorElement ee = GetCurrentEditorElement();
            ee?.Close();
        }

        private async void Command_CloseAll()
        {
            EditorElement[] editors = GetAllEditorElements();
            if (editors == null)
            {
                return;
            }
            if (editors.Length > 0)
            {
                bool unsavedEditorsExisting = editors.Aggregate(false, (current, t) => current | t.NeedsSave);
                bool forceSave = false;
                if (unsavedEditorsExisting)
                {
                    StringBuilder str = new StringBuilder();
                    for (int i = 0; i < editors.Length; ++i)
                    {
                        if (i == 0)
                        { str.Append(editors[i].Parent.Title.Trim('*')); }
                        else
                        { str.AppendLine(editors[i].Parent.Title.Trim('*')); }
                    }
                    var result = await this.ShowMessageAsync(Program.Translations.SaveFollow, str.ToString(), MessageDialogStyle.AffirmativeAndNegative, MetroDialogOptions);
                    if (result == MessageDialogResult.Affirmative)
                    {
                        forceSave = true;
                    }
                }
                foreach (var editor in editors)
                {
                    editor.Close(forceSave, forceSave);
                }
            }
        }

        private void Command_Undo()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee?.editor.CanUndo == true)
            {
                ee.editor.Undo();
            }
        }

        private void Command_Redo()
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee?.editor.CanRedo == true)
            {
                ee.editor.Redo();
            }
        }

        private void Command_Cut()
        {
            EditorElement ee = GetCurrentEditorElement();
            ee?.editor.Cut();
        }

        private void Command_Copy()
        {
            EditorElement ee = GetCurrentEditorElement();
            ee?.editor.Copy();
        }

        private void Command_Paste()
        {
            EditorElement ee = GetCurrentEditorElement();
            ee?.editor.Paste();
        }

        private void Command_FlushFoldingState(bool state)
        {
            EditorElement ee = GetCurrentEditorElement();
            if (ee?.foldingManager != null)
            {
                var foldings = ee.foldingManager.AllFoldings;
                foreach (var folding in foldings)
                {
                    folding.IsFolded = state;
                }
            }
        }

        private void Command_JumpTo()
        {
            EditorElement ee = GetCurrentEditorElement();
            ee?.ToggleJumpGrid();
        }

        private void Command_SelectAll()
        {
            EditorElement ee = GetCurrentEditorElement();
            ee?.editor.SelectAll();
        }

		private void Command_ToggleCommentLine()
		{
			EditorElement ee = GetCurrentEditorElement();
            ee?.ToggleCommentOnLine();
        }

        private void Command_TidyCode(bool All)
        {
            var editors = All ? GetAllEditorElements() : new[] { GetCurrentEditorElement() };
            foreach (var ee in editors)
            {
                ee?.editor.Document.BeginUpdate();
                string source = ee?.editor.Text;
                ee?.editor.Document.Replace(0, source.Length, SPSyntaxTidy.TidyUp(source));
                ee?.editor.Document.EndUpdate();
            }
        }

    }
}
