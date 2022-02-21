using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace AssetBuilder.Controls
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

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (suspend) return;
            suspend = true;
            TextRange tr = new TextRange(Document.ContentStart, Document.ContentEnd);
            tr.ClearAllProperties();
            //TextSelection ts = Selection;
            Colour(this);
            //Selection.Select(ts.Start, ts.End);
            suspend = false;
        }

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

        private static void Colour(ABRichTextBox t)
        {
            setBetween(t, '<', '>', Colors.Red);
            setBetween(t, '<', " >", Colors.DarkRed);
            setCharacter(t, "=[]{}~<>/".ToCharArray(), Colors.Blue);
            setCharacter(t, '|', Colors.Red);
            setCharacter(t, '"', Colors.Black);
            setBetween(t, '>', '<', Colors.Black);
            setBetween(t, '\"', '\"', Colors.Blue);
        }

        private static void setCharacter(ABRichTextBox t, char p, Color color)
        {
            TextPointer pos = t.IndexOf(p, t.Document.ContentStart);
            while (pos != null)
            {
                TextRange tr = new TextRange(pos,
                    pos.GetPositionAtOffset(1));
                tr.ApplyPropertyValue(ForegroundProperty, new SolidColorBrush(color));
                pos = t.IndexOf(p, tr.End);
            }
        }

        private static void setCharacter(ABRichTextBox t, char[] p, Color color)
        {
            TextPointer pos = t.IndexOfAny(p, t.Document.ContentStart);
            while (pos != null)
            {
                TextRange tr = new TextRange(pos,
                    pos.GetPositionAtOffset(1));
                tr.ApplyPropertyValue(ForegroundProperty, new SolidColorBrush(color));
                pos = t.IndexOfAny(p, tr.End);
            }
        }

        private static void setBetween(ABRichTextBox t, char start, char finish, Color color)
        {
            setBetween(t, start, finish, color, null);
        }

        private static void setBetween(ABRichTextBox t, char start, string finish, Color color)
        {
            setBetween(t, start, finish.ToCharArray(), color, null);
        }

        private static void setBetween(ABRichTextBox t, char start, char finish, Color color, object newFont)
        {
            setBetween(t, start, new char[] { finish }, color, newFont);
        }

        private static void setBetween(ABRichTextBox t, char start, string finish, Color color, object newFont)
        {
            setBetween(t, start, finish.ToCharArray(), color, newFont);
        }

        private static void setBetween(ABRichTextBox t, char start, char[] finish, Color color, object newFont)
        {
            TextPointer[] pos = new TextPointer[2];
            pos[0] = t.IndexOf(start, t.Document.ContentStart);
            while (pos[0] != null)
            {
                pos[1] = t.IndexOfAny(finish, pos[0].GetPositionAtOffset(1));
                if (pos[1] != null)
                {
                    TextRange tr = new TextRange(pos[0].GetPositionAtOffset(1), pos[1]);
                    tr.ApplyPropertyValue(ForegroundProperty, new SolidColorBrush(color));
                    //if (newFont != null) t.SelectionFont = newFont;
                }
                else break;
                pos[0] = t.IndexOf(start, pos[1].GetPositionAtOffset(1));
            }
        }
    }
}
