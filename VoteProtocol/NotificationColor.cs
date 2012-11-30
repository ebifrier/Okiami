using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 通知されるメッセージの色です。
    /// </summary>
    /// <remarks>
    /// 投稿主の強さなどによってメッセージの色を変えることがあります。
    /// また、放送から放送にコメントを転送するときにコメントの色を
    /// そのままにするために使われます。
    /// </remarks>
    [DataContract()]
    public enum NotificationColor
    {
        /// <summary>
        /// デフォルト色
        /// </summary>
        Default,
        /// <summary>
        /// 黒
        /// </summary>
        Black,
        /// <summary>
        /// 白
        /// </summary>
        White,
        /// <summary>
        /// 赤
        /// </summary>
        Red,
        /// <summary>
        /// ピンク
        /// </summary>
        Pink,
        /// <summary>
        /// オレンジ
        /// </summary>
        Orange,
        /// <summary>
        /// 黄
        /// </summary>
        Yellow,
        /// <summary>
        /// 緑
        /// </summary>
        Green,
        /// <summary>
        /// シアン
        /// </summary>
        Cyan,
        /// <summary>
        /// 青
        /// </summary>
        Blue,
        /// <summary>
        /// 紫
        /// </summary>
        Purple,
    }
}
