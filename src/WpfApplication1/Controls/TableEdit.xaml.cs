using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Windows.Controls.Ribbon;
using System.Xml;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for TableEdit.xaml
	/// </summary>
	public partial class TableEdit : ABRibbonWindow
	{
		public static TableEdit TableEditForm = null;

		#region Constructor

		public TableEdit()
		{
			InitializeComponent();
			TableEditForm = this;
			// Insert code required on object creation below this point.
		}

		#endregion

		#region Command Bindings

		public static RoutedUICommand cmdAddRow = new RoutedUICommand("cmdAddRow", "cmdAddRow", typeof(TableEdit));
		public static RoutedUICommand cmdDeleteRow = new RoutedUICommand("cmdDeleteRow", "cmdDeleteRow", typeof(TableEdit));
		public static RoutedUICommand cmdInsertRow = new RoutedUICommand("cmdInsertRow", "cmdInsertRow", typeof(TableEdit));
		public static RoutedUICommand cmdRefresh = new RoutedUICommand("cmdRefresh", "cmdRefresh", typeof(TableEdit));
		public static RoutedUICommand cmdCancelChanges = new RoutedUICommand("cmdCancelChanges", "cmdCancelChanges", typeof(TableEdit));
		public static RoutedUICommand cmdSaveChanges = new RoutedUICommand("cmdSaveChanges", "cmdSaveChanges", typeof(TableEdit));
		public static RoutedUICommand cmdScriptTable = new RoutedUICommand("cmdScriptTable", "cmdScriptTable", typeof(TableEdit));
		public static RoutedUICommand cmdCopyScript = new RoutedUICommand("cmdCopyScript", "cmdCopyScript", typeof(TableEdit));
		public static RoutedUICommand cmdSaveScript = new RoutedUICommand("cmdSaveScript", "cmdSaveScript", typeof(TableEdit));

		#endregion

		#region Private Variables

		ObservableCollection<XmlNode> source;
		TextBox xml = null;
		DataGrid dg = null;
		XmlNode data = null;
		List<string> columns = null;
		string TableName = null;
		bool loaded = false;
		bool edited = false;

		#endregion

		#region Private Methods

		private void getData()
		{
			data = DataAccess.getDataNode("ab_TableEdit", new string[] {
                "@TableName", TableName,
                "@xml", "<root command=\"get\" />"
            }, true);
		}

		private void RefreshData()
		{
			var sc = dg.Items.SortDescriptions;
			var sd = dg.Columns.Select(f => f.SortDirection).ToList();
			getGrid();
			int i = 0;
			dg.Columns.ToList().ForEach(f => f.SortDirection = sd[i++]);
			dg.Items.SortDescriptions.Clear();
			foreach (var item in sc)
				dg.Items.SortDescriptions.Add(item);
			ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dg.ItemsSource);
			lcv.CustomSort = new CustomSort(dg.Items.SortDescriptions);
		}

		private void getGrid()
		{
			edited = false;
			loaded = false;
			dg = new DataGrid
			{
				HeadersVisibility = DataGridHeadersVisibility.All,
				RowHeaderWidth = 20,
				IsReadOnly = false,
				CanUserDeleteRows = true,
				CanUserSortColumns = true,
				CanUserAddRows = true,
				AutoGenerateColumns = false,
				AlternatingRowBackground = new LinearGradientBrush(Color.FromRgb(248, 248, 255), Color.FromRgb(225, 225, 255), 90F)
			};
            dg.CommandBindings.Add(TableName.Equals("Encyclopaedia", StringComparison.InvariantCultureIgnoreCase)
                ? new CommandBinding(ApplicationCommands.Copy, DataGridCopyOverride)
                : new CommandBinding(ApplicationCommands.Copy, DataGridViewCopyOverride));

            xml = new TextBox { AcceptsReturn = true, IsReadOnly = true, IsReadOnlyCaretVisible = true };
			dg.BeginningEdit += new EventHandler<DataGridBeginningEditEventArgs>(dg_BeginningEdit);
			dg.Sorting += new DataGridSortingEventHandler(dg_Sorting);

			source = new ObservableCollection<XmlNode>();
			source.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(source_CollectionChanged);

			Dictionary<string, List<string[]>> combos = new Dictionary<string, List<string[]>>();
			columns = getColumns(data, combos, source);
			dg.ItemsSource = source;
			grid.Children.Clear();
			grid.Children.Add(dg);
			grid.Children.Add(xml);

			foreach (var item in columns)
			{
				if (item.StartsWith("index_") || item.StartsWith("value_") || item.StartsWith("lookup_"))
				{
					string header = item.Substring(item.IndexOf('_') + 1);
					if (combos.ContainsKey("display_" + header))
					{
						dg.Columns.Add(new DataGridComboBoxColumn
						{
							Header = header,
							ItemsSource = combos["display_" + header],
							DisplayMemberPath = "[1]",
							SelectedValuePath = "[0]",
							SelectedValueBinding = new Binding { XPath = item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
							SortMemberPath = "display_" + header //"[display_" + header + "].InnerText"
						});
					}
					else
					{
						dg.Columns.Add(new DataGridTextColumn
						{
							SortMemberPath = item, //"[" + item + "].InnerText",
							Header = header,
							Binding = new Binding { XPath = item, Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged },
							IsReadOnly = !item.StartsWith("value_")
						});
					}
				}
			}
		}

	    private void DataGridCopyOverride(object sender, ExecutedRoutedEventArgs e)
	    {
            Clipboard.SetText(string.Join("\n", (sender as DataGrid).SelectedItems.OfType<XmlNode>().Select(f => string.Format("~E{0}#{1}~", f["value_Title"].InnerText, f["value_ArticleID"].InnerText))));
        }

        private void DataGridViewCopyOverride(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(sender is DataGrid dataSourceTable)) return;

            var displayedColumnSelection = dataSourceTable.Columns.Select(s => s.SortMemberPath).ToList();
            var sb = new StringBuilder();
            foreach (var item in dataSourceTable.SelectedItems)
            {
                var innerXml = (XmlElement)item;
                foreach (var displayedValue in displayedColumnSelection.Select(col => innerXml.SelectSingleNode(col)?.InnerText))
                {
                    sb.Append(displayedValue).Append("\t");
                }

                sb.AppendLine();
            }

            Clipboard.SetText(sb.ToString());
        }

		private List<string> getColumns(XmlNode xe, Dictionary<string, List<string[]>> combos, ObservableCollection<XmlNode> src)
		{
			List<string> columns = new List<string>();

			foreach (XmlNode item in xe.SelectNodes("*[data='0']"))
			{
				foreach (XmlNode field in item.ChildNodes)
				{
					if (field.Name.StartsWith("display_"))
					{
						string header = field.Name.Substring(field.Name.IndexOf('_') + 1);
						string value = item.SelectSingleNode("*[self::index_" + header + " or self::value_" + header + "]").InnerText;
						if (!combos.ContainsKey(field.Name)) combos.Add(field.Name, new List<string[]>());
						if (!combos[field.Name].Any(f => f[0] == value)) combos[field.Name].Add(new string[] { value, field.InnerText });
					}
				}
			}

			foreach (XmlNode item in xe.SelectNodes("*[data!='0']"))
			{
				List<string> row = new List<string>();
				int fi = 0;
				bool isData = false;
				foreach (XmlNode field in item.ChildNodes)
				{
					string header = field.Name.Substring(field.Name.IndexOf('_') + 1);
					if (field.Name == "data" && field.InnerText == "1") isData = true;
					if (field.Name.StartsWith("display_"))
					{
						string value = item.SelectSingleNode("*[self::index_" + header + " or self::value_" + header + "]").InnerText;
						if (!combos.ContainsKey(field.Name)) combos.Add(field.Name, new List<string[]>());
						if (!combos[field.Name].Any(f => f[0] == value)) combos[field.Name].Add(new string[] { value, field.InnerText });
					}
					else if (field.Name != "data") row.Add(field.InnerText);
					if (field.Name != "data")
					{
						if (!columns.Contains(field.Name))
						{
							columns.Insert(fi, field.Name);
						}
						fi++;
					}
					if (combos.ContainsKey("display_" + header) && item["display_" + header] == null)
					{
						string[] lookup = combos["display_" + header].Find(f => f[0] == field.InnerText);
						if (lookup != null) item.AppendChild(item.OwnerDocument.CreateElement("display_" + header)).InnerText = lookup[1];
					}
				}
				if (isData) src.Add(item);
			}
			loaded = true;
			return columns;
		}

		private static void SaveOldValues(XmlNode xn)
		{
			foreach (XmlNode item in xn.ChildNodes)
			{
				if (item.Name.StartsWith("value_")) xn.AppendChild(xn.OwnerDocument.CreateElement(item.Name.Replace("value_", "old_"))).InnerText = item.InnerText;
			}
		}

		#endregion

		#region Events

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			XmlNode xn = DataAccess.getDataNode("ab_TableEdit", new string[] {
                "@xml", "<root command=\"list\" />"
            }, true);
			cmbTableList.DisplayMemberPath = "TableName";
			cmbTableList.ItemsSource = xn.SelectNodes("Table").OfType<XmlNode>();
		}

		private void cmbTableList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TableName = cmbTableList.SelectedValue.ToString();
			getData();
			getGrid();
			//dg.Columns[0].SortDirection = ListSortDirection.Ascending;
			//dg.Items.SortDescriptions.Add(new SortDescription(dg.Columns[0].SortMemberPath, ListSortDirection.Ascending));
		}

		void dg_Sorting(object sender, DataGridSortingEventArgs e)
		{
			e.Handled = true;
			ListCollectionView lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dg.ItemsSource);
			e.Column.SortDirection = (e.Column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;
			if ((Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift) dg.Items.SortDescriptions.Clear();
			dg.Items.SortDescriptions.Add(new SortDescription(e.Column.SortMemberPath, (ListSortDirection)e.Column.SortDirection));
			lcv.CustomSort = new CustomSort(dg.Items.SortDescriptions);
		}

		void dg_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			edited = true;
			XmlNode xn = e.Row.Item as XmlNode;
			if (xn["Edit"] == null && xn["New"] == null)
			{
				xn.AppendChild(xn.OwnerDocument.CreateElement("Edit"));
				SaveOldValues(xn);
			}
		}

		void source_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (loaded) edited = true;
			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
			{
				foreach (var item in e.OldItems)
				{
					if (item is XmlNode)
					{
						XmlNode xn = item as XmlNode;
						if (xn["Edit"] != null) xn.RemoveChild(xn["Edit"]);
						else SaveOldValues(xn);
						if (xn["New"] != null) data.RemoveChild(xn);
						else
						{
							xn.AppendChild(xn.OwnerDocument.CreateElement("Delete"));
							if (!xn.ChildNodes.OfType<XmlNode>().Any(f => f.Name.StartsWith("index_")))
							{
								foreach (XmlNode s in source)
								{
									bool match = true;
									foreach (XmlNode node in xn.ChildNodes)
									{
										if (node.Name.StartsWith("old_"))
										{
											if (s[node.Name] != null)
											{
												if (s[node.Name].InnerText != node.InnerText) match = false;
											}
											else if (s[node.Name.Replace("old_", "value_")].InnerText != node.InnerText) match = false;
										}
										if (!match) break;
									}
									if (match && s["New"] == null) s.AppendChild(s.OwnerDocument.CreateElement("New"));
								}
							}
						}
					}
				}
			}
		}

		#endregion

		#region Command Can Execute

		private void AddRow_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (columns != null && data != null && source != null) e.CanExecute = true;
		}

		private void DeleteRow_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (dg != null && source != null && dg.SelectedItems.Count > 0) e.CanExecute = true;
		}

		private void InsertRow_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (dg != null && source != null && dg.SelectedItems.Count == 1) e.CanExecute = true;
		}

		private void Refresh_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (dg != null && source != null) e.CanExecute = true;
		}

		private void CancelChanges_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (data != null && dg != null && edited) e.CanExecute = true;
		}

		private void SaveChanges_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (data != null && dg != null && edited) e.CanExecute = true;
		}

		private void ScriptTable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (cmbTableList.SelectedItem != null) e.CanExecute = true;
		}

		private void CopyScript_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (xml != null && xml.Text != "") e.CanExecute = true;
		}

		private void SaveScript_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (xml != null && xml.Text != "") e.CanExecute = true;
		}

		#endregion

		#region Execute Methods

		private void AddRow_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			XmlNode newRow = data.AppendChild(data.OwnerDocument.CreateElement("Table"));
			newRow.AppendChild(data.OwnerDocument.CreateElement("New"));
			foreach (var item in columns)
			{
				newRow.AppendChild(data.OwnerDocument.CreateElement(item));
			}
			source.Add(newRow);
		}

		private void DeleteRow_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			List<XmlNode> rowsToDelete = new List<XmlNode>(dg.SelectedItems.OfType<XmlNode>());
			foreach (var item in rowsToDelete)
			{
				source.Remove(item);
			}
		}

		private void InsertRow_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			XmlNode newRow = data.AppendChild(data.OwnerDocument.CreateElement("Table"));
			newRow.AppendChild(data.OwnerDocument.CreateElement("New"));
			int i = 0;
			foreach (var item in columns)
			{
				XmlNode elem = newRow.AppendChild(data.OwnerDocument.CreateElement(item));
				if (item.StartsWith("value_") && i++ == 0 && dg.SelectedItem is XmlNode) elem.InnerText = (dg.SelectedItem as XmlNode).SelectSingleNode(item).InnerText;
			}
			source.Insert(dg.SelectedIndex, newRow);
		}

		private void CancelChanges_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			getData();
			RefreshData();
		}

		private void SaveChanges_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			XmlDocument doc = new XmlDocument();
			XmlNode root = doc.AppendChild(doc.CreateElement("root"));
			root.Attributes.Append(doc.CreateAttribute("command")).Value = "edit";
			foreach (XmlNode item in data.SelectNodes("Table[Edit or New or Delete]"))
			{
				XmlNode importnode = doc.ImportNode(item, true);
				root.AppendChild(importnode);
			}
			data = DataAccess.getDataNode("ab_TableEdit", new string[] {
				"@TableName", TableName,
				"@xml", doc.OuterXml
			}, true);

			RefreshData();
		}

		private void ScriptTable_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			XmlNode xn = DataAccess.getDataNode("ab_TableEdit", new string[] {
				"@TableName", TableName,
				"@xml", "<root command=\"script\" />"
            }, true);
			StringBuilder script = new StringBuilder();
			foreach (XmlNode item in xn.SelectNodes("*/*"))
			{
				script.AppendLine(item.InnerText);
			}
			string SQLScript = "--\n";
			SQLScript += string.Format("-- Table: {0}\n", TableName);
			SQLScript += string.Format("-- Machine : {0}\n", Environment.MachineName);
			SQLScript += string.Format("-- User : {0}\n", Environment.UserName);
			SQLScript += string.Format("-- Date : {0}\n", DateTime.Now.ToString("MMMM dd, yyyy"));
            SQLScript += string.Format("-- Time : {0}\n", DateTime.Now.ToString("HH:mm:ss"));
            SQLScript += "Set NoCount On\n";
            SQLScript += "--\n\n" + script.ToString();

			xml.Text = SQLScript;
		}

		private void CopyScript_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Clipboard.SetText(xml.Text);
		}

		private void SaveScript_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			AlgoLoader.SaveScript(xml.Text);
		}

		private void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			getData();
			getGrid();
		}

		#endregion
	}

	class CustomSort : System.Collections.IComparer
	{
		//public ListSortDirection Direction { get; set; }
		//public DataGridColumn Column { get; set; }
		public Dictionary<string, ListSortDirection> Columns { get; set; }

		public CustomSort(SortDescriptionCollection sort)
		{
			//Direction = column.SortDirection == null ? ListSortDirection.Ascending : (ListSortDirection)column.SortDirection;
			//Column = column;
			Columns = new Dictionary<string, ListSortDirection>();
			foreach (var item in sort)
			{
				if (!Columns.ContainsKey(item.PropertyName)) Columns.Add(item.PropertyName, item.Direction);

				//int start = item.PropertyName.IndexOf('[');
				//int end = item.PropertyName.IndexOf(']');
				//if (end > start)
				//{
				//    string columnname = item.PropertyName.Substring(start + 1, (end - start) - 1);
				//    if(!Columns.ContainsKey(columnname)) Columns.Add(columnname, item.Direction);
				//}
			}
		}

		#region IComparer Members

		int System.Collections.IComparer.Compare(object x, object y)
		{
			double a; double b;
			int result = 0;
			foreach (var ColumnName in Columns)
			{
				if (x is XmlNode && y is XmlNode)
				{
					string xa = (x as XmlNode)[ColumnName.Key] != null ? (x as XmlNode)[ColumnName.Key].InnerText : "";
					string xb = (y as XmlNode)[ColumnName.Key] != null ? (y as XmlNode)[ColumnName.Key].InnerText : "";
					if (double.TryParse(xa, out a) && double.TryParse(xb, out b))
					{
						if (ColumnName.Value == ListSortDirection.Ascending)
							result = a.CompareTo(b);
						else result = b.CompareTo(a);
					}
					else
					{
						if (ColumnName.Value == ListSortDirection.Ascending)
							result = xa.CompareTo(xb);
						else result = xb.CompareTo(xa);
					}
				}
				else
				{
					if (ColumnName.Value == ListSortDirection.Ascending)
						result = x.ToString().CompareTo(y.ToString());
					else result = y.ToString().CompareTo(x.ToString());
				}
				if(result != 0) return result;
			}
			return 0;
		}

		#endregion
	}
}
