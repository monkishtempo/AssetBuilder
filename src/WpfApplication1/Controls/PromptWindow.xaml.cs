using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using AssetBuilder.ViewModels;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for PromptWindow.xaml
	/// </summary>
	public partial class PromptWindow : ABWindow
	{
		public string Comment = "";
		CheckBox[] chkSource;
		CheckBox[] chkType;

		public PromptWindow(bool newAsset)
		{
			InitializeComponent();
			this.Loaded += new RoutedEventHandler(PromptWindow_Loaded);
			chkSource = new CheckBox[] { chkStaff, chkReviewer, chkUser };
			chkType = new CheckBox[] { chkNew, chkSpell, chkClarify, chkCategory, chkAdditional, chkNoApproval, chkBullet, chkOther };
			if (newAsset)
			{
				chkStaff.IsChecked = true;
				chkNew.IsChecked = true;
			}

            chkReviewer.IsChecked = true;
            cmdReviewer.Text = Environment.UserName;
		}

		void PromptWindow_Loaded(object sender, RoutedEventArgs e)
		{
			populateReviewers(0);
			TextBox tb = getChild<TextBox>(cmdReviewer);
			if(tb != null) tb.MaxLength = 50;
		}

		private void populateReviewers(int parameter)
		{
			XDocument doc = new XDocument(new XElement("root", new XAttribute("command", "reviewers")));
			if (parameter == 1 && !findItem(cmdReviewer, cmdReviewer.Text) && cmdReviewer.Text != "")
				doc.Element("root").Add(new XElement("Add", cmdReviewer.Text));
			if (parameter == -1 && findItem(cmdReviewer, cmdReviewer.Text))
				doc.Element("root").Add(new XElement("Delete", new XAttribute("id", (cmdReviewer.SelectedItem as ListItem).ID)));
			cmdReviewer.Items.Clear();
			XElement xe = DataAccess.getData("ab_updateasset", new string[] {
				"@xml", doc.ToString()
			}, false);
			var reviewers = from reviewer in xe.Elements("Table") select reviewer;
			foreach (var reviewer in reviewers)
			{
				ListItem li = new ListItem
				{
					ID = int.Parse(reviewer.Element("reviewerID").Value),
					Value = reviewer.Element("reviewerName").Value
				};
				cmdReviewer.Items.Add(li);
			}
		}

		bool findItem(ComboBox cb, string text)
		{
			foreach (var item in cb.Items)
			{
				if (item.ToString() == text) return true;
			}
			return false;
		}

		public static type getChild<type>(DependencyObject dp)
		{
			if (dp == null) return default(type);
			type ret = default(type);

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dp); i++)
			{
				object p = VisualTreeHelper.GetChild(dp, i);
				if (p == null || p is type)
				{
					ret = (type)p;
					break;
				}
				else ret = getChild<type>(p as DependencyObject);
			}
			return ret;
		}

		private void btnOk_Click(object sender, RoutedEventArgs e)
		{
			bool bSource = false;
			bool bReviewer = true;
			bool bType = false;
			
			Comment = "Source: ";
			foreach (CheckBox chk in chkSource)
			{
				if (chk.IsChecked == true)
				{
					if (bSource) Comment += ", ";
					bSource = true;
					Comment += chk.Content;
				}
				if (chk.Name == "chkReviewer" && chk.IsChecked == true)
				{
					if (cmdReviewer.Text != "")
						Comment += " - " + cmdReviewer.Text;
					else
						bReviewer = false;
				}
			}
			Comment += "\nType: ";
			foreach (CheckBox chk in chkType)
			{
				if (chk.IsChecked == true)
				{
					if (bType) Comment += ", ";
					bType = true;
					Comment += chk.Content;
				}
			}
			if (txtReasoning.Text != "") Comment += "\nReason: " + txtReasoning.Text;
			if (txtEditor.Text != "") Comment += "\nEditor response: " + txtEditor.Text;

			if (bSource && bType && bReviewer) DialogResult = true;
			else
			{
				string error = "Invalid reason for asset change :-\n\n";
				if (!bSource) error += "   - You need supply a Source\n";
				if (!bType) error += "   - You need supply a Type of change\n";
				if (!bReviewer) error += "   - You need supply a Reviewer\n";
				MessageBox.Show(error, "Error saving asset", MessageBoxButton.OK, MessageBoxImage.Stop);
			}            
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void addClick(object sender, RoutedEventArgs e)
		{
			populateReviewers(1);
		}

		private void deleteClick(object sender, RoutedEventArgs e)
		{
			populateReviewers(-1);
		}
	}
}
