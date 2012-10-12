using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;

using Ragnarok;
using Ragnarok.Presentation;
using Ragnarok.Net.ProtoBuf;

namespace VoteSystem.Client.Command
{
    using Protocol;
    using Protocol.Vote;

    public static partial class Commands
    {
        /// <summary>
        /// ログイン前の画像選択コマンドです。
        /// </summary>
        public readonly static ICommand SelectImageAsOwner =
            new RoutedUICommand(
                "イメージ画像を選択します。",
                "SelectImageAsOwner",
                typeof(Window));

        /// <summary>
        /// ローカル環境での投票を選択するコマンドです。
        /// </summary>
        public readonly static ICommand SelectLocalVote =
            new RoutedUICommand(
                "ローカル環境で投票を開始します。",
                "SelectLocalVote",
                typeof(Window));
        /// <summary>
        /// 投票ルームを作るコマンドです。
        /// </summary>
        public readonly static ICommand CreateVoteRoom =
            new RoutedUICommand(
                "投票ルームを作成します。",
                "CreateVoteRoom",
                typeof(Window));
        /*public readonly static ICommand ShowVoteRoomInfoCommand =
            new RoutedUICommand(
                "投票ルームの情報を得ます。",
                "ShowVoteRoomInfoCommand",
                typeof(Window));*/
        /// <summary>
        /// 投票ルームへの入室コマンドです。
        /// </summary>
        public readonly static ICommand EnterVoteRoom =
            new RoutedUICommand(
                "投票ルームに入室します。",
                "EnterVoteRoom",
                typeof(Window));
        /// <summary>
        /// 投票ルームからの退出コマンドです。
        /// </summary>
        public readonly static ICommand LeaveVoteRoom =
            new RoutedUICommand(
                "投票ルームから退出します。",
                "LeaveVoteRoom",
                typeof(Window));

        /// <summary>
        /// 投票を時間制限つきで開始するコマンドです。
        /// </summary>
        public readonly static ICommand StartVoteWithLimit =
            new RoutedUICommand(
                "投票を時間制限つきで開始します。",
                "StartVoteWithLimit",
                typeof(Window));
        /// <summary>
        /// 投票を時間制限無しで開始するコマンドです。
        /// </summary>
        public readonly static ICommand StartVoteWithNolimit =
            new RoutedUICommand(
                "投票を時間制限無しで開始します。",
                "StartVoteWithNolimit",
                typeof(Window));
        /// <summary>
        /// 投票を一時停止するコマンドです。
        /// </summary>
        public readonly static ICommand PauseVote =
            new RoutedUICommand(
                "投票を一時停止します。",
                "PauseVote",
                typeof(Window));
        /// <summary>
        /// 投票を停止するコマンドです。
        /// </summary>
        public readonly static ICommand StopVote =
            new RoutedUICommand(
                "投票を停止します。",
                "StopVote",
                typeof(Window));

        /// <summary>
        /// 投票時間の再設定コマンドです。
        /// </summary>
        public readonly static ICommand SetVoteSpan =
            new RoutedUICommand(
                "投票時間を再設定します。",
                "SetVoteSpan",
                typeof(Window));
        /// <summary>
        /// 投票時間の追加コマンドです。
        /// </summary>
        public readonly static ICommand AddVoteSpan =
            new RoutedUICommand(
                "投票時間を追加します。",
                "AddVoteSpan",
                typeof(Window));
        /// <summary>
        /// 全投票時間の追加コマンドです。
        /// </summary>
        public readonly static ICommand AddTotalVoteSpan =
            new RoutedUICommand(
                "全投票時間を追加します。",
                "AddTotalVoteSpan",
                typeof(Window));

        /// <summary>
        /// 投票結果をすべてクリアするコマンドです。
        /// </summary>
        public readonly static ICommand ClearVote =
            new RoutedUICommand(
                "投票結果をすべてクリアします。",
                "ClearVote",
                typeof(Window));
        /// <summary>
        /// 延長要求結果をすべてクリアするコマンドです。
        /// </summary>
        public readonly static ICommand ClearTimeExtendDemand =
            new RoutedUICommand(
                "延長要求結果をすべてクリアします。",
                "ClearTimeExtendDemand",
                typeof(Window));
        /// <summary>
        /// 評価値をすべてクリアするコマンドです。
        /// </summary>
        public readonly static ICommand ClearEvaluationPoint =
            new RoutedUICommand(
                "評価値をすべてクリアします。",
                "ClearEvaluationPoint",
                typeof(Window));
        /// <summary>
        /// 一言メッセージを設定するコマンドです。
        /// </summary>
        public readonly static ICommand SetMessage =
            new RoutedUICommand(
                "一言メッセージを設定します。",
                "SetMessage",
                typeof(Window));
        /// <summary>
        /// 投票ルームに通知を送るコマンドです。
        /// </summary>
        public readonly static ICommand SendNotification =
            new RoutedUICommand(
                "投票ルームに通知を送ります。",
                "SendNotification",
                typeof(Window));
    }

    /// <summary>
    /// 通知を送るときに必要なパラメーターです。
    /// </summary>
    public class SendNotificationParameter
    {
        /// <summary>
        /// 送信される通知を取得または設定します。
        /// </summary>
        public Notification Notification
        {
            get;
            set;
        }

        /// <summary>
        /// 放送主からのメッセージ送信かどうかを取得または設定します。
        /// </summary>
        public bool IsFromLiveOwner
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 投票関連コマンドの実行メソッドなどを持ちます。
    /// </summary>
    public static class VoteCommands
    {
        /// <summary>
        /// コマンドを指定のオブジェクトにバインディングします。
        /// </summary>
        public static void Bind(CommandBindingCollection bindings)
        {
            bindings.Add(
                new CommandBinding(
                    Commands.SelectImageAsOwner,
                    ExecuteSelectImageAsOwner,
                    CanExecuteCommand));

            bindings.Add(
                new CommandBinding(
                    Commands.SelectLocalVote,
                    ExecuteSelectLocalVote,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.CreateVoteRoom,
                    ExecuteCreateVoteRoom,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.EnterVoteRoom,
                    ExecuteEnterVoteRoom,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.LeaveVoteRoom,
                    ExecuteLeaveVoteRoom,
                    CanExecuteCommand));

            bindings.Add(
                new CommandBinding(
                    Commands.StartVoteWithLimit,
                    ExecuteStartVoteWithLimit,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.StartVoteWithNolimit,
                    ExecuteStartVoteWithNolimit,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.PauseVote,
                    ExecutePauseVote,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.StopVote,
                    ExecuteStopVote,
                    CanExecuteCommand));

            bindings.Add(
                new CommandBinding(
                    Commands.SetVoteSpan,
                    ExecuteSetVoteTime,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.AddVoteSpan,
                    ExecuteAddVoteTime,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.AddTotalVoteSpan,
                    ExecuteAddTotalVoteTime,
                    CanExecuteCommand));

            bindings.Add(
                new CommandBinding(
                    Commands.ClearVote,
                    ExecuteClearVote,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.ClearTimeExtendDemand,
                    ExecuteClearTimeExtendDemand,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.ClearEvaluationPoint,
                    ExecuteClearEvaluationPoint,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.SetMessage,
                    ExecuteSetMessage,
                    CanExecuteCommand));
            bindings.Add(
                new CommandBinding(
                    Commands.SendNotification,
                    ExecuteSendNotification,
                    CanExecuteCommand));
        }

        /// <summary>
        /// 投票ルームに渡される放送主の画像が選択可能かどうかです。
        /// </summary>
        public static void CanExecuteCommand(object sender,
                                             CanExecuteRoutedEventArgs e)
        {
            if (e.Command == Commands.SelectImageAsOwner)
            {
                e.CanExecute = !Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.SelectLocalVote)
            {
                e.CanExecute = !Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.CreateVoteRoom)
            {
                e.CanExecute = !Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.EnterVoteRoom)
            {
                var roomInfo = Global.VoteRoomInfoModel.SelectedVoteRoomInfo;

                e.CanExecute = (
                    !Global.VoteClient.IsLogined &&
                    (roomInfo != null && roomInfo.Validate()));
            }
            else if (e.Command == Commands.LeaveVoteRoom)
            {
                e.CanExecute = Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.StartVoteWithLimit)
            {
                if (Global.VoteClient.VoteState == VoteState.Pause)
                {
                    // 一時停止中なら現在の残り時間にあったボタンしか押せません。
                    e.CanExecute = (
                        Global.VoteClient.IsLogined &&
                        !Global.VoteClient.IsVoteSpanNolimit);
                }
                else
                {
                    // 投票中は投票開始ボタンは押せません。
                    e.CanExecute = (
                        Global.VoteClient.IsLogined &&
                        Global.VoteClient.VoteState != VoteState.Voting);
                }
            }
            else if (e.Command == Commands.StartVoteWithNolimit)
            {
                // 一時停止中なら現在の残り時間にあったボタンしか押せません。
                if (Global.VoteClient.VoteState == VoteState.Pause)
                {
                    e.CanExecute = (
                        Global.VoteClient.IsLogined &&
                        Global.VoteClient.IsVoteSpanNolimit);
                }
                else
                {
                    // 投票中は投票開始ボタンは押せません。
                    e.CanExecute = (
                        Global.VoteClient.IsLogined &&
                        Global.VoteClient.VoteState != VoteState.Voting);
                }
            }
            else if (e.Command == Commands.PauseVote)
            {
                e.CanExecute = (
                    Global.VoteClient.IsLogined &&
                    Global.VoteClient.VoteState == VoteState.Voting);
            }
            else if (e.Command == Commands.StopVote)
            {
                e.CanExecute = (
                    Global.VoteClient.IsLogined &&
                    Global.VoteClient.VoteState != VoteState.Stop);
            }
            else if (e.Command == Commands.SetVoteSpan ||
                     e.Command == Commands.AddVoteSpan)
            {
                e.CanExecute = (
                    Global.VoteClient.IsLogined &&
                    (Global.VoteClient.VoteState == VoteState.Voting ||
                     Global.VoteClient.VoteState == VoteState.Pause) &&
                    !Global.VoteClient.IsVoteSpanNolimit);
            }
            else if (e.Command == Commands.ClearVote)
            {
                e.CanExecute = Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.ClearTimeExtendDemand)
            {
                e.CanExecute = Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.ClearEvaluationPoint)
            {
                e.CanExecute = Global.VoteClient.IsLogined;
            }
            else if (e.Command == Commands.SendNotification)
            {
                e.CanExecute = Global.VoteClient.IsLogined;
            }
            else
            {
                e.CanExecute = true;
            }

            e.Handled = true;
        }

        /// <summary>
        /// データ送信にかかる遅延時間(参考)です。
        /// </summary>
        private readonly static TimeSpan FractionTime = TimeSpan.FromSeconds(0.99);

        /// <summary>
        /// ウィンドウから時間間隔を取得します。
        /// </summary>
        private static TimeSpan? GetTimeSpan(TimeSpan defaultValue)
        {
            // 時間間隔をウィンドウから取得します。
            var window = new View.TimeSpanWindow(defaultValue);
            var result = window.ShowDialogCenterMouse();
            if (result == null || !result.Value)
            {
                return null;
            }

            return window.TimeSpan;
        }

        /// <summary>
        /// 投票ルームに渡される放送主の画像を選択します。
        /// </summary>
        public static void ExecuteSelectImageAsOwner(object sender,
                                                     ExecutedRoutedEventArgs e)
        {
            var imageWindow = new View.SelectImageWindow();

            var result = imageWindow.ShowDialog();
            if (result == null || !result.Value)
            {
                // キャンセルされた場合はなにもせずに帰ります。
                return;
            }

            var selectedImageUrl = imageWindow.SelectedImageUrl;
            if (string.IsNullOrEmpty(selectedImageUrl))
            {
                // 選択されたイメージがない場合も帰ります。
                return;
            }

            Global.MainModel.ImageUrl = imageWindow.SelectedImageUrl;
        }

        /// <summary>
        /// ローカル投票を選択します。
        /// </summary>
        public static void ExecuteSelectLocalVote(object sender,
                                                  ExecutedRoutedEventArgs e)
        {
            if (!ProtocolUtil.CheckVoteRoomName(
                Global.MainModel.NickName))
            {
                MessageUtil.ErrorMessage(
                    "ニックネームが正しくありませぬ o(><*)o");
                return;
            }

            if (!ProtocolUtil.CheckVoteRoomName(
                Global.MainModel.VoteRoomName))
            {
                MessageUtil.ErrorMessage(
                    "ルーム名が正しくありませぬ o(><*)o");
                return;
            }

            try
            {
                Global.VoteClient.Connect(
                    "localhost",
                    ServerSettings.VotePort);

                // 投票ルーム名などは設定されていない可能性があるため、
                // あらかじめ設定してある固定値を使います。
                Global.VoteClient.CreateVoteRoom(
                    Global.MainModel.VoteRoomName,
                    Global.MainModel.VoteRoomPassword,
                    Global.MainModel.Id,
                    Global.MainModel.NickName,
                    Global.MainModel.ImageUrl,
                    (sender_, e_) =>
                    {
                        if (e_.ErrorCode != ErrorCode.None)
                        {
                            MessageUtil.ErrorMessage(
                                e_.ErrorCode,
                                "投票ルームの作成に失敗しました (≧ヘ≦)");
                        }
                        else
                        {
                            MessageUtil.Message(
                                "投票ルームの作成に成功しました (≧∇≦)b");
                        }
                    });
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票ルームの作成に失敗しました (≧ヘ≦)");
            }
        }

        /// <summary>
        /// 新たな投票部屋を作成します。
        /// </summary>
        public static void ExecuteCreateVoteRoom(object sender,
                                                 ExecutedRoutedEventArgs e)
        {
            if (!ProtocolUtil.CheckVoteRoomName(
                Global.MainModel.NickName))
            {
                MessageUtil.ErrorMessage(
                    "ニックネームが正しくありませぬ o(><*)o");
                return;
            }

            if (!ProtocolUtil.CheckVoteRoomName(
                Global.MainModel.VoteRoomName))
            {
                MessageUtil.ErrorMessage(
                    "ルーム名が正しくありませぬ o(><*)o");
                return;
            }

            try
            {
                // 投票サーバーに接続します。
                Global.VoteClient.Connect(
                    ServerSettings.VoteAddress,
                    ServerSettings.VotePort);

                // 部屋の作成要求をサーバーに出します。
                Global.VoteClient.CreateVoteRoom(
                    Global.MainModel.VoteRoomName,
                    Global.MainModel.VoteRoomPassword,
                    Global.MainModel.Id,
                    Global.MainModel.NickName,
                    Global.MainModel.ImageUrl,
                    (sender_, e_) =>
                    {
                        if (e_.ErrorCode != ErrorCode.None)
                        {
                            MessageUtil.ErrorMessage(
                                e_.ErrorCode,
                                "投票ルームの作成に失敗しました (≧ヘ≦)");
                        }
                        else
                        {
                            MessageUtil.Message(
                                "投票ルームの作成に成功しました (≧∇≦)b");
                        }
                    });
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票ルームの作成に失敗しました (≧ヘ≦)");
            }
        }

        /// <summary>
        /// 既存の投票ルームに入室します。
        /// </summary>
        public static void ExecuteEnterVoteRoom(object sender,
                                                ExecutedRoutedEventArgs e)
        {
            if (Global.VoteClient.IsLogined)
            {
                MessageUtil.ErrorMessage(
                    "すでに投票ルームに入室しています o(><*)o");
                return;
            }
            
            var roomInfo = Global.VoteRoomInfoModel.SelectedVoteRoomInfo;
            if (roomInfo == null)
            {
                MessageUtil.ErrorMessage(
                    "投票ルームが選択されていませぬ o(><*)o");
                return;
            }

            var password = "";

            // パスワードが必要ならそれを入力してもらいます。
            if (roomInfo.HasPassword)
            {
                password = Global.MainModel.VoteRoomPassword;

                if (string.IsNullOrEmpty(password))
                {
                    MessageUtil.ErrorMessage(
                        "パスワードがありませぬ o(><*)o");
                    return;
                }
            }

            try
            {
                // 投票サーバーに接続します。
                Global.VoteClient.Connect(
                    ServerSettings.VoteAddress,
                    ServerSettings.VotePort);

                // 入室要求をサーバーに送ります。
                Global.VoteClient.EnterVoteRoom(
                    roomInfo.Id,
                    password,
                    Global.MainModel.Id,
                    Global.MainModel.NickName,
                    Global.MainModel.ImageUrl,
                    (sender_, e_) =>
                        Global.UIProcess(
                            () => EnterVoteRoomDone(e_)));
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票ルームへの入室に失敗しました (≧ヘ≦)");
            }
        }

        /// <summary>
        /// 投票ルーム入室要求のレスポンス受信後に呼ばれます。
        /// </summary>
        public static void EnterVoteRoomDone(
            PbResponseEventArgs<EnterVoteRoomResponse> e)
        {
            if (e.ErrorCode != ErrorCode.None)
            {
                MessageUtil.ErrorMessage(e.ErrorCode,
                    "入室に失敗しました (￣＿￣*)");
                return;
            }

            MessageUtil.Message(
                string.Format(
                    "投票ルーム(No.{0})に入室しました (≧ω≦)ｂ",
                    e.Response.RoomInfo.Id));
        }

        /// <summary>
        /// 投票ルームから退出します。
        /// </summary>
        public static void ExecuteLeaveVoteRoom(object sender,
                                                ExecutedRoutedEventArgs e)
        {
            try
            {
                /*if (Global.VoteClient.VoteState != VoteState.Stop)
                {
                    var result = MessageBox.Show(
                        "投票が停止中ではありませんが\n退室してもいいですか？",
                        "退室確認",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question,
                        MessageBoxResult.No);
                    if (result != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                else*/
                if (Global.VoteClient.IsVoteRoomOwner)
                {
                    if (!MessageUtil.Confirm(
                        "ルームオーナーですが\n退室してもいいですか？",
                        "退室確認"))
                    {
                        return;
                    }
                }

                Global.VoteClient.LeaveVoteRoom(
                    (sender_, e_) =>
                    {
                        if (e_.ErrorCode != ErrorCode.None)
                        {
                            MessageUtil.ErrorMessage(
                                e_.ErrorCode,
                                "投票ルームからの退出に失敗しました (≧◇≦)");
                        }
                        else
                        {
                            MessageUtil.Message(
                                "投票ルームから退出しました (≧ω≦)ｂ");
                        }
                    });
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票ルームからの退出に失敗しました (≧◇≦)");
            }
        }

        /// <summary>
        /// 制限時間付きで投票を開始します。
        /// </summary>
        public static void ExecuteStartVoteWithLimit(object sender,
                                                     ExecutedRoutedEventArgs e)
        {
            try
            {
                // 再開の場合、時間の入力はしません。
                if (Global.VoteClient.VoteState == VoteState.Pause)
                {
                    Global.VoteClient.StartVote(TimeSpan.FromSeconds(-1));
                    return;
                }

                // 時間をウィンドウから取得します。
                var timeSpan = GetTimeSpan(Global.Settings.DefaultVoteTimeSpan);
                if (timeSpan == null)
                {
                    return;
                }

                // 制限時間を秒が繰り上がらない程度に増やします。
                // これは2:00ぴったりの投票時間を指定したときに、
                // 「残り２分です」と読み上げるためなどに必要な処理です。
                Global.VoteClient.StartVote(timeSpan.Value + FractionTime);

                // 設定した時間を次回のために保存します。
                Global.Settings.DefaultVoteTimeSpan = timeSpan.Value;
                Global.Settings.Save();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票の開始に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 制限時間無しで投票を開始します。
        /// </summary>
        public static void ExecuteStartVoteWithNolimit(object sender,
                                                       ExecutedRoutedEventArgs e)
        {
            try
            {
                Global.VoteClient.StartVote(TimeSpan.FromSeconds(-1));
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票の開始に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 投票を一時停止します。
        /// </summary>
        public static void ExecutePauseVote(object sender,
                                            ExecutedRoutedEventArgs e)
        {
            try
            {
                Global.VoteClient.PauseVote();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票の一時停止に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 投票を停止します。
        /// </summary>
        public static void ExecuteStopVote(object sender,
                                           ExecutedRoutedEventArgs e)
        {
            if (Global.VoteClient.VoteState == VoteState.Voting ||
                Global.VoteClient.VoteState == VoteState.Pause)
            {
                if (!MessageUtil.Confirm(
                    "投票を停止してもよろしいですか？",
                    "停止確認"))
                {
                    return;
                }
            }

            try
            {
                Global.VoteClient.StopVote();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票の停止に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 投票時間を再設定します。
        /// </summary>
        public static void ExecuteSetVoteTime(object sender,
                                              ExecutedRoutedEventArgs e)
        {
            try
            {
                // 時間間隔をウィンドウから取得します。
                var timeSpan = GetTimeSpan(Global.Settings.SetVoteTimeSpan);
                if (timeSpan == null)
                {
                    return;
                }

                // 制限時間を秒が繰り上がらない程度に増やします。
                // (遅延時間分などを余分に追加)
                Global.VoteClient.SetVoteSpan(timeSpan.Value + FractionTime);

                // 設定した時間を次回のために保存します。
                Global.Settings.SetVoteTimeSpan = timeSpan.Value;
                Global.Settings.Save();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票時間の再設定に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 投票時間を追加します。
        /// </summary>
        public static void ExecuteAddVoteTime(object sender,
                                              ExecutedRoutedEventArgs e)
        {
            try
            {
                // 追加時間をウィンドウから取得します。
                var timeSpan = GetTimeSpan(Global.Settings.AddVoteTimeSpan);
                if (timeSpan == null)
                {
                    return;
                }

                // 残り時間の追加をします。
                // (データ送信などにかかる遅延時間は考慮しません)
                Global.VoteClient.AddVoteSpan(timeSpan.Value);

                // 設定した時間を次回のために保存します。
                Global.Settings.AddVoteTimeSpan = timeSpan.Value;
                Global.Settings.Save();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票時間の追加に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 全投票時間を追加します。
        /// </summary>
        public static void ExecuteAddTotalVoteTime(object sender,
                                                   ExecutedRoutedEventArgs e)
        {
            try
            {
                // 追加時間をウィンドウから取得します。
                var timeSpan = GetTimeSpan(Global.Settings.AddTotalVoteTimeSpan);
                if (timeSpan == null)
                {
                    return;
                }

                // 残り時間の追加をします。
                // (データ送信などにかかる遅延時間は考慮しません)
                Global.VoteClient.AddTotalVoteSpan(timeSpan.Value);

                // 設定した時間を次回のために保存します。
                Global.Settings.AddTotalVoteTimeSpan = timeSpan.Value;
                Global.Settings.Save();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "持ち時間の追加に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 投票結果をすべてクリアします。
        /// </summary>
        public static void ExecuteClearVote(object sender,
                                            ExecutedRoutedEventArgs e)
        {
            if (!MessageUtil.Confirm(
                "投票結果をすべてクリアしてもいいですか？",
                "確認"))
            {
                return;
            }

            try
            {
                Global.VoteClient.ClearVote();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "投票結果のクリアに失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 延長要求結果をすべてクリアします。
        /// </summary>
        public static void ExecuteClearTimeExtendDemand(object sender,
                                                        ExecutedRoutedEventArgs e)
        {
            try
            {
                Global.VoteClient.ClearTimeExtendDemand();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "延長要求結果のクリアに失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 評価値をクリアします。
        /// </summary>
        public static void ExecuteClearEvaluationPoint(object sender,
                                                       ExecutedRoutedEventArgs e)
        {
            try
            {
                Global.VoteClient.ClearEvaluationPoint();
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "評価値のクリアに失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 一言メッセージを設定します。
        /// </summary>
        public static void ExecuteSetMessage(object sender,
                                             ExecutedRoutedEventArgs e)
        {
            try
            {
                var message = (string)e.Parameter;

                Global.VoteClient.SetParticipantAttribute(
                    null,
                    null,
                    message,
                    null);
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "一言メッセージの設定に失敗しました o(＞ω＜)o");
            }
        }

        /// <summary>
        /// 通知を送ります。
        /// </summary>
        public static void ExecuteSendNotification(object sender,
                                                   ExecutedRoutedEventArgs e)
        {
            try
            {
                var parameter = (SendNotificationParameter)e.Parameter;

                Global.VoteClient.SendNotification(
                    parameter.Notification,
                    parameter.IsFromLiveOwner);
            }
            catch (Exception ex)
            {
                MessageUtil.ErrorMessage(ex,
                    "通知の送信に失敗しました o(＞ω＜)o");
            }
        }
    }
}
