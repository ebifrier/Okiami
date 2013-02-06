using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.Model
{
    /// <summary>
    /// システムメッセージなどを音声付きで通知するためのオブジェクトです。
    /// </summary>
    [Serializable()]
    [DataContract()]
    public sealed class SystemMessage : NotifyObject
    {
        /// <summary>
        /// メッセージ通知時にコメントを投稿するかどうかを取得または設定します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public bool IsPostComment
        {
            get { return GetValue<bool>("IsPostComment"); }
            set { SetValue("IsPostComment", value); }
        }

        /*/// <summary>
        /// メッセージ通知を
        /// </summary>
        [DataMember(Order = 10, IsRequired = true)]
        public bool IsPostOwnerComment
        {
            get { return GetValue<bool>("IsPostOwnerComment"); }
            set { SetValue("IsPostOwnerComment", value); }
        }*/

        /// <summary>
        /// メッセージ通知に使用するコメント投稿者の名前を取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string CommentName
        {
            get { return GetValue<string>("CommentName"); }
            set { SetValue("CommentName", value); }
        }

        /// <summary>
        /// メッセージ通知に使用するコメント内容を取得または設定します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public string CommentText
        {
            get { return GetValue<string>("CommentText"); }
            set { SetValue("CommentText", value); }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SystemMessage()
        {
            IsPostComment = true;
        }
    }
}
