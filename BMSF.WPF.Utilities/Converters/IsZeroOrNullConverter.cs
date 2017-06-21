namespace BMSF.WPF.Utilities.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class IsZeroOrNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return true;
            if (value is decimal)
                return (decimal) value == 0.00m;
            if (value is int)
                return (int) value == 0.00m;
            if (value is long)
                return (long) value == 0.00m;
            if (value is short)
                return (short) value == 0.00m;
            if (value is string)
                return (string) value == "";
            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}