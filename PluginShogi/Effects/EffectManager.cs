using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

using Ragnarok;
using Ragnarok.Shogi;
using Ragnarok.ObjectModel;
using Ragnarok.Presentation;
using Ragnarok.Presentation.Extra.Effect;
using Ragnarok.Presentation.Extra.Element;
using Ragnarok.Presentation.Shogi;
using Ragnarok.Presentation.Shogi.View;

namespace VoteSystem.PluginShogi.Effects
{
    using View;
    using Model;

    /// <summary>
    /// エフェクトの使用フラグです。
    /// </summary>
    [Flags()]
    public enum EffectFlag
    {
        /// <summary>
        /// すべて無効。
        /// </summary>
        None = 0,

        /// <summary>
        /// 一手前に動かした駒を強調表示します。
        /// </summary>
        PrevCell = (1 << 0),
        /// <summary>
        /// 動かせるマスを強調表示します。
        /// </summary>
        MovableCell = (1 << 1),
        /// <summary>
        /// 手番側を強調表示します。
        /// </summary>
        Teban = (1 << 2),

        /// <summary>
        /// 背景エフェクトを使用します。
        /// </summary>
        Background = (1 << 8),
        /// <summary>
        /// 駒に関するエフェクトを使用します。
        /// </summary>
        Piece = (1 << 9),
        /// <summary>
        /// 囲いエフェクトを使用します。
        /// </summary>
        Castle = (1 << 10),
        /// <summary>
        /// 投票エフェクトを使用します。
        /// </summary>
        Vote = (1 << 11),
        /// <summary>
        /// 自動再生の専用エフェクトを使用します。
        /// </summary>
        AutoPlay = (1 << 12),
        /// <summary>
        /// 自動再生のカットインエフェクトを使用します。
        /// </summary>
        AutoPlayCutIn = (1 << 13),
        /// <summary>
        /// 簡易背景を使用します。
        /// </summary>
        SimpleBackground = (1 << 14),

        /// <summary>
        /// 全フラグ
        /// </summary>
        All = (PrevCell | MovableCell | Teban |
               Background | SimpleBackground | Piece | Castle | Vote |
               AutoPlay | AutoPlayCutIn),
    }

    /// <summary>
    /// エフェクトの管理を行います。
    /// </summary>
    public sealed class EffectManager : NotifyObject, IEffectManager
    {
        /// <summary>
        /// 投票エフェクトの最短表示インターバルです。
        /// </summary>
        private readonly TimeSpan VoteInterval = TimeSpan.FromMilliseconds(100);

        private HashSet<string> castleEffectedBag = new HashSet<string>();
        private int moveCount;
        private EffectObject prevMovedCell;
        private EffectObject movableCell;
        private EffectObject tebanCell;
        private Stopwatch votedTimer = new Stopwatch();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public EffectManager()
        {
            this.votedTimer.Start();
        }

        /// <summary>
        /// エフェクトを表示するオブジェクトを取得または設定します。
        /// </summary>
        public ShogiUIElement3D Container
        {
            get;
            set;
        }

        /// <summary>
        /// 背景エフェクトを表示するコントロールを取得または設定します。
        /// </summary>
        public BackgroundUIElement3D Background
        {
            get;
            set;
        }

        /// <summary>
        /// 設定オブジェクトを取得します。
        /// </summary>
        private Settings Settings
        {
            get { return ShogiGlobal.Settings; }
        }

        /// <summary>
        /// マスの横幅を取得します。
        /// </summary>
        private double CellWidth
        {
            get { return Container.CellSize.Width; }
        }

        /// <summary>
        /// エフェクトを有効にするかどうかを取得または設定します。
        /// </summary>
        public bool EffectEnabled
        {
            get { return GetValue<bool>("EffectEnabled"); }
            set { SetValue("EffectEnabled", value); }
        }

        /// <summary>
        /// エフェクトフラグを取得または設定します。
        /// </summary>
        public EffectFlag EffectFlag
        {
            get { return GetValue<EffectFlag>("EffectFlag"); }
            set { SetValue("EffectFlag", value); }
        }

        /// <summary>
        /// エフェクトＳＥを使用するかどうかを取得または設定します。
        /// </summary>
        public bool IsUseEffectSound
        {
            get { return GetValue<bool>("IsUseEffectSound"); }
            set { SetValue("IsUseEffectSound", value); }
        }

        /// <summary>
        /// エフェクト音量を取得または設定します。
        /// </summary>
        public int EffectVolume
        {
            get { return GetValue<int>("EffectVolume"); }
            set { SetValue("EffectVolume", value); }
        }

        /// <summary>
        /// 自動再生エフェクトを有効にするかどうかを取得または設定します。
        /// </summary>
        public bool IsAutoPlayEffect
        {
            get { return GetValue<bool>("IsAutoPlayEffect"); }
            set { SetValue("IsAutoPlayEffect", value); }
        }

        /// <summary>
        /// エフェクト用の指し手カウントを取得または設定します。
        /// </summary>
        public int EffectMoveCount
        {
            get { return GetValue<int>("EffectMoveCount"); }
            set { SetValue("EffectMoveCount", value); }
        }

        /// <summary>
        /// エフェクトの個別の使用フラグを取得します。
        /// </summary>
        private bool HasEffectFlag(EffectFlag flag)
        {
            if (Settings == null)
            {
                return false;
            }

            return Settings.HasEffectFlag(flag);
        }

        /// <summary>
        /// 最後に表示するエフェクトを表示します。
        /// </summary>
        public void SetLastEffect()
        {
            using (LazyLock())
            {
                if (EffectMoveCount > 4)
                {
                    EffectMoveCount = 1000;
                }
            }
        }

        /// <summary>
        /// 音量の調節を行います。
        /// </summary>
        private void AdjustVolume(EffectObject effect)
        {
            var percent = (Settings.SD_IsUseEffectSound ?
                Settings.SD_EffectVolume : 0);
            var rate = MathEx.Between(0, 100, percent) / 100.0;

            effect.MultiplyStartVolume(rate);
        }

        #region データコンテキスト
        /// <summary>
        /// 通常のデータコンテキストを作成します。
        /// </summary>
        private EffectContext CreateContext(
            Position position,
            double z = ShogiUIElement3D.EffectZ)
        {
            var p = Container.GetPiecePos(position);
            var s = Container.CellSize;

            return new EffectContext()
            {
                Coord = new Vector3D(p.X, p.Y, z),
                BaseScale = new Vector3D(s.Width, s.Height, 1.0),
            };
        }

        /// <summary>
        /// セルエフェクトのコンテキストを作成します。
        /// </summary>
        private CellEffectContext CreateCellContext(
            Position position,
            Position cellPosition = null,
            double z = ShogiUIElement3D.BanEffectZ)
        {
            return CreateCellContext(new[] { position }, cellPosition, z);
        }

        /// <summary>
        /// セルエフェクトのコンテキストを作成します。
        /// </summary>
        private CellEffectContext CreateCellContext(
            IEnumerable<Position> positions,
            Position cellPosition = null,
            double z = ShogiUIElement3D.BanEffectZ)
        {
            var bp = Container.BanBounds.TopLeft;
            var bs = Container.BanBounds.Size;
            var s = Container.CellSize;

            var flipPosition =
                (cellPosition == null || Container.ViewSide == BWType.Black
                ? cellPosition
                : cellPosition.Flip());
            var flipPositions =
                (positions == null || Container.ViewSide == BWType.Black
                ? positions
                : positions.Where(_ => _ != null).Select(_ => _.Flip()));

            return new CellEffectContext()
            {
                CellPosition = flipPosition,
                CellPositions = flipPositions.ToArray(),
                BanCoord = new Vector3D(bp.X, bp.Y, z),
                BanScale = new Vector3D(bs.Width, bs.Height, 1.0),
                BaseScale = new Vector3D(s.Width, s.Height, 1.0),
            };
        }
        #endregion

        #region マスエフェクト関係
        /// <summary>
        /// 前回動かした駒の位置を設定します。
        /// </summary>
        private void UpdatePrevMovedCell()
        {
            if (this.prevMovedCell != null)
            {
                Container.RemoveBanEffect(this.prevMovedCell);
                this.prevMovedCell = null;
            }

            if (!HasEffectFlag(EffectFlag.PrevCell))
            {
                return;
            }

            var board = (Container != null ? Container.Board : null);
            if (board != null && board.LastMove != null)
            {
                var position = board.LastMove.NewPosition;

                var cell = EffectTable.PrevMovedCell.LoadEffect();
                if (cell != null)
                {
                    cell.DataContext = CreateCellContext(position);

                    Container.AddBanEffect(cell);
                    this.prevMovedCell = cell;
                }
            }
        }

        /// <summary>
        /// 駒を動かせる位置を光らせます。
        /// </summary>
        private void UpdateMovableCell(Position position, BoardPiece piece)
        {
            if (this.movableCell != null)
            {
                Container.RemoveBanEffect(this.movableCell);
                this.movableCell = null;
            }

            if (!HasEffectFlag(EffectFlag.MovableCell))
            {
                return;
            }

            var board = Container.Board;
            if (board == null || piece == null)
            {
                return;
            }

            // 移動可能もしくは駒打ち可能な全マスを取得します。
            var isMove = (position != null);
            var movePositions =
                from file in Enumerable.Range(1, Board.BoardSize)
                from rank in Enumerable.Range(1, Board.BoardSize)
                let move = new BoardMove()
                {
                    OldPosition = position,
                    NewPosition = new Position(file, rank),
                    BWType = piece.BWType,
                    ActionType = (isMove ? ActionType.None : ActionType.Drop),
                    DropPieceType = (isMove ? PieceType.None : piece.PieceType),
                }
                where board.CanMove(move)
                select move.NewPosition;

            // 移動可能なマスにエフェクトをかけます。
            var movableCell = EffectTable.MovableCell.LoadEffect();
            if (movableCell != null)
            {
                movableCell.DataContext = CreateCellContext(movePositions, position);

                Container.AddBanEffect(movableCell);
                this.movableCell = movableCell;
            }
        }

        /// <summary>
        /// 手番の表示を行います。
        /// </summary>
        private void UpdateTeban(BWType teban)
        {
            if (this.tebanCell != null)
            {
                Container.RemoveBanEffect(this.tebanCell);
                this.tebanCell = null;
            }

            if (!HasEffectFlag(EffectFlag.Teban))
            {
                return;
            }

            // 手番なしの時はオブジェクトを消去して帰ります。
            if (teban == BWType.None)
            {
                return;
            }

            // 移動可能なマスにエフェクトをかけます。
            var tebanCell = EffectTable.Teban.LoadEffect();
            if (tebanCell != null)
            {
                if (Container.ViewSide != BWType.Black)
                {
                    teban = teban.Toggle();
                }

                tebanCell.DataContext = CreateCellContext((Position)null)
                    .Apply(_ => _.BWType = teban);

                Container.AddBanEffect(tebanCell);
                this.tebanCell = tebanCell;
            }
        }
        #endregion

        #region 囲いエフェクト
        private static Position ViewFlip(Position position, BWType side)
        {
            return (side == BWType.Black ? position : position.Flip());
        }

        /// <summary>
        /// 囲いエフェクトを追加します。
        /// </summary>
        private void AddCastleEffect(CastleInfo castle, Position position, BWType side)
        {
            var p = Container.GetPiecePos(position);

            // 字形を描くパーティクルの放出角度
            var angle = (position.File < 5 ? 0.0 : 180.0);

            // 囲い画像の名前
            var imageName = (string.IsNullOrEmpty(castle.ImageName) ?
                "Other.png" :
                castle.ImageName);

            var dic = new Dictionary<string, object>
            {
                {"CastleName", castle.Name},
                {"CastleNameLen", castle.Name.Length},
                {"CastleId", castle.Id},
                {"CastleImage", imageName},
                {"StartAngle", angle},
                {"StartXY", new Vector(p.X, p.Y)},
            };

            var effect = EffectTable.Castle.LoadEffect(dic);
            if (effect != null)
            {
                effect.DataContext = CreateCellContext(
                    castle.PieceList.Select(_ => ViewFlip(_.Position, side)),
                    position,
                    ShogiUIElement3D.EffectZ);

                // 囲いエフェクト中は駒の移動を停止します。
                var model = ShogiGlobal.ShogiModel;
                var oldEditMode = model.EditMode;
                effect.Terminated += (_, __) => model.EditMode = oldEditMode;
                model.EditMode = EditMode.NoEdit;

                AdjustVolume(effect);
                Container.AddEffect(effect);
            }
        }

        /// <summary>
        /// 必要なら囲いエフェクトを出します。
        /// </summary>
        private bool AddCastleEffect(BoardMove move)
        {
            if (!HasEffectFlag(EffectFlag.Castle))
            {
                return false;
            }

            var castle = CastleInfo
                .Detect(Container.Board, move.BWType, move.NewPosition)
                .Where(_ => !this.castleEffectedBag.Contains(move.BWType + _.Name))
                .FirstOrDefault();

            if (castle != null)
            {
                AddCastleEffect(castle, move.NewPosition, move.BWType);

                this.castleEffectedBag.Add(move.BWType + castle.Name);
                foreach (var name in castle.BaseCastleList)
                {
                    this.castleEffectedBag.Add(move.BWType + name);
                }
            }

            return (castle != null);
        }
        #endregion

        #region 背景エフェクト
        private static Dictionary<string, List<string>> backgroundImageDic =
            new Dictionary<string, List<string>>();
        private static List<string> GetBackgroundImageList(string key)
        {
            List<string> list;

            if (backgroundImageDic.TryGetValue(key, out list))
            {
                return list;
            }

            try
            {
                string[] extensions = { ".png", ".jpg", ".tif", ".bmp" };
                var path = Path.Combine(
                    EffectInfo.BackgroundBaseDir.LocalPath, key);

                list = Directory
                    .EnumerateFiles(path, "Back*.*")
                    .Select(_ => Path.GetFileName(_))
                    .Where(_ => extensions.Contains(Path.GetExtension(_)))
                    .OrderBy(_ => _)
                    .ToList();

                backgroundImageDic.Add(key, list);
                return list;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "背景画像リストの取得に失敗しました。");

                return new List<string>();
            }
        }

        /// <summary>
        /// 背景ファイルの選択などを行います。
        /// </summary>
        private BackgroundContext CreateBackgroundContext(string key)
        {
            var list = GetBackgroundImageList(key);
            var imageFileName = list.FirstOrDefault();
            if (string.IsNullOrEmpty(imageFileName))
            {
                return null;
            }

            var context = new BackgroundContext();
            context.ImageUri = string.Format("../{0}/{1}", key, imageFileName);

            return context;
        }

        /// <summary>
        /// 背景選択に使うキーを取得します。
        /// </summary>
        private string GetBackgroundKey(int moveCount)
        {
            var Unit = 30;

            if (moveCount >= Unit * 3)
            {
                return "WinterEffect";
            }
            else if (moveCount >= Unit * 2)
            {
                return "AutumnEffect";
            }
            else if (moveCount >= Unit)
            {
                return "SummerEffect";
            }
            else
            {
                return "SpringEffect";
            }
        }

        /// <summary>
        /// 背景の設定を行います。
        /// </summary>
        private void TrySetBackground(string key)
        {
            if (Background == null)
            {
                return;
            }

            // SimpleBackgroundの使用時のみエフェクト名を切り替えます。
            var effectName = (
                HasEffectFlag(EffectFlag.SimpleBackground) ?
                "SimpleEffect" : key);

            // 背景エフェクトの作成。
            var effectInfo = new EffectInfo(effectName, null);
            var effect = effectInfo.LoadBackground();
            if (effect != null)
            {
                var context = CreateBackgroundContext(key);
                if (context != null)
                {
                    effect.Name += context.ImageUri;
                    effect.DataContext = context;
                }
            }

            // effectはnullのことがあります。
            Background.AddEntity(effect);
        }

        /// <summary>
        /// 背景エフェクトを更新します。
        /// </summary>
        public void UpdateBackground()
        {
            if (Container == null)
            {
                return;
            }

            WPFUtil.UIProcess(() =>
            {
                // 必要なら背景エフェクトを無効にします。
                if (!HasEffectFlag(EffectFlag.Background))
                {
                    TrySetBackground(null);
                    return;
                }

                var key = GetBackgroundKey(this.moveCount);
                TrySetBackground(key);
            });
        }

        /// <summary>
        /// 現局面の差し手が進んだときに呼ばれます。
        /// </summary>
        public void ChangeMoveCount(int moveCount)
        {
            this.moveCount = moveCount;

            UpdateBackground();
        }
        #endregion

        #region 変化エフェクト
        /// <summary>
        /// エフェクトの進み具合を計算します。
        /// </summary>
        private double MoveCountRate(int begin, int end)
        {
            var range = end - begin;

            return ((double)(Math.Min(EffectMoveCount, end) - begin) / range);
        }

        /// <summary>
        /// 変化エフェクトを追加します。
        /// </summary>
        private void AddVariationEffect(EffectInfo effectInfo, Position position,
                                        double rate)
        {
            var dic = new Dictionary<string, object>
            {
                {"Rate", rate},
            };

            var effect = effectInfo.LoadEffect(dic);
            if (effect == null)
            {
                return;
            }

            AdjustVolume(effect);
            WPFUtil.UIProcess(() =>
            {
                effect.DataContext = CreateContext(position);
                Container.AddEffect(effect);
            });
        }

        /// <summary>
        /// 変化エフェクトを表示します。
        /// </summary>
        private void VariationEffect(BoardMove move)
        {
            if (!IsAutoPlayEffect)
            {
                return;
            }

            if (!HasEffectFlag(EffectFlag.AutoPlay))
            {
                return;
            }

            if (EffectMoveCount >= 1000)
            {
                AddEffect(
                    EffectTable.VariationLast,
                    move.NewPosition);
            }
            else if (EffectMoveCount <= 3)
            {
                AddMoveEffect(
                    move.NewPosition,
                    move);
            }
            else if (EffectMoveCount <= 6)
            {
                AddVariationEffect(
                    EffectTable.VariationFirst,
                    move.NewPosition,
                    MoveCountRate(4, 6));
            }
            else
            {
                AddVariationEffect(
                    EffectTable.VariationSecond,
                    move.NewPosition,
                    MoveCountRate(7, 9));
            }

            // 指し手のカウンタを進めます。
            EffectMoveCount += 1;
        }
        #endregion

        #region 駒のエフェクト関係
        /// <summary>
        /// エフェクトを追加します。
        /// </summary>
        private void AddEffect(EffectObject effect, Position position)
        {
            if (effect == null)
            {
                return;
            }

            AdjustVolume(effect);
            WPFUtil.UIProcess(() =>
            {
                effect.DataContext = CreateContext(position);

                Container.AddEffect(effect);
            });
        }

        /// <summary>
        /// エフェクトを追加します。
        /// </summary>
        private void AddEffect(EffectInfo effectInfo, Position position)
        {
            var effect = effectInfo.LoadEffect();

            AddEffect(effect, position);
        }

        /// <summary>
        /// 手番によるパーティクルの色を取得します。
        /// </summary>
        private static Color TebanParticleColor(BWType bwType)
        {
            return (bwType == BWType.Black ? Colors.Red : Colors.Blue);
        }

        /// <summary>
        /// 駒を動かしたときのエフェクトです。
        /// </summary>
        private void AddMoveEffect(Position position, BoardMove move)
        {
            var table = new Dictionary<string, object>
            {
                { "Color",  TebanParticleColor(move.BWType) },
            };

            var effect = EffectTable.PieceMove.LoadEffect(table);
            AddEffect(effect, position);
        }

        /// <summary>
        /// 駒取りエフェクトです。
        /// </summary>
        private void AddTookEffect(Position position, BoardPiece tookPiece)
        {
            var bwType = tookPiece.BWType.Toggle();
            var bp = Container.GetPiecePos(position);
            var ep = Container.GetCapturedPiecePos(bwType, tookPiece.PieceType);
            var d = Vector3D.Subtract(ep, bp);
            var rad = Math.Atan2(d.Y, d.X) + Math.PI;

            var table = new Dictionary<string, object>
            {
                { "TargetXY",  new Vector(d.X, d.Y) },
                { "StartAngle", MathEx.ToDeg(rad) },
                { "Color",  TebanParticleColor(bwType) },
            };

            var effect = EffectTable.PieceTook.LoadEffect(table);
            AddEffect(effect, position);
        }
        #endregion

        #region オーバーライド
        /// <summary>
        /// 初期化時・オブジェクトの破棄時などに呼ばれます。
        /// </summary>
        public void Clear()
        {
            if (Container == null)
            {
                return;
            }

            UpdateTeban(BWType.None);
            UpdateMovableCell(null, null);
            UpdatePrevMovedCell();
            UpdateBackground();

            this.castleEffectedBag.Clear();
        }

        /// <summary>
        /// 局面更新時に呼ばれます。
        /// </summary>
        public void InitEffect(BWType bwType)
        {
            if (Container == null)
            {
                return;
            }

            UpdateTeban(bwType);
            UpdatePrevMovedCell();
            ((IEffectManager)this).EndMove();
        }

        /// <summary>
        /// 駒の移動を開始したときに呼ばれます。
        /// </summary>
        void IEffectManager.BeginMove(Position position, BoardPiece piece)
        {
            if (Container == null)
            {
                return;
            }

            UpdateMovableCell(position, piece);
        }

        /// <summary>
        /// 駒の移動が終わったときに呼ばれます。
        /// </summary>
        void IEffectManager.EndMove()
        {
            if (Container == null)
            {
                return;
            }

            UpdateMovableCell(null, null);
        }

        /// <summary>
        /// 玉の位置を取得します。
        /// </summary>
        private Position FindGyoku(Board board, BWType bwType)
        {
            var positions =
                from file in Enumerable.Range(1, Board.BoardSize)
                from rank in Enumerable.Range(1, Board.BoardSize)
                let piece = board[file, rank]
                where piece != null &&
                      piece.PieceType == PieceType.Gyoku &&
                      piece.BWType == bwType
                select new Position(file, rank);
            if (positions.Count() != 1)
            {
                return null;
            }

            return positions.FirstOrDefault();
        }

        /// <summary>
        /// 投票時のエフェクトを表示します。
        /// </summary>
        public void Voted(Move move)
        {
            if (Container == null)
            {
                return;
            }

            var board = Container.Board;
            if (board == null)
            {
                return;
            }

            if (!HasEffectFlag(EffectFlag.Vote))
            {
                return;
            }

            // 放送ログをまとめて読み込むと、
            // 投票エフェクトが大量に発生することがあります。
            if (this.votedTimer.Elapsed < VoteInterval)
            {
                return;
            }
            
            // 投了時は玉の位置にエフェクトをかけます。
            var position =
                ( move.IsResigned
                ? FindGyoku(board, board.Turn)
                : move.NewPosition);
            if (position == null)
            {
                return;
            }

            var effect = EffectTable.Vote.LoadEffect();
            if (effect != null)
            {
                AddEffect(effect, position);
            }

            this.votedTimer.Restart();
        }

        /// <summary>
        /// 投了します。
        /// </summary>
        public void Resign()
        {
            if (Container == null)
            {
                return;
            }

            var board = Container.Board;
            if (board == null)
            {
                return;
            }

            // 投了時は玉の位置にエフェクトをかけます。
            var position = FindGyoku(board, board.Turn);
            if (position == null)
            {
                return;
            }

            AddEffect(EffectTable.Win, position);
        }

        /// <summary>
        /// 変化エフェクトのカットインを表示します。
        /// </summary>
        public bool VariationCutIn()
        {
            if (Container == null)
            {
                return false;
            }

            if (!HasEffectFlag(EffectFlag.AutoPlayCutIn))
            {
                return false;
            }

            if (MathEx.RandInt(0, 2) == 0)
            {
                AddEffect(EffectTable.VariationCutIn1, null);
            }
            else
            {
                AddEffect(EffectTable.VariationCutIn2, null);
            }

            return true;
        }

        /// <summary>
        /// エフェクトを追加します。
        /// </summary>
        void IEffectManager.Moved(BoardMove move, bool isUndo)
        {
            if (Container == null)
            {
                return;
            }

            // 前回動かした駒を強調表示します。
            // これはエフェクト機能がオフの時でも表示します。
            UpdatePrevMovedCell();

            if (!EffectEnabled)
            {
                return;
            }

            // アンドゥ時
            if (isUndo)
            {
                if (move.OldPosition != null &&
                    HasEffectFlag(EffectFlag.Piece))
                {
                    AddMoveEffect(move.OldPosition, move);
                }

                UpdateTeban(move.BWType);
                return;
            }

            UpdateTeban(move.BWType.Toggle());

            if (IsAutoPlayEffect)
            {
                VariationEffect(move);
            }
            else
            {
                var castleAdded = AddCastleEffect(move);

                if (!castleAdded && HasEffectFlag(EffectFlag.Piece))
                {
                    if (move.TookPiece != null)
                    {
                        AddTookEffect(move.NewPosition, move.TookPiece);
                    }

                    if (move.ActionType == ActionType.Drop)
                    {
                        AddEffect(EffectTable.PieceDrop, move.NewPosition);
                    }
                    else if (move.ActionType == ActionType.Promote)
                    {
                        AddEffect(EffectTable.Promote, move.NewPosition);
                    }

                    AddMoveEffect(move.NewPosition, move);
                }
            }
        }
        #endregion
    }
}
