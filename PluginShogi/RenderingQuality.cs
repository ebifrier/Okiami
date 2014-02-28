using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

using Ragnarok;
using Ragnarok.Utility;

namespace VoteSystem.PluginShogi
{
    /// <summary>
    /// レンダリングの品質を決めます。
    /// </summary>
    public enum RenderingQuality
    {
        /// <summary>
        /// 最高品質です。
        /// </summary>
        [LabelDescription(Label = "映像重視")]
        Best,
        /// <summary>
        /// 通常品質です。
        /// </summary>
        [LabelDescription(Label = "バランス")]
        Normal,
        /// <summary>
        /// 処理が重い場合に使います。
        /// </summary>
        [LabelDescription(Label = "速度重視")]
        Poor,
    }

    /// <summary>
    /// エンドロールの品質です。
    /// </summary>
    public enum EndRollQuality
    {
        /// <summary>
        /// 最高品質です。
        /// </summary>
        [LabelDescription(Label = "最高品質")]
        Best,
        /// <summary>
        /// 映像重視の品質です。
        /// </summary>
        [LabelDescription(Label = "映像重視")]
        High,
        /// <summary>
        /// 通常の品質です。
        /// </summary>
        [LabelDescription(Label = "バランス")]
        Normal,
        /// <summary>
        /// 速度重視の設定です。
        /// </summary>
        [LabelDescription(Label = "速度重視")]
        Poor,
    }

    /// <summary>
    /// 描画品質に関わる情報を扱います。
    /// </summary>
    public static class RenderingQualityUtil
    {
        /// <summary>
        /// 描画品質に対応するFPSを取得します。
        /// </summary>
        public static double GetFPS(RenderingQuality q)
        {
            switch (q)
            {
                case RenderingQuality.Best:
                    return 60.0;
                case RenderingQuality.Normal:
                    return 30.0;
                case RenderingQuality.Poor:
                    return 20.0;
            }

            return 30.0;
        }
    }

    /// <summary>
    /// エンディングの品質に関わる情報を扱います。
    /// </summary>
    public static class EndRollQualityUtil
    {
        /// <summary>
        /// エンディングの品質に対応するFPSを取得します。
        /// </summary>
        public static double GetFPS(EndRollQuality q)
        {
            switch (q)
            {
                case EndRollQuality.Best:
                    return 240.0;
                case EndRollQuality.High:
                    return 60.0;
                case EndRollQuality.Normal:
                    return 30.0;
                case EndRollQuality.Poor:
                    return 20.0;
            }

            return 30.0;
        }
    }
}
