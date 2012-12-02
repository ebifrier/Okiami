using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Net;
using Ragnarok.Net.ProtoBuf;
using Ragnarok.ObjectModel;

namespace VoteSystem.Server
{
    using VoteSystem.Protocol;
    using VoteSystem.Protocol.Vote;
    using VoteSystem.Server.VoteStrategy;

    /// <summary>
    /// 投票の時間管理をを行うオブジェクトです。
    /// </summary>
    /// <remarks>
    /// 投票の開始／停止、時間管理などを行います。
    /// </remarks>
    public class VoteTimeKeeper : NotifyObject, ILogObject
    {
        private readonly VoteRoom voteRoom;
        private DateTime votePauseTimeNtp = DateTime.MinValue;
        private readonly Timer timer;

        /// <summary>
        /// ログ出力用の名前を取得します。
        /// </summary>
        public string LogName
        {
            get { return "投票時間管理"; }
        }

        /// <summary>
        /// 投票状態を取得または設定します。
        /// </summary>
        public VoteState VoteState
        {
            get { return GetValue<VoteState>("VoteState"); }
            private set { SetValue("VoteState", value); }
        }

        /// <summary>
        /// Ntpサーバーから取得した投票開始時刻を取得します。
        /// </summary>
        public DateTime VoteStartTimeNtp
        {
            get { return GetValue<DateTime>("VoteStartTimeNtp"); }
            private set { SetValue("VoteStartTimeNtp", value); }
        }

        /// <summary>
        /// 投票期間を取得します。
        /// </summary>
        /// <remarks>
        /// 時間制限がない場合は、値がMaxValueになります。
        /// </remarks>
        public TimeSpan VoteSpan
        {
            get { return GetValue<TimeSpan>("VoteSpan"); }
            private set { SetValue("VoteSpan", value); }
        }

        /// <summary>
        /// 全投票期間を取得します。
        /// </summary>
        /// <remarks>
        /// 時間制限がない場合は、値がMaxValueになります。
        /// </remarks>
        public TimeSpan TotalVoteSpan
        {
            get { return GetValue<TimeSpan>("TotalVoteSpan"); }
            private set { SetValue("TotalVoteSpan", value); }
        }

        /// <summary>
        /// 投票時間が無制限かどうか取得します。
        /// </summary>
        [DependOnProperty("VoteSpan")]
        public bool IsVoteNoLimit
        {
            get
            {
                return (VoteSpan == TimeSpan.MaxValue);
            }
        }

        /// <summary>
        /// 全投票時間が無制限かどうか取得します。
        /// </summary>
        [DependOnProperty("TotalVoteSpan")]
        public bool IsTotalVoteNoLimit
        {
            get
            {
                return (TotalVoteSpan == TimeSpan.MaxValue);
            }
        }

        /// <summary>
        /// 投票の残り時間を取得します。
        /// </summary>
        [DependOnProperty("VoteState")]
        [DependOnProperty("VoteSpan")]
        public TimeSpan VoteLeaveTime
        {
            get
            {
                using (LazyLock())
                {
                    if (VoteSpan == TimeSpan.MaxValue)
                    {
                        return TimeSpan.MaxValue;
                    }

                    // 残り時間を計算します。
                    var baseTimeNtp = Ragnarok.Net.NtpClient.GetTime();
                    switch (VoteState)
                    {
                        case VoteState.Voting:
                            // 終了時刻から現在時刻を減算し、残り時間を出します。
                            var endTimeNtp = VoteStartTimeNtp + VoteSpan;
                            return (endTimeNtp - baseTimeNtp);
                        case VoteState.Pause:
                            return VoteSpan;
                        case VoteState.Stop:
                        case VoteState.End:
                            return TimeSpan.Zero;
                    }

                    return TimeSpan.Zero;
                }
            }
        }

        /// <summary>
        /// 投票を開始します。
        /// </summary>
        public void StartVote(TimeSpan voteSpan)
        {
            // 与えられた時刻が正しいか調べます。
            if (voteSpan <= TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    "投票時間が０以下になっています。");
            }

            /*if (!IsTotalVoteNoLimit && voteSpan == TimeSpan.MaxValue)
            {
                throw new InvalidOperationException(
                    "全投票時間が無制限ではありません。");
            }*/

            using (LazyLock())
            {
                switch (VoteState)
                {
                    case VoteState.Voting:
                        // 投票中ならなにもしません。
                        return;

                    case VoteState.Stop:
                    case VoteState.End:
                        // 即座に投票を開始します。
                        VoteStartTimeNtp = NtpClient.GetTime();
                        VoteSpan = MathEx.Max(
                            TimeSpan.Zero,
                            MathEx.Min(voteSpan, TotalVoteSpan));
                        VoteState = VoteState.Voting;
                        break;

                    case VoteState.Pause:
                        VoteStartTimeNtp = NtpClient.GetTime();
                        VoteState = VoteState.Voting;
                        break;
                }

                // 投票停止時間を検出するタイマを開始します。
                AdjustTimer();
            }

            this.voteRoom.Updated();
            this.voteRoom.BroadcastSystemNotification(
                SystemNotificationType.VoteStart);
        }

        /// <summary>
        /// 投票を一時停止します。
        /// </summary>
        public void PauseVote()
        {
            using (LazyLock())
            {
                if (VoteState != VoteState.Voting)
                {
                    Log.Error(this,
                        "投票中でないため、一時停止できませんでした。");
                    return;
                }

                this.votePauseTimeNtp = NtpClient.GetTime();
                this.VoteState = VoteState.Pause;

                // (一時停止時刻 - 開始時刻)だけ投票時間は減ります。
                var diff = this.votePauseTimeNtp - VoteStartTimeNtp;
                
                if (!IsVoteNoLimit)
                {
                    VoteSpan -= diff;
                }

                if (!IsTotalVoteNoLimit)
                {
                    TotalVoteSpan -= diff;
                }

                // 投票停止時間を検出するタイマを停止します。
                AdjustTimer();
            }

            this.voteRoom.Updated();
            this.voteRoom.BroadcastSystemNotification(
                SystemNotificationType.VotePause);
        }

        /// <summary>
        /// 投票終了/停止時に呼ばれます。
        /// </summary>
        private void VoteEnded(VoteState state, TimeSpan subTime)
        {
            using (LazyLock())
            {
                VoteSpan = TimeSpan.Zero;
                VoteState = state;

                // 経過時間だけ投票時間は減ります。
                if (!IsTotalVoteNoLimit && subTime != TimeSpan.MaxValue)
                {
                    TotalVoteSpan -= subTime;
                }

                // 投票終了時に延長要求もクリアします。
                this.voteRoom.VoteModel.ClearTimeExtendDemand();

                AdjustTimer();
            }
        }

        /// <summary>
        /// 投票を停止します。
        /// </summary>
        public void StopVote()
        {
            StopVote(TimeSpan.Zero);
        }

        /// <summary>
        /// 投票を停止します。
        /// </summary>
        public void StopVote(TimeSpan addTotalVoteTimeSpan)
        {
            using (LazyLock())
            {
                if (VoteState == VoteState.Stop)
                {
                    return;
                }

                // (経過時間 = 投票時間 - 残り時間)だけ全投票時間は減ります。
                var progressTime =
                    (VoteSpan != TimeSpan.MaxValue && VoteLeaveTime != TimeSpan.MaxValue
                    ? VoteSpan - VoteLeaveTime
                    : TimeSpan.Zero);

                VoteEnded(VoteState.Stop, progressTime);

                // 必要なら全投票時間を再設定します。
                AddTotalVoteSpanInternal(addTotalVoteTimeSpan);
            }

            this.voteRoom.Updated();
            this.voteRoom.BroadcastSystemNotification(
                SystemNotificationType.VoteStop);
        }

        /// <summary>
        /// 投票時間を追加します。(メッセージは出さない)
        /// </summary>
        /// <remarks>
        /// 条件）
        /// ・投票時間は持ち時間以上には設定できない。
        /// </remarks>
        private bool AddVoteSpanInternal(TimeSpan diff)
        {
            using (LazyLock())
            {
                var span = VoteSpan;

                // 持ち時間が有限なら、投票時間も有限にしかできません。
                if (TotalVoteSpan != TimeSpan.MaxValue)
                {
                    span = MathEx.Min(span, TotalVoteSpan);
                }

                // 投票時間が無限なら、時間の増減はできません。
                if (span == TimeSpan.MaxValue)
                {
                    return false;
                }

                switch (VoteState)
                {
                    case VoteState.Voting:
                    case VoteState.Pause:
                        // 残り時間が０以下なら０にします。
                        VoteSpan = MathEx.Max(
                            TimeSpan.Zero,
                            MathEx.Min(span + diff, TotalVoteSpan));
                        break;

                    case VoteState.Stop:
                    case VoteState.End:
                        return false;
                }

                // 投票停止時間を検出するタイマを開始します。
                AdjustTimer();
            }

            return true;
        }

        /// <summary>
        /// 全投票時間を延長(短縮)します。
        /// </summary>
        private bool AddTotalVoteSpanInternal(TimeSpan diff)
        {
            using (LazyLock())
            {
                if (IsTotalVoteNoLimit)
                {
                    return false;
                }

                TotalVoteSpan = MathEx.Max(TotalVoteSpan + diff, TimeSpan.Zero);
                
                // 持ち時間を変えた後、投票時間を再調整します。
                AddVoteSpanInternal(TimeSpan.Zero);

                return true;
            }
        }

        /// <summary>
        /// 投票時間を再設定します。
        /// </summary>
        public void SetVoteSpan(TimeSpan span)
        {
            // 投票時間は投票開始時間(ntp)と全体の長さを元に決定されます。
            // つまり、残り時間を与えられた時間に合わせるためには
            // 少し工夫して行う必要があります。
            // ここでは現在の残り時間と指定の時間を、全体の投票時間に加算する
            // ことでこの処理を行っています。
            var leaveTime = VoteLeaveTime; // 現在の残り時間

            if (AddVoteSpanInternal(span - leaveTime))
            {
                this.voteRoom.Updated();
                this.voteRoom.BroadcastSystemNotification(
                    SystemNotificationType.ChangeVoteSpan);
            }
        }

        /// <summary>
        /// 投票時間を延長(短縮)します。
        /// </summary>
        public void AddVoteSpan(TimeSpan diff)
        {
            if (AddVoteSpanInternal(diff))
            {
                this.voteRoom.Updated();
                this.voteRoom.BroadcastSystemNotification(
                    SystemNotificationType.ChangeVoteSpan);
            }
        }

        /// <summary>
        /// 全投票時間を再設定します。
        /// </summary>
        public void SetTotalVoteSpan(TimeSpan span)
        {
            using (LazyLock())
            {
                // 無制限投票中なら、全投票時間を有限時間にすることはできません。
                /*if (IsVoteNoLimit && span != TimeSpan.MaxValue)
                {
                    Log.Error(this,
                        "無制限投票中に全投票時間を有限にすることはできません。");

                    return;
                }*/

                TotalVoteSpan = MathEx.Max(span, TimeSpan.Zero);

                // 持ち時間を変えた後、投票時間を再調整します。
                AddVoteSpanInternal(TimeSpan.Zero);
                this.voteRoom.Updated();
            }
        }

        /// <summary>
        /// 全投票時間を延長(短縮)します。
        /// </summary>
        public void AddTotalVoteSpan(TimeSpan diff)
        {
            if (AddTotalVoteSpanInternal(diff))
            {
                // 持ち時間を変えた後、投票時間を再調整します。
                AddVoteSpanInternal(TimeSpan.Zero);
                this.voteRoom.Updated();
            }
        }

        /// <summary>
        /// 投票時間が終わっているか調べます。
        /// </summary>
        private void CheckVoteEnd()
        {
            using (LazyLock())
            {
                // -1秒になったら投票を終了します。
                if (VoteState == VoteState.Voting &&
                    VoteLeaveTime < TimeSpan.Zero)
                {
                    VoteEnded(VoteState.End, VoteSpan);

                    this.voteRoom.Updated();
                    this.voteRoom.BroadcastSystemNotification(
                        SystemNotificationType.VoteEnd);
                }
            }
        }

        /// <summary>
        /// 現在の投票状態に合わせてタイマーの状態を変更します。
        /// </summary>
        public void AdjustTimer()
        {
            using (LazyLock())
            {
                switch (VoteState)
                {
                    case VoteState.Voting:
                        StartTimer((int)VoteLeaveTime.TotalMilliseconds);
                        break;
                    case VoteState.Pause:
                    case VoteState.Stop:
                    case VoteState.End:
                        StopTimer();
                        return;
                }
            }
        }

        /// <summary>
        /// 指定時間後にタイマーが呼び出されるように設定します。
        /// </summary>
        private void StartTimer(int milliseconds)
        {
            // 時間間隔は０以上に設定します。
            milliseconds = Math.Max(0, milliseconds);

            // 指定時間後に呼び出されるようにします。
            // 本来は一回でいいですが、チェックミスに対応するため300ms間隔で
            // 連続的に呼び出すようにしています。
            this.timer.Change(
                milliseconds,
                300);
        }

        /// <summary>
        /// タイマーを止めます。
        /// </summary>
        private void StopTimer()
        {
            this.timer.Change(
                Timeout.Infinite,
                Timeout.Infinite);
        }

        #region コマンド処理
        /// <summary>
        /// コネクションにコマンドなどのハンドラを登録します。
        /// </summary>
        public void ConnectHandlers(PbConnection connection)
        {
            connection.AddCommandHandler<StartVoteCommand>(
                HandleStartVoteCommand);
            connection.AddCommandHandler<PauseVoteCommand>(
                HandlePauseVoteCommand);
            connection.AddCommandHandler<StopVoteCommand>(
                HandleStopVoteCommand);
            connection.AddCommandHandler<SetVoteSpanCommand>(
                HandleSetVoteSpanCommand);
            connection.AddCommandHandler<AddVoteSpanCommand>(
                HandleAddVoteSpanCommand);
            connection.AddCommandHandler<SetTotalVoteSpanCommand>(
                HandleSetTotalVoteSpanCommand);
            connection.AddCommandHandler<AddTotalVoteSpanCommand>(
                HandleAddTotalVoteSpanCommand);
        }

        /// <summary>
        /// コネクションからコマンドなどのハンドラを削除します。
        /// </summary>
        public void DisconnectHandlers(PbConnection connection)
        {
            connection.RemoveHandler<StartVoteCommand>();
            connection.RemoveHandler<PauseVoteCommand>();
            connection.RemoveHandler<StopVoteCommand>();
            connection.RemoveHandler<SetVoteSpanCommand>();
            connection.RemoveHandler<AddVoteSpanCommand>();
            connection.RemoveHandler<SetTotalVoteSpanCommand>();
            connection.RemoveHandler<AddTotalVoteSpanCommand>();
        }

        /// <summary>
        /// 投票を開始します。
        /// </summary>
        private void HandleStartVoteCommand(
            object sender, PbCommandEventArgs<StartVoteCommand> e)
        {
            var voteSpan = (e.Command.Seconds < 0 ?
                TimeSpan.MaxValue :
                TimeSpan.FromSeconds(e.Command.Seconds));

            StartVote(voteSpan);
        }

        /// <summary>
        /// 投票を一時停止します。
        /// </summary>
        private void HandlePauseVoteCommand(
            object sender, PbCommandEventArgs<PauseVoteCommand> e)
        {
            PauseVote();
        }

        /// <summary>
        /// 投票を中止します。
        /// </summary>
        private void HandleStopVoteCommand(
            object sender, PbCommandEventArgs<StopVoteCommand> e)
        {
            StopVote();
        }

        /// <summary>
        /// 投票の残り時間を設定します。
        /// </summary>
        private void HandleSetVoteSpanCommand(
            object sender, PbCommandEventArgs<SetVoteSpanCommand> e)
        {
            // 変更時間を取得します。
            var span = TimeSpan.FromSeconds(e.Command.Seconds);

            SetVoteSpan(span);
        }

        /// <summary>
        /// 投票の残り時間を変更します。
        /// </summary>
        private void HandleAddVoteSpanCommand(
            object sender, PbCommandEventArgs<AddVoteSpanCommand> e)
        {
            // 変更時間を取得します。
            var diff = TimeSpan.FromSeconds(e.Command.DiffSeconds);

            AddVoteSpan(diff);
        }

        /// <summary>
        /// 全投票の残り時間を設定します。
        /// </summary>
        private void HandleSetTotalVoteSpanCommand(
            object sender, PbCommandEventArgs<SetTotalVoteSpanCommand> e)
        {
            // 変更時間を取得します。
            var span = TimeSpan.FromSeconds(e.Command.Seconds);

            SetTotalVoteSpan(span);
        }

        /// <summary>
        /// 全投票の残り時間を変更します。
        /// </summary>
        private void HandleAddTotalVoteSpanCommand(
            object sender, PbCommandEventArgs<AddTotalVoteSpanCommand> e)
        {
            // 変更時間を取得します。
            var diff = TimeSpan.FromSeconds(e.Command.DiffSeconds);

            AddTotalVoteSpan(diff);
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteTimeKeeper(VoteRoom voteRoom)
        {
            this.voteRoom = voteRoom;
            
            VoteState = VoteState.Stop;
            VoteStartTimeNtp = DateTime.MinValue;
            VoteSpan = TimeSpan.Zero;
            TotalVoteSpan = TimeSpan.FromSeconds(120 * 60 + 0.9);

            this.timer = new Timer(
                (_) => Util.SafeCall(CheckVoteEnd),
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }
    }
}
