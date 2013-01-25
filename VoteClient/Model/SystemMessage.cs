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
    public sealed class SystemMessage : NotifyObject
    {
        /// <summary>
        /// メッセージ通知時にコメントを投稿するかどうかを取得または設定します。
        /// </summary>
        public bool IsPostComment
        {
            get { return GetValue<bool>("IsPostComment"); }
            set { SetValue("IsPostComment", value); }
        }

        /*/// <summary>
        /// メッセージ通知を
        /// </summary>
        public bool IsPostOwnerComment
        {
            get { return GetValue<bool>("IsPostOwnerComment"); }
            set { SetValue("IsPostOwnerComment", value); }
        }*/

        /// <summary>
        /// メッセージ通知に使用するコメント投稿者の名前を取得または設定します。
        /// </summary>
        public string CommentName
        {
            get { return GetValue<string>("CommentName"); }
            set { SetValue("CommentName", value); }
        }

        /// <summary>
        /// メッセージ通知に使用するコメント内容を取得または設定します。
        /// </summary>
        public string CommentText
        {
            get { return GetValue<string>("CommentText"); }
            set { SetValue("CommentText", value); }
        }
    }
}
