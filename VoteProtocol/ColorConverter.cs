using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok.NicoNico;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 投票サーバーとニコニコの色を相互変換します。
    /// </summary>
    public static class ColorConverter
    {
        /// <summary>
        /// 投票サーバーから得られたメッセージ色をニコニコの
        /// コメント色に変換します。
        /// </summary>
        public static CommentColor ToNicoColor(NotificationColor color,
                                               bool isPremium)
        {
            return (
                isPremium ?
                ToPremiumNicoColor(color) :
                ToNormalNicoColor(color));
        }

        /// <summary>
        /// 投票サーバーの色をニコニコのコメントカラーとして使える文字列に変換します。
        /// </summary>
        public static string ToNicoColorString(NotificationColor msgColor,
                                               bool isPremium)
        {
            var nicoColor = ToNicoColor(msgColor, isPremium);

            return CommentStringizer.GetColorString(nicoColor);
        }

        /// <summary>
        /// ブロードキャストメッセージの色を一般会員用の色に変換します。
        /// </summary>
        public static CommentColor ToNormalNicoColor(NotificationColor color)
        {
            switch (color)
            {
                case NotificationColor.Default:
                    return CommentColor.Default;
                case NotificationColor.Black:
                    return CommentColor.Black;
                case NotificationColor.White:
                    return CommentColor.White;
                case NotificationColor.Red:
                    return CommentColor.Red;
                case NotificationColor.Pink:
                    return CommentColor.Pink;
                case NotificationColor.Orange:
                    return CommentColor.Orange;
                case NotificationColor.Yellow:
                    return CommentColor.Yellow;
                case NotificationColor.Green:
                    return CommentColor.Green;
                case NotificationColor.Cyan:
                    return CommentColor.Cyan;
                case NotificationColor.Blue:
                    return CommentColor.Blue;
                case NotificationColor.Purple:
                    return CommentColor.Purple;
            }

            return CommentColor.Default;
        }

        /// <summary>
        /// ブロードキャストメッセージの色をプレミアム会員用の色に変換します。
        /// </summary>
        public static CommentColor ToPremiumNicoColor(NotificationColor color)
        {
            switch (color)
            {
                case NotificationColor.Default:
                    return CommentColor.Default;
                case NotificationColor.Black:
                    return CommentColor.Black;
                case NotificationColor.White:
                    return CommentColor.White2;
                case NotificationColor.Red:
                    return CommentColor.Red2;
                case NotificationColor.Pink:
                    return CommentColor.Pink;
                case NotificationColor.Orange:
                    return CommentColor.Orange2;
                case NotificationColor.Yellow:
                    return CommentColor.Yellow2;
                case NotificationColor.Green:
                    return CommentColor.Green2;
                case NotificationColor.Cyan:
                    return CommentColor.Cyan;
                case NotificationColor.Blue:
                    return CommentColor.Blue;
                case NotificationColor.Purple:
                    return CommentColor.Purple2;
            }

            return CommentColor.Default;
        }

        /// <summary>
        /// ニコ生用の色をブロードキャストメッセージの色に変換します。
        /// </summary>
        public static NotificationColor ToMessageColor(CommentColor color)
        {
            switch (color)
            {
                case CommentColor.Default:
                    return NotificationColor.Default;
                case CommentColor.White:
                case CommentColor.White2:
                    return NotificationColor.White;
                case CommentColor.Black:
                    return NotificationColor.Black;
                case CommentColor.Red:
                case CommentColor.Red2:
                    return NotificationColor.Red;
                case CommentColor.Pink:
                    return NotificationColor.Pink;
                case CommentColor.Orange:
                case CommentColor.Orange2:
                    return NotificationColor.Orange;
                case CommentColor.Yellow:
                case CommentColor.Yellow2:
                    return NotificationColor.Yellow;
                case CommentColor.Green:
                case CommentColor.Green2:
                    return NotificationColor.Green;
                case CommentColor.Cyan:
                    return NotificationColor.Cyan;
                case CommentColor.Blue:
                    return NotificationColor.Blue;
                case CommentColor.Purple:
                case CommentColor.Purple2:
                    return NotificationColor.Purple;
            }

            return NotificationColor.Default;
        }
    }
}
