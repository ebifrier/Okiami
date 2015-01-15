using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

using Ragnarok;
using Ragnarok.Utility;

namespace VoteSystem.Protocol.View
{
    /// <summary>
    /// 整数型のポイントを文字列に変換します。
    /// </summary>
    public class PointToStringConverter : IMultiValueConverter
    {
        /// <summary>
        /// 整数型のポイントを文字列に変換します。
        /// </summary>
        public object Convert(object[] values, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                var ivalue = (int)values[0];
                var isFullWidth = (bool)(values[1] is bool ? values[1] : false);

                // 半角/全角の指定つきです。
                return IntConverter.Convert(
                    (isFullWidth ? NumberType.Big : NumberType.Normal),
                    ivalue);
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex, "変換エラー");

                return "0";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetType,
                                    object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
