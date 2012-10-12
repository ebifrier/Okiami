using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Utility;
using Ragnarok.ObjectModel;

namespace VoteSystem.Server
{
    using Protocol;
    using Protocol.Vote;

    /// <summary>
    /// サーバーの全体に影響するオブジェクトなどを保持します。
    /// </summary>
    public class GlobalControl
    {
        private static readonly GlobalControl instance = new GlobalControl();

        private readonly List<VoteRoom> voteRoomList = new List<VoteRoom>();

        /// <summary>
        /// クラス唯一のインスタンスを取得します。
        /// </summary>
        public static GlobalControl Instance
        {
            get
            {
                return instance;
            }
        }

        #region 投票ルーム
        /// <summary>
        /// 将棋ルームの数を取得します。
        /// </summary>
        public int VoteRoomCount
        {
            get
            {
                lock (this.voteRoomList)
                {
                    return this.voteRoomList.Count;
                }
            }
        }

        /// <summary>
        /// 全投票ルームを取得します。
        /// </summary>
        /// <remarks>
        /// 投票ルームは番号=配列インデックスとなっているため、
        /// nullのことがあります。
        /// </remarks>
        public VoteRoom[] VoteRoomList
        {
            get
            {
                lock (this.voteRoomList)
                {
                    return this.voteRoomList.ToArray();
                }
            }
        }

        /// <summary>
        /// 投票ルーム情報を取得します。
        /// </summary>
        public VoteRoomInfo[] VoteRoomInfoList
        {
            get
            {
                lock (this.voteRoomList)
                {
                    return this.voteRoomList
                        .Select(room => (room != null ? room.Info : null))
                        .ToArray();
                }
            }
        }
        
        /// <summary>
        /// 投票ルームのＩＤを取得します。
        /// </summary>
        private int GetNextVoteRoomId()
        {
            lock (this.voteRoomList)
            {
                var index = this.voteRoomList.FindIndex(
                    room => room == null);

                if (index < 0)
                {
                    return this.voteRoomList.Count;
                }
                else
                {
                    return index;
                }
            }
        }

        /// <summary>
        /// 新しい投票ルームを作成します。
        /// </summary>
        public VoteRoom CreateVoteRoom(VoteParticipant voteRoomOwner,
                                       string roomName, string password)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                return null;
            }

            var room = new VoteRoom(
                voteRoomOwner,
                GetNextVoteRoomId(),
                roomName, password);

            lock (this.voteRoomList)
            {
                if (room.Id < this.voteRoomList.Count)
                {
                    this.voteRoomList[room.Id] = room;
                }
                else
                {
                    this.voteRoomList.Insert(room.Id, room);
                }

                return room;
            }
        }

        /// <summary>
        /// 投票ルームを削除します。
        /// </summary>
        public void RemoveVoteRoom(int roomId)
        {
            lock (this.voteRoomList)
            {
                if (roomId < 0 || this.voteRoomList.Count <= roomId)
                {
                    return;
                }

                this.voteRoomList[roomId] = null;
            }
        }

        /// <summary>
        /// 指定のＩＤを持つ投票ルームを探します。
        /// </summary>
        public VoteRoom FindVoteRoom(int roomId)
        {
            lock (this.voteRoomList)
            {
                if (roomId < 0 || this.voteRoomList.Count <= roomId)
                {
                    return null;
                }

                return this.voteRoomList[roomId];
            }
        }
        #endregion

        /// <summary>
        /// 秘密のコンストラクタ
        /// </summary>
        private GlobalControl()
        {
        }
    }
}
