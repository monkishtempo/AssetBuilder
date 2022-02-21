using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace AssetBuilder.Controls
{
    class GridLengthAnimation : AnimationTimeline
    {
        public GridLength From
        {
            get { return (GridLength)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for From.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));

        public GridLength To
        {
            get { return (GridLength)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for To.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));

        protected override Freezable CreateInstanceCore()
        {
            return new GridLengthAnimation();
        }

        public GridLengthAnimation()
        {
        }

        public GridLengthAnimation(double from, double to, Duration duration, GridUnitType gridUnit)
        {
            Duration = duration;
            From = new GridLength(from, gridUnit);
            To = new GridLength(to, gridUnit);
        }

        public override Type TargetPropertyType
        {
            get { return typeof(GridLength); }
        }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double fromValue = ((GridLength)GetValue(GridLengthAnimation.FromProperty)).Value;
            double toValue = ((GridLength)GetValue(GridLengthAnimation.ToProperty)).Value;

            return new GridLength((animationClock.CurrentProgress.Value * (toValue - fromValue)) + fromValue, this.To.GridUnitType);
        }
    }
}
