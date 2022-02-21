using AssetBuilder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurveVisualiser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : TransparentControlWindow
    {
        List<TextBox> tb = new List<TextBox>();
        TextBox _textbox = null;
        Dictionary<string, double> extents = new Dictionary<string, double>()
        {
            { "MinX", 0 },
            { "MinY", 0 },
            { "MaxX", 100 },
            { "MaxY", 100 },
            { "Pres", 2 },
        };

        public MainWindow(TextBox textbox)
        {
            InitializeComponent();
            tb.AddRange(controls.Children.OfType<TextBox>());

            points.CollectionChanged += points_CollectionChanged;
            ReDrawFromTextBox(textbox, null);
            textbox.TextChanged += ReDrawFromTextBox;
            _textbox = textbox;
            Owner = Window.GetWindow(textbox);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _textbox.TextChanged -= ReDrawFromTextBox;
        }

        private void ReDrawFromTextBox(object textbox, EventArgs e)
        {
            if (!render) return;
            render = false;
            points.Clear();
            string[] prms = (textbox as TextBox).Text.Replace(" ", "").Split('\'');
            if (prms.Length == 3)
            {
                string[] ls = prms[0].Split(',');
                if (ls.Length > 1 && ls[1] == "0") Linear.IsChecked = true; else Linear.IsChecked = false;
                string[] xypoints = prms[1].Split(',');
                for (int i = 0; i < xypoints.Length - 1; i += 2)
                {
                    double x, y;
                    if (double.TryParse(xypoints[i], out x) && double.TryParse(xypoints[i + 1], out y))
                        points.Add(new Point(x, y));
                }
                if (points.Count > 0 && setlabels)
                {
                    minx = points.Select(f => f.X).Min();
                    maxx = points.Select(f => f.X).Max();
                    miny = points.Select(f => f.Y).Min();
                    maxy = points.Select(f => f.Y).Max();
                    double padx = (maxx - minx) / 15;
                    double pady = (maxy - miny) / 15;
                    int rx = (int)Math.Round(Math.Log10(padx) * -1, 0) + 1;
                    int ry = (int)Math.Round(Math.Log10(pady) * -1, 0) + 1;
                    pres = Math.Max(rx, ry);
                    double[] m = {
                        Math.Round(minx - padx, rx),
                        Math.Round(maxx + padx, rx),
                        Math.Round(miny - pady, ry),
                        Math.Round(maxy + pady, ry),
                    };
                    MoveOrScaleGraph(m, true);
                }
                else
                {
                    render = true;
                    renderGraph();
                }
            }
            else
            {
                render = true;
                renderGraph();
            }
        }

        void points_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            pointLabel.Text = points.OrderBy(f => f.X).Select(f => f.X + ", " + f.Y + "\r\n").Aggregate("", (total, next) => total += next);
            UpdateTextBox();
        }

        private void UpdateTextBox()
        {
            if (_textbox != null && render && !_textbox.IsReadOnly)
            {
                render = false;
                string[] tf = _textbox.Text.Split('\'');
                string format;
                if (tf.Length == 3)
                    format = tf[0] + "'{0}'" + tf[2];
                else format = "=dbo.GetY(a, 0.3333, '{0}')";
                tf = format.Split(',');
                if (tf.Length == 3)
                {
                    if (Linear.IsChecked == true) format = tf[0] + ", 0," + tf[2];
                    else format = tf[0] + ", 0.3333," + tf[2];
                }
                string XYpoints = points.OrderBy(f => f.X).Select(f => f.X + ", " + f.Y).Aggregate("", (total, next) => total += (total != "" ? ", " : "") + next);
                _textbox.Text = string.Format(format, XYpoints);
                BindingExpression binding = _textbox.GetBindingExpression(TextBox.TextProperty);
                if (binding != null) binding.UpdateSource();
                render = true;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox t = sender as TextBox;
            double d;
            if (!double.TryParse(t.Text, out d)) t.Tag = "Error";
            else
            {
                if (extents.ContainsKey(t.Name)) extents[t.Name] = d;
                t.Tag = null;
                if (!tb.Any(f => f.Tag != null && f.Tag.ToString() == "Error") && graph != null) renderGraph();
            }
        }

        double xc = 1000;
        double yc = 500;
        double xd = 100;
        double yd = 50;

        double minx = 0;
        double miny = 0;
        double maxx = 0;
        double maxy = 0;
        double pres = 0;
        Pen linePen = new Pen(Brushes.Green, 3);
        List<Rect> labels = new List<Rect>();
        bool setlabels = true;

        private void renderGraph(bool overrideSetLabels = false)
        {
            if (graph == null || !render) return;
            if (overrideSetLabels) setlabels = true;
            Pen shapeOutlinePen = new Pen(Brushes.Black, 2);
            shapeOutlinePen.Freeze();

            minx = extents["MinX"];
            miny = extents["MinY"];
            maxx = extents["MaxX"];
            maxy = extents["MaxY"];
            pres = extents["Pres"];

            double square = Math.Max((maxx - minx) / 10, (maxy - miny) / 10);

            xc = (Width / 632) * 1000;
            yc = ((Height - 50) / (Width - 245)) * xc;
            xd = xc / 10;
            yd = yc / 10;

            // Create a DrawingGroup
            DrawingGroup dGroup = new DrawingGroup();
            var pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            // Obtain a DrawingContext from  
            // the DrawingGroup. 
            using (DrawingContext dc = dGroup.Open())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, new Rect(-175, -50, xc + 275, yc + 125));
                if (setlabels)
                    labels.Clear();
                currentLabel = -1;
                //dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, new Rect(0, 0, xc, yc));
                // Draw grid
                for (int i = 0; i < 11; i++)
                {
                    dc.DrawLine(shapeOutlinePen, new Point(0, i * yd), new Point(xc, i * yd));
                    dc.DrawLine(shapeOutlinePen, new Point(i * xd, 0), new Point(i * xd, yc));
                    string xl = OutputNumber(i * ((maxx - minx) / 10) + minx, '#');
                    string yl = OutputNumber(i * ((maxy - miny) / 10) + miny);
                    if (setlabels)
                    {
                        labels.Add(Extension.DrawText(dc, xl, i * xd, yc + 20, TextAlignment.Center, System.Windows.VerticalAlignment.Top, pixelsPerDip));
                        labels.Add(Extension.DrawText(dc, yl, -20, (10 - i) * yd, TextAlignment.Right, System.Windows.VerticalAlignment.Center, pixelsPerDip));
                    }
                    else
                    {
                        bool nearLabel = false;
                        nearLabel = insideRect(scalePoint, labels[i * 2]);
                        if (nearLabel) currentLabel = i;
                        Extension.DrawText(dc, xl, i * xd, yc + 20, TextAlignment.Center, System.Windows.VerticalAlignment.Top, pixelsPerDip, nearLabel ? Brushes.Green : Brushes.Black);
                        nearLabel = insideRect(scalePoint, labels[i * 2 + 1]);
                        if (nearLabel) currentLabel = i;
                        Extension.DrawText(dc, yl, -20, (10 - i) * yd, TextAlignment.Right, System.Windows.VerticalAlignment.Center, pixelsPerDip, nearLabel ? Brushes.Green : Brushes.Black);
                    }
                }

                setlabels = false;

                //// Draw curve
                //if (points.Count > 1)
                //{
                //    string XYpoints = points.OrderBy(f => f.X).Select(f => f.X + ", " + f.Y).Aggregate("", (total, next) => total += (total != "" ? ", " : "") + next);
                //    for (int i = 0; i < xc; i++)
                //    {
                //        double px = scale(i, xc, minx, maxx);
                //        double py = (double)UserDefinedFunctions.GetY(px, Linear.IsChecked == true ? 0 : 0.3333, XYpoints);
                //        double iy = scale(py - miny, maxy - miny, yc, 0);
                //        if (iy >= 0 && iy <= yc)
                //            dc.DrawEllipse(Brushes.Green, null, new Point(i, iy), 2, 2);
                //    }
                //}

                dc.PushClip(new RectangleGeometry(new Rect(0, 0, xc, yc)));
                if (points.Count > 1)
                {
                    Path path = new Path();
                    PathFigure pf = new PathFigure() { IsClosed = false, IsFilled = false };
                    List<Curve.DataPoint> dPoints = getDataPoints();

                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        BezierPoints bp = UserDefinedFunctions.getPoints(dPoints, Linear.IsChecked == true ? 0 : 0.3333, i);
                        if (i == 0) pf.StartPoint = new Point(bp.X0, bp.Y0);
                        BezierSegment bs = new BezierSegment(new Point(bp.X1, bp.Y1), new Point(bp.X2, bp.Y2), new Point(bp.X3, bp.Y3), true);
                        pf.Segments.Add(bs);
                    }
                    PathGeometry pg = new PathGeometry(new PathFigure[] { pf });
                    path.Data = pg;
                    dc.DrawGeometry(null, linePen, path.Data);
                    ExtendLine(dc, dPoints[0], dPoints[1], 0);
                    ExtendLine(dc, dPoints[dPoints.Count - 1], dPoints[dPoints.Count - 2], xc);
                }

                dc.Pop();

                currentPoint = -1;
                // Draw Points
                foreach (var item in points)
                {
                    double px = Extension.Scale(item.X - minx, maxx - minx, 0, xc);
                    double py = Extension.Scale(item.Y - miny, maxy - miny, yc, 0);
                    Brush brush = Brushes.Red;
                    if (nearEnough(item))
                    {
                        brush = Brushes.Green;
                        currentPoint = points.IndexOf(item);
                    }
                    if (px >= 0 && px <= xc & py >= 0 && py <= yc) dc.DrawEllipse(brush, shapeOutlinePen, new Point(px, py), 10, 10);
                }
            }

            // Display the drawing using an image control.
            DrawingImage dImageSource = new DrawingImage(dGroup);
            bounds = dImageSource.Drawing.Bounds;
            graph.Source = dImageSource;

        }

        private void ExtendLine(DrawingContext dc, Curve.DataPoint dp0, Curve.DataPoint dp1, double extent)
        {
            dc.DrawLine(linePen, new Point(extent, dp0.Y - ((dp1.Y - dp0.Y) / (dp1.X - dp0.X) * (dp0.X - extent))), new Point(dp0.X, dp0.Y));
        }

        List<Curve.DataPoint> getDataPoints()
        {
            List<Curve.DataPoint> list = new List<Curve.DataPoint>();
            foreach (var item in points.OrderBy(f => f.X))
                list.Add(new Curve.DataPoint() { X = Extension.Scale(item.X - minx, maxx - minx, 0, xc), Y = Extension.Scale(item.Y - miny, maxy - miny, yc, 0) });
            return list;
        }

        private bool nearEnough(Point item)
        {
            if (graphPos.HasValue)
            {
                double px = Extension.Scale(item.X - minx, maxx - minx, 0, xc);
                double py = Extension.Scale(item.Y - miny, maxy - miny, yc, 0);
                double gx = Extension.Scale(graphPos.Value.X - minx, maxx - minx, 0, xc);
                double gy = Extension.Scale(graphPos.Value.Y - miny, maxy - miny, yc, 0);
                if (Math.Abs(px - gx) < 10 && Math.Abs(py - gy) < 10) return true;
            }
            return false;
        }

        private bool insideRect(Point item, Rect r)
        {
            return item.X >= r.X && item.X <= r.X + r.Width && item.Y >= r.Y && item.Y <= r.Y + r.Height;
        }

        Rect bounds;
        Point? graphPos;
        Point? origin;
        Point? reScaleFrom;
        double reScaleValue = 0;
        int reScalePos = 0;
        bool reScaleVertical = true;
        int currentPoint = -1;
        int currentLabel = -1;

        private void graph_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point? p = getPoint(e);
            if (p.HasValue)
            {
                reScaleFrom = null;
                if (e.RightButton == MouseButtonState.Pressed) origin = graphPos;
                if (currentPoint == -1)
                {
                    if (points.Count(f => f.X == graphPos.Value.X) > 0) return;
                    if (e.LeftButton == MouseButtonState.Pressed) points.Add(p.Value);
                }
                else if (e.RightButton == MouseButtonState.Pressed)
                {
                    points.RemoveAt(currentPoint);
                }
            }
            else if (labels.Any(f => insideRect(scalePoint, f)))
            {
                reScaleFrom = getPoint(e, true);
                if (scalePoint.Y > yc + 20)
                {
                    reScaleVertical = false;
                    reScalePos = (int)Math.Round((reScaleFrom.Value.X - minx) / ((maxx - minx) / 10), 0);
                    reScaleValue = reScalePos * ((maxx - minx) / 10) + minx;
                }
                else
                {
                    reScaleVertical = true;
                    reScalePos = (int)Math.Round((reScaleFrom.Value.Y - miny) / ((maxy - miny) / 10), 0);
                    reScaleValue = reScalePos * ((maxy - miny) / 10) + miny;
                }
            }
            renderGraph();
        }

        private void graph_MouseMove(object sender, MouseEventArgs e)
        {
            pres = extents["Pres"];
            string format = string.Format("{{0:F{0}}} x {{1:F{0}}}", pres);
            graphPos = getPoint(e);
            if (e.LeftButton == MouseButtonState.Pressed && reScaleFrom != null)
            {
                Point reScaleTo = getPoint(e, true).Value;
                double[] m = {
                        reScaleVertical ? minx : (minx - (reScaleTo.X - reScaleFrom.Value.X)),
                        reScaleVertical ? maxx : (maxx - (reScaleTo.X - reScaleFrom.Value.X)),
                        !reScaleVertical  ? miny : (miny - (reScaleTo.Y - reScaleFrom.Value.Y)),
                        !reScaleVertical  ? maxy : (maxy - (reScaleTo.Y - reScaleFrom.Value.Y))
                    };

                MoveOrScaleGraph(m);
            }
            else if (e.RightButton == MouseButtonState.Pressed && reScaleFrom != null)
            {
                Point reScaleTo = getPoint(e, true).Value;
                double[] m = { minx, maxx, miny, maxy };
                if (scalePoint.Y > yc + 20)
                {
                    double xm = (maxx - minx) / 10;
                    int tpos = (int)Math.Round((reScaleTo.X - minx) / xm, 0);
                    if (reScalePos > 0 && tpos != 0) m[1] = (((reScaleValue - minx) / tpos) * 10) + minx;
                    else if (reScalePos == 0 && tpos != 10) m[0] = maxx - (((maxx - reScaleValue) / (10 - tpos)) * 10);
                }
                else
                {
                    double ym = (maxy - miny) / 10;
                    int tpos = (int)Math.Round((reScaleTo.Y - miny) / ym, 0);
                    if (reScalePos > 0 && tpos != 0) m[3] = (((reScaleValue - miny) / tpos) * 10) + miny;
                    else if (reScalePos == 0 && tpos != 10) m[2] = maxy - (((maxy - reScaleValue) / (10 - tpos)) * 10);
                }

                MoveOrScaleGraph(m, true);
            }
            if (e.RightButton != MouseButtonState.Pressed) origin = null;
            if (graphPos.HasValue)
            {
                pos.Content = string.Format(format, graphPos.Value.X, graphPos.Value.Y);

                if (e.RightButton == MouseButtonState.Pressed && origin.HasValue)
                {
                    pos.Content += " " + string.Format(format, origin.Value.X, origin.Value.Y);
                    double[] m = {
                        minx - (graphPos.Value.X - origin.Value.X),
                        maxx - (graphPos.Value.X - origin.Value.X),
                        miny - (graphPos.Value.Y - origin.Value.Y),
                        maxy - (graphPos.Value.Y - origin.Value.Y),
                    };

                    MoveOrScaleGraph(m);
                }
                else if (e.LeftButton == MouseButtonState.Pressed && currentPoint > -1)
                {
                    if (points.Count(f => f.X == graphPos.Value.X && points.IndexOf(f) != currentPoint) > 0) return;
                    points.RemoveAt(currentPoint);
                    points.Insert(currentPoint, new Point(graphPos.Value.X, graphPos.Value.Y));
                    renderGraph();
                }
                // Renders hover actions on points when a change occurs;
                else
                {
                    if (points.Any(f => nearEnough(f)))
                    {
                        if (currentPoint == -1) renderGraph();
                    }
                    else
                    {
                        if (currentPoint != -1) renderGraph();
                    }
                }
            }
            else
            {
                pos.Content = "";
                currentPoint = -1;
                if (labels.Any(f => insideRect(scalePoint, f)))
                {
                    if (currentLabel == -1) renderGraph();
                }
                else
                {
                    if (currentLabel != -1) renderGraph();
                }
            }
        }

        Point scalePoint;

        private Point? getPoint(MouseEventArgs e, bool returnOutside = false)
        {
            if (bounds == null) return null;
            int pres = int.Parse(Pres.Text);
            Point s = GetScalePoint(e);
            scalePoint = s;
            double px = Extension.Scale(s.X, xc, minx, maxx);
            double py = Extension.Scale(yc - s.Y, yc, miny, maxy);
            if (returnOutside || (px >= minx && px <= maxx && py >= miny && py <= maxy))
                return new Point(Math.Round(px, pres), Math.Round(py, pres));
            else return null;
        }

        ObservableCollection<Point> points = new ObservableCollection<Point>();

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            points.Clear();
            renderGraph();
        }

        private void Linear_Checked(object sender, RoutedEventArgs e)
        {
            renderGraph();
            UpdateTextBox();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (opening != null)
            {
                opening.Opacity = (sender as Slider).Value / 100.0;
            }
        }

        double x = 0, y = 0;
        private void controls_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(controls);
                x = p.X;
                y = p.Y;
                System.Diagnostics.Debug.Write(string.Format("{0} {1}", x, y));
            }
        }

        private void controls_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(controls);
                this.Left += p.X - x;
                this.Top += p.Y - y;
            }
        }

        private void DockPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void graph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            renderGraph(true);
        }

        private void graph_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double sc = e.Delta > 0 ? 1.25 : 0.8;
            Point s = GetScalePoint(e);

            double rx = s.X / xc;
            double ry = s.Y / yc;

            double px = Extension.Scale(s.X, xc, minx, maxx);
            double py = Extension.Scale(yc - s.Y, yc, miny, maxy);

            double[] m = {
                px - ((maxx - minx) * rx / sc),
                px + ((maxx - minx) * (1 - rx) / sc),
                py - ((maxy - miny) * ry / sc),
                py + ((maxy - miny) * (1 - ry) / sc)
            };

            MoveOrScaleGraph(m);
        }

        private Point GetScalePoint(MouseEventArgs e)
        {
            Point p = e.GetPosition(graph);
            double sx = Extension.Scale(p.X, graph.ActualWidth, bounds.Left, bounds.Right);
            double sy = Extension.Scale(p.Y, graph.ActualHeight, bounds.Top, bounds.Bottom);
            return new Point(sx, sy);
        }

        bool render = true;

        private void MoveOrScaleGraph(double[] m, bool ignoreShift = false)
        {
            if (m[3] <= m[2] || m[1] <= m[0]) return;
            if (!ignoreShift && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                double xm = (maxx - minx) / 10;
                double ym = (maxy - miny) / 10;
                m[0] = Math.Round(m[0] / xm, 0) * xm;
                m[1] = Math.Round(m[1] / xm, 0) * xm;
                m[2] = Math.Round(m[2] / ym, 0) * ym;
                m[3] = Math.Round(m[3] / ym, 0) * ym;
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                for (int i = 0; i < points.Count; i++)
                {
                    Point p = new Point(
                        Math.Round(m[0] + (((points[i].X - minx) / (maxx - minx)) * (m[1] - m[0])), (int)pres),
                        Math.Round(m[2] + (((points[i].Y - miny) / (maxy - miny)) * (m[3] - m[2])), (int)pres)
                    );
                    points.RemoveAt(i);
                    points.Insert(i, p);
                }
            }
            render = false;
            MinX.Text = OutputNumber(m[0]);
            MaxX.Text = OutputNumber(m[1]);
            MinY.Text = OutputNumber(m[2]);
            MaxY.Text = OutputNumber(m[3]);
            render = true;
            renderGraph(true);
        }

        private string OutputNumber(double m, char format = '#')
        {
            return string.Format(string.Format("{{0:0.{0}}}", new string(format, (int)pres)), m);
        }

        private void Round_Click(object sender, RoutedEventArgs e)
        {
            MoveOrScaleGraph(new double[] {
                Math.Round(minx, 0),
                Math.Round(maxx, 0),
                Math.Round(miny, 0),
                Math.Round(maxy, 0),
            });
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as TextBox).SelectAll();
        }
    }
}
