using System;
using System.Globalization;
using Avalonia.Data.Converters;


namespace Exodus.Helpers.Converters;

public class NumericUpDownValueConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
            return 0;
        return ((IConvertible)value).ToDecimal(culture);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            if (targetType == typeof(int))
                return 0;
            if (targetType == typeof(double))
                return 0.00000001;
            throw new ArgumentNullException($"Unsupported type: {targetType.FullName}");
        }

        if (targetType == typeof(int))
            return ((IConvertible)value).ToInt32(culture);
        if (targetType == typeof(double))
            return ((IConvertible)value).ToDouble(culture);
        throw new ArgumentNullException($"Unsupported type: {targetType.FullName}");
    }
}