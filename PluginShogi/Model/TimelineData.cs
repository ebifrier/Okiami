using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Ragnarok;

namespace VoteSystem.PluginShogi.Model
{
    /// <summary>
    /// 時間管理をするためのクラスです。
    /// </summary>
    public sealed class TimelineData
    {
        /// <summary>
        /// フェードインが始まる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeInStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// フェードインを行う時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeInSpan
        {
            get;
            set;
        }

        /// <summary>
        /// フェードインが終わる時間を取得します。
        /// </summary>
        public TimeSpan FadeInEndTime
        {
            get { return (FadeInStartTime + FadeInSpan); }
        }

        /// <summary>
        /// フェードアウトが始まる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeOutStartTime
        {
            get;
            set;
        }

        /// <summary>
        /// フェードアウトが終わる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeOutSpan
        {
            get;
            set;
        }

        /// <summary>
        /// フェードアウトが終わる時間を取得または設定します。
        /// </summary>
        public TimeSpan FadeOutEndTime
        {
            get { return (FadeOutStartTime + FadeOutSpan); }
        }

        /// <summary>
        /// フェードイン終了後からフェードアウト開始までの時間を取得します。
        /// </summary>
        public TimeSpan VisibleSpan
        {
            get { return (FadeOutStartTime - FadeInEndTime); }
        }

        /// <summary>
        /// フェードイン開始からフェードアウト終了後までの時間を取得します。
        /// </summary>
        public TimeSpan FullVisibleSpan
        {
            get { return (FadeOutEndTime - FadeInStartTime); }
        }

        /// <summary>
        /// フェードイン・フェードアウトなどの、進行度を取得します。
        /// </summary>
        public double GetRatio(TimeSpan position)
        {
            if (position < FadeInStartTime)
            {
                // フェードイン前なら進行度は０
                return 0.0;
            }
            else if (position < FadeInEndTime)
            {
                var current = FadeInEndTime - position;
                var r = current.TotalSeconds / FadeInSpan.TotalSeconds;

                return MathEx.InterpLiner(1.0, 0.0, r);
            }
            else if (position < FadeOutStartTime)
            {
                // 表示中は進行度は１
                return 1.0;
            }
            else if (position < FadeOutEndTime)
            {
                var current = FadeOutEndTime - position;
                var r = current.TotalSeconds / FadeOutSpan.TotalSeconds;

                return MathEx.InterpLiner(0.0, 1.0, r);
            }

            // フェードアウト後なら進行度は０
            return 0.0;
        }
    }
}
