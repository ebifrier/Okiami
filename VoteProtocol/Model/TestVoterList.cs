using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Ragnarok;

namespace VoteSystem.Protocol.Model
{
    using Vote;

    /// <summary>
    /// ネットワークの投票を行います。
    /// </summary>
    public static class TestVoterList
    {
        private static int idCounter = 0;
        private static int liveCounter = 0;
        private static int anonymousCount = 0;

        private static string NextId
        {
            get
            {
                var id = Interlocked.Increment(ref idCounter);

                return id.ToString();
            }
        }

        private static VoterInfo CreateInfo(string name, string skill = null)
        {
            var liveData = (liveCounter++ < 30 ?
                new LiveData(LiveSite.NicoNama, "lv142093226") : null);
            var id = (anonymousCount++ < 100 ?
                Guid.NewGuid().ToString() : NextId);

            return new VoterInfo(id, name, skill)
            {
                LiveData = liveData,
            };
        }

        /// <summary>
        /// テスト用
        /// </summary>
        public static VoterList GetTestVoterList()
        {
            var list = new List<VoterInfo>
            {
                //CreateInfo("ーーーー", "----"),
                //CreateInfo("^'''''", "____"),
                CreateInfo("テスト", "０１２３４５６７８９０１２３４５６７８９０１２３４５６７８９０１２３４５６７８９"),
                CreateInfo("９８７６５４３２１０９８７６５４３２１０", "テスト"),
                CreateInfo("フルチンパイルドライバー", "おつおつおつおつ今来たよ～　って終わってるしｗ"),
                CreateInfo("(｀・ω・´)", "永世15級"),
                CreateInfo("hoki", "ワシの手柄や"),
                CreateInfo("あすか", "はじめてだけど"),
                CreateInfo("うんｋお", "ダークフレイムマスター"),
                CreateInfo("えびきらい", "せっかくなので"),
                CreateInfo("えろじじぃ", "美少女リスナーふやしてちょ"),
                CreateInfo("かきふらい", "もう疲れました"),
                CreateInfo("ぐら", "主の義弟"),
                CreateInfo("これは",　"デスノートです"),
                CreateInfo("でびたん", "気力はもうない"),
                CreateInfo("とまと", "ご飯食べてくる"),
                CreateInfo("とまとりあ", "最初のｇｄｇｄが無くなる日"),
                CreateInfo("なっちゃん", "コメなんてうってあげない///"),
                CreateInfo("参加なんて無かった", "それでいいじゃないか"),
                CreateInfo("エヴィ初号機", "パターンオレンジ"),
                CreateInfo("コミュどうしよう", "やめとくか"),
                CreateInfo("ハワイアン初春", "真冬のハワイから"),
                CreateInfo("ボナンザ", "1分で4時間と戦う"),
                CreateInfo("以下", "人狼参加者一覧"),
                CreateInfo("関根", "名人"),
                CreateInfo("焼き鳥", "参加者のみなさんおつでした"),
                CreateInfo("将棋仮面", "途中からＲＯＭってたわ"),
                CreateInfo("人狼王", "早く頼みます"),
                CreateInfo("徳川", "大名"),
                CreateInfo("某視聴者", "６時間おつーした"),
                CreateInfo("明", "棋力竜王"),
                CreateInfo("えぶふらい", "もっとしたかった"),
                CreateInfo("紅茶", "皆さんお疲れ様でしたー"),
                CreateInfo("野月", "えびちゃんあいしてる"),
                CreateInfo("スタッフ", "棋聖戦みてました"),
                CreateInfo("master", "遅れてきたNOT真打"),
                CreateInfo("名無しのリスナー", "楽しかったよー"),
                CreateInfo("三窮名無し", "おもろかった"),
                CreateInfo("えびふらい", "ありがとうございました"),
                CreateInfo("あああ", "第二回もやってね"),
                CreateInfo("とりちゃん", "エビちゃんと西尾先生とスタッフの皆さんお疲れ様でした"),
                CreateInfo("之びふ5い", "戦いはまだ終わってない(経済的な意味で)"),
                CreateInfo("砂漠", "楽しかったです"),
                CreateInfo("大山康晴", "天国から"),
                CreateInfo("さぬき", "職場から見守ったよ～！"),
                CreateInfo("森内名人", "つまらん将棋をしてしまったんご"),
                CreateInfo("24歳の24棋士", "将棋盤がお茶臭い　"),
                CreateInfo("かぜましょうに", "またやりましょう"),
                CreateInfo("えびふらい", "面白かったですお疲れ様"),
                CreateInfo("ナンバー8", "お疲れ様でした"),
                CreateInfo("ozawa24", "勉強になりました"),
                CreateInfo("とっても", "お疲れ様"),
                CreateInfo("tukisima", "楽しかったよ～"),
                CreateInfo("通りすがりの誰か", "いつものエビちゃんだった"),
                CreateInfo("エスペラード", "悔しいお"),
                CreateInfo("２２銀", "エビさんスタッフのみなさん乙"),
                CreateInfo("ああああ", "お疲れ様でした"),
                CreateInfo("おつかれさま", "楽しかったけど難しかったー次は終盤に時間使おう"),
                CreateInfo("エナツ", "えびさん企画最高でした"),
                CreateInfo("あｍ", "楽しかったよー　"),
                CreateInfo("豊中の受け師", "楽しかったけど難しかったー次は終盤に時間使おう"),
                CreateInfo("言霊", "ミラーしてた人"),
                CreateInfo("あまちゃん", "楽しかった"),
                CreateInfo("espelade", "最高の企画でした"),
                CreateInfo("茶化し係", "楽しめたよー"),
                CreateInfo("スタッフ", "棋聖戦の休憩中"),
                CreateInfo("noda", "主もスタッフもお疲れ様でした"),
                CreateInfo("ふぁあ", "リスナーはそこまで勝ちにこだわってない"),
                CreateInfo("エビさんはスタッフでおいしくいただきました", "もっとしたかった"),
                CreateInfo("ヽ(*´∀｀*)ﾉ", "定番のファンタ。楽しかったです"),
                CreateInfo("hoki", "よくやった。よくやったよ・・・"),
                CreateInfo("将棋仮面", "バイト先から"),
                CreateInfo("野生", "おつおつでした"),
                CreateInfo("主にバリカンを", "髪長すぎｗ"),
                CreateInfo("渡辺明", "これがプロです"),
                CreateInfo("つーてんかく", "いい最終回だった"),
                CreateInfo("之びふ5い", "戦いはまだ終わってない(費用回収の意味で)　"),
                CreateInfo("じむ", "面白かったです"),
                CreateInfo("おれら", "またやろうね！"),
                CreateInfo("うんｋお", "乙彼でした"),
                CreateInfo("ああああ", "クジラ強くしろ"),
                CreateInfo("揚げ出し豆腐おいしい", "惜しかった！"),
                CreateInfo("選挙に行った都民", "雪かきで腰痛い"),
                CreateInfo("えびふらい", "GPSFish強すぎ"),
                CreateInfo("32歩", "この1手が誰も読めなかった"),
                CreateInfo("えばふらい", "強かった"),
                CreateInfo("とまと", "自動で名前取得したらええ"),
                CreateInfo("無名", "初めて最初から最後まで参加しました"),
                CreateInfo("とりふらい", "俺が本物だから！"),
                CreateInfo("エスペラード", "ハッシーに負けた"),
                CreateInfo("寄付金返せ", "はよ"),
                CreateInfo("機巧うんｋおは傷つかない", "負けたけど"),
            };

            var liveOwnerList = new List<string>
            {
                "のぼる",
	            "二次元人",
	            "にくくい",
	            "かど",	
	            "メイビス",
	            "森太",
	            "たかし",
	            "にしにっぽん",
	            "オグリ",
	            "ねんのために",
	            "うぬこうじ",
	            "不☆通",
	            "人参",
	            "ベルシタス",
	            "できすぎ",
	            "サスケ",
	            "龍氣槍",
	            "みーご",
	            "ルカワ",
	            "616",
	            "らふぃ",	
	            "かるあ",
	            "るーせる",
	            "ねぎま",
	            "じゃんぼ",
	            "生ちゃん",
	            "うにうに",
	            "kendo-arashi",
	            "さまんさneo",
	            "みどりの"
            };

            var voterList = new VoterList
            {
                UnjoinedVoterCount = 236,
                DonorAmount = 160000,
                TotalLiveCount = 100,
                TotalLiveVisitorCount = 1000,
                TotalLiveCommentCount = 120000,
            };
            var x = new List<VoterInfo>(list);
            Enumerable.Range(0, 100)
                .ForEach(_ => x.AddRange(list));
            voterList.JoinedVoterList.AddRange(x);

            voterList.DonorViewList.AddRange(
                list.OrderBy(_ => Guid.NewGuid())
                    .Select(_ => _.Name));

            voterList.LiveOwnerList.AddRange(
                from name in liveOwnerList
                select new VoterInfo
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                });

            return voterList;
        }
    }
}
