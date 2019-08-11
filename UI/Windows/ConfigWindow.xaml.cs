using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Spedit.Interop;
using Spedit.Utils;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextBox = System.Windows.Controls.TextBox;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ConfigWindow : MetroWindow
    {
        private bool NeedsSMDefInvalidation;
        private bool AllowChange;

        public ConfigWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			foreach (Config config in Program.ConfigList.Configs)
            {
                ConfigListBox.Items.Add(new ListBoxItem { Content = config.Name });
            }
            ConfigListBox.SelectedIndex = Program.ConfigList.CurrentConfig;
        }

        private void ConfigListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadConfigToUI(ConfigListBox.SelectedIndex);
        }

        private void LoadConfigToUI(int index)
        {
            if (index < 0 || index >= Program.ConfigList.Configs.Count)
            {
                return;
            }
            AllowChange = false;
            Config c = Program.ConfigList.Configs[index];
            C_Name.Text = c.Name;
            StringBuilder SMDirOut = new StringBuilder();
            foreach (string dir in c.SMDirectories) { SMDirOut.Append(dir.Trim() + ";"); }
            C_SMDir.Text = SMDirOut.ToString();
            C_AutoCopy.IsChecked = c.AutoCopy;
            C_CopyDir.Text = c.CopyDirectory;
            C_ServerFile.Text = c.ServerFile;
            C_ServerArgs.Text = c.ServerArgs;
            C_PreBuildCmd.Text = c.PreCmd;
            C_PostBuildCmd.Text = c.PostCmd;
            C_OptimizationLevel.Value = c.OptimizeLevel;
            C_VerboseLevel.Value = c.VerboseLevel;
            C_DeleteAfterCopy.IsChecked = c.DeleteAfterCopy;
            C_FTPHost.Text = c.FTPHost;
            C_FTPUser.Text = c.FTPUser;
            C_FTPPW.Password = c.FTPPassword;
            C_FTPDir.Text = c.FTPDir;
            C_RConEngine.SelectedIndex = (c.RConUseSourceEngine) ? 0 : 1;
            C_RConIP.Text = c.RConIP;
            C_RConPort.Text = c.RConPort.ToString();
            C_RConPW.Password = c.RConPassword;
            C_RConCmds.Text = c.RConCommands;
            AllowChange = true;
        }

        private void NewButton_Clicked(object sender, RoutedEventArgs e)
        {
            Program.ConfigList.Configs.Add(new Config { Name = Program.Translations.NewConfig, Standard = false, OptimizeLevel = 2, VerboseLevel = 1 });
            ConfigListBox.Items.Add(new ListBoxItem { Content = Program.Translations.NewConfig });
        }

        private void DeleteButton_Clicked(object sender, RoutedEventArgs e)
        {
            int index = ConfigListBox.SelectedIndex;
            if (Program.ConfigList.Configs[index].Standard)
            {
                this.ShowMessageAsync(Program.Translations.CannotDelConf, Program.Translations.YCannotDelConf, MessageDialogStyle.Affirmative, MetroDialogOptions);
                return;
            }

            Program.ConfigList.Configs.RemoveAt(index);
            ConfigListBox.Items.RemoveAt(index);
            if (index == Program.ConfigList.CurrentConfig)
            {
                Program.ConfigList.CurrentConfig = 0;
            }
            ConfigListBox.SelectedIndex = 0;
        }

        private void C_Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange)
                return; 
            string name = C_Name.Text;
            Program.ConfigList.Configs[ConfigListBox.SelectedIndex].Name = name;
            ((ListBoxItem)ConfigListBox.SelectedItem).Content = name;
        }

        private void C_SMDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!AllowChange) { return; }
            string[] SMDirs = C_SMDir.Text.Split(';');
            Program.ConfigList.Configs[ConfigListBox.SelectedIndex].SMDirectories = SMDirs.Select(dir => dir.Trim()).ToArray();
            NeedsSMDefInvalidation = true;
        }

        private void C_CopyDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].CopyDirectory = C_CopyDir.Text;
        }

        private void C_ServerFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].ServerFile = C_ServerFile.Text;
        }

        private void C_ServerArgs_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].ServerArgs = C_ServerArgs.Text;
        }

        private void C_PostBuildCmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].PostCmd = C_PostBuildCmd.Text;
        }

        private void C_PreBuildCmd_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].PreCmd = C_PreBuildCmd.Text;
        }

        private void C_OptimizationLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].OptimizeLevel = (int)C_OptimizationLevel.Value;
        }

        private void C_VerboseLevel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].VerboseLevel = (int)C_VerboseLevel.Value;
        }

        private void C_AutoCopy_Changed(object sender, RoutedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].AutoCopy = C_AutoCopy.IsChecked.Value;
        }

        private void C_DeleteAfterCopy_Changed(object sender, RoutedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].DeleteAfterCopy = C_DeleteAfterCopy.IsChecked.Value;
        }

        private void C_FTPHost_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].FTPHost = C_FTPHost.Text;
        }

        private void C_FTPUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].FTPUser = C_FTPUser.Text;
        }

        private void C_FTPPW_TextChanged(object sender, RoutedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].FTPPassword = C_FTPPW.Password;
        }

        private void C_FTPDir_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].FTPDir = C_FTPDir.Text;
        }

        private void C_RConEngine_Changed(object sender, RoutedEventArgs e)
        {
            if (AllowChange && ConfigListBox.SelectedIndex >= 0)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].RConUseSourceEngine = (C_RConEngine.SelectedIndex == 0);
        }

        private void C_RConIP_TextChanged(object sender, RoutedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].RConIP = C_RConIP.Text;
        }

        private void C_RConPort_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!AllowChange) { return; }
            ushort newPort;
            if (!ushort.TryParse(C_RConPort.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out newPort))
            {
                newPort = 27015;
                C_RConPort.Text = "27015";
            }
            Program.ConfigList.Configs[ConfigListBox.SelectedIndex].RConPort = newPort;
        }

        private void C_RConPW_TextChanged(object sender, RoutedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].RConPassword = C_RConPW.Password;
        }

        private void C_RConCmds_TextChanged(object sender, RoutedEventArgs e)
        {
            if (AllowChange)
                Program.ConfigList.Configs[ConfigListBox.SelectedIndex].RConCommands = C_RConCmds.Text;
        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            if (NeedsSMDefInvalidation)
            {
                foreach (Config config in Program.ConfigList.Configs)
                {
                    config.InvalidateSMDef();
                }
            }
            Program.MainWindow.FillConfigMenu();
            Program.MainWindow.ChangeConfig(Program.ConfigList.Current.Name);

            ConfigLoader.Save(Program.ConfigList);
        }

		private void Language_Translate()
		{
			if (Program.Translations.IsDefault)
			{
				return;
			}
			NewButton.Content = Program.Translations.New;
			DeleteButton.Content = Program.Translations.Delete;
			NameBlock.Text = Program.Translations.Name;
			ScriptingDirBlock.Text = Program.Translations.ScriptDir;
			DelimitWiBlock.Text = $"({Program.Translations.DelimiedWi} ; )";
			CopyDirBlock.Text = Program.Translations.CopyDir;
			ServerExeBlock.Text = Program.Translations.ServerExe;
			ServerStartArgBlock.Text = Program.Translations.serverStartArgs;
			PreBuildBlock.Text = Program.Translations.PreBuildCom;
			PostBuildBlock.Text = Program.Translations.PostBuildCom;
			OptimizeBlock.Text = Program.Translations.OptimizeLvl;
			VerboseBlock.Text = Program.Translations.VerboseLvl;
			C_AutoCopy.Content = Program.Translations.AutoCopy;
			C_DeleteAfterCopy.Content = Program.Translations.DeleteOldSMX;
			FTPHostBlock.Text = Program.Translations.FTPHost;
			FTPUserBlock.Text = Program.Translations.FTPUser;
			FTPPWBlock.Text = Program.Translations.FTPPw;
			FTPDirBlock.Text = Program.Translations.FTPDir;
			CMD_ItemC.Text = Program.Translations.CMDLineCom;
			ItemC_EditorDir.Content = "{editordir} - " + Program.Translations.ComEditorDir;
			ItemC_ScriptDir.Content = "{scriptdir} - " + Program.Translations.ComScriptDir;
			ItemC_CopyDir.Content = "{copydir} - " + Program.Translations.ComCopyDir;
			ItemC_ScriptFile.Content = "{scriptfile} - " + Program.Translations.ComScriptFile;
			ItemC_ScriptName.Content = "{scriptname} - " + Program.Translations.ComScriptName;
			ItemC_PluginFile.Content = "{pluginfile} - " + Program.Translations.ComPluginFile;
			ItemC_PluginName.Content = "{pluginname} - " + Program.Translations.ComPluginName;
			RConEngineBlock.Text = Program.Translations.RConEngine;
			RConIPBlock.Text = Program.Translations.RConIP;
			RConPortBlock.Text = Program.Translations.RconPort;
			RConPWBlock.Text = Program.Translations.RconPw;
			RConComBlock.Text = Program.Translations.RconCom;
			Rcon_MenuC.Text = Program.Translations.RConCMDLineCom;
			MenuC_PluginsReload.Content = "{plugins_reload} - " + Program.Translations.ComPluginsReload;
			MenuC_PluginsLoad.Content = "{plugins_load} - " + Program.Translations.ComPluginsLoad;
			MenuC_PluginsUnload.Content = "{plugins_unload} - " + Program.Translations.ComPluginsUnload;
		}

		private ICommand textBoxButtonFolderCmd;

        public ICommand TextBoxButtonFolderCmd
        {
            set { }
            get
            {
                if (textBoxButtonFolderCmd == null)
                {
                    var cmd = new SimpleCommand();
                    cmd.CanExecutePredicate = o =>
                    {
                        return true;
                    };
                    cmd.ExecuteAction = o =>
                    {
                        if (o is TextBox box)
                        {
                            var dialog = new FolderBrowserDialog();
                            var result = dialog.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                box.Text = dialog.SelectedPath;
                            }
                        }
                    };
                    textBoxButtonFolderCmd = cmd;
                    return cmd;
                }

                return textBoxButtonFolderCmd;
            }
        }

        private ICommand textBoxButtonFileCmd;

        public ICommand TextBoxButtonFileCmd
        {
            set { }
            get
            {
                if (textBoxButtonFileCmd == null)
                {
                    var cmd = new SimpleCommand();
                    cmd.CanExecutePredicate = o =>
                    {
                        return true;
                    };
                    cmd.ExecuteAction = o =>
                    {
                        if (o is TextBox box)
                        {
                            var dialog = new OpenFileDialog();
                            dialog.Filter = "Executables *.exe|*.exe|All Files *.*|*.*";
                            dialog.Multiselect = false;
                            dialog.CheckFileExists = true; dialog.CheckPathExists = true;
                            dialog.Title = Program.Translations.SelectExe;
                            var result = dialog.ShowDialog();
                            if (result.Value)
                            {
                                FileInfo fInfo = new FileInfo(dialog.FileName);
                                if (fInfo.Exists)
                                {
                                    box.Text = fInfo.FullName;
                                }
                            }
                        }
                    };
                    textBoxButtonFileCmd = cmd;
                    return cmd;
                }

                return textBoxButtonFileCmd;
            }
        }


        private class SimpleCommand : ICommand
        {
            public Predicate<object> CanExecutePredicate { get; set; }
            public Action<object> ExecuteAction { get; set; }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public void Execute(object parameter)
            {
                ExecuteAction?.Invoke(parameter);
            }
        }

    }
}
