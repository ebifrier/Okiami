using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.NicoNico.Live;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Shogi;

namespace VoteSystem.PluginShogi.ViewModel
{
    using Protocol;
    using Protocol.Vote;
    using PluginShogi.Model;
    using PluginShogi.View;

    /// <summary>
    /// 変化の再生状態を示します。
    /// </summary>
    public enum VariationState
    {
        /// <summary>
        /// 何もしていません。
        /// </summary>
        None,
        /// <summary>
        /// 変化再生中です。
        /// </summary>
        Playing,
    }

    /// <summary>
    /// ShogiWindowのモデルクラスです。
    /// </summary>
    public class ShogiWindowViewModel : NotifyObject
    {
        private static readonly TimeSpan AutoPlayMinimumLeaveTime =
            TimeSpan.FromSeconds(20);

        private Board currentBoard;
        private Board board;

        private bool isCheckingAutoPlay = false;
        private readonly Queue<AutoPlayEx> autoPlayList =
            new Queue<AutoPlayEx>();
        private AutoPlay currentAutoPlay;
        private readonly VariationManager moveManager = new VariationManager();
        private readonly NotifyCollection<string> commentCandidates;
        private readonly CommentClient commentClient;

        /// <summary>
        /// 設定オブジェクトを取得します。
        /// </summary>
        public Settings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// グローバルモデルを取得します。
        /// </summary>
        public GlobalModel GlobalModel
        {
            get;
            private set;
        }

        /// <summary>
        /// 投票クライアントを取得します。
        /// </summary>
        public Client.Model.VoteClient VoteClient
        {
            get { return ShogiGlobal.VoteClient; }
        }

        /// <summary>
        /// クライアントのモデルオブジェクトを取得します。
        /// </summary>
        public Client.Model.MainModel ClientModel
        {
            get { return ShogiGlobal.ClientModel; }
        }

        /// <summary>
        /// 盤面を取得または設定します。
        /// </summary>
        public Board Board
        {
            get
            {
                return this.board;
            }
            set
            {
                using (LazyLock())
                {
                    if (this.currentBoard == value)
                    {
                        value = value.Clone();
                    }

                    if (this.board != value)
                    {
                        if (this.board != null)
                        {
                            this.board.BoardChanged -= board_BoardChanged;
                        }

                        this.board = value;

                        if (this.board != null)
                        {
                            this.board.BoardChanged += board_BoardChanged;
                        }

                        this.RaisePropertyChanged("Board");
                    }
                }
            }
        }

        /// <summary>
        /// 現局面を取得または設定します。
        /// </summary>
        public Board CurrentBoard
        {
            get
            {
                return this.currentBoard;
            }
            private set
            {
                using (LazyLock())
                {
                    if (this.board == value)
                    {
                        value = value.Clone();
                    }

                    if (this.currentBoard != value)
                    {
                        this.currentBoard = value;

                        this.RaisePropertyChanged("CurrentBoard");
                    }
                }
            }
        }

        /// <summary>
        /// 現局面からの指し手を取得します。
        /// </summary>
        public ReadOnlyCollection<Move> MoveFromCurrentBoard
        {
            get { return GetValue<ReadOnlyCollection<Move>>("MoveFromCurrentBoard"); }
            private set { SetValue("MoveFromCurrentBoard", value); }
        }

        /// <summary>
        /// 現局面からの指し手を文字列として取得します。
        /// </summary>
        public string MoveTextFromCurrentBoard
        {
            get { return GetValue<string>("MoveTextFromCurrentBoard"); }
            private set { SetValue("MoveTextFromCurrentBoard", value); }
        }

        /// <summary>
        /// 変化のコメントを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "AS_Comment")]
        public string Comment
        {
            get { return ShogiGlobal.Settings.AS_Comment; }
            set { ShogiGlobal.Settings.AS_Comment = value; }
        }

        /// <summary>
        /// コメント一覧を取得します。
        /// </summary>
        public NotifyCollection<string> CommentCandidates
        {
            get { return this.commentCandidates; }
        }

        /// <summary>
        /// 変化の管理オブジェクトを取得します。(XAML参照用)
        /// </summary>
        public VariationManager MoveManager
        {
            get { return this.moveManager; }
        }

        /// <summary>
        /// 放送URLを取得または設定します。
        /// </summary>
        public string LiveUrl
        {
            get { return GetValue<string>("LiveUrl"); }
            set { SetValue("LiveUrl", value); }
        }

        /// <summary>
        /// 放送オブジェクトを取得します。
        /// </summary>
        public CommentClient CommentClient
        {
            get { return this.commentClient; }
        }

        /// <summary>
        /// 放送に接続されているか調べます。
        /// </summary>
        //[DependOnProperty(typeof(CommentClient), "IsConnected")]
        public bool IsConnectedToLive
        {
            get { return this.commentClient.IsConnected; }
        }

        /// <summary>
        /// 手番を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Board), "Turn")]
        public BWType Turn
        {
            get { return this.board.Turn; }
            set { this.board.Turn = value; }
        }

        /// <summary>
        /// 編集モードを取得または設定します。
        /// </summary>
        public EditMode EditMode
        {
            get { return GetValue<EditMode>("EditMode"); }
            set { SetValue("EditMode", value); }
        }

        /// <summary>
        /// 手を戻すことができるか取得します。
        /// </summary>
        [DependOnProperty(typeof(Board), "CanUndo")]
        public bool CanUndo
        {
            get
            {
                return (
                    EditMode != EditMode.NoEdit &&
                    VariationState == VariationState.None &&
                    this.board.CanUndo);
            }
        }

        /// <summary>
        /// 手を進めることができるか取得します。
        /// </summary>
        [DependOnProperty(typeof(Board), "CanRedo")]
        public bool CanRedo
        {
            get
            {
                return (
                    EditMode != EditMode.NoEdit &&
                    VariationState == VariationState.None &&
                    this.board.CanRedo);
            }
        }

        /// <summary>
        /// 変化が再生中かどうかを取得します。
        /// </summary>
        public VariationState VariationState
        {
            get { return GetValue<VariationState>("VariationState"); }
            set { SetValue("VariationState", value); }
        }

        /// <summary>
        /// 変化再生時の色を取得または設定します。
        /// </summary>
        public double VariationBorderOpacity
        {
            get { return GetValue<double>("VariationBorderOpacity"); }
            set { SetValue("VariationBorderOpacity", value); }
        }

        #region 見た目系
        /// <summary>
        /// 盤などの画像読み込みを行います。
        /// </summary>
        private BitmapImage CreateImage<T>(T value)
            where T : struct
        {
            try
            {
                var imageUri = ImageUtil.GetImageUri(value);
                if (imageUri == null)
                {
                    return null;
                }

                var image = new BitmapImage(imageUri);
                image.Freeze();

                return image;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "{0}: 画像の読み込みに失敗しました。", value);

                return null;
            }
        }

        /// <summary>
        /// 駒画像を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_KomaImage")]
        public BitmapImage KomaImage
        {
            get { return CreateImage(ShogiGlobal.Settings.SD_KomaImage); }
        }

        /// <summary>
        /// 盤画像を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BanImage")]
        public BitmapImage BanImage
        {
            get { return CreateImage(ShogiGlobal.Settings.SD_BanImage); }
        }

        /// <summary>
        /// 駒台画像を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_KomadaiImage")]
        public BitmapImage KomadaiImage
        {
            get { return CreateImage(ShogiGlobal.Settings.SD_KomadaiImage); }
        }

        /// <summary>
        /// 盤の不透明度を取得します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "SD_BanOpacity")]
        public double BanOpacity
        {
            get { return ShogiGlobal.Settings.SD_BanOpacity; }
        }
        #endregion

        /// <summary>
        /// 現局面からの指し手を取得します。
        /// </summary>
        private List<Move> BuildMoveListFromCurrentBoard()
        {
            var tmpBoard = Board.Clone();

            // 局面の手数を現局面まで戻して、局面が一致するか確認します。
            while (tmpBoard.MoveCount > CurrentBoard.MoveCount)
            {
                tmpBoard.Undo();
            }

            if (tmpBoard.MoveCount != CurrentBoard.MoveCount ||
                !tmpBoard.BoardEquals(CurrentBoard))
            {
                return null;
            }

            // 表示されている局面を戻すと現局面と一致するので、
            // 現局面からの指し手を文字列に直します。
            return Board.MakeMoveList(CurrentBoard.MoveCount, false);
        }

        /// <summary>
        /// 現局面からの指し手を更新します。
        /// </summary>
        private void UpdateMoveTextFromMoveList(IEnumerable<Move> moveList)
        {
            if (moveList == null)
            {
                MoveFromCurrentBoard = new ReadOnlyCollection<Move>(new Move[0]);
                MoveTextFromCurrentBoard = null;
                return;
            }

            // 表示されている局面を戻すと現局面と一致するので、
            // 現局面からの指し手を文字列に直します。
            MoveFromCurrentBoard = new ReadOnlyCollection<Move>(moveList.ToList());

            MoveTextFromCurrentBoard = string.Join("",
                moveList.Select(move =>
                    Stringizer.ToString(move, MoveTextStyle.Simple)));
        }

        /// <summary>
        /// 現局面からの指し手を更新します。
        /// </summary>
        private void UpdateMoveTextFromCurrentBoard()
        {
            var moveList = BuildMoveListFromCurrentBoard();

            UpdateMoveTextFromMoveList(moveList);
        }

        /// <summary>
        /// 盤面が変更される前に必要なら局面のコピーを行います。
        /// </summary>
        private void CloneOnWriteBoard()
        {
            // 現局面と表示されている局面が同一オブジェクトなら、
            // 表示用の盤面をコピーし、現局面に表示局面の
            // 変更の影響がでないようにします。
            if (ReferenceEquals(Board, CurrentBoard))
            {
                Board = CurrentBoard.Clone();
            }
        }

        /// <summary>
        /// 局面の更新を行います。
        /// </summary>
        /// <remarks>
        /// クラス外から局面を設定するときは、このメソッドを使ってください。
        /// </remarks>
        public void SetBoard(Board board)
        {
            using (LazyLock())
            {
                if (board == null)
                {
                    return;
                }

                // クラス外から局面が設定されたときは、
                // 自動再生用の変化をすべて消去します。
                ClearAutoPlay();
                StopAutoPlay();

                Board = board;
                CloneOnWriteBoard();
            }

            WPFUtil.InvalidateCommand();
        }

        /// <summary>
        /// 現局面の更新を行います。
        /// </summary>
        /// <remarks>
        /// クラス外から局面を設定するときは、このメソッドを使ってください。
        /// </remarks>
        public void SetCurrentBoard(Board currentBoard)
        {
            using (LazyLock())
            {
                if (currentBoard != null)
                {
                    CurrentBoard = currentBoard;

                    CloneOnWriteBoard();
                }
            }

            WPFUtil.InvalidateCommand();
        }

        /// <summary>
        /// 変化を追加し、可能なら再生します。
        /// </summary>
        public bool AddVariation(IEnumerable<Move> moveList,
                                 string comment, bool isAutoPlay)
        {
            if (moveList == null)
            {
                return false;
            }

            // ここで変化を作成。
            var variation = Variation.Create(
                this.currentBoard,
                moveList, comment, true);
            if (variation == null)
            {
                return false;
            }

            return AddVariation(variation, true, isAutoPlay);
        }

        /// <summary>
        /// 変化を追加し、可能なら再生します。
        /// </summary>
        public bool AddVariation(Variation variation, bool addVariation,
                                 bool isAutoPlay)
        {
            if (variation == null)
            {
                return false;
            }

            // 将棋コントロールに変化を追加します。
            if (addVariation)
            {
                this.moveManager.AddVariation(variation);
            }

            // 自動再生用に変化を追加します。
            if (isAutoPlay)
            {
                var autoPlay = new AutoPlayEx(variation)
                {
                    EffectManager = ShogiGlobal.EffectManager,
                    IsChangeBackground = true,
                    IsConfirmPlay = true,
                    IsUseCutIn = Settings.HasEffectFlag(Effects.EffectFlag.AutoPlayCutIn),
                    Interval = Settings.SD_AutoPlayInterval,
                };

                StartAutoPlay(autoPlay);
            }

            return true;
        }

        /// <summary>
        /// 変化の自動再生を開始します。
        /// </summary>
        public void StartAutoPlay(AutoPlayEx autoPlay)
        {
            if (autoPlay == null || !autoPlay.Validate())
            {
                return;
            }

            // ウィンドウが非表示なら変化を表示しません。
            if (ShogiGlobal.MainWindow == null)
            {
                return;
            }

            this.autoPlayList.Enqueue(autoPlay);
            BeginAutoPlay();
        }

        /// <summary>
        /// 変化を再生するか確かめ、再生する場合はそれを返します。
        /// </summary>
        private AutoPlayEx GetNextAutoPlay()
        {
            // 再生中なら再生しません。
            if (VariationState == VariationState.Playing ||
                this.isCheckingAutoPlay)
            {
                return null;
            }

            try
            {
                // ここで変化の再生フラグを立てます。
                // 確認中に再度確認するのを防ぐためです。
                this.isCheckingAutoPlay = true;

                if (!this.autoPlayList.Any())
                {
                    return null;
                }

                // 次の再生用オブジェクトを取り出します。
                var autoPlay = this.autoPlayList.Dequeue();

                // ウィンドウが非表示なら変化を表示しません。
                if (ShogiGlobal.MainWindow == null)
                {
                    ClearAutoPlay();
                    return null;
                }

                // 投票残り時間が短い場合は、自動再生を行いません。
                if (VoteClient.VoteState == VoteState.Voting &&
                    VoteClient.VoteLeaveTime < AutoPlayMinimumLeaveTime)
                {
                    ClearAutoPlay();
                    return null;
                }

                // 必要なら再生の許可を取ります。
                if (autoPlay.IsConfirmPlay)
                {
                    var dialog = DialogUtil.CreateDialog(
                        ShogiGlobal.MainWindow,
                        autoPlay.ConfirmMessage,
                        "再生確認",
                        MessageBoxButton.YesNo,
                        MessageBoxResult.No);
                    dialog.ShowDialog();
                    if (dialog.ResultButton != MessageBoxResult.Yes)
                    {
                        this.isCheckingAutoPlay = false;
                        return GetNextAutoPlay();
                    }
                }

                return autoPlay;
            }
            finally
            {
                this.isCheckingAutoPlay = false;
            }
        }

        /// <summary>
        /// 変化を再生します。
        /// </summary>
        private void BeginAutoPlay()
        {
            var shogi = ShogiGlobal.ShogiControl;
            if (shogi == null)
            {
                return;
            }

            var autoPlay = GetNextAutoPlay();
            if (autoPlay == null)
            {
                return;
            }

            autoPlay.Stopped += (_, __) => StopAutoPlay();
            this.currentAutoPlay = autoPlay;

            VariationState = VariationState.Playing;
            Board = autoPlay.Board;

            // 変化停止時の処理
            WPFUtil.InvalidateCommand();

            shogi.StartAutoPlay(autoPlay);            
        }

        /// <summary>
        /// 自動再生が終わる時/終わらせる時に呼びます。
        /// </summary>
        public void StopAutoPlay()
        {
            if (this.currentAutoPlay == null)
            {
                return;
            }

            this.currentAutoPlay = null;
            VariationState = VariationState.None;

            // 変化停止時の処理
            WPFUtil.InvalidateCommand();

            // もし次の変化があればそれも表示します。
            BeginAutoPlay();
        }

        /// <summary>
        /// 自動再生リストを空にし、自動再生をすべて停止します。
        /// </summary>
        public void ClearAutoPlay()
        {
            this.autoPlayList.Clear();
        }

        void board_BoardChanged(object sender, BoardChangedEventArgs e)
        {
            var moveList = BuildMoveListFromCurrentBoard();
            
            if (moveList != null)
            {
                // 動かした駒を変化として登録します。
                AddVariation(moveList, string.Empty, false);

                UpdateMoveTextFromCurrentBoard();
            }

            WPFUtil.InvalidateCommand();
        }

        void commentClient_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsConnected")
            {
                this.RaisePropertyChanged("IsConnectedToLive");
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ShogiWindowViewModel(Settings settings, GlobalModel globalModel, Board board)
        {
            this.commentCandidates = new NotifyCollection<string>(
                new []
                {
                    "こんな感じ？",
                    "どやあああ",
                    "う～ん",
                });

            Settings = settings;
            GlobalModel = globalModel;

            VariationState = VariationState.None;

            CurrentBoard = board.Clone();
            Board = board.Clone();

            this.commentClient = VoteSystem.Client.Global.CreateCommentClient();
            this.commentClient.IsSupressLog = true;
            this.commentClient.PropertyChanged += commentClient_PropertyChanged;

            this.moveManager.SetCurrentBoard(this.currentBoard);

            AddPropertyChangedHandler(
                "CurrentBoard",
                (_, __) =>
                {
                    // 現局面を更新します。
                    this.moveManager.SetCurrentBoard(CurrentBoard);
                    UpdateMoveTextFromCurrentBoard();
                });
            AddPropertyChangedHandler(
                "Board",
                (_, __) => UpdateMoveTextFromCurrentBoard());

            this.AddDependModel(settings);
            this.AddDependModel(globalModel);
        }
    }
}
