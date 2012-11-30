using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VoteSystem.Protocol
{
    /// <summary>
    /// サーバー/クライアント双方に送られる通知です。
    /// </summary>
    [Serializable()]
    [DataContract()]
    public class Notification
    {
        /// <summary>
        /// メッセージタイプを取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public NotificationType Type
        {
            get;
            set;
        }

        /// <summary>
        /// システムメッセージの種類を取得または設定します。
        /// </summary>
        /// <remarks>
        /// <see cref="Type"/>が<see cref="NotificationType.System"/>
        /// でない場合は、使われません。
        /// </remarks>
        [DataMember(Order = 2, IsRequired = true)]
        public SystemNotificationType SystemType
        {
            get;
            set;
        }

        /// <summary>
        /// メッセージ色を取得または設定します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public NotificationColor Color
        {
            get;
            set;
        }

        /// <summary>
        /// メッセージテキストを取得または設定します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// 通知の投稿元の放送を取得または設定します。
        /// </summary>
        [DataMember(Order = 5, IsRequired = true)]
        public LiveRoomData FromLiveRoom
        {
            get;
            set;
        }

        /// <summary>
        /// 投稿者のＩＤを取得または設定します。
        /// </summary>
        [DataMember(Order = 6, IsRequired = true)]
        public string VoterId
        {
            get;
            set;
        }

        /// <summary>
        /// 投稿者の名前を取得または設定します。
        /// </summary>
        [DataMember(Order = 8, IsRequired = false)]
        public string VoterName
        {
            get;
            set;
        }

        /// <summary>
        /// メッセージが投稿された時刻を取得または設定します。
        /// </summary>
        [DataMember(Order = 7, IsRequired = true)]
        public DateTime Timestamp
        {
            get;
            set;
        }

        /// <summary>
        /// オブジェクトの簡易コピーを作成します。
        /// </summary>
        public Notification Clone()
        {
            return (Notification)MemberwiseClone();
        }

        /// <summary>
        /// オブジェクトの各プロパティが正しく設定されているか調べます。
        /// </summary>
        public bool Validate()
        {
            if (!Enum.IsDefined(typeof(NotificationType), this.Type))
            {
                return false;
            }

            if (!Enum.IsDefined(typeof(NotificationColor), this.Color))
            {
                return false;
            }

            if (string.IsNullOrEmpty(this.Text))
            {
                return false;
            }

            // 通知は放送から来ないことがあるためnullでも許されますが、
            // もしnullでない場合は正しい放送ＩＤを持っていないといけません。
            if (this.FromLiveRoom != null)
            {
                if (!this.FromLiveRoom.Validate())
                {
                    return false;
                }
            }

            if (string.IsNullOrEmpty(this.VoterId))
            {
                return false;
            }

            if (this.Timestamp == DateTime.MinValue)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Notification()
        {
            this.Type = NotificationType.Unknown;
            this.SystemType = SystemNotificationType.Unknown;
            this.Color = NotificationColor.Default;
            this.Timestamp = DateTime.MinValue;
        }
    }
}
