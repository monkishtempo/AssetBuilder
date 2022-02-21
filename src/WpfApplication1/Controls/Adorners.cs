using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace AssetBuilder.Controls
{
    public class HighLightAdorner : Adorner
    {
        private static readonly Brush search;
        private static readonly Brush add;
        private static readonly Brush delete;
        private static readonly Brush edit;
        private static Brush hidden;
        private static readonly Brush b;
        private static Brush r;
        private static Pen searchPen;
        private static Pen addPen;
        private static Pen deletePen;
        private static Pen editPen;
        private static Pen hiddenPen;
        private static Pen bp;
        private static Pen rp;
        private int Start { get; set; }
        private int Length { get; set; }
		private Rect LastChar { get; set; }
		private double TextWidth;
		private double TextHeight;
		private string type = "";
        bool isError = false;
        bool isHidden = false;

        static HighLightAdorner()
        {
            search = new SolidColorBrush(Colors.Yellow) { Opacity = 0.3 };
            add = new SolidColorBrush(Colors.Yellow) { Opacity = 0.3 };
            delete = new SolidColorBrush(Colors.Red) { Opacity = 0.3 };
            edit = new SolidColorBrush(Colors.DodgerBlue) { Opacity = 0.3 };
            hidden = new SolidColorBrush(Colors.Red) { Opacity = 1 };
            b = new SolidColorBrush(Colors.Green);
            r = new SolidColorBrush(Colors.Red);
            search.Opacity = 0.3;
            hidden.Opacity = 1;
            b.Opacity = 0.3;
            r.Opacity = 0.3;
            searchPen = new Pen(search, 1);
            addPen = new Pen(add, 1);
            deletePen = new Pen(delete, 1);
            editPen = new Pen(edit, 1);
            hiddenPen = new Pen(hidden, 1);
            bp = new Pen(b, 1);
            rp = new Pen(r, 1);
        }

        public HighLightAdorner(UIElement tb, NLFail bounds, bool IsError)
            : this(tb, bounds.Position, bounds.Start)
        {
            isError = IsError;
            ToolTip = bounds.Message;
        }

        public HighLightAdorner(UIElement tb, int start, int length, string Message)
            : base(tb)
        {
            IsHitTestVisible = false;
            type = "search";
            if (Message == "edit" || Message == "add" || Message == "delete") type = Message;
            Start = start;
            Length = length;
            ReDraw();
            ToolTip = Message;
        }

        public void ReDraw()
        {
			TextBox tb = (this.AdornedElement as TextBox);
            Bounds = tb.getCharRect(Start, Length);
			LastChar = tb.GetRectFromCharacterIndex((Start + Length) - 1, true);
			TextWidth = tb.ActualWidth;
			TextHeight = tb.ActualHeight;
            if (Bounds.Height == 1) isHidden = true; else isHidden = false;
			if (Bounds.Width + Bounds.X > TextWidth) Bounds = new Rect(Bounds.X, Bounds.Y, TextWidth - Bounds.X, Bounds.Height);
            this.InvalidateVisual();
        }

        public HighLightAdorner(UIElement tb, Rect bounds, int start)
            : base(tb)
        {
            //Rect r = new Rect(tb.TranslatePoint(bounds.TopLeft, cnv), tb.TranslatePoint(bounds.BottomRight, cnv));
            Start = start;
            Length = 1;
            Bounds = bounds;
        }

        public Rect Bounds
        {
            get { return (Rect)GetValue(BoundsProperty); }
            set { SetValue(BoundsProperty, value); }
        }
        public static readonly DependencyProperty BoundsProperty =
            DependencyProperty.Register("Bounds", typeof(Rect), typeof(HighLightAdorner), new UIPropertyMetadata(default(Rect)));

        protected override void OnRender(DrawingContext drawingContext)
        {
			if (type == "search" || type == "add" || type == "edit" || type == "delete")
			{
                var brush = search;
                var pen = searchPen;
                if (type == "add") { brush = add; pen = addPen; }
                else if (type == "edit") { brush = edit; pen = editPen; }
                else if (type == "delete") { brush = delete; pen = deletePen; }
                drawingContext.DrawRectangle(isHidden ? hidden : brush, isHidden ? hiddenPen : pen, Bounds);
				if (!isHidden)
				{
					double top = Bounds.Y;
					double add = Bounds.Height;
					while (top < LastChar.Y && TextHeight > top + add)
					{
						top += add;
						drawingContext.DrawRectangle(brush, pen, new Rect(0, top, top < LastChar.Y ? TextWidth : LastChar.X, TextHeight < top + LastChar.Height ? TextHeight - top : LastChar.Height));
						add = LastChar.Height;
					}
				}
			}
			else if (isError)
				drawingContext.DrawRectangle(r, rp, Bounds);
			else
				drawingContext.DrawRectangle(b, bp, Bounds);
        }
    }

    public class TextAdorner : Adorner
    {
        readonly Control parent;
        readonly Typeface typeface;
        Brush brush = Brushes.Black;
        private string DefaultString;
        private string _Text = "";
		TextBoxBase tbb;
        private double pixelsPerDip;
        public string Text { get { return _Text; } set { if (value != "") _Text = value; else _Text = DefaultString; InvalidateVisual(); } }

        public TextAdorner(Control control, string defaultString, Color colour)
            : this(control, defaultString)
        {
            brush = new SolidColorBrush(colour);
        }

        public TextAdorner(Control control, string defaultString)
            : base(control)
        {
            IsClipEnabled = true;
            AdornedElement.ClipToBounds = true;
            IsHitTestVisible = false;
            DefaultString = defaultString;
            _Text = defaultString;
            parent = control;
			if (parent is TextBox) tbb = ((TextBoxBase)parent);
            typeface = new Typeface(control.FontFamily, control.FontStyle, control.FontWeight, control.FontStretch);
            pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
			FormattedText ft = new FormattedText(Text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, parent.FontSize, brush, pixelsPerDip);
			if (tbb != null)
			{
				ft.MaxTextWidth = parent.ActualWidth > 10 ? parent.ActualWidth-10 : 0;
				if (parent.ActualHeight <= 0) ft.MaxTextHeight = 10;
				else ft.MaxTextHeight = parent.ActualHeight;
			}
            drawingContext.DrawText(ft, new Point(5, 3));
        }

		public void ReDraw()
		{
			this.InvalidateVisual();
		}
    }

    public class NLFail
    {
        public Rect Position { get; set; }
        public int Start { get; set; }
        public string Message { get; set; }
    }

    public class NLError
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Match { get; set; }
    }
}
