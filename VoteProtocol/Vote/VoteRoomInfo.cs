using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Protocol.Vote
{
    /// <summary>
    /// 将棋投票中の部屋情報です。
    /// </summary>
    [DataContract()]
    [Serializable()]
    public sealed class VoteRoomInfo : NotifyObject, IEquatable<VoteRoomInfo>
    {
        private NotifyCollection<VoteParticipantInfo> participantList =
            new NotifyCollection<VoteParticipantInfo>();

        /// <summary>
        /// 部屋IDを取得します。
        /// </summary>
        [DataMember(Order = 1, IsRequired = true)]
        public int Id
        {
            get { return GetValue<int>("Id"); }
            set { SetValue("Id", value); }
        }

        /// <summary>
        /// 名前を取得または設定します。
        /// </summary>
        [DataMember(Order = 2, IsRequired = true)]
        public string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }
        }

        /// <summary>
        /// 投票ルームのオーナーの番号を取得します。
        /// </summary>
        [DataMember(Order = 3, IsRequired = true)]
        public int OwnerNo
        {
            get { return GetValue<int>("OwnerNo"); }
            set { SetValue("OwnerNo", value); }
        }

        /// <summary>
        /// パスワードを使用するかどうかを取得または設定します。
        /// </summary>
        [DataMember(Order = 4, IsRequired = true)]
        public bool HasPassword
        {
            get { return GetValue<bool>("HasPassword"); }
            set { SetValue("HasPassword", value); }
        }

        /// <summary>
        /// パスワードを取得または設定します。
        /// </summary>
        [DataMember(Order = 5, IsRequired = false)]
        public string Password
        {
            get { return GetValue<string>("Password"); }
            set { SetValue("Password", value); }
        }

        /// <summary>
        /// 投票状態を取得または設定します。
        /// </summary>
        [DataMember(Order = 6, IsRequired = true)]
        public VoteState State
        {
            get { return GetValue<VoteState>("State"); }
            set { SetValue("State", value); }
        }

        /// <summary>
        /// 投票モードを取得または設定します。
        /// </summary>
        [DataMember(Order = 7, IsRequired = true)]
        public VoteMode Mode
        {
            get { return GetValue<VoteMode>("Mode"); }
            set { SetValue("Mode", value); }
        }

        /// <summary>
        /// 全コメントをミラーするモードかどうかを取得または設定します。
        /// </summary>
        [DataMember(Order = 13, IsRequired = true)]
        public bool IsMirrorMode
        {
            get { return GetValue<bool>("IsMirrorMode"); }
            set { SetValue("IsMirrorMode", value); }
        }

        /// <summary>
        /// <see cref="VoteSpan"/>の基準時刻を取得または設定です。
        /// </summary>
        /// <remarks>
        /// 一時停止が再開されると、<see cref="BaseTimeNtp"/>は
        /// その時刻に再設定されます。
        /// </remarks>
        [DataMember(Order = 8, IsRequired = true)]
        public DateTime BaseTimeNtp
        {
            get { return GetValue<DateTime>("BaseTimeNtp"); }
            set { SetValue("BaseTimeNtp", value); }
        }

        /// <summary>
        /// 投票が終了するまでの全残り時間を取得または設定します。
        /// 投票中の場合は投票開始時からの時間です。
        /// </summary>
        [DataMember(Order = 9, IsRequired = true)]
        public TimeSpan TotalVoteSpan
        {
            get { return GetValue<TimeSpan>("TotalVoteSpan"); }
            set { SetValue("TotalVoteSpan", value); }
        }

        /// <summary>
        /// 今の投票が終了するまでの時間間隔を取得または設定します。
        /// 投票中の場合は投票開始時からの時間です。
        /// </summary>
        [DataMember(Order = 10, IsRequired = true)]
        public TimeSpan VoteSpan
        {
            get { return GetValue<TimeSpan>("VoteSpan"); }
            set { SetValue("VoteSpan", value); }
        }

        /// <summary>
        /// 一時停止される前までに経過した時刻を取得または設定します。
        /// </summary>
        /// <remarks>
        /// 一時停止されなければ、経過時間は０です。
        /// </remarks>
        [DataMember(Order = 14, IsRequired = true)]
        public TimeSpan ProgressSpan
        {
            get { return GetValue<TimeSpan>("ProgressSpan"); }
            set { SetValue("ProgressSpan", value); }
        }

        /// <summary>
        /// その部屋に接続している参加者の情報を取得または設定します。
        /// </summary>
        /// <remarks>
        /// Listのみ更新したい場合があるので、privateにしてはいけません。
        /// </remarks>
        [DataMember(Order = 11, IsRequired = true)]
        public NotifyCollection<VoteParticipantInfo> ParticipantList
        {
            get
            {
                return this.participantList;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                if (this.participantList != value)
                {
                    this.participantList = value;

                    this.RaisePropertyChanged("ParticipantList");
                }
            }
        }

        /// <summary>
        /// 部屋の作成された日時を取得します。
        /// </summary>
        [DataMember(Order = 12, IsRequired = true)]
        public DateTime CreateTime
        {
            get { return GetValue<DateTime>("CreateTime"); }
            set { SetValue("CreateTime", value); }
        }

        /// <summary>
        /// オブジェクトの各プロパティが正しく設定されているか調べます。
        /// </summary>
        public bool Validate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return false;
            }

            if (ParticipantList == null)
            {
                return false;
            }

            if (!ParticipantList.All(_ => _.Validate()))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 等値性を判断します。
        /// </summary>
        public override bool Equals(object obj)
        {
            var result = this.PreEquals(obj);
            if (result.HasValue)
            {
                return result.Value;
            }

            return Equals(obj as VoteRoomInfo);
        }

        /// <summary>
        /// 等値性を判断します。
        /// </summary>
        public bool Equals(VoteRoomInfo other)
        {
            if ((object)other == null)
            {
                return false;
            }

            return (this.Id == other.Id);
        }

        /// <summary>
        /// == 演算子を実装します。
        /// </summary>
        public static bool operator ==(VoteRoomInfo lhs, VoteRoomInfo rhs)
        {
            return Util.GenericEquals(lhs, rhs);
        }

        /// <summary>
        /// != 演算子を実装します。
        /// </summary>
        public static bool operator !=(VoteRoomInfo lhs, VoteRoomInfo rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// ハッシュコードを取得します。
        /// </summary>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        /// <summary>
        /// デシリアライズ後に呼ばれます。
        /// </summary>
        [OnDeserialized()]
        private void OnDeserialized(StreamingContext context)
        {
            // リストは要素数が０だとnullになることがあります。
            if (this.participantList == null)
            {
                this.participantList = new NotifyCollection<VoteParticipantInfo>();
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteRoomInfo()
        {
            Id = -1;
            Name = string.Empty;
            OwnerNo = -1;
            State = VoteState.Stop;
            Mode = VoteMode.Shogi;
            IsMirrorMode = false;
            CreateTime = DateTime.Now;
        }
    }
}
