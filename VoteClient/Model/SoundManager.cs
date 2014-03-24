using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

using Ragnarok;

namespace VoteSystem.Client.Model
{
    using Protocol.Vote;
    using Protocol.Model;

    /// <summary>
    /// 音声ファイルを再生します。
    /// </summary>
    public sealed class SoundManager
    {
        private readonly Ragnarok.Extra.Sound.SoundManager manager;
        private List<SoundSetInfo> soundInfoList =
            new List<SoundSetInfo>();
        private SoundSetInfo selectedSoundSet;

        /// <summary>
        /// SE再生が可能かどうかを取得します。
        /// </summary>
        public bool CanUseSound
        {
            get
            {
                if (this.manager == null)
                {
                    return false;
                }
                
                return this.manager.CanUseSound;
            }
        }

        /// <summary>
        /// 音声ディレクトリの一覧を取得します。
        /// </summary>
        public List<SoundSetInfo> SoundInfoList
        {
            get
            {
                return this.soundInfoList;
            }
        }

        /// <summary>
        /// 現在選択中のＳＥセットを取得または設定します。
        /// </summary>
        public SoundSetInfo SelectedSoundSet
        {
            get
            {
                return this.selectedSoundSet;
            }
            set
            {
                if (this.selectedSoundSet != value)
                {
                    this.selectedSoundSet = value;

                    SoundPathUpdated();
                }
            }
        }

        /// <summary>
        /// 評価値が変化したときに呼ばれます。
        /// </summary>
        private void SoundPathUpdated()
        {
            if (SelectedSoundSet == null)
            {
                return;
            }

            // 選択された画像セットを保存します。
            Global.Settings.SoundSetDir = SelectedSoundSet.DirectoryName;
        }

        /// <summary>
        /// SE再生を行います。
        /// </summary>
        public void PlaySE(string filename)
        {
            if (this.manager == null)
            {
                return;
            }

            // 例外は投げません。
            this.manager.PlaySE(filename, 1.0, true, true);
        }

        /// <summary>
        /// 投票の開始/終了などのサウンドを再生します。
        /// </summary>
        public void PlayVoteSE(VoteState voteState)
        {
            if (!CanUseSound)
            {
                return;
            }

            switch (voteState)
            {
                case VoteState.Voting:
                    PlaySE("vote_start.wav");
                    break;
                case VoteState.Stop:
                    PlaySE("vote_stop.wav");
                    break;
                case VoteState.Pause:
                    PlaySE("vote_pause.wav");
                    break;
                case VoteState.End:
                    PlaySE("vote_end.wav");
                    break;
            }
        }

        /// <summary>
        /// 残り時間に合わせて秒読みなどのサウンドを再生します。
        /// </summary>
        public void PlayCountdownSE(int seconds)
        {
            if (!CanUseSound)
            {
                return;
            }

            if (seconds == 3 * 60)
            {
                PlaySE("nokori_3min.wav");
            }
            else if (seconds == 2 * 60)
            {
                PlaySE("nokori_2min.wav");
            }
            else if (seconds == 1 * 60)
            {
                PlaySE("nokori_1min.wav");
            }
            else if (seconds == 30)
            {
                PlaySE("30byou.wav");
            }
            else if (seconds == 20)
            {
                PlaySE("40byou.wav");
            }
            else if (seconds == 10)
            {
                PlaySE("50byou.wav");
            }
            else if (0 <= seconds && seconds < 10)
            {
                // 残り1秒なら9, 2秒なら8と読む。
                var filename = string.Format("{0}.wav", 10 - seconds);

                PlaySE(filename);
            }
        }

        /// <summary>
        /// サウンドディレクトリを初期化します。
        /// </summary>
        private void InitSoundDir()
        {
            try
            {
                // Data\Soundのサブディレクトリを
                // 音声ファイルの入ったディレクトリとして列挙します。
                this.soundInfoList =
                    InfoBase.ReadInfoDirectory<SoundSetInfo>(@"Data/Sound");

                var dirName = Global.Settings.SoundSetDir;
                var info = this.soundInfoList.FirstOrDefault(
                    _ => _.DirectoryName == dirName);

                if (this.soundInfoList.Any() && info == null)
                {
                    info = this.soundInfoList[0];
                    Global.Settings.SoundSetDir = info.DirectoryName;                    

                    // 設定は保存します。
                    Global.Settings.Save();
                }

                this.selectedSoundSet = info;
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "サウンドリスト取得中にエラーが発生しました。");

                this.soundInfoList = new List<SoundSetInfo>();
            }
        }

        /// <summary>
        /// 設定オブジェクトの音量お変更を反映します。
        /// </summary>
        private void Settings_SEVolumeUpdated()
        {
            this.manager.Volume = Global.Settings.SEVolume;
        }

        /// <summary>
        /// 設定オブジェクトの音声セット変更を反映します。
        /// </summary>
        private void Settings_SoundSetDirUpdated()
        {
            var thisAsm = Assembly.GetEntryAssembly();

            this.manager.DefaultPath = Path.Combine(
                Path.GetDirectoryName(thisAsm.Location),
                "Data/Sound",
                Global.Settings.SoundSetDir ?? "");
        }

        /// <summary>
        /// 音声プレイヤーオブジェクトを初期化します。
        /// </summary>
        public SoundManager()
        {
            try
            {
                this.manager = new Ragnarok.Extra.Sound.SoundManager();

                // 初期設定
                Settings_SEVolumeUpdated();
                Settings_SoundSetDirUpdated();

                Global.Settings.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "SEVolume")
                    {
                        Settings_SEVolumeUpdated();
                    }
                    else if (e.PropertyName == "SoundSetDir")
                    {
                        Settings_SoundSetDirUpdated();
                    }
                };

                // 音声セットのディレクトリ一覧を更新します。
                InitSoundDir();
            }
            catch (Exception ex)
            {
                Log.ErrorException(ex,
                    "サウンドオブジェクトの初期化に失敗しました。");
            }
        }
    }
}
