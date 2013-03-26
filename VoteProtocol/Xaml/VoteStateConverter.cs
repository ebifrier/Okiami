using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Globalization;

using Ragnarok;

namespace VoteSystem.Protocol.Xaml
{
    using Vote;

    /// <summary>
    /// 投票状態を文字列に変換します。
    /// </summary>
    [ValueConversion(typeof(VoteState), typeof(string))]
    public sealed class VoteStateConverter : IValueConverter
    {
        /// <summary>
        /// 投票状態を文字列に変換します。
        /// </summary>
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                var state = (VoteState)value;

                return ProtocolUtil.GetVoteStateText(state);
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                Log.ErrorException(ex,
                    "投票状態の変換に失敗しました。");
            }

            return null;
        }

        /// <summary>
        /// 実装されていません。
        /// </summary>
        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
