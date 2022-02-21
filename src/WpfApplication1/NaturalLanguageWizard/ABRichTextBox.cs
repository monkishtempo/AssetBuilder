using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace NaturalLanguageWizard
{
    public class ABRichTextBox : RichTextBox
    {
        public string Text
        {
            get
            {
                TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
                return tr.Text;
            }
            set
            {
                TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
                tr.Text = value;
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ABRichTextBox));

        bool suspend = false;

        System.Timers.Timer t = new System.Timers.Timer(1000);

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed);            
        }

        void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            suspend = true;

            lock (this)
            {
                t.Enabled = false;

                var tr = new TextRange(Document.ContentStart, Document.ContentEnd);

                this.Dispatcher.Invoke(
                    new Action(delegate()
                {
                    //tr.ClearAllProperties();
					AssetBuilder.SyntaxHilighter.SyntaxHilighter.SyntaxHilight(this, null);
                    //Colour(this);
                }),
                        DispatcherPriority.Normal);
            }
            suspend = false;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            if (!suspend)
            {
                base.OnTextChanged(e);

                t.Enabled = true;
                t.Interval = 100;
            }
        }

        //protected override void OnTextInput(System.Windows.Input.TextCompositionEventArgs e)
        //{
        //    base.OnTextInput(e);

        //    //if (this.Dispatcher.CheckAccess())
        //    //{
        //    t.Enabled = true;
        //    t.Interval = 1000;
        //    //}
        //}

        TextPointer IndexOf(char p, TextPointer from)
        {
            TextPointer start = from;
            while (start != null && start.CompareTo(Document.ContentEnd) < 0)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = start.GetTextInRun(LogicalDirection.Forward);
                    int pos = text.IndexOf(p);
                    if (pos >= 0) return start.GetPositionAtOffset(pos);
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }
            return null;
        }

        TextPointer IndexOfAny(char[] p, TextPointer from)
        {
            TextPointer start = from;
            while (start != null && start.CompareTo(Document.ContentEnd) < 0)
            {
                if (start.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = start.GetTextInRun(LogicalDirection.Forward);
                    int pos = text.IndexOfAny(p);
                    if (pos >= 0) return start.GetPositionAtOffset(pos);
                }
                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }
            return null;
        }

        #region Brushes

        private static SolidColorBrush black = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush red = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush blue = new SolidColorBrush(Colors.Blue);
        private static SolidColorBrush darkRed = new SolidColorBrush(Colors.DarkRed);

        #endregion

        private static void Colour(ABRichTextBox t)
        {
			//setBetween(t, '<', '>', red);
			//setBetween(t, '<', " >", darkRed);
			//setCharacter(t, "=[]{}~<>/".ToCharArray(), blue);
			//setCharacter(t, '|', red);
			//setCharacter(t, '"', black);
			//setBetween(t, '>', '<', black);
			//setBetween(t, '\"', '\"', blue);

			//t.SetValue(ForegroundProperty, black);
        }

        private static void setCharacter(ABRichTextBox t, char p, SolidColorBrush brush)
        {
            TextPointer pos = t.IndexOf(p, t.Document.ContentStart);
            while (pos != null)
            {
                TextRange tr = new TextRange(pos,
                    pos.GetPositionAtOffset(1));
                tr.ApplyPropertyValue(ForegroundProperty, brush);
                pos = t.IndexOf(p, tr.End);
            }
        }

        private static void setCharacter(ABRichTextBox t, char[] p, SolidColorBrush brush)
        {
            TextPointer pos = t.IndexOfAny(p, t.Document.ContentStart);
            while (pos != null)
            {
                TextRange tr = new TextRange(pos,
                    pos.GetPositionAtOffset(1));
                tr.ApplyPropertyValue(ForegroundProperty, brush);
                pos = t.IndexOfAny(p, tr.End);
            }
        }

        private static void setBetween(ABRichTextBox t, char start, char finish, SolidColorBrush brush)
        {
            setBetween(t, start, finish, brush, null);
        }

        private static void setBetween(ABRichTextBox t, char start, string finish, SolidColorBrush brush)
        {
            setBetween(t, start, finish.ToCharArray(), brush, null);
        }

        private static void setBetween(ABRichTextBox t, char start, char finish, SolidColorBrush brush, object newFont)
        {
            setBetween(t, start, new char[] { finish }, brush, newFont);
        }

        private static void setBetween(ABRichTextBox t, char start, string finish, SolidColorBrush brush, object newFont)
        {
            setBetween(t, start, finish.ToCharArray(), brush, newFont);
        }

        private static void setBetween(ABRichTextBox t, char start, char[] finish, SolidColorBrush brush, object newFont)
        {
            TextPointer[] pos = new TextPointer[2];
            pos[0] = t.IndexOf(start, t.Document.ContentStart);
            while (pos[0] != null)
            {
                pos[1] = t.IndexOfAny(finish, pos[0].GetPositionAtOffset(1));
                if (pos[1] != null)
                {
                    TextRange tr = new TextRange(pos[0].GetPositionAtOffset(1), pos[1]);
                    tr.ApplyPropertyValue(ForegroundProperty, brush);
                    //if (newFont != null) t.SelectionFont = newFont;
                }
                else break;
                pos[0] = t.IndexOf(start, pos[1].GetPositionAtOffset(1));
            }
        }
    }
}
