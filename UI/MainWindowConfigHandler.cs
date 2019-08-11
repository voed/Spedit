using System.Linq;
using Spedit.UI.Components;
using Spedit.UI.Windows;
using System.Windows;
using System.Windows.Controls;
using Spedit.Interop;

namespace Spedit.UI
{
    public partial class MainWindow
    {
        public void FillConfigMenu()
        {
            ConfigMenu.Items.Clear();
            foreach (MenuItem item in Program.ConfigList.Configs.Select(config => new MenuItem
            {
                Header = config.Name, 
                IsCheckable = true, 
                IsChecked = config == Program.ConfigList.Current
            }))
            {
                item.Click += item_Click;
                ConfigMenu.Items.Add(item);
            }

            ConfigMenu.Items.Add(new Separator());
            MenuItem editItem = new MenuItem() { Header = Properties.Resources.EditConfig };
            editItem.Click += editItem_Click;
            ConfigMenu.Items.Add(editItem);
        }

        private void editItem_Click(object sender, RoutedEventArgs e)
        {
            ConfigWindow configWindow = new ConfigWindow() { Owner = this, ShowInTaskbar = false };
            configWindow.ShowDialog();
        }

        private void item_Click(object sender, RoutedEventArgs e)
        {
            string name = (string)(((MenuItem)sender).Header);
            ChangeConfig(name);
        }

        private void ConfigSelected(object sender, RoutedEventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            string Name = (string)item.Header;
        }
        

        public void ChangeConfig(string name)
        {
            foreach (Config config in Program.ConfigList.Configs.Where(config => config.Name == name))
            {
                config.LoadSMDef();
                for (int i = 0; i < ConfigMenu.Items.Count - 2; ++i)
                {
                    ((MenuItem)ConfigMenu.Items[i]).IsChecked = (name == (string)(((MenuItem)ConfigMenu.Items[i]).Header));
                }

                Program.ConfigList.Current = config;

                Program.OptionsObject.Program_SelectedConfig = name;

                foreach (EditorElement element in GetAllEditorElements())
                {
                    element.LoadAutoCompletes();
                    element.editor.SyntaxHighlighting = new AeonEditorHighlighting();
                    element.InvalidateVisual();
                }
            }

        }

    }
}
