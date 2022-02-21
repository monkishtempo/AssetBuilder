using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace CurveVisualiser
{
    public class TransparentControlWindow : AssetBuilder.Controls.ABWindow
    {
        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;

        private Dictionary<ResizeDirection, Cursor> cursors = new Dictionary<ResizeDirection, Cursor> 
        {
            {ResizeDirection.Top, Cursors.SizeNS},
            {ResizeDirection.Bottom, Cursors.SizeNS},
            {ResizeDirection.Left, Cursors.SizeWE},
            {ResizeDirection.Right, Cursors.SizeWE},
            {ResizeDirection.TopLeft, Cursors.SizeNWSE},
            {ResizeDirection.BottomRight, Cursors.SizeNWSE},
            {ResizeDirection.TopRight, Cursors.SizeNESW},
            {ResizeDirection.BottomLeft, Cursors.SizeNESW} 
        };

        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //private static extern IntPtr MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            hwndSource = HwndSource.FromVisual(this) as HwndSource;
        }

        protected void DragWindow(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }

        protected void ResetCursor(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                FrameworkElement element = e.OriginalSource as FrameworkElement;

                //Hack - only reset cursors if the orginal source isn't a draghandle
                if (element != null && !element.Name.Contains("DragHandle"))
                    this.Cursor = Cursors.Arrow;
            }
        }

        protected void ResizeIfPressed(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ResizeDirection direction = GetDirectionFromName(element.Name);

            this.Cursor = cursors[direction];

            if (e.LeftButton == MouseButtonState.Pressed)
                ResizeWindow(direction);
        }

        //Point pointontitle;
        //protected void MoveIfPressed(object sender, MouseEventArgs e)
        //{
        //    FrameworkElement element = sender as FrameworkElement;
        //    Point pe = e.GetPosition(element);
        //    Point p = element.PointToScreen(pe);

        //    if (e.LeftButton == MouseButtonState.Pressed)
        //        MoveWindow(hwndSource.Handle, (int)(p.X - pointontitle.X), (int)(p.Y - pointontitle.Y), (int)this.Width, (int)this.Height, false);
        //    else
        //        pointontitle = pe;
        //}

        private static ResizeDirection GetDirectionFromName(string name)
        {
            //Hack - Assumes the drag handels are all named *DragHandle
            string enumName = name.Replace("DragHandle", "");
            return (ResizeDirection)Enum.Parse(typeof(ResizeDirection), enumName);
        }

        private void ResizeWindow(ResizeDirection direction)
        {
            SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
    }
}