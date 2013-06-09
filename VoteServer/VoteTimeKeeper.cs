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
    /// 投票の開始／停止、時間管理などを行うオブジェクトです。
    /// </summary>
    /// <remarks>
    /// 条件）
    /// ・投票時間は全投票時間（持ち時間）以上に設定できません。
    /// ・投票時間は停止時に秒の小数点数が.3に設定されます。
    /// ・投票時間／全投票時間は秒の単位でしか、追加・設定できません。
    /// 
    /// 理由）
    /// １、投票時間と全投票時間の小数点数が違うと
    /// 　　画面表示のタイミングがずれることがあります。
    /// ２、常にコンマ数秒の余裕があるため、２分ジャストの投票時間でも
    /// 　　各クライアントが「残り２分です」という通知を発行できます。
    /// 　　この余裕がないと通信遅延のせいで、クライアントからは
    /// 　　投票時間が１分５９．９秒くらいに見えてしまいます。
    /// 
    /// 管理方法）
    /// 投票時間／全投票時間は、投票開始時刻(これは両者共通)と投票の全期間の
    /// セットで管理されます。投票残り時間は現在時刻を使い随時計算します。
    /// 投票時間の全期間は投票状態によって値が変わりますが、
    /// 全投票時間は投票状態にかかわりなく一貫して管理されます。
    /// ・投票中　→　投票時間の全期間はそのままの値
    /// ・停止中　→　投票時間の全期間は０
    /// ・一時停止中　→　投票時間の全期間は投票残り時間。一時停止時に時間が再計算されます。
    /// </remarks>
    public class VoteTimeKeeper : NotifyObject, ILogObject
    {
        /// <summary>
        /// 投票時間の既定の端数です。
        /// </summary>
        private static readonly double Fraction = 0.3;

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
            private set
            {
                if (value != TimeSpan.MinValue && value != TimeSpan.MaxValue)
                {
                    // ミリ秒以下の部分はTotalVoteSpanと合わせます。
                    // そうしないと両者の表示タイミングがずれてしまいます。
                    value = TimeSpan.FromMilliseconds(
                        (int)value.TotalSeconds * 1000 +
                        TotalVoteSpan.Milliseconds);
                }

                SetValue("VoteSpan", value);
            }
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
            private set
            {
                SetValue("TotalVoteSpan", value);

                // ミリ秒を合わせるための処理です。
                VoteSpan = VoteSpan;
            }
        }

        /// <summary>
        /// 投票時間が無制限かどうか取得します。
        /// </summary>
        [DependOnProperty("VoteSpan")]
        public bool IsVoteNoLimit
        {
            get { return (VoteSpan == TimeSpan.MaxValue); }
        }

        /// <summary>
        /// 全投票時間が無制限かどうか取得します。
        /// </summary>
        [DependOnProperty("TotalVoteSpan")]
        public bool IsTotalVoteNoLimit
        {
            get { return (TotalVoteSpan == TimeSpan.MaxValue); }
        }

        /// <summary>
        /// 全時間から現在までの経過時間を考慮し、投票残り時間を計算します。
        /// </summary>
        private TimeSpan CalcLeaveTime(TimeSpan allSpan)
        {
            return ProtocolUtil.CalcVoteLeaveTime(
                VoteState,
                VoteStartTimeNtp,
                allSpan);
        }

        /// <summary>
        /// 全時間から現在までの経過時間を考慮し、投票残り時間を計算します。
        /// </summary>
        private TimeSpan CalcTotalLeaveTime(TimeSpan allSpan)
        {
            return ProtocolUtil.CalcTotalVoteLeaveTime(
                VoteState,
                VoteStartTimeNtp,
                allSpan);
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

                        // 開始前にも投票時間を正規化しておきます。
                        NormalizeVoteSpan();
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
        /// 時間の秒数の小数点以下を切り捨てます。
        /// </summary>
        private TimeSpan FloorSeconds(TimeSpan span)
        {
            var seconds = span.TotalSeconds;
            seconds = Math.Floor(seconds);

            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// 投票時間と全投票時間の秒数の小数点を規定値に合わせます。
        /// </summary>
        private void NormalizeVoteSpan()
        {
            using (LazyLock())
            {
                if (!IsVoteNoLimit && VoteSpan > TimeSpan.Zero)
                {
                    var seconds = VoteSpan.TotalSeconds;
                    seconds = Math.Floor(seconds) + Fraction;

                    VoteSpan = TimeSpan.FromSeconds(seconds);
                }

                if (!IsTotalVoteNoLimit && TotalVoteSpan > TimeSpan.Zero)
                {
                    var seconds = TotalVoteSpan.TotalSeconds;
                    seconds = Math.Floor(seconds) + Fraction;

                    TotalVoteSpan = TimeSpan.FromSeconds(seconds);
                }
            }
        }

        /// <summary>
        /// 投票終了/停止時に呼ばれます。
        /// </summary>
        private void VoteEnded(VoteState state)
        {
            using (LazyLock())
            {
                // 全投票の残り時間は状態変更前に取得します。
                var leaveTime = CalcLeaveTime(TotalVoteSpan);

                VoteSpan = TimeSpan.Zero;
                VoteState = state;

                // 全投票時間は減ります。
                if (!IsTotalVoteNoLimit)
                {
                    TotalVoteSpan = leaveTime;
                }

                // 全投票時間の秒数を合わせます。
                //NormalizeVoteSpan();

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

                // 経過時間だけ全投票時間は減ります。
                VoteEnded(VoteState.Stop);

                // 必要なら全投票時間を再設定します。
                AddTotalVoteSpanInternal(addTotalVoteTimeSpan);
            }

            this.voteRoom.Updated();
            this.voteRoom.BroadcastSystemNotification(
                SystemNotificationType.VoteStop);
        }

        /// <summary>
        /// 計算された残り時間が<paramref name="minimum"/>以下であれば、
        /// 時刻調整を行います。
        /// </summary>
        /// <remarks>
        /// <paramref name="voteSpan"/>は今の残り時間ではなく、
        /// 投票開始～終了までの時間を表しています。
        /// 
        /// ここで調整するのは投票の「残り時間」のため、
        /// 少し計算が複雑になっています。
        /// </remarks>
        private TimeSpan? AdjustVoteSpan(TimeSpan voteSpan, TimeSpan minimum)
        {
            if (minimum != TimeSpan.MinValue &&
                CalcLeaveTime(voteSpan) < minimum)
            {
                var oldLeaveTime = CalcLeaveTime(VoteSpan);
                if (oldLeaveTime < minimum)
                {
                    // 元の時間もminimum以下であれば、時刻調整は行いません。
                    return null;
                }
                else
                {
                    // 元の時間がminimum以上であれば、時刻をminimumに設定します。
                    // 投票期間から今の残り時間を引くと、
                    // 残り時間を０にできます。
                    voteSpan = (VoteSpan - oldLeaveTime) + minimum;
                }
            }

            return voteSpan;
        }

        /// <summary>
        /// 投票時間を追加します。(メッセージは出さない)
        /// </summary>
        /// <remarks>
        /// 有限時間から無限大時間への変更、またはその逆は
        /// このメソッドではできません。
        /// 
        /// 条件）
        /// ・投票時間は全投票時間以上には設定できない。
        /// </remarks>
        private bool AddVoteSpanInternal(TimeSpan addSpan, TimeSpan minimum)
        {
            using (LazyLock())
            {
                // 停止中は投票時間を変更できません（必ず０です）
                if (VoteState == VoteState.Stop || VoteState == VoteState.End)
                {
                    return false;
                }

                // 時間無限はパス
                if (IsVoteNoLimit)
                {
                    return false;
                }

                // 投票時間が無限なら、時間の増減はできません。
                if (addSpan == TimeSpan.MaxValue)
                {
                    Log.Error("全投票時間に無限大時間を追加することはできません。");
                    return false;
                }

                // 条件:０ <= 投票時間 <= 全投票時間
                var newSpan = MathEx.Max(
                    TimeSpan.Zero,
                    MathEx.Min(VoteSpan + addSpan, TotalVoteSpan));

                // 残り時間がminimum以下にならないように調整します。
                var adjustedSpan = AdjustVoteSpan(newSpan, minimum);
                if (adjustedSpan == null)
                {
                    return false;
                }

                VoteSpan = adjustedSpan.Value;

                // 投票停止時間を検出するタイマを開始します。
                AdjustTimer();
            }

            return true;
        }

        /// <summary>
        /// 全投票時間を追加します。
        /// </summary>
        /// <remarks>
        /// 有限時間から無限大時間への変更、またはその逆は
        /// このメソッドではできません。
        /// </remarks>
        private bool AddTotalVoteSpanInternal(TimeSpan addSpan)
        {
            using (LazyLock())
            {
                if (IsTotalVoteNoLimit)
                {
                    return false;
                }

                if (addSpan == TimeSpan.MaxValue)
                {
                    Log.Error("全投票時間に無限大時間を追加することはできません。");
                    return false;
                }

                // 時間再設定でなければ追加です。
                TotalVoteSpan = MathEx.Max(
                    TotalVoteSpan + addSpan, TimeSpan.Zero);

                // 全投票時間を変えた後、投票時間を再調整します。
                AddVoteSpanInternal(TimeSpan.Zero, TimeSpan.MaxValue);
                return true;
            }
        }

        /// <summary>
        /// 投票時間を再設定します。
        /// </summary>
        public void SetVoteSpan(TimeSpan newSpan)
        {
            SetVoteSpan(newSpan, TimeSpan.MinValue);
        }

        /// <summary>
        /// 投票時間を再設定します。
        /// </summary>
        public void SetVoteSpan(TimeSpan newSpan, TimeSpan minimum)
        {
            newSpan = FloorSeconds(newSpan);

            if (newSpan != TimeSpan.MaxValue)
            {
                // 投票時間は投票開始時間(ntp)と全体の長さを元に決定されます。
                // つまり、今の残り時間を指定の時間にするためには
                // 少し工夫して行う必要があり、
                // ここでは<現在の残り時間>と<新しい残り時間>の差を、
                // 全体の投票時間に加算することで実現しています。
                var leaveTime = CalcLeaveTime(VoteSpan);

                // <新しい残り時間> - <現在の残り時間>
                // を投票の全期間に足すことで、新しい期間を計算します。
                newSpan -= leaveTime;
            }

            if (AddVoteSpanInternal(newSpan, minimum))
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
            AddVoteSpan(diff, TimeSpan.MinValue);
        }

        /// <summary>
        /// 投票時間を延長(短縮)します。
        /// </summary>
        public void AddVoteSpan(TimeSpan diff, TimeSpan minimum)
        {
            diff = FloorSeconds(diff);

            if (AddVoteSpanInternal(diff, minimum))
            {
                this.voteRoom.Updated();
                this.voteRoom.BroadcastSystemNotification(
                    SystemNotificationType.ChangeVoteSpan);
            }
        }

        /// <summary>
        /// 全投票時間を再設定します。
        /// </summary>
        public void SetTotalVoteSpan(TimeSpan newSpan)
        {
            newSpan = FloorSeconds(newSpan);

            if (newSpan != TimeSpan.MaxValue)
            {
                // 投票時間は投票開始時間(ntp)と全体の長さを元に決定されます。
                // つまり、今の残り時間を指定の時間にするためには
                // 少し工夫する必要があり、
                // ここでは<現在の残り時間>と<新しい残り時間>の差を、
                // 全体の投票時間に加算することで実現しています。
                var leaveTime = CalcTotalLeaveTime(TotalVoteSpan);

                // <新しい残り時間> - <現在の残り時間>
                // を投票の全期間に足すことで、新しい期間を計算します。
                newSpan -= leaveTime;
            }

            if (AddTotalVoteSpanInternal(newSpan))
            {
                this.voteRoom.Updated();
                this.voteRoom.BroadcastSystemNotification(
                    SystemNotificationType.ChangeVoteSpan);
            }
        }

        /// <summary>
        /// 全投票時間を延長(短縮)します。
        /// </summary>
        public void AddTotalVoteSpan(TimeSpan diff)
        {
            diff = FloorSeconds(diff);

            if (AddTotalVoteSpanInternal(diff))
            {
                this.voteRoom.Updated();
                this.voteRoom.BroadcastSystemNotification(
                    SystemNotificationType.ChangeVoteSpan);
            }
        }

        /// <summary>
        /// 投票時間が終わっているか調べます。
        /// </summary>
        private void CheckVoteEnd()
        {
            using (LazyLock())
            {
                // 0秒になったら投票を終了します。
                if (VoteState != VoteState.Voting ||
                    CalcLeaveTime(VoteSpan) > TimeSpan.Zero)
                {
                    return;
                }

                VoteEnded(VoteState.End);
            }

            // 投票ルーム関係のメソッドはロック外で呼び出します。
            this.voteRoom.Updated();
            this.voteRoom.BroadcastSystemNotification(
                SystemNotificationType.VoteEnd);
        }

        /// <summary>
        /// 現在の投票状態に合わせてタイマーの状態を変更します。
        /// </summary>
        public void AdjustTimer()
        {
            TimeSpan span;

            using (LazyLock())
            {
                switch (VoteState)
                {
                    case VoteState.Voting:
                        span = CalcLeaveTime(VoteSpan);
                        StartTimer((int)span.TotalMilliseconds);
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
            // 変更時間を取得します。
            var span = TimeSpan.FromSeconds(e.Command.AddTotalTimeSeconds);

            StopVote(span);
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
