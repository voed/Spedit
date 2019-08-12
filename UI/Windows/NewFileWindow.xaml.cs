using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using Spedit.Interop;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class NewFileWindow : MetroWindow
    {
        private const string PathStr = "amxmodx\\scripts";
        private const string TemplatePath = @"amxmodx\templates\Templates.xml";

        [XmlElement("Templates")]
        public List<TemplateInfo> TemplateList;
        public NewFileWindow()
        {
            InitializeComponent();
            if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			ParseTemplateFile();
            TemplateListBox.SelectedIndex = 0;
        }

        private void ParseTemplateFile()
        {
            TemplateList = new List<TemplateInfo>();
            if (File.Exists(TemplatePath))
            {
                using (Stream stream = File.OpenRead(TemplatePath))
                {
                    TemplateList = (List<TemplateInfo>)new XmlSerializer(typeof(List<TemplateInfo>), new XmlRootAttribute("Templates")).Deserialize(stream);
                    TemplateListBox.ItemsSource = TemplateList.Select(template => template.Name).ToList();
                }
            }
        }

        private void TemplateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TemplateInfo templateInfo = TemplateList[TemplateListBox.SelectedIndex];
            PrevieBox.Text = File.ReadAllText(templateInfo.Path);
            PathBox.Text = Path.Combine(PathStr, templateInfo.NewName);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FileInfo destFile = new FileInfo(PathBox.Text);
            TemplateInfo templateInfo = TemplateList[TemplateListBox.SelectedIndex];
            File.Copy(templateInfo.Path, destFile.FullName, true);
            Program.MainWindow.TryLoadSourceFile(destFile.FullName, true, true, true);
            Close();
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

    [XmlType(TypeName = "Template")]
    public class TemplateInfo
    {
        private static readonly string _folderPath = @"amxmodx\templates\";
        [XmlAttribute] 
        public string Name = string.Empty;
        [XmlAttribute("File")]
        public string FileName = string.Empty;

        [XmlIgnore]
        public string Path => System.IO.Path.Combine(_folderPath, FileName);

        [XmlAttribute]
        public string NewName = string.Empty;

    }
}
