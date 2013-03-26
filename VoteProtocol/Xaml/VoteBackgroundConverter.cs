using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

using Ragnarok;
using Ragnarok.Presentation;

namespace VoteSystem.Protocol.Xaml
{
    using Vote;

    /// <summary>
    /// 投票状態をブラシに変換します。
    /// </summary>
    public sealed class VoteBackgroundConverter : IMultiValueConverter
    {
        /// <summary>
        /// 背景の色を返します。
        /// </summary>
        private static Color GetColor(VoteState state, TimeSpan leaveTime)
        {
            switch (state)
            {
                case VoteState.Voting:
                    if (leaveTime < TimeSpan.FromSeconds(30))
                    {
                        return Color.FromArgb(160, 230, 0, 0);
                    }
                    else if (leaveTime < TimeSpan.FromSeconds(120))
                    {
                        return WPFUtil.MakeColor(180, Colors.DarkOrange);
                    }
                    else
                    {
                        return WPFUtil.MakeColor(128, Colors.DarkGray);
                    }
                case VoteState.End:
                    //return WPFUtil.MakeColor(160, Colors.DarkViolet);
                    return Colors.Transparent;
                case VoteState.Pause:
                    return WPFUtil.MakeColor(127, Colors.Goldenrod);
                case VoteState.Stop:
                    return Colors.Transparent;
            }

            return Colors.Transparent;
        }

        /// <summary>
        /// 投票状態を表すブラシに変換します。
        /// </summary>
        public object Convert(object[] value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            try
            {
                var state = (VoteState)value[0];
                var leaveTime = (TimeSpan)value[1];

                var color = GetColor(state, leaveTime);
                var brush = new SolidColorBrush(color);
                brush.Freeze();

                return brush;
            }
            catch (Exception ex)
            {
                Util.ThrowIfFatal(ex);

                Log.ErrorException(ex,
                    "投票状態の背景変換に失敗しました。");
            }

            return null;
        }

        /// <summary>
        /// 実装されていません。
        /// </summary>
        public object[] ConvertBack(object value, Type[] targetType,
                                    object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
