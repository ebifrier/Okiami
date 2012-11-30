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
    public class SystemMessage : IModel
    {
        private bool isPostComment = true;
        //private bool isPostOwnerComment = true;
        private string commentName = "";
        private string commentText = "";
        
        /// <summary>
        /// プロパティ値の変更を通知するイベントです。
        /// </summary>
        [field:NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ値の変更を通知します。
        /// </summary>
        void IModel.NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                Global.UIProcess(() =>
                    handler(this, e));
            }
        }

        /// <summary>
        /// メッセージ通知時にコメントを投稿するかどうかを取得または設定します。
        /// </summary>
        public bool IsPostComment
        {
            get
            {
                return this.isPostComment;
            }
            set
            {
                if (this.isPostComment != value)
                {
                    this.isPostComment = value;

                    this.RaisePropertyChanged("IsPostComment");
                }
            }
        }

        /*/// <summary>
        /// メッセージ通知を
        /// </summary>
        public bool IsPostOwnerComment
        {
            get
            {
                return this.isPostOwnerComment;
            }
            set
            {
                if (this.isPostOwnerComment != value)
                {
                    this.isPostOwnerComment = value;

                    this.RaisePropertyChanged("IsPostOwnerComment");
                }
            }
        }*/

        /// <summary>
        /// メッセージ通知に使用するコメント投稿者の名前を取得または設定します。
        /// </summary>
        public string CommentName
        {
            get
            {
                return this.commentName;
            }
            set
            {
                if (this.commentName != value)
                {
                    this.commentName = value;

                    this.RaisePropertyChanged("CommentName");
                }
            }
        }

        /// <summary>
        /// メッセージ通知に使用するコメント内容を取得または設定します。
        /// </summary>
        public string CommentText
        {
            get
            {
                return this.commentText;
            }
            set
            {
                if (this.commentText != value)
                {
                    this.commentText = value;

                    this.RaisePropertyChanged("CommentText");
                }
            }
        }
    }
}
