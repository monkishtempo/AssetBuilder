using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NaturalLanguageWizard
{
    public enum MarginDirection
    {
        Left,
        Top,
        Right,
        Bottom
    }

    static class WPFExtensions
    {        
        //public static bool IsText(this QuestionType type)
        //{
        //    return type == QuestionType.SingleAnswer
        //        || type == QuestionType.MultipleAnswer
        //        || type == QuestionType.FreeText;
        //}

        public static void SaveAndClear(this TextBox tb)
        {
            tb.Tag = tb.Text;

            tb.Text = string.Empty;
        }

        public static void RestoreLast(this TextBox tb)
        {
            tb.Text = tb.Tag.ToString();
        }

        public static void SaveAndInitialise(this SaveableRadioButton[] buttons, SaveableRadioButton initial)
        {
            foreach (var button in buttons.Where(b => b != initial))
            {
                button.SaveAndInitialise(false);
            }

            buttons.Single(b => b == initial).SaveAndInitialise(true);
        }

        public static void RestoreLast(this SaveableRadioButton[] buttons)
        {
            foreach (var button in buttons)
            {
                button.RestoreLast();
            }
        }

        public static bool IsDelete(this Key key)
        {
            return key == Key.Delete;
        }

        public static bool IsBack(this Key key)
        {
            return key == Key.Back;
        }

        public static bool IsDeleteOrBack(this Key key)
        {
            return key.IsBack() || key.IsDelete();
        }

        public static bool HasCaretAtExtremeRight(this TextBox textbox)
        {
            return textbox.CaretIndex == textbox.Text.Length;
        }

        public static bool HasCaretAtExtremeLeft(this TextBox textbox)
        {
            return textbox.CaretIndex == 0;
        }

        public static bool IsANumber(this string text)
        {
            int result = 0;

            return int.TryParse(text, out result);
        }

        public static void ExpandOnlyIfSelected(this Expander exp, Expander selectedExpander)
        {
            exp.IsExpanded = (exp == selectedExpander);
        }

        public static string GetReplacedText(this TextBox tb, string text)
        {
            var ss = tb.SelectionStart;
            var sl = tb.SelectionLength;

            return tb.Text.Remove(ss, sl).Insert(ss, text);
        }

        public static Func<object, bool> ConvertToFunc(this Predicate<object> p)
        {
            return new Func<object, bool>(a => p(a));
        }

        public static void SetMargin(this FrameworkElement fe, double length, MarginDirection margin)
        {
            var left = (margin == MarginDirection.Left ? length : fe.Margin.Left);
            var top = (margin == MarginDirection.Top ? length : fe.Margin.Top);
            var right = (margin == MarginDirection.Right ? length : fe.Margin.Right);
            var bottom = (margin == MarginDirection.Bottom ? length : fe.Margin.Bottom);

            fe.Margin = new Thickness(left, top, right, bottom);
        }
    }
}
