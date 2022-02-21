using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using AssetBuilder.Controls;

namespace AssetBuilder
{
	/// <summary>
	/// Interaction logic for Goal.xaml
	/// </summary>
	public partial class Goal : Window
	{
		XElement bound;
		ObservableCollection<XElement> goallist;

		public Goal()
		{
			this.InitializeComponent();
			XElement xn = DataAccess.getData("usp_GetGoalXml", new string[] {
				//"@GoalId", "2",
			}, true);

			var goals = from g in xn.Elements("Table").Elements("GOAL")
						select XElement.Parse(g.Value);

			goallist = new ObservableCollection<XElement>(goals);
			GoalList.ItemsSource = goallist;

			XElement xe = XElement.Parse(xn.Element("Table").Element("GOAL").Value);
			bound = xe;
			LayoutRoot.DataContext = bound;
			// Insert code required on object creation below this point.
			recs.Add(txtSR, "CurrentQuestion");
			recs.Add(txtPR, "ValueQuestion");
			recs.Add(txtTR, "TargetQuestion");
			ques.Add(txtSQ, "CurrentQuestion");
			ques.Add(txtPQ, "ValueQuestion");
			ques.Add(txtTQ, "TargetQuestion");
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(bound.ToString());
		}

		Dictionary<TextBox, string> recs = new Dictionary<TextBox, string>();
		Dictionary<TextBox, string> ques = new Dictionary<TextBox, string>();

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(recs.Any(f => f.Key.Text != "") || ques.Any(f => f.Key.Text != ""))
			{
				if(bound.Element("Questions") == null) bound.Add(new XElement("Questions", new XElement("Question", null)));
				if(bound.Element("Questions").Element("Question") == null) bound.Element("Questions").Add(new XElement("Question", null));
			}
			else
			{
				//bound.Element("Questions").Remove();
				return;
			}
			
			var question = bound.Element("Questions").Element("Question");

			foreach (var item in recs)
			{
				if (item.Key.Text != "" && question.Element(item.Value) == null) question.Add(new XElement(item.Value, new XAttribute("recommendation", "")));
			}

			foreach (var item in ques)
			{
				if (item.Key.Text != "" && question.Element(item.Value) == null) question.Add(new XElement(item.Value, new XAttribute("recommendation", "")));
				if (item.Key.Text != "" && question.Element(item.Value).Element("Text") == null) question.Element(item.Value).Add(new XElement("Text", ""));
			}
		}

		private void txtConc_Loaded(object sender, RoutedEventArgs e)
		{
			Intel i = new Intel(sender as TextBox, canvas, IntelListMakers.conclookup);
			(sender as TextBox).GotFocus += new RoutedEventHandler(Goal_GotFocus);
		}

		void Goal_GotFocus(object sender, RoutedEventArgs e)
		{
			TextBox t = sender as TextBox;
			if (t.Text == "") t.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(t), 0, Key.Space) { RoutedEvent = TextBox.KeyUpEvent });
		}

		private void GoalList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			LayoutRoot.DataContext = null;
			bound = (sender as ListBox).SelectedItem as XElement;
			LayoutRoot.DataContext = bound;
		}

		private void addGoal(object sender, RoutedEventArgs e)
		{
			goallist.Add(XElement.Parse("<Goal id=\"-1\" name=\"New Goal\" />"));
		}

		private void addElement(object sender, RoutedEventArgs e)
		{
			string[] f = (sender as Button).CommandParameter.ToString().Split('|');
			if (f.Length < 3) return;
			if (bound.Element(f[0]) == null) bound.Add(new XElement(f[0], ""));
			bound.Element(f[0]).Add(new XElement(f[1], new XAttribute(f[2], "")));
		}
	}
}