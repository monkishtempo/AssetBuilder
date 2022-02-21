using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace AssetBuilder.Controls
{
	public class Intel : ListBox
	{
		#region Public Properties

		public Canvas canvas { get; set; }
		public TextBox textBox { get; set; }
		public List<IntelItem> FullList { get; set; }

		public delegate List<IntelItem> GetList(string text, TextBox tb);

		#endregion

		#region Private Variables

		string textChange = "";
		int cursorPosition = 0;
		IntelModel model;
		Dictionary<string, IntelModel> invokeList = new Dictionary<string, IntelModel>();
		DispatcherTimer timer;

		#endregion

		#region Constructor

		public Intel(TextBox textbox, Canvas canvas, Dictionary<string, IntelModel> intelModel = null)
		{
			if (intelModel != null) this.invokeList = intelModel;
			textBox = textbox;
			this.canvas = canvas;
			textBox.KeyUp -= new KeyEventHandler(tb_KeyUp);
			textBox.KeyUp += new KeyEventHandler(tb_KeyUp);
			this.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
			DataTemplate dt = new DataTemplate();
			FrameworkElementFactory tb = new FrameworkElementFactory(typeof(TextBlock));
			tb.SetBinding(TextBlock.TextProperty, new Binding("Display"));
			tb.SetBinding(TextBlock.ToolTipProperty, new Binding("ToolTip"));
			dt.VisualTree = tb;
			MaxHeight = 200;
			MaxWidth = 500;
			MinWidth = 50;

			this.ItemTemplate = dt; // Application.Current.Resources["myTaskTemplate"] as DataTemplate;
			this.Loaded += new RoutedEventHandler(Intel_Loaded);
		}

		void Intel_Loaded(object sender, RoutedEventArgs e)
		{
			double r = (double)this.GetValue(Canvas.LeftProperty) + this.ActualWidth;
			double b = (double)this.GetValue(Canvas.TopProperty) + this.ActualHeight;
			if (r > canvas.ActualWidth) this.SetValue(Canvas.LeftProperty, canvas.ActualWidth - this.ActualWidth);
			if (b > canvas.ActualHeight) this.SetValue(Canvas.TopProperty, canvas.ActualHeight - this.ActualHeight);
		}

		#endregion

		#region TextBox Events

		private void tb_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox t = sender as TextBox;
			int cursor = t.SelectionStart + t.SelectionLength;
			if (this.Parent != null)
			{
				if (e.Key == Key.Escape) Clear();
				if (e.Key == Key.Down || e.Key == Key.Up)
				{
					this.Focus();
					if (this.SelectedIndex > -1)
					{
						getListItem().Focus();
						e.Handled = true;
					}
				}
				if ((e.Key == Key.Enter || e.Key == Key.Tab) && this.SelectedItem != null)
				{
					InsertText();
					e.Handled = true;
				}
				else if (e.Key == Key.Enter || e.Key == Key.Tab)
				{
					Clear();
				}
			}
		}

		private void tb_KeyUp(object sender, KeyEventArgs e)
		{
			//if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Space) e.Handled = true;
			if (e.Key != Key.Escape && this.Parent == null && textBox.SelectionLength == 0 && !textBox.IsReadOnly)
			{
				Invoke();
			}
		}

		private void Invoke()
		{
			string testString = textBox.Text.Substring(0, textBox.SelectionStart);
			bool display = false;
			var match = invokeList.FirstOrDefault(item => (item.Key.StartsWith("Regex:") && Regex.IsMatch(testString, item.Key.Substring(6), RegexOptions.Multiline)) || (!item.Key.StartsWith("Regex:") && testString.EndsWith(item.Key)) || (item.Key.StartsWith("FullMatch:") && textBox.Text == item.Key.Substring(10)));
			//var method = getListMethods.FirstOrDefault(item => (item.Key.StartsWith("Regex:") && Regex.IsMatch(testString, item.Key.Substring(6))) || (!item.Key.StartsWith("Regex:") && testString.EndsWith(item.Key)));
			if (match.Key != null)
			{
				model = match.Value;
				if (model.getListMethod != null)
				{
					FullList = getListMethod("");
					display = true;
				}
				else if (model.values != null)
				{
					FullList = match.Value.values;
					display = true;
				}
			}
			if (display && FullList != null && FullList.Count > 0) Init();
		}

		private void tb_SelectionChanged(object sender, RoutedEventArgs e)
		{
			int cursor = getCursorPosition();
			if (cursor < cursorPosition) Clear();
			else
			{
				textChange = textBox.Text.Substring(cursorPosition, cursor - cursorPosition);
				if (FullList == null || FullList.Count() == 0 || (FullList.Count == 1 && FullList[0] == textChange)) Clear();
				else if (model.endings != null && textChange.Length > 0 && model.endings.Contains(textChange[textChange.Length - 1])) InsertText((IntelItem)this.SelectedItem + (model.AppendEnding ? textChange[textChange.Length - 1].ToString() : ""));
				else
				{
					List<IntelItem> items;
					if (model.getListMethod != null) items = getListMethod(textChange);
					else items = new List<IntelItem>(FullList.Where(f => f.Display.StartsWith(textChange)));
					if (items == null || items.Count() == 0 || (items.Count == 1 && items[0] == textChange)) Clear();
					else this.ItemsSource = items;
				}
			}
		}

		private List<IntelItem> getListMethod(string textChange)
		{
			if (model.NoDelay) return getList();
			else return delayGetListMethod(textChange);
		}

		void tb_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (this.Parent != null) Clear();
		}

		void tb_LostFocus(object sender, RoutedEventArgs e)
		{
			if (!(IsFocused || IsKeyboardFocused || IsKeyboardFocusWithin) && !(textBox.IsKeyboardFocused || textBox.IsFocused)) Clear();
		}

		#endregion

		#region Overidden Events

		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsKeyboardFocusWithinChanged(e);
			if (!(IsFocused || IsKeyboardFocused || IsKeyboardFocusWithin) && !(textBox.IsKeyboardFocused || textBox.IsFocused)) Clear();
		}

		protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
		{
			base.OnMouseDoubleClick(e);
			InsertText();
			Invoke();
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			base.OnPreviewKeyDown(e);
			if (e.Key == Key.Tab || e.Key == Key.Space || e.Key == Key.Enter) InsertText();
			if (e.Key == Key.Enter || e.Key == Key.Tab) e.Handled = true;
			if (e.Key == Key.Escape) Clear();
			if (!(e.Key == Key.Down || e.Key == Key.Up)) textBox.Focus();
		}

		#endregion

		#region Instance Methods

		private void Clear(bool clearTimer = true)
		{
			if (clearTimer && timer != null)
			{
				timer.Stop();
				timer = null;
			}
			canvas.Children.Clear();
			textChange = "";
			cursorPosition = getCursorPosition();
			textBox.PreviewKeyDown -= new KeyEventHandler(tb_KeyDown);
			textBox.PreviewMouseDown -= new MouseButtonEventHandler(tb_MouseDown);
			textBox.SelectionChanged -= new RoutedEventHandler(tb_SelectionChanged);
			textBox.LostFocus -= new RoutedEventHandler(tb_LostFocus);
		}

		public void Dispose()
		{
			Clear();
			textBox.KeyUp -= new KeyEventHandler(tb_KeyUp);
			textBox = null;
			this.canvas = null;
		}

		private int getCursorPosition()
		{
			return getCursorPosition(textBox);
		}

		ListBoxItem getListItem()
		{
			return this.ItemContainerGenerator.ContainerFromIndex(this.SelectedIndex) as ListBoxItem;
		}

		private List<IntelItem> delayGetListMethod(string p)
		{
            if (model.IsStatic) return getList();

            Focusable = false;
			if (timer != null) timer.Stop();
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += new EventHandler(delayGetListMethod_Tick);
			timer.Start();

			return p == "" ? new List<IntelItem>(new IntelItem[] { "Building list..." }) : FullList;
		}

		void delayGetListMethod_Tick(object sender, EventArgs e)
		{
            timer.Stop();
            timer = null;
            FullList = getList();
            ItemsSource = FullList;
            Focusable = true;
        }

        private List<IntelItem> getList()
        {
            return model.getListMethod(textChange, textBox);
        }

		private void Init()
		{
			Focusable = true;
			int cursor = getCursorPosition(textBox);
			Rect r = textBox.GetRectFromCharacterIndex(cursor);
			Point pos = textBox.TranslatePoint(r.BottomRight, canvas);
			Clear(false);

			textBox.PreviewKeyDown += new KeyEventHandler(tb_KeyDown);
			textBox.PreviewMouseDown += new MouseButtonEventHandler(tb_MouseDown);
			textBox.SelectionChanged += new RoutedEventHandler(tb_SelectionChanged);
			textBox.LostFocus += new RoutedEventHandler(tb_LostFocus);

			this.SetValue(Canvas.LeftProperty, pos.X);
			this.SetValue(Canvas.TopProperty, pos.Y);
			this.ItemsSource = FullList;			
			canvas.Children.Add(this);
		}

		private void InsertText()
		{
			InsertText((IntelItem)this.SelectedItem);
		}

		private void InsertText(string text)
		{
			int cursor = getCursorPosition(textBox);
			int oldcursor = cursorPosition;
			Clear();
			if (this.SelectedItem != null)
			{
				textBox.Text = textBox.Text.Substring(0, oldcursor) + text + textBox.Text.Substring(cursor);
				textBox.SelectionLength = 0;
				textBox.SelectionStart = oldcursor + text.Length;
			}
			textBox.Focus();
		}

		#endregion

		#region Static Methods

		private static int getCursorPosition(TextBox t)
		{
			int cursor = t.SelectionStart + t.SelectionLength;
			return cursor;
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

		#endregion
	}

	public class IntelItem
	{
		public string Value { get; set; }
		public object ToolTip { get; set; }
		public string Display { get; set; }
        public int CategoryTypeID { get; set; }

		public static implicit operator string(IntelItem ii)
		{
			if (ii == null || ii.Value == null) return "";
			return ii.Value;
		}

		public static implicit operator IntelItem(string s)
		{
			return new IntelItem { Display = s, Value = s };
		}

		public override string ToString()
		{
			return Display;
		}
	}

	public class IntelModel
	{
		public char[] endings { get; set; }
		public List<IntelItem> values { get; set; }
		public string append { get; set; }
		public Intel.GetList getListMethod { get; set; }
		public bool IsStatic { get; set; }
		public bool NoDelay { get; set; }
		public bool AppendEnding { get; set; }

		public static implicit operator IntelModel(string[] s)
		{
			return getModel(s);
		}

		public static implicit operator IntelModel(List<string> s)
		{
			return getModel(s);
		}

		public static implicit operator IntelModel(IntelItem[] s)
		{
			return new IntelModel { values = new List<IntelItem>(s) };
		}

		public static IntelModel getModel(IEnumerable<string> s)
		{
			return new IntelModel { values = s.Select(f => (IntelItem)f).ToList() };
		}

		public static implicit operator IntelModel(Intel.GetList g)
		{
			return new IntelModel { getListMethod = g };
		}
	}
}
