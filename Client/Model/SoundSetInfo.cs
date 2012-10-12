using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace VoteSystem.Client.Model
{
    /// <summary>
    /// ＳＥ用の情報を保持します。
    /// </summary>
    [DataContract()]
    public class SoundSetInfo : InfoBase
    {
        /// <summary>
        /// ＳＥ情報が書かれた情報ファイルを読み込みます。
        /// </summary>
        public static SoundSetInfo Read(string filepath)
        {
            return ReadInternal<SoundSetInfo>(filepath);
        }
    }
}
