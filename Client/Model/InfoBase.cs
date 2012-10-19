﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;

using Ragnarok;
using Ragnarok.Utility;

namespace VoteSystem.Client.Model
{
    /// <summary>
    /// 情報ファイルの基底クラスです。
    /// </summary>
    [DataContract()]
    public class InfoBase
    {
        /// <summary>
        /// info.jsonがあったパスを取得します。
        /// </summary>
        public string BasePath
        {
            get;
            private set;
        }

        /// <summary>
        /// これらのファイルがあるディレクトリ名を取得または設定します。
        /// </summary>
        public string DirectoryName
        {
            get;
            set;
        }

        /// <summary>
        /// タイトルを取得または設定します。
        /// </summary>
        [DataMember(Name = "title")]
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// 作者名を取得または設定します。
        /// </summary>
        [DataMember(Name = "author")]
        public string AuthorName
        {
            get;
            set;
        }

        /// <summary>
        /// ニコ生のコミュニティ番号を取得します。
        /// </summary>
        [DataMember(Name = "nicommunity")]
        public string NicoCommunity
        {
            get;
            set;
        }

        /// <summary>
        /// ニコ生コミュニティのURLを取得または設定します。
        /// </summary>
        public string NicoCommunityUrl
        {
            get
            {
                if (string.IsNullOrEmpty(NicoCommunity))
                {
                    return null;
                }

                return Ragnarok.NicoNico.NicoString.CommunityInfoUrl(
                    NicoCommunity);
            }
        }

        /// <summary>
        /// 作者のtwitterアカウントを取得または設定します。
        /// </summary>
        [DataMember(Name = "twitter")]
        public string TwitterId
        {
            get;
            set;
        }

        /// <summary>
        /// twitterへのURLを取得します。
        /// </summary>
        public string TwitterUrl
        {
            get
            {
                if (string.IsNullOrEmpty(TwitterId))
                {
                    return null;
                }

                return string.Format(
                    "http://twitter.com/#!/{0}",
                    TwitterId);
            }
        }

        /// <summary>
        /// pixivのIDを取得または設定します。
        /// </summary>
        [DataMember(Name = "pixiv")]
        public int PixivId
        {
            get;
            set;
        }

        /// <summary>
        /// PixivへのURLを取得します。
        /// </summary>
        public string PixivUrl
        {
            get
            {
                if (PixivId <= 0)
                {
                    return null;
                }

                return string.Format(
                    "http://www.pixiv.net/member.php?id={0}",
                    PixivId);
            }
        }

        /// <summary>
        /// ブログのURLを取得または設定します。
        /// </summary>
        [DataMember(Name = "blog")]
        public string BlogUrl
        {
            get;
            set;
        }

        /// <summary>
        /// メールアドレスを取得または設定します。
        /// </summary>
        [DataMember(Name = "mail")]
        public string MailAddress
        {
            get;
            set;
        }

        /// <summary>
        /// ホームページのURLを取得または設定します。
        /// </summary>
        [DataMember(Name = "homepage")]
        public string HomepageUrl
        {
            get;
            set;
        }

        /// <summary>
        /// コメントを取得または設定します。
        /// </summary>
        [DataMember(Name = "comment")]
        public string Comment
        {
            get;
            set;
        }

        /// <summary>
        /// 情報が書かれた情報ファイルを読み込みます。
        /// </summary>
        protected static T ReadInternal<T>(string filepath)
            where T : InfoBase
        {
            // パスをフルパスに直します。
            var fullpath = Path.GetFullPath(filepath);

            var obj = Json.DeserializeFromFile<T>(fullpath);
            if (obj == null)
            {
                return null;
            }

            // 成功したらパスを設定します。
            obj.BasePath = Path.GetDirectoryName(fullpath);
            obj.DirectoryName = Path.GetFileName(obj.BasePath);

            if (string.IsNullOrEmpty(obj.Title))
            {
                obj.Title = obj.DirectoryName;
            }

            return obj;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public InfoBase()
        {
            PixivId = -1;
        }
    }
}