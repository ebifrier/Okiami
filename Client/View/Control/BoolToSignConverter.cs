using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

namespace VoteSystem.Client.View.Control
{
    /// <summary>
    /// 真偽値を○×に変換します。
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class BoolToSignConverter : IValueConverter
    {
        /// <summary>
        /// 真偽値を○×に変換します。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter,
                              CultureInfo culture)
        {
            var flag = (bool)value;

            return (flag ? "○" : "×");
        }

        /// <summary>
        /// 
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter,
                                  CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
