using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using SourcepawnCondenser.SourcemodDefinition;

namespace Spedit.UI.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class SPDefinitionWindow : MetroWindow
    {
        SPDefEntry[] defArray;
        ListViewItem[] items;
        Timer searchTimer = new Timer(1000.0);

        public SPDefinitionWindow()
        {
            InitializeComponent();
			Language_Translate();
			if (Program.OptionsObject.Program_AccentColor != "Red" || Program.OptionsObject.Program_Theme != "BaseDark")
			{ ThemeManager.ChangeAppStyle(this, ThemeManager.GetAccent(Program.OptionsObject.Program_AccentColor), ThemeManager.GetAppTheme(Program.OptionsObject.Program_Theme)); }
			errorSearchBoxBrush.Freeze();
            var def = Program.ConfigList.Current.GetSMDef();
            if (def == null)
            {
                MessageBox.Show(Properties.Resources.ConfigWrongPars, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Warning);
                Close();
                return;
            }
            List<SPDefEntry> defList = def.Functions.Cast<SPDefEntry>().ToList();
            defList.AddRange(def.Constants.Cast<SPDefEntry>());
            defList.AddRange(def.Enums.Cast<SPDefEntry>());
            defList.AddRange(def.Defines.Cast<SPDefEntry>());

            foreach (var e in defList)
			{
				if (string.IsNullOrWhiteSpace(e.Name))
				{
					e.Name = $"--{Properties.Resources.NoName}--";
				}
			}
			defList.Sort((a, b) => string.Compare(a.Name, b.Name));
            defArray = defList.ToArray();
            int defArrayLength = defArray.Length;
            items = new ListViewItem[defArrayLength];
            for (int i = 0; i < defArrayLength; ++i)
            {
                items[i] = new ListViewItem { Content = defArray[i].Name, Tag = defArray[i].Entry };
                SPBox.Items.Add(items[i]);
            }
            searchTimer.Elapsed += searchTimer_Elapsed;
        }

        void searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DoSearch();
			searchTimer.Stop();
        }

        private void SPFunctionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object obj = SPBox.SelectedItem;
            if (obj == null) { return; }
            ListViewItem item = (ListViewItem)obj;
            object TagValue = item.Tag;
            if (TagValue != null)
            {
                switch (TagValue)
                {
                    case SMFunction sm:
                    {
                        SPNameBlock.Text = sm.Name;
                        SPFullNameBlock.Text = sm.FullName;
                        SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Properties.Resources.PosLen, sm.Index, sm.Length)})";
                        SPTypeBlock.Text = "Function";
                        SPCommentBox.Text = sm.CommentString;
                        return;
                    }

                    case SMConstant sm:
                    {
                        SPNameBlock.Text = sm.Name;
                        SPFullNameBlock.Text = string.Empty;
                        SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Properties.Resources.PosLen, sm.Index, sm.Length)})";
                        SPTypeBlock.Text = "Constant";
                        SPCommentBox.Text = string.Empty;
                        return;
                    }

                    case SMEnum sm:
                    {
                        SPNameBlock.Text = sm.Name;
                        SPFullNameBlock.Text = string.Empty;
                        SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Properties.Resources.PosLen, sm.Index, sm.Length)})";
                        SPTypeBlock.Text = "Enum " + sm.Entries.Length + " entries";
                        StringBuilder outString = new StringBuilder();
                        for (int i = 0; i < sm.Entries.Length; ++i)
                        {
                            outString.Append((i + ".").PadRight(5, ' '));
                            outString.AppendLine(sm.Entries[i]);
                        }
                        SPCommentBox.Text = outString.ToString();
                        return;
                    }

                    case SMDefine sm:
                    {
                        SPNameBlock.Text = sm.Name;
                        SPFullNameBlock.Text = string.Empty;
                        SPFileBlock.Text = sm.File + ".inc" + $" ({string.Format(Properties.Resources.PosLen, sm.Index, sm.Length)})";
                        SPTypeBlock.Text = "Definition";
                        SPCommentBox.Text = string.Empty;
                        return;
                    }

                    case string value:
                        SPNameBlock.Text = (string)item.Content;
                        SPFullNameBlock.Text = value;
                        SPFileBlock.Text = string.Empty;
                        SPCommentBox.Text = string.Empty;
                        return;
                }
            }
            SPNameBlock.Text = (string)item.Content;
            SPFullNameBlock.Text = string.Empty;
			SPFileBlock.Text = string.Empty;
			SPTypeBlock.Text = string.Empty;
			SPCommentBox.Text = string.Empty;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SPProgress.IsIndeterminate = true;
            searchTimer.Stop();
            searchTimer.Start();
        }

		Brush errorSearchBoxBrush = new SolidColorBrush(Color.FromArgb(0x50, 0xA0, 0x30, 0));
        private void DoSearch()
        {
            Dispatcher?.Invoke(() =>
                {
                    int itemCount = defArray.Length;
                    string searchString = SPSearchBox.Text.ToLowerInvariant();
                    bool foundOccurence = false;
                    SPBox.Items.Clear();
                    for (int i = 0; i < itemCount; ++i)
                    {
                        if (defArray[i].Name.ToLowerInvariant().Contains(searchString))
                        {
                            foundOccurence = true;
                            SPBox.Items.Add(items[i]);
                        }
                    }
                    if (foundOccurence)
                    {
                        SPSearchBox.Background = Brushes.Transparent;
                    }
                    else
                    {
                        SPSearchBox.Background = errorSearchBoxBrush;
                    }
                    SPProgress.IsIndeterminate = false;
                });
        }

		private void Language_Translate()
		{
			TextBoxHelper.SetWatermark(SPSearchBox, Properties.Resources.Search);
			/*if (Properties.Resources.IsDefault)
			{
				return;
			}*/
		}

		private class SPDefEntry
        {
            public string Name;
            public object Entry;

			public static explicit operator SPDefEntry(SMFunction func)
			{
				return new SPDefEntry { Name = func.Name, Entry = func };
			}
			public static explicit operator SPDefEntry(SMConstant sm)
			{
				return new SPDefEntry { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMDefine sm)
			{
				return new SPDefEntry { Name = sm.Name, Entry = sm };
			}
			public static explicit operator SPDefEntry(SMEnum sm)
			{
				return new SPDefEntry { Name = sm.Name, Entry = sm };
			}
        }
    }
}
