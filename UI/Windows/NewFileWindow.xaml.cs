using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class NewFileWindow : MetroWindow
    {
        string PathStr = "sourcepawn\\scripts";
        Dictionary<string, TemplateInfo> TemplateDictionary;
        public NewFileWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			ParseTemplateFile();
            TemplateListBox.SelectedIndex = 0;
        }

        private void ParseTemplateFile()
        {
            TemplateDictionary = new Dictionary<string, TemplateInfo>();
            if (File.Exists("sourcepawn\\templates\\Templates.xml"))
            {
                using (Stream stream = File.OpenRead("sourcepawn\\templates\\Templates.xml"))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(stream);
                    if (doc.ChildNodes.Count > 0)
                    {
                        if (doc.ChildNodes[0].Name == "Templates")
                        {
                            XmlNode mainNode = doc.ChildNodes[0];
                            for (int i = 0; i < mainNode.ChildNodes.Count; ++i)
                            {
                                if (mainNode.ChildNodes[i].Name == "Template")
                                {
                                    XmlAttributeCollection attributes = mainNode.ChildNodes[i].Attributes;
                                    string nameStr = attributes?["Name"].Value;
                                    string fileNameStr = attributes?["File"].Value;
                                    string newNameStr = attributes?["NewName"].Value;
                                    string filePathStr = Path.Combine("sourcepawn\\templates\\", fileNameStr);
                                    if (File.Exists(filePathStr))
                                    {
                                        TemplateDictionary.Add(nameStr, new TemplateInfo { Name = nameStr, FileName = fileNameStr, Path = filePathStr, NewName = newNameStr });
                                        TemplateListBox.Items.Add(nameStr);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TemplateInfo templateInfo = TemplateDictionary[(string)TemplateListBox.SelectedItem];
            PrevieBox.Text = File.ReadAllText(templateInfo.Path);
            PathBox.Text = Path.Combine(PathStr, templateInfo.NewName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileInfo destFile = new FileInfo(PathBox.Text);
            TemplateInfo templateInfo = TemplateDictionary[(string)TemplateListBox.SelectedItem];
            File.Copy(templateInfo.Path, destFile.FullName, true);
            Program.MainWindow.TryLoadSourceFile(destFile.FullName, true, true, true);
            Close();
        }

		private void Language_Translate()
		{
            PreviewBlock.Text = $"{Properties.Resources.Preview}:";//todo remove this
			SaveButton.Content = Properties.Resources.Save;
		}

        private ICommand textBoxButtonFileCmd;

        public ICommand TextBoxButtonFileCmd
        {
            set { }
            get
            {
                if (textBoxButtonFileCmd == null)
                {
                    var cmd = new SimpleCommand
                    {
                        CanExecutePredicate = o => true,
                        ExecuteAction = o =>
                        {
                            if (o is TextBox box)
                            {
                                var dialog = new SaveFileDialog
                                {
                                    AddExtension = true,
                                    Filter = "Sourcepawn Files (*.sp *.inc)|*.sp;*.inc|All Files (*.*)|*.*",
                                    OverwritePrompt = true,
                                    Title = Properties.Resources.NewFile
                                };
                                var result = dialog.ShowDialog();
                                if (result != null && result.Value)
                                {
                                    box.Text = dialog.FileName;
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

    public class TemplateInfo
    {
        public string Name;
        public string FileName;
        public string Path;
        public string NewName;
    }
}
