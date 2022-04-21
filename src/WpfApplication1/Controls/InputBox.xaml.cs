using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using AssetBuilder.Classes;

namespace AssetBuilder.Controls
{
	/// <summary>
	/// Interaction logic for InputBox.xaml
	/// </summary>
	public partial class InputBox : Window
	{
        InputBoxValidate[] ibv = new[] { InputBoxValidate.None };
        private ExportRecordData _exportRecordData;
        Grid[] grds = null;
        Grid grd = null;
        bool appendPipe = false;
        private TextBox[] __Texts;
        private TextBox[] _Texts 
        { 
            get { return __Texts == null ? new[] { textBox1 } : __Texts; } 
            set => __Texts = value;
        }
        
        public string Text { 
            get => textBox1.Text;
            set => textBox1.Text = value;
        }

        public string[] Texts
        {
            get { return _Texts == null ? new[] { textBox1.Text } : _Texts.Select(f => f.Text).ToArray(); }
        }
        
        public string this[int i]
        {
            get => _Texts.Length > i && i >= 0 ? _Texts[i].Text : null;
            set { if (_Texts.Length > i && i >= 0) _Texts[i].Text = value; }
        }

        public ExportRecordData ExportReportResponse
        {
            get => _exportRecordData;
            set
            {
                _exportRecordData = value;

                // Bindable properties:
                var jiraBinding = new Binding(nameof(_exportRecordData.JiraReferences)) { Source = _exportRecordData, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                var reasonBinding = new Binding(nameof(_exportRecordData.Reason)) { Source = _exportRecordData, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                var releaseNoteBinding = new Binding(nameof(_exportRecordData.AlgoReleaseNoteLink)) { Source = _exportRecordData, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged };
                
                txtJiraTickets.SetBinding(TextBox.TextProperty, jiraBinding);
                txtExportReason.SetBinding(TextBox.TextProperty, reasonBinding);
                txtAlgoReleaseNotes.SetBinding(TextBox.TextProperty, releaseNoteBinding);
                
                // Non-bindable properties:
                lblExportUser.Content = value.ExportedBy;
                lblExportDate.Content = value.ExportDateTimeDisplay;

                // Prepare form/validation binding:
                btnOK.IsEnabled = _exportRecordData.IsValid;
                _exportRecordData.PropertyChanged += ExportDataPropertyChanged;
            }
        }

        private void ExportDataPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_exportRecordData != null) btnOK.IsEnabled = _exportRecordData.IsValid;
        }

        public InputBox(string prompt, string title, string[] type, WindowStartupLocation wsl, InputBoxValidate[] validate = null)
        {
            InitializeComponent();
            var len = type.Length;
            if (len > 1)
            {
                _Texts = new TextBox[len];
                _Texts[0] = textBox1;
                ibv = new InputBoxValidate[len];
                for (var i = 1; i < len; i++)
                {
                    InputPanel.RowDefinitions.Add(new RowDefinition());
                    var l = new Label { Content = type[i] };
                    l.SetValue(Grid.RowProperty, i);
                    var b = new TextBox { MinWidth = 200 };
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
            grds = new[] { Comments, AlgoFields, QuestionFields, AnswerFields, ConclusionFields, BulletFields, MergeLanguage, TitleFields, McKessonXml, ExportReportData, InputPanelCheckBox };
			if (type.Contains("|"))
			{
				if (type.StartsWith("Asset Fields")) appendPipe = true;
				grd = grds[int.Parse(type.Split('|')[1])];
				grd.Visibility = Visibility.Visible;
				InputPanel.Visibility = Visibility.Collapsed;
				foreach (var item in grd.Children)
				{
					if (item is CheckBox cb)
					{
                        cb.Checked += cb_Checked;
						cb.Unchecked += cb_Checked;
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
			var s = "";
			if (appendPipe) s = "|";
			foreach (var item in grd.Children)
			{
				if (item is CheckBox cb)
				{
                    if (cb.IsChecked == true)
					{
						var content = cb.Content.ToString();
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
            var enable = true;
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
                        var dt = DateTime.MinValue;
                        if (!DateTime.TryParse(text, out dt))
                            enable = false;
                        break;
                    case InputBoxValidate.Int:
                        var x = 0;
                        if (!int.TryParse(text, out x))
                            enable = false;
                        break;
                    case InputBoxValidate.Required:
                        enable = !string.IsNullOrWhiteSpace(text);
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
