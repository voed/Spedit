using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using System.Text;
using System.IO;
using SourcepawnCondenser.Tokenizer;
using SourcepawnCondenser.SourcemodDefinition;

namespace CondenserTest
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			StringBuilder str = new StringBuilder();
			List<string> files = new List<string>();
			files.AddRange(Directory.GetFiles(@"C:\Users\Jelle\Desktop\coding\sm-scripting\_Sourcemod Plugins\1.7_5255", "*.inc", SearchOption.AllDirectories));
			str.AppendLine(files.Count.ToString());
			foreach (var f in files)
			{
				str.AppendLine(File.ReadAllText(f));
			}
            ExpandBox.IsChecked = false;
            //str.AppendLine(File.ReadAllText(@"C:\Users\Jelle\Desktop\coding\sm-scripting\CondenserTestFile.inc"));
            textBox.Text = str.ToString();
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string text = textBox.Text;
			Stopwatch watch = new Stopwatch();
			watch.Start();
			List<Token> tList = Tokenizer.TokenizeString(text, false);
			watch.Stop();
			Token[] t = tList.ToArray();
			double tokenToTextLength = (double)t.Length / (double)text.Length;
			string subTitle = watch.ElapsedMilliseconds + " ms  -  tokenL/textL: " + tokenToTextLength + "  (" + t.Length + " / " + text.Length + ")";
			tokenStack.Children.Clear();
			int i = 0;
			if (t.Length < 10000)
			{
				foreach (var token in t)
				{
					++i;
                    Grid g = new Grid {Background = ChooseBackgroundFromTokenKind(token.Kind), Tag = token};
                    g.MouseLeftButtonUp += G_MouseLeftButtonUp;
					g.HorizontalAlignment = HorizontalAlignment.Stretch;
					g.Children.Add(new TextBlock() { Text = token.Kind.ToString() + " - '" + token.Value + "'", IsHitTestVisible = false });
					tokenStack.Children.Add(g);
				}
			}
			termTree.Items.Clear();
			watch.Reset();
			watch.Start();
			SourcepawnCondenser.Condenser c = new SourcepawnCondenser.Condenser(text, "");
			var def = c.Condense();
			watch.Stop();
			subTitle += "  -  condenser: " + watch.ElapsedMilliseconds.ToString() + " ms";
			Title = subTitle;
			bool expand = ExpandBox.IsChecked.Value;
			TreeViewItem functionItem = new TreeViewItem() { Header = "functions (" + def.Functions.Count.ToString() + ")", IsExpanded = expand };
			foreach (var f in def.Functions)
			{
                TreeViewItem item = new TreeViewItem {Header = f.Name, IsExpanded = expand, Tag = f};
                item.MouseLeftButtonUp += ItemFunc_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + f.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + f.Length.ToString() });
				item.Items.Add(new TreeViewItem() { Header = "Kind: " + f.FunctionKind.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "ReturnType: " + f.ReturnType });
				item.Items.Add(new TreeViewItem() { Header = "Comment: >>" + f.CommentString + "<<", Background = Brushes.LightGray });
				for (int j = 0; j < f.Parameters.Length; ++j)
				{
					item.Items.Add(new TreeViewItem() { Header = "Parameter " + (j + 1).ToString() + ": " + f.Parameters[j], Background = ((j + 1) % 2 == 0) ? Brushes.LightGray : Brushes.White });
				}
				functionItem.Items.Add(item);
			}
			termTree.Items.Add(functionItem);
			TreeViewItem enumItem = new TreeViewItem() { Header = "enums (" + def.Enums.Count.ToString() + ")", IsExpanded = expand };
			foreach (var en in def.Enums)
			{
                TreeViewItem item = new TreeViewItem
                {
                    Header = (string.IsNullOrWhiteSpace(en.Name)) ? "no name" : en.Name,
                    IsExpanded = expand,
                    Tag = en
                };
                item.MouseLeftButtonUp += ItemEnum_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + en.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + en.Length.ToString() });
				for (int j = 0; j < en.Entries.Length; ++j)
				{
					item.Items.Add(new TreeViewItem() { Header = "Entry " + (j + 1).ToString() + ": " + en.Entries[j], Background = (j % 2 == 0) ? Brushes.LightGray : Brushes.White });
				}
				enumItem.Items.Add(item);
			}
			termTree.Items.Add(enumItem);

			TreeViewItem dItem = new TreeViewItem() { Header = "defines (" + def.Defines.Count.ToString() + ")", IsExpanded = expand };
			foreach (var d in def.Defines)
			{
                TreeViewItem item = new TreeViewItem {Header = d.Name, IsExpanded = expand, Tag = d};
                item.MouseLeftButtonUp += Itemppd_MouseLeftButtonUp;
				item.Items.Add(new TreeViewItem() { Header = "Index: " + d.Index.ToString(), Background = Brushes.LightGray });
				item.Items.Add(new TreeViewItem() { Header = "Length: " + d.Length.ToString() });
				dItem.Items.Add(item);
			}
			termTree.Items.Add(dItem);
            TreeViewItem cItem = new TreeViewItem() { Header = "constants (" + def.Constants.Count.ToString() + ")", IsExpanded = expand };
            foreach (var cn in def.Constants)
            {
                TreeViewItem item = new TreeViewItem {Header = cn.Name, IsExpanded = expand, Tag = cn};
                item.MouseLeftButtonUp += Itemc_MouseLeftButtonUp;
                item.Items.Add(new TreeViewItem() { Header = "Index: " + cn.Index.ToString(), Background = Brushes.LightGray });
                item.Items.Add(new TreeViewItem() { Header = "Length: " + cn.Length.ToString() });
                cItem.Items.Add(item);
            }
            termTree.Items.Add(cItem);
        }

		private void ItemFunc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
            if (token is SMFunction function)
            {
                textBox.Focus();
                textBox.Select(function.Index, function.Length);
            }
        }

		private void ItemEnum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
            if (token is SMEnum tokenEnum)
            {
                textBox.Focus();
                textBox.Select(tokenEnum.Index, tokenEnum.Length);
            }
        }


		private void Itemppd_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((TreeViewItem)sender).Tag;
            if (token is SMDefine define)
            {
                textBox.Focus();
                textBox.Select(define.Index, define.Length);
            }
        }

        private void Itemc_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var token = ((TreeViewItem)sender).Tag;
            if (token is SMConstant constant)
            {
                textBox.Focus();
                textBox.Select(constant.Index, constant.Length);
            }
        }


        private void G_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			var token = ((Grid)sender).Tag;
            if (token is Token t)
            {
                textBox.Focus();
                textBox.Select(t.Index, t.Length);
            }
        }

		private Brush ChooseBackgroundFromTokenKind(TokenKind kind)
		{
			switch (kind)
			{
				case TokenKind.BraceClose:
				case TokenKind.BraceOpen: return Brushes.LightGray;
				case TokenKind.Character: return Brushes.LightSalmon;
				case TokenKind.EOF: return Brushes.LimeGreen;
				case TokenKind.Identifier: return Brushes.LightSteelBlue;
				case TokenKind.Number: return Brushes.LightSeaGreen;
				case TokenKind.ParenthesisClose:
				case TokenKind.ParenthesisOpen: return Brushes.LightSlateGray;
				case TokenKind.Quote: return Brushes.LightGoldenrodYellow;
				case TokenKind.EOL: return Brushes.Aqua;
				case TokenKind.SingleLineComment:
				case TokenKind.MultiLineComment: return Brushes.Honeydew;
				default: return Brushes.IndianRed;
			}
		}

        private void CaretPositionChangedEvent(object sender, RoutedEventArgs e)
        {
            CaretLabel.Content = textBox.CaretIndex.ToString() + " / " + textBox.SelectionLength.ToString();
        }
	}
}
