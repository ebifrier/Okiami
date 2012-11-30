using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// メッセージ送信時に使われるイベントデータです。
    /// </summary>
    public class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// 送信用メッセージを取得します。
        /// </summary>
        public virtual Notification Notification
        {
            get;
            private set;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NotificationEventArgs(Notification notification)
        {
            this.Notification = notification;
        }
    }
}
