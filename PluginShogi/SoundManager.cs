using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Ragnarok;

namespace VoteSystem.PluginShogi
{
    /// <summary>
    /// 音声ファイルを再生します。
    /// </summary>
    [CLSCompliant(false)]
    public class SoundManager : Ragnarok.Extra.Sound.SoundManager
    {
        /// <summary>
        /// 音声プレイヤーオブジェクトを初期化します。
        /// </summary>
        public SoundManager()
        {
            try
            {
                PlayInterval = TimeSpan.FromSeconds(0.5);

                DefaultPath = Path.Combine(
                    AssemblyLocation, "ShogiData");

                Volume = 50;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "サウンドオブジェクトの初期化に失敗しました。");
            }
        }
    }
}
