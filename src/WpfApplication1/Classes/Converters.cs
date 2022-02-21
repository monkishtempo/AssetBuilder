using System;
using System.Windows.Data;
using System.Xml;

namespace AssetBuilder
{
	[ValueConversion(typeof(double), typeof(bool))]
	class GreaterThan : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null || parameter == null) return false;
			double dValue = 0;
			double tValue;
			if (value is string && double.TryParse(value as string, out tValue)) dValue = tValue;
			else if (value is XmlNode && double.TryParse((value as XmlNode).InnerText, out tValue)) dValue = tValue;
			else return false;
			double prm;
			if (double.TryParse(parameter as string, out prm) && dValue > prm) return true;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	[ValueConversion(typeof(DateTime), typeof(string))]
	class DateConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			DateTime date = DateTime.MinValue;
			if (value is string) date = DateTime.Parse(value as string);
			else if (value is XmlNode) date = DateTime.Parse((value as XmlNode).InnerText);
			string format = "yyyy-MM-dd";
			if (parameter is string) format = parameter as string;
			return date.ToString(format);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}

	[ValueConversion(typeof(bool), typeof(string))]
	class BoolConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null || parameter == null) return false;
			return (string)value == (string)parameter;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null || parameter == null) return null;

			if ((bool)value) return parameter;
			return null;
		}

		#endregion
	}

    [ValueConversion(typeof(object), typeof(bool))]
    public class ExistsConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is string)) return value != null;
            return !string.IsNullOrWhiteSpace((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
