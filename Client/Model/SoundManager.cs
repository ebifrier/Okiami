using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Ragnarok;

namespace VoteSystem.Client.Model
{
    using VoteSystem.Protocol.Vote;

    /// <summary>
    /// 音声ファイルを再生します。
    /// </summary>
    public class SoundManager : Ragnarok.Extra.Sound.SoundManager
    {
        private List<SoundSetInfo> soundInfoList =
            new List<SoundSetInfo>();
        private SoundSetInfo selectedSoundSet;

        /// <summary>
        /// 音声ディレクトリの一覧を取得します。
        /// </summary>
        public List<SoundSetInfo> SoundDirList
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

                    UpdateSoundPath();
                }
            }
        }

        /// <summary>
        /// 評価値が変化したときに呼ばれます。
        /// </summary>
        private void UpdateSoundPath()
        {
            if (SelectedSoundSet == null)
            {
                return;
            }

            // 選択された画像セットを保存します。
            Global.Settings.SoundSetDir = SelectedSoundSet.DirectoryName;
        }

        /// <summary>
        /// 投票の開始/終了などのサウンドを再生します。
        /// </summary>
        public void PlayVoteSE(VoteState voteState)
        {
            if (!CanUseSound || !Global.Settings.IsUseSE)
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
            if (!CanUseSound || !Global.Settings.IsUseSE)
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
                var fullpath = Path.GetFullPath(@"Data\Sound");
                if (!Directory.Exists(fullpath))
                {
                    this.soundInfoList = new List<SoundSetInfo>();
                    return;
                }

                // Data\Soundのサブディレクトリを
                // 音声ファイルの入ったディレクトリとして列挙します。
                this.soundInfoList =
                    Directory.EnumerateDirectories(fullpath)
                    .Select(_ => Path.Combine(_, "info.json"))
                    .Select(SoundSetInfo.Read)
                    .Where(_ => _ != null)
                    .ToList();

                var dirName = Global.Settings.SoundSetDir;
                var info = this.soundInfoList.FirstOrDefault(
                    _ => _.DirectoryName == dirName);

                if (this.soundInfoList.Any() && info == null)
                {
                    Global.Settings.SoundSetDir = this.soundInfoList[0].DirectoryName;

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
        /// 音声プレイヤーオブジェクトを初期化します。
        /// </summary>
        public SoundManager()
        {
            try
            {
                DefaultPath = Path.Combine(
                    AssemblyLocation, "Data", "Sound",
                    Global.Settings.SoundSetDir ?? "");
                Volume = Global.Settings.SEVolume;

                Global.Settings.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "SEVolume")
                    {
                        Volume = Global.Settings.SEVolume;
                    }
                    else if (e.PropertyName == "SoundSetDir")
                    {
                        DefaultPath = Path.Combine(
                            AssemblyLocation, "Data", "Sound",
                            Global.Settings.SoundSetDir ?? "");
                    }
                };

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
