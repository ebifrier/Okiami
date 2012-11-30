using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

using Ragnarok;
using Ragnarok.ObjectModel;

namespace VoteSystem.Client.ViewModel
{
    using Protocol.Vote;
    using VoteSystem.Client.Model;

    /// <summary>
    /// 投票結果などを表示するウィンドウのモデルデータです。
    /// </summary>
    public class VoteResultWindowViewModel : DynamicViewModel
    {
        private readonly VoteClient voteClient;
        private Color movableBackgroundColor =
            Color.FromArgb(128, 0, 8, 80);

        #region 見た目用のプロパティ
        /// <summary>
        /// 移動時のウィンドウ色を取得または設定します。
        /// </summary>
        public Color VR_MovableBackgroundColor
        {
            get
            {
                return this.movableBackgroundColor;
            }
            set
            {
                if (this.movableBackgroundColor != value)
                {
                    this.movableBackgroundColor = value;

                    this.RaisePropertyChanged("MovableBackgroundColor");
                }
            }
        }

        /// <summary>
        /// フォントファミリを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "VR_FontFamilyName")]
        public FontFamily VR_FontFamily
        {
            get
            {
                return new FontFamily(Global.Settings.VR_FontFamilyName);
            }
        }

        /// <summary>
        /// 太字かどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "VR_FontWeight")]
        public bool VR_IsBold
        {
            get
            {
                return (Global.Settings.VR_FontWeight >= FontWeights.Bold);
            }
            set
            {
                Global.Settings.VR_FontWeight =
                    (value ? FontWeights.Bold : FontWeights.Regular);
            }
        }

        /// <summary>
        /// 斜体かどうかを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "VR_FontStyle")]
        public bool VR_IsItalic
        {
            get
            {
                return (Global.Settings.VR_FontStyle == FontStyles.Oblique);
            }
            set
            {
                Global.Settings.VR_FontStyle =
                    (value ? FontStyles.Oblique : FontStyles.Normal);
            }
        }

        /// <summary>
        /// フォントの縁の太さを取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "VR_IsEdged")]
        [DependOnProperty(typeof(Settings), "VR_FontEdgeLengthInternal")]
        public decimal VR_FontEdgeLength
        {
            get
            {
                return (Global.Settings.VR_IsEdged ?
                    Global.Settings.VR_FontEdgeLengthInternal : 0);
            }
            set
            {
                Global.Settings.VR_FontEdgeLengthInternal = value;
            }
        }
        #endregion

        /// <summary>
        /// 時間延長要求ポイントを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteResult")]
        public int TimeExtendPoint
        {
            get
            {
                return this.voteClient.VoteResult.TimeExtendPoint;
            }
        }

        /// <summary>
        /// 時間延長拒否ポイントを取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteResult")]
        public int TimeStablePoint
        {
            get
            {
                return this.voteClient.VoteResult.TimeStablePoint;
            }
        }

        /// <summary>
        /// 評価値を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteResult")]
        public double EvaluationPoint
        {
            get
            {
                return this.voteClient.VoteResult.EvaluationPoint;
            }
        }

        /// <summary>
        /// 表示用の投票結果の表示数を取得または設定します。
        /// </summary>
        [DependOnProperty(typeof(Settings), "VR_DisplayResultCount")]
        public int DisplayResultCount
        {
            get { return Global.Settings.VR_DisplayResultCount; }
            set { Global.Settings.VR_DisplayResultCount = value; }
        }

        /// <summary>
        /// 表示用の投票結果を取得します。
        /// </summary>
        [DependOnProperty(typeof(VoteClient), "VoteResult")]
        [DependOnProperty("DisplayResultCount")]
        public IEnumerable<VoteCandidatePair> DisplayResult
        {
            get
            {
                var candidateList = this.voteClient.VoteResult
                    .CandidateList
                    .ToList();
                var count = Global.Settings.VR_DisplayResultCount;

                if (candidateList.Count >= count)
                {
                    // 候補が指定の数より多い場合は、多い分を切り取ります。
                    return candidateList.Take(count);
                }
                else
                {
                    // 候補の数が不足している場合は、ダミーの結果を末尾に追加します。
                    var shortCount = count - candidateList.Count;

                    candidateList.AddRange(
                        from i in Enumerable.Range(0, shortCount)
                        select new VoteCandidatePair()
                        {
                            Candidate = null,
                            Point = 0
                        });

                    return candidateList;
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public VoteResultWindowViewModel(VoteClient voteClient)
        {
            this.voteClient = voteClient;

            this.AddDependModel(voteClient);
            this.AddDependModel(Global.MainViewModel);
            this.AddDependModel(Global.Settings);
        }
    }
}
