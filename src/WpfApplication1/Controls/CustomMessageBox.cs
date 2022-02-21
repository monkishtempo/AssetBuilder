using AssetBuilder.AssetControls;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Diva.Controls.Simple
{
    public partial class CustomMessageBox : Form
    {
        public CustomMessageBox()
        {
            InitializeComponent();
            pictureBox1.Image = System.Drawing.SystemIcons.Warning.ToBitmap();
        }

        public string Message { get { return label1.Text; } set { label1.Text = value; } }
        public string Caption { get { return Text; } set { Text = value; } }
        public string Result { get; set; }

        public static string Show(string text, string caption, string[] buttons)
        {
            return Show(text, caption, buttons, buttons.FirstOrDefault(), buttons.LastOrDefault());
        }

        public static string Show(string text, string caption, string[] buttons, string AcceptButton, string CancelButton, bool LinkToVisio = false)
        {
            CustomMessageBox cm;
            if (!LinkToVisio) cm = new CustomMessageBox { Message = text, Caption = caption };
            else
            {
                cm = new CustomMessageBox { Message = text, Caption = caption };
                cm.label1.Visible = false;
                cm.panel1.Visible = true;
                foreach (var item in text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] data = item.Substring(0, item.IndexOf(' ')).Split('|');
                    string linktext = data[2] + item.Substring(item.IndexOf(' '));
                    LinkLabel l = new LinkLabel { Text = linktext, AutoSize = true, LinkArea = new LinkArea(0, linktext.IndexOf(' ')) };
                    l.Links[0].LinkData = data[2];
                    l.LinkClicked += delegate(object sender, LinkLabelLinkClickedEventArgs e)
                    {
                        usageShape us = new usageShape() { AlgoID = data[0], NodeID = data[1], ShapeName = e.Link.LinkData.ToString() };
                        bool VisioFound;
                        assetControl.HighlightVisioShape(us, out VisioFound, null);
                    };
                    cm.panel1.Controls.Add(l);
                }
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                string item = buttons[buttons.Length - (1 + i)];
                Button b = new Button { Text = item, Height = 28, AutoSize = true, Padding = new Padding(8, 0, 8, 0) };
                if (item == AcceptButton) cm.AcceptButton = b;
                if (item == CancelButton) cm.CancelButton = b;
                cm.buttonPanel.Controls.Add(b);
                b.Click += delegate(object sender, EventArgs e)
                {
                    //Button b = sender as Button;
                    cm.Result = b.Text;
                    cm.Close();
                };
            }
            cm.Activated += delegate(object sender, EventArgs e) { (cm.AcceptButton as Button).Focus(); };
            cm.ShowDialog();
            return cm.Result;
        }
    }
}
