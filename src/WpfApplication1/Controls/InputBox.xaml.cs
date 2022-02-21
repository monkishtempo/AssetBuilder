using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for InputBox.xaml
	/// </summary>
	public partial class InputBox : Window
	{
        public string Text { get { return textBox1.Text; } set { textBox1.Text = value; } }
        private TextBox[] __Texts;
        private TextBox[] _Texts { get { return __Texts == null ? new[] { textBox1 } : __Texts; } set { __Texts = value; } } 
        public string[] Texts { get { return _Texts == null ? new[] { textBox1.Text } : _Texts.Select(f => f.Text).ToArray(); } }
        InputBoxValidate[] ibv = new[] { InputBoxValidate.None };
		Grid[] grds = null;
		Grid grd = null;
		bool appendPipe = false;

        public string this[int i]
        {
            get { return (_Texts.Length > i && i >= 0) ? _Texts[i].Text : null; }
            set { if (_Texts.Length > i && i >= 0) _Texts[i].Text = value; }
        }

        public InputBox(string prompt, string title, string[] type, WindowStartupLocation wsl, InputBoxValidate[] validate = null)
        {
            InitializeComponent();
            int len = type.Length;
            if (len > 1)
            {
                _Texts = new TextBox[len];
                _Texts[0] = textBox1;
                ibv = new InputBoxValidate[len];
                for (int i = 1; i < len; i++)
                {
                    InputPanel.RowDefinitions.Add(new RowDefinition());
                    Label l = new Label { Content = type[i] };
                    l.SetValue(Grid.RowProperty, i);
                    TextBox b = new TextBox { MinWidth = 200 };
                    b.Tag = i;
                    b.SetValue(Grid.ColumnProperty, 1);
                    b.SetValue(Grid.RowProperty, i);
                    b.TextChanged += textBox1_TextChanged;
                    InputPanel.Children.Add(l);
                    InputPanel.Children.Add(b);
                    ibv[i] = validate != null && validate.Length > i ? validate[i] : InputBoxValidate.None;
                    _Texts[i] = b;
                    /*
            <Label Content="test" Name="lblType"/>
            <TextBox Name="textBox1" MinWidth="200" TextChanged="textBox1_TextChanged" TabIndex="0" Grid.Column="1"/>
                     */
                }
            }
            ibv[0] = validate != null && validate.Length > 0 ? validate[0] : InputBoxValidate.None;
            lblType.Content = type != null && type.Length > 0 ? type[0] : "";
            lblPromt.Content = prompt;
            Title = title;
            WindowStartupLocation = wsl;
            textBox1_TextChanged(textBox1, null);
        }

        public InputBox(string prompt, string title, string type, WindowStartupLocation wsl, InputBoxValidate validate = InputBoxValidate.None)
		{
			InitializeComponent();
            grds = new Grid[] { Comments, AlgoFields, QuestionFields, AnswerFields, ConclusionFields, BulletFields, MergeLanguage, TitleFields, McKessonXml, InputPanelCheckBox };
			if (type.Contains("|"))
			{
				if (type.StartsWith("Asset Fields")) appendPipe = true;
				grd = grds[int.Parse(type.Split('|')[1])];
				grd.Visibility = System.Windows.Visibility.Visible;
				InputPanel.Visibility = System.Windows.Visibility.Collapsed;
				foreach (var item in grd.Children)
				{
					if (item is CheckBox)
					{
						CheckBox cb = item as CheckBox;
						cb.Checked += new RoutedEventHandler(cb_Checked);
						cb.Unchecked += new RoutedEventHandler(cb_Checked);
                        if (cb.Content != null) cb.IsChecked = true;
                        else cb.Content = type.Split('|')[0];
                    }
				}
			}
			lblType.Content = type;
            lblPromt.Content = prompt;
            Title = title;
			WindowStartupLocation = wsl;
			ibv[0] = validate;
			textBox1_TextChanged(textBox1, null);
		}

		void cb_Checked(object sender, RoutedEventArgs e)
		{
			string s = "";
			if (appendPipe) s = "|";
			foreach (var item in grd.Children)
			{
				if (item is CheckBox)
				{
					CheckBox cb = item as CheckBox;
					if (cb.IsChecked == true)
					{
						string content = cb.Content.ToString();
						if (cb.CommandParameter != null) content = cb.CommandParameter.ToString();
						if (content == "Blank" || (s != "" && s != "|")) s += "|";
						if (content != "Blank") s += content;
						if (content.Contains("information")) s += "|" + content.Replace("information", "infomation");
					}
				}
			}
			if (appendPipe) s += "|";
			textBox1.Text = s;
		}

		private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
		{
            bool enable = true;
            foreach (var t in _Texts)
            {
                var text = t.Text;
                var i = 0;
                if (t.Tag != null && t.Tag is int) i = (int)t.Tag;
                switch (ibv[i])
                {
                    case InputBoxValidate.None:
                        break;
                    case InputBoxValidate.Date:
                        DateTime dt = DateTime.MinValue;
                        if (!DateTime.TryParse(text, out dt))
                            enable = false;
                        break;
                    case InputBoxValidate.Int:
                        int x = 0;
                        if (!int.TryParse(text, out x))
                            enable = false;
                        break;
                    case InputBoxValidate.Required:
                        enable = !string.IsNullOrWhiteSpace(text);
                        break;
                    default:
                        break;
                }
                btnOK.IsEnabled = enable;
            }
        }

		private void btnOK_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			textBox1.Focus();
		}

        private void chkBox1_Checked(object sender, RoutedEventArgs e)
        {

        }
	}

	public enum InputBoxValidate
	{
		None,
		Date,
		Int,
        Required
	}
}
