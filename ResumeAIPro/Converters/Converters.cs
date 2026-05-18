using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ResumeAIPro.Converters
{
    // ── Bool / Int → Visibility ──────────────────────────────────
    // Handles: bool, int (0=false, >0=true), string (empty=false)
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = value switch
            {
                bool b   => b,
                int  i   => i > 0,
                string s => !string.IsNullOrEmpty(s),
                _        => false
            };
            if (parameter is string p && p == "Inverse") flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type t, object p, CultureInfo c)
            => value is Visibility v && v == Visibility.Visible;
    }

    // ── Int Score → Brush ────────────────────────────────────────
    public class ScoreToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int score = value is int i ? i : 0;
            var color = score switch
            {
                >= 80 => Color.FromRgb(16, 185, 129),
                >= 60 => Color.FromRgb(6,  182, 212),
                >= 40 => Color.FromRgb(245,158,  11),
                _     => Color.FromRgb(239, 68,  68)
            };
            return new SolidColorBrush(color);
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => DependencyProperty.UnsetValue;
    }

    // ── String → Brush (hex) ─────────────────────────────────────
    public class HexToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (value is string hex)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            }
            catch { }
            return new SolidColorBrush(Colors.Gray);
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => DependencyProperty.UnsetValue;
    }

    // ── Int → Percent Width (for progress bars) ──────────────────
    public class ScoreToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double score = value is int i ? i : 0;
            double maxWidth = parameter is string s && double.TryParse(s, out double w) ? w : 300;
            return score / 100.0 * maxWidth;
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => DependencyProperty.UnsetValue;
    }

    // ── HiringOutlook → Background Brush ─────────────────────────
    public class OutlookToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (value?.ToString()) switch
            {
                "Strong" => Color.FromRgb(16,  185, 129),
                "Good"   => Color.FromRgb(6,   182, 212),
                "Fair"   => Color.FromRgb(245, 158,  11),
                "Weak"   => Color.FromRgb(239,  68,  68),
                _        => Color.FromRgb(100,  116, 139)
            };
            return new SolidColorBrush(color);
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => DependencyProperty.UnsetValue;
    }

    // ── Not Bool ─────────────────────────────────────────────────
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is bool b ? !b : true;
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => v is bool b ? !b : false;
    }

    // ── Int → string ─────────────────────────────────────────────
    public class ScoreToStringConverter : IValueConverter
    {
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is int i ? $"{i}" : "0";
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => DependencyProperty.UnsetValue;
    }

    // ── Score → Glow Color (Color not Brush, for DropShadowEffect) ──
    public class ScoreToGlowColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int score = value is int i ? i : 0;
            return score switch
            {
                >= 80 => Color.FromRgb(16, 185, 129),
                >= 60 => Color.FromRgb(6,  182, 212),
                >= 40 => Color.FromRgb(245,158,  11),
                _     => Color.FromRgb(239, 68,  68)
            };
        }
        public object ConvertBack(object v, Type t, object p, CultureInfo c)
            => DependencyProperty.UnsetValue;
    }
}
