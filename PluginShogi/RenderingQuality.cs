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
        [LabelDescription(Label = "最高品質")]
        Best,
        
        /// <summary>
        /// 通常品質です。
        /// </summary>
        [LabelDescription(Label = "通常品質")]
        Normal,

        /// <summary>
        /// 処理が重い場合に使います。
        /// </summary>
        [LabelDescription(Label = "低品質")]
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
}
