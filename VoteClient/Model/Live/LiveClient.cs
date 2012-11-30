using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.ObjectModel;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Client.Model.Live
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// クライアント側の放送情報を管理する基底クラスです。
    /// </summary>
    /// <remarks>
    /// ニコニコやUstreamなどの生放送への接続や切断、
    /// またはその管理などを行います。。
    /// </remarks>
    public abstract class LiveClient : ILazyModel, IParentModel
    {
        private readonly object SyncRoot = new object();
        private readonly LazyModelObject lazyModelObject = new LazyModelObject();
        private readonly List<object> dependModelList = new List<object>();
        private readonly MainModel participant;
        private readonly LiveSite liveSite;
        private readonly string liveSiteTitle;
        private LiveData liveData;
        private LiveAttribute attribute;
        //private LiveAttribute oldLiveAttribute = null;
        private string liveUrlText = "";

        /// <summary>
        /// 放送に接続します。
        /// </summary>
        public abstract void ConnectCommand();

        /// <summary>
        /// 放送から切断します。
        /// </summary>
        public abstract void DisconnectCommand();

        /// <summary>
        /// 受信した通知を各放送オブジェクトで処理します。
        /// </summary>
        public abstract void HandleNotification(Notification notification);

        /// <summary>
        /// プロパティ値の変更通知イベントです。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// プロパティ値変更を通知します。
        /// </summary>
        public void NotifyPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = this.PropertyChanged;

            if (handler != null)
            {
                Global.UIProcess(() =>
                    Util.SafeCall(() =>
                       handler(this, e)));
            }
        }

        /// <summary>
        /// 依存モデルのリストを取得します。
        /// </summary>
        List<object> IParentModel.DependModelList
        {
            get
            {
                return this.dependModelList;
            }
        }

        /// <summary>
        /// イベント発火の遅延用オブジェクトを取得します。
        /// </summary>
        LazyModelObject ILazyModel.LazyModelObject
        {
            get
            {
                return this.lazyModelObject;
            }
        }

        /// <summary>
        /// 投票参加者オブジェクトを取得します。
        /// </summary>
        public MainModel Participant
        {
            get
            {
                return this.participant;
            }
        }

        /// <summary>
        /// 投票用オブジェクトを取得します。
        /// </summary>
        public VoteClient VoteClient
        {
            get
            {
                return this.participant.VoteClient;
            }
        }

        /// <summary>
        /// 放送サイトの種類を取得します。
        /// </summary>
        public LiveSite LiveSite
        {
            get
            {
                return this.liveSite;
            }
        }

        /// <summary>
        /// 生放送サイトの名前を取得します。
        /// </summary>
        public string LiveSiteTitle
        {
            get
            {
                return this.liveSiteTitle;
            }
        }

        /// <summary>
        /// 扱っている放送のIDを取得します。
        /// </summary>
        public LiveData LiveData
        {
            get
            {
                return this.liveData;
            }
            protected set
            {
                using (new LazyModelLock(this, SyncRoot))
                {
                    if (this.liveData != value)
                    {
                        this.liveData = value;

                        this.RaisePropertyChanged("LiveData");
                    }
                }
            }
        }

        /// <summary>
        /// 放送タイトルを取得します。
        /// </summary>
        [DependOnProperty("LiveData")]
        public string LiveTitle
        {
            get
            {
                if (this.liveData == null)
                {
                    return "";
                }

                return this.liveData.LiveTitle;
            }
        }

        /// <summary>
        /// ユーザーから入力された放送URLを取得または設定します。
        /// </summary>
        public string LiveUrlText
        {
            get
            {
                return this.liveUrlText;
            }
            set
            {
                using (new LazyModelLock(this, SyncRoot))
                {
                    this.liveUrlText = value;

                    this.RaisePropertyChanged("LiveUrlText");
                }
            }
        }

        /// <summary>
        /// 放送の属性値を取得します。
        /// </summary>
        public LiveAttribute Attribute
        {
            get
            {
                return this.attribute;
            }
            private set
            {
                using (new LazyModelLock(this, SyncRoot))
                {
                    if (value != null)
                    {
                        this.RemoveDependModel(this.attribute);
                        this.attribute = value;
                        this.AddDependModel(this.attribute);

                        this.RaisePropertyChanged("Attribute");
                    }
                }
            }
        }

        /// <summary>
        /// システムメッセージを取得します。
        /// </summary>
        public static string GetVoteSystemMessage(SystemNotificationType type)
        {
            SystemMessage soundMessage = null;

            switch (type)
            {
                case SystemNotificationType.VoteStart:
                    soundMessage = Global.Settings.VoteStartSystemMessage;
                    break;
                case SystemNotificationType.VoteEnd:
                    soundMessage = Global.Settings.VoteEndSystemMessage;
                    break;
                case SystemNotificationType.VotePause:
                    soundMessage = Global.Settings.VotePauseSystemMessage;
                    break;
                case SystemNotificationType.VoteStop:
                    soundMessage = Global.Settings.VoteStopSystemMessage;
                    break;
                case SystemNotificationType.ChangeVoteSpan:
                    soundMessage = Global.Settings.ChangeVoteSpanSystemMessage;
                    break;
            }

            if (soundMessage == null || !soundMessage.IsPostComment)
            {
                return null;
            }

            return soundMessage.CommentText;
        }

        /// <summary>
        /// 放送への接続時に呼ばれます。
        /// </summary>
        protected void LiveConnected(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                throw new InvalidOperationException(
                    "放送IDが正しくありません。");
            }

            using (new LazyModelLock(this, SyncRoot))
            {
                this.LiveData = liveData;

                if (!VoteClient.IsConnected)
                {
                    return;
                }

                // 投票クライアントに放送のIDを設定します。
                VoteClient.OperateLive(
                    LiveOperation.Add,
                    liveData,
                    Attribute,
                    LiveConnectedCallback);
            }
        }

        /// <summary>
        /// 放送ルームの追加処理が終わったときに呼び出されます。
        /// </summary>
        private void LiveConnectedCallback(
            object sender,
            PbResponseEventArgs<LiveOperationResponse> e)
        {
            if (e.ErrorCode != ErrorCode.None)
            {
                // 同一IDがあった場合のエラーは無視します。
                if (e.ErrorCode == ErrorCode.LiveAlreadyExists)
                {
                    return;
                }

                MessageUtil.ErrorMessage(
                    e.ErrorCode,
                    "放送ルームの作成に失敗しました (*_ _)人");                
                return;
            }
        }

        /// <summary>
        /// 放送からの切断時に呼ばれます。
        /// </summary>
        protected void LiveDisconnected()
        {
            using (new LazyModelLock(this, SyncRoot))
            {
                if (this.LiveData == null)
                {
                    return;
                }

                // 放送IDはここでクリアするため一時変数に保存します。
                var liveData = this.LiveData;
                this.LiveData = null;

                if (!VoteClient.IsConnected)
                {
                    return;
                }

                try
                {
                    // 投票クライアントに放送のIDを設定します。
                    VoteClient.OperateLive(
                        LiveOperation.Remove,
                        liveData,
                        null,
                        LiveDisconnectedCallback);
                }
                catch
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 放送ルームの削除処理が終わったときに呼び出されます。
        /// </summary>
        private void LiveDisconnectedCallback(
            object sender,
            PbResponseEventArgs<LiveOperationResponse> e)
        {
            if (e.ErrorCode != ErrorCode.None)
            {
                //MessageUtil.ErrorMessage(
                //    "放送ルームの削除に失敗しました (*_ _)人");
                return;
            }
        }

        /// <summary>
        /// 放送の各属性を更新します。
        /// </summary>
        protected void LiveAttributeChanged()
        {
            if (!VoteClient.IsConnected)
            {
                return;
            }

            if (this.LiveData == null)
            {
                return;
            }

            using (new LazyModelLock(this, SyncRoot))
            {
                // 放送属性を設定します。
                VoteClient.OperateLive(
                    LiveOperation.SetAttribute,
                    LiveData,
                    Attribute,
                    LiveAttributeChangedCallback);
            }
        }

        /// <summary>
        /// 放送ルームの属性設定処理が終わったときに呼び出されます。
        /// </summary>
        private void LiveAttributeChangedCallback(
            object sender,
            PbResponseEventArgs<LiveOperationResponse> e)
        {
            if (e.ErrorCode != ErrorCode.None)
            {
                MessageUtil.ErrorMessage(
                    e.ErrorCode,
                    "放送のプロパティ変更に失敗しました (*_ _)人");
                return;
            }
        }

        /// <summary>
        /// 放送の状態やプロパティなどをサーバーに伝えます。
        /// </summary>
        public void VoteLogined()
        {
            if (this.LiveData == null)
            {
                return;
            }

            LiveConnected(this.LiveData);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        protected LiveClient(MainModel participant, LiveSite liveSite)
        {
            this.participant = participant;
            this.liveSite = liveSite;
            this.liveSiteTitle = EnumEx.GetEnumLabel(liveSite);
            this.Attribute = new LiveAttribute();

            this.PropertyChanged +=
                (sender, e) => LiveAttributeChanged();
            this.Attribute.PropertyChanged +=
                (sender, e) => LiveAttributeChanged();
        }
    }
}
