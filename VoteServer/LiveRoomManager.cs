using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.ObjectModel;

namespace VoteSystem.Server
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 放送ルームを管理します。
    /// </summary>
    internal sealed class LiveRoomManager : NotifyObject
    {
        private readonly VoteParticipant liveOwner;
        private readonly Dictionary<LiveData, LiveRoom> liveRoomDic =
            new Dictionary<LiveData, LiveRoom>();
        
        /// <summary>
        /// 放送ルームがあるかどうかを取得します。
        /// </summary>
        public bool HasLiveRoom
        {
            get
            {
                using (LazyLock())
                {
                    return this.liveRoomDic.Any();
                }
            }
        }
        
        /// <summary>
        /// 放送主が所属する放送のURLを取得します。
        /// </summary>
        public LiveData[] LiveDataList
        {
            get
            {
                using (LazyLock())
                {
                    return this.liveRoomDic.Select(
                        pair => pair.Value.LiveData).ToArray();
                }
            }
        }
        
        /// <summary>
        /// 放送が変わったことを通知します。
        /// </summary>
        private void OnLiveChanged()
        {
            this.RaisePropertyChanged("HasLiveRoom");
            this.RaisePropertyChanged("LiveDataList");
        }
        
        /// <summary>
        /// 与えられた放送IDを持つ放送を返します。
        /// </summary>
        public LiveRoom GetLiveRoom(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                return null;
            }

            using (LazyLock())
            {
                LiveRoom liveRoom;
                if (!this.liveRoomDic.TryGetValue(liveData, out liveRoom))
                {
                    return null;
                }

                return liveRoom;
            }
        }

        /// <summary>
        /// 放送ルームを作成します。
        /// </summary>
        public void CreateLiveRoom(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                throw new ArgumentException(
                    "与えられた放送情報が正しくありません。", "liveData");
            }

            using (LazyLock())
            {
                // 同じ放送が登録されている場合は帰ります。
                if (GetLiveRoom(liveData) != null)
                {
                    return;
                }

                var liveRoom = new LiveRoom(this.liveOwner, liveData);
                if (liveRoom == null)
                {
                    throw new Exception("放送ルームの作成に失敗しました。");
                }

                // 放送リストに追加します。
                this.liveRoomDic.Add(liveData, liveRoom);
                OnLiveChanged();

                Log.Debug(
                    "放送ルームを作成しました。(Id = {0})",
                    liveRoom.LiveData);
            }
        }

        /// <summary>
        /// 放送ルームを削除します。
        /// </summary>
        public bool RemoveLiveRoom(LiveData liveData)
        {
            if (liveData == null || !liveData.Validate())
            {
                throw new ArgumentException(
                    "与えられた放送情報が正しくありません。", "liveData");
            }

            using (LazyLock())
            {
                var liveRoom = GetLiveRoom(liveData);
                if (liveRoom == null)
                {
                    return false;
                }

                // 放送を閉じます。
                this.liveRoomDic.Remove(liveData);
                liveRoom.Dispose();
                
                OnLiveChanged();

                Log.Debug(
                    "放送ルームを削除しました。(Id = {0})", liveData);
                return true;
            }
        }

        /// <summary>
        /// 放送ルームをすべて削除します。
        /// </summary>
        public void ClearLiveRoom()
        {
            using (LazyLock())
            {
                foreach (var pair in this.liveRoomDic)
                {
                    pair.Value.Dispose();
                }

                this.liveRoomDic.Clear();
                OnLiveChanged();
            }
        }

        #region コマンド処理
        /// <summary>
        /// 放送関連の操作を行います。
        /// </summary>
        internal void HandleLiveOperationRequest(
            object sender,
            PbRequestEventArgs<LiveOperationRequest,
                               LiveOperationResponse> e)
        {
            var liveData = e.Request.LiveData;
            if (liveData == null || !liveData.Validate())
            {
                Log.Error(
                    "放送IDが正しくありません。");

                e.ErrorCode = ErrorCode.Argument;
                return;
            }

            switch (e.Request.Operation)
            {
                case LiveOperation.LiveAdd:
                    AddLiveRequest(e);
                    break;
                case LiveOperation.LiveRemove:
                    RemoveLiveRequest(e);
                    break;
                case LiveOperation.LiveSetAttribute:
                    SetLiveAttributeRequest(e);
                    break;
                case LiveOperation.LiveGetAttribute:
                    GetLiveAttributeRequest(e);
                    break;
                default:
                    e.ErrorCode = ErrorCode.InvalidLiveOperation;

                    // 操作IDが正しくありません。
                    Log.Error(
                        "操作IDが正しくありません。");
                    break;
            }
        }

        /// <summary>
        /// 放送を新たに追加します。
        /// </summary>
        private void AddLiveRequest(
            PbRequestEventArgs<LiveOperationRequest,
                               LiveOperationResponse> e)
        {
            var liveData = e.Request.LiveData;

            using (LazyLock())
            {
                // すでに同じ放送があるか調べます。
                if (GetLiveRoom(liveData) != null)
                {
                    e.ErrorCode = ErrorCode.LiveAlreadyExists;
                    return;
                }

                // 指定の放送ルームを新たに作成します。
                CreateLiveRoom(liveData);

                // 放送主として登録します。
                this.liveOwner.AddLiveOwnerVoter();

                if (e.Request.Attribute == null)
                {
                    e.Response = new LiveOperationResponse
                    {
                        Operation = LiveOperation.LiveAdd,
                        LiveData = liveData,
                        Attribute = null,
                    };
                }
                else
                {
                    // もし属性があれば、それも一緒に設定します。
                    SetLiveAttributeRequest(e);

                    // もし属性の設定に失敗したら、放送の追加自体を
                    // なかったことにします。
                    if (e.ErrorCode != ErrorCode.None)
                    {
                        RemoveLiveRoom(liveData);
                    }
                    else
                    {
                        e.Response = new LiveOperationResponse
                        {
                            Operation = LiveOperation.LiveAdd,
                            LiveData = e.Response.LiveData,
                            Attribute = e.Response.Attribute,
                        };
                    }
                }
            }
        }

        /// <summary>
        /// 放送を削除します。
        /// </summary>
        private void RemoveLiveRequest(
            PbRequestEventArgs<LiveOperationRequest,
                               LiveOperationResponse> e)
        {
            var liveData = e.Request.LiveData;

            using (LazyLock())
            {
                // 放送ルームを削除します。
                if (!RemoveLiveRoom(liveData))
                {
                    e.ErrorCode = ErrorCode.LiveNotExists;
                    return;
                }

                e.Response = new LiveOperationResponse
                {
                    Operation = LiveOperation.LiveRemove,
                    LiveData = liveData,
                    Attribute = null,
                };
            }
        }

        /// <summary>
        /// 放送関連の属性を設定します。
        /// </summary>
        private void SetLiveAttributeRequest(
            PbRequestEventArgs<LiveOperationRequest,
                               LiveOperationResponse> e)
        {
            var attribute = e.Request.Attribute;
            if (attribute == null)
            {
                Log.Error(
                    "放送設定の属性値がnullです。");

                e.ErrorCode = ErrorCode.Argument;
                return;
            }

            // 指定のIDの放送がなければエラー
            var liveData = e.Request.LiveData;
            var live = GetLiveRoom(liveData);
            if (live == null)
            {
                Log.Error(
                    "放送[{0}]が見つかりません。", liveData);

                e.ErrorCode = ErrorCode.LiveNotExists;
                return;
            }

            // 属性を設定します。
            live.Attribute = attribute;

            // デバッグ用
            Log.Debug(
                "設定されたAttribute System={0} Confirm={1}, Mirror={2}",
                attribute.SystemCommentTypeMask,
                attribute.ConfirmCommentTypeMask,
                attribute.MirrorCommentTypeMask);

            e.Response = new LiveOperationResponse
            {
                Operation = LiveOperation.LiveSetAttribute,
                LiveData = liveData,
                Attribute = live.Attribute,
            };
        }

        /// <summary>
        /// 放送関連の属性を取得します。
        /// </summary>
        private void GetLiveAttributeRequest(
            PbRequestEventArgs<LiveOperationRequest,
                               LiveOperationResponse> e)
        {
            var liveData = e.Request.LiveData;

            // 指定のIDの放送がなければエラー
            var live = GetLiveRoom(liveData);
            if (live == null)
            {
                e.ErrorCode = ErrorCode.LiveNotExists;
                return;
            }

            e.Response = new LiveOperationResponse
            {
                Operation = LiveOperation.LiveGetAttribute,
                LiveData = liveData,
                Attribute = live.Attribute,
            };
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LiveRoomManager(VoteParticipant liveOwner)
        {
            this.liveOwner = liveOwner;
        }
    }
}
