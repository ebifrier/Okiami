using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// 将棋用の持ち時間などを保持します。
    /// </summary>
    [Serializable()]
    public sealed class SimpleTimeSpan : NotifyObject
    {
        /// <summary>
        /// 時間を使用するかどうかを取得または設定します。
        /// </summary>
        public bool IsUse
        {
            get { return GetValue<bool>("IsUse"); }
            set { SetValue("IsUse", value); }
        }

        /// <summary>
        /// 持ち時間の分を取得または設定します。
        /// </summary>
        public int Minutes
        {
            get { return GetValue<int>("Minutes"); }
            set { SetValue("Minutes", value); }
        }

        /// <summary>
        /// 持ち時間の秒を取得または設定します。
        /// </summary>
        public int Seconds
        {
            get { return GetValue<int>("Seconds"); }
            set { SetValue("Seconds", value); }
        }

        /// <summary>
        /// 時間間隔を取得します。
        /// </summary>
        public TimeSpan TimeSpan
        {
            get { return TimeSpan.FromSeconds(Minutes * 60 + Seconds); }
        }

        /// <summary>
        /// トータルの秒数を取得します。
        /// </summary>
        public int TotalSeconds
        {
            get { return (Minutes * 60 + Seconds); }
        }

        /// <summary>
        /// nullでない and IsUseが真 ならば真を返します。
        /// </summary>
        public static bool NotNullAndUse(SimpleTimeSpan value)
        {
            return (value != null && value.IsUse);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SimpleTimeSpan()
        {
            IsUse = true;
        }
    }
}
