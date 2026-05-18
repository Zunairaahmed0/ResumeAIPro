using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace ResumeAIPro.Controls
{
    public partial class ScoreArcControl : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(ScoreArcControl),
                new PropertyMetadata(0, OnValueChanged));

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(ScoreArcControl),
                new PropertyMetadata("SCORE", OnLabelChanged));

        public static readonly DependencyProperty ShowSubTextProperty =
            DependencyProperty.Register(nameof(ShowSubText), typeof(bool), typeof(ScoreArcControl),
                new PropertyMetadata(true, OnShowSubTextChanged));

        private static readonly DependencyProperty AnimatedValueProperty =
            DependencyProperty.Register("AnimatedValue", typeof(double), typeof(ScoreArcControl),
                new PropertyMetadata(0.0, OnAnimatedValueChanged));

        public int    Value       { get => (int)GetValue(ValueProperty);       set => SetValue(ValueProperty, value); }
        public string Label       { get => (string)GetValue(LabelProperty);    set => SetValue(LabelProperty, value); }
        public bool   ShowSubText { get => (bool)GetValue(ShowSubTextProperty);set => SetValue(ShowSubTextProperty, value); }

        public ScoreArcControl()
        {
            InitializeComponent();
            Loaded += (_, _) =>
            {
                SetValue(AnimatedValueProperty, 0.0);
                TriggerAnimation(Value);
            };
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScoreArcControl c && c.IsLoaded)
                c.TriggerAnimation((int)e.NewValue);
        }

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScoreArcControl c) c.LabelText.Text = e.NewValue?.ToString() ?? "";
        }

        private static void OnShowSubTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScoreArcControl c)
                c.SubText.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnAnimatedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScoreArcControl c) c.RenderArc((double)e.NewValue);
        }

        private void TriggerAnimation(int target)
        {
            var current = (double)GetValue(AnimatedValueProperty);
            var anim = new DoubleAnimation(current, target, TimeSpan.FromSeconds(1.0))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            BeginAnimation(AnimatedValueProperty, anim);
        }

        private void RenderArc(double value)
        {
            const double cx = 80, cy = 80, r = 60;
            const double startAngle = 135, totalSweep = 270;

            BackArc.Data = BuildArc(cx, cy, r, startAngle, totalSweep);

            var sweep = value / 100.0 * totalSweep;
            ForeArc.Data = sweep > 0.5 ? BuildArc(cx, cy, r, startAngle, sweep) : null;

            // Score-matched color
            var (hexColor, glowColor) = value switch
            {
                >= 80 => ("#10B981", Color.FromRgb(16, 185, 129)),
                >= 60 => ("#06B6D4", Color.FromRgb(6, 182, 212)),
                >= 40 => ("#F59E0B", Color.FromRgb(245, 158, 11)),
                _     => ("#EF4444", Color.FromRgb(239, 68, 68))
            };

            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
            ForeArc.Stroke   = brush;
            ValueText.Foreground = brush;

            // Update glow colors
            if (ForeArc.Effect is DropShadowEffect arcGlow)
                arcGlow.Color = glowColor;
            if (ValueText.Effect is DropShadowEffect textGlow)
                textGlow.Color = glowColor;

            ValueText.Text = ((int)Math.Round(value)).ToString();
            LabelText.Text = Label;
        }

        private static PathGeometry BuildArc(double cx, double cy, double r, double startDeg, double sweepDeg)
        {
            if (sweepDeg < 0.1) return new PathGeometry();
            if (sweepDeg >= 360) sweepDeg = 359.99;

            double startRad = startDeg * Math.PI / 180.0;
            double endRad   = (startDeg + sweepDeg) * Math.PI / 180.0;

            var sp = new Point(cx + r * Math.Cos(startRad), cy + r * Math.Sin(startRad));
            var ep = new Point(cx + r * Math.Cos(endRad),   cy + r * Math.Sin(endRad));

            var figure = new PathFigure { StartPoint = sp, IsClosed = false };
            figure.Segments.Add(new ArcSegment(ep, new Size(r, r), 0, sweepDeg > 180,
                SweepDirection.Clockwise, true));

            var geo = new PathGeometry();
            geo.Figures.Add(figure);
            return geo;
        }
    }
}
