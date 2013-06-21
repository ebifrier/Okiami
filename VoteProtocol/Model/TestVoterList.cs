using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

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
                CreateInfo("フルチンパイルドライバー", "おつおつおつおつ今来たよ～　って終わってるしｗ"),
                CreateInfo("(｀・ω・´)", "永世15級"),
                CreateInfo("12きう"),
                CreateInfo("sc", "1級"),
                CreateInfo("Azumi", "六段"),
                CreateInfo("kennin", "6段です"),
                CreateInfo("Kagemiro", "初段"),
                CreateInfo("L.C", "3段"),
                CreateInfo("McAfee", "0段"),
                CreateInfo("acorn"),
                CreateInfo("an", "特技遠隔操作"),
                CreateInfo("boki", "フィギュアオタ"),
                CreateInfo("cco", "二段"),
                CreateInfo("gg", "三段"),
                CreateInfo("heppoko", "１０級"),
                CreateInfo("hoki", "ワシの手柄や"),
                CreateInfo("hysdk", "同期不可"),
                CreateInfo("i7bona", "１０級　こんな感じ？"),
                CreateInfo("kenji", "三段"),
                CreateInfo("kokujira", "ハムに勝てるくらい"),
                CreateInfo("miminari", "人類滅亡級"),
                CreateInfo("rom", "100級"),
                CreateInfo("sussoss", "狩人"),
                CreateInfo("syd"),
                CreateInfo("tb", "9級"),
                CreateInfo("tutty", "３級"),
                CreateInfo("５段"),
                CreateInfo("名無しさん", "５段"),
                CreateInfo("ｓｙｄ", "5段"),
                CreateInfo("あじふらい", "15段"),
                CreateInfo("あすか", "はじめてだけど"),
                CreateInfo("あと一人", "微妙な棋力"),
                CreateInfo("いえろー"),
                CreateInfo("うんｋお", "ダークフレイムマスター"),
                CreateInfo("うんｋふらい", "１５級"),
                CreateInfo("えびきらい", "せっかくなので"),
                CreateInfo("えびふらい", "最強の１２級"),
                CreateInfo("えびふりゃ", "10級"),
                CreateInfo("えびふりゃー", "5段"),
                CreateInfo("えぴふらい", "12段"),
                CreateInfo("えろじじぃ", "7段　美少女リスナーふやしてちょ"),
                CreateInfo("えろふらい", "16級"),
                CreateInfo("およよ", "24で７級"),
                CreateInfo("お空ちゃん", "クジラちゃんの味方"),
                CreateInfo("お茶", "２級"),
                CreateInfo("かうんたーうぇいと"),
                CreateInfo("かきふらい", "希望"),
                CreateInfo("かきふらい", "もう疲れました"),
                CreateInfo("かきふりゃー", "10級"),
                CreateInfo("かたり", "共有co"),
                CreateInfo("かにふらい", "初心者"),
                CreateInfo("きすふらい"),
                CreateInfo("ぐら", "主の義弟"),
                CreateInfo("これは　デスノートです"),
                CreateInfo("ごだん", "五段"),
                CreateInfo("さしこ", "ボナの操作"),
                CreateInfo("しくる", "二段"),
                CreateInfo("じゃあ", "くそ弱い"),
                CreateInfo("するわ"),
                CreateInfo("その他", "2級"),
                CreateInfo("だんじりてぃー", "希望四段"),
                CreateInfo("ちくわ裏返す", "裏ちくわ"),
                CreateInfo("ちゅら"),
                CreateInfo("ちゅら", "二段"),
                CreateInfo("てけとー", "3段"),
                CreateInfo("てんてー", "希望　"),
                CreateInfo("でびたん", "気力はまだない"),
                CreateInfo("とある原発のレール癌", "24 1100"),
                CreateInfo("とまと", "ご飯食べてくる"),
                CreateInfo("とまとりあ", "最初のｇｄｇｄが無くなる日"),
                CreateInfo("どぅおおーんっ", "３級"),
                CreateInfo("なっちゃん", "コメなんてうってあげない///"),
                CreateInfo("ななす㈹とかい", "Σ段"),
                CreateInfo("なんちゃって初段"),
                CreateInfo("参加なんて無かった", "それでいいじゃないか"),
                CreateInfo("にーいち", "１０級"),
                CreateInfo("ぬるて", "初段"),
                CreateInfo("はむさんど", "１５級"),
                CreateInfo("ばる", "ハムにかろうじて勝つレベル"),
                CreateInfo("ふぁあ", "ｋｈ"),
                CreateInfo("まんｋなめなめ"),
                CreateInfo("みたらし"),
                CreateInfo("めきお", "だっつ～の●ｍ●ｖ"),
                CreateInfo("ゆういち", "70級"),
                CreateInfo("よっしー", "１５級"),
                CreateInfo("アズミ", "六段"),
                CreateInfo("アヒル", "24で5級"),
                CreateInfo("エビフライぶつけんぞ"),
                CreateInfo("エヴィ初号機", "パターンオレンジ"),
                CreateInfo("ガリガリチャン"),
                CreateInfo("クジラちゃんの味方お空ちゃん"),
                CreateInfo("コテハン", "15級"),
                CreateInfo("コミュどうしよう", "やめとくか"),
                CreateInfo("シャア", "女の子好き"),
                CreateInfo("タイラント", "１０級"),
                CreateInfo("ダン", "棋力は内緒"),
                CreateInfo("ッシャー！", "いつでも半ズボン"),
                CreateInfo("トミーwww"),
                CreateInfo("ドキュモ"),
                CreateInfo("ノヴァ", "北の勇者(笑)"),
                CreateInfo("ハチロク", "４級"),
                CreateInfo("ハムサンド"),
                CreateInfo("ハム将棋", "真・最強の１２級"),
                CreateInfo("ハワイアン初春", "真冬のハワイから"),
                CreateInfo("バーン", "大魔王15級"),
                CreateInfo("パックマニア"),
                CreateInfo("フルチンパイルドライバー", "R2005"),
                CreateInfo("ボナンザ", "2分で4時間と戦う"),
                CreateInfo("マールボーロ", "おつでした"),
                CreateInfo("メルセデス", "眠い～"),
                CreateInfo("モモカジュ"),
                CreateInfo("ユーフォリア", "囲碁8級"),
                CreateInfo("ルキウス・ディス・ミレイユ"),
                CreateInfo("ルパン", "捕鯨成功"),
                CreateInfo("ヱヴィ初号機", "パターンオレンジ"),
                CreateInfo("愛さま", "棋力"),
                CreateInfo("以下", "人狼参加者一覧"),
                CreateInfo("羽生"),
                CreateInfo("円蔵", "三級"),
                CreateInfo("俺は村人サバイバー", "（実は狼やったことないから)"),
                CreateInfo("還元", "3級"),
                CreateInfo("関根", "名人"),
                CreateInfo("希望"),
                CreateInfo("希望・・・"),
                CreateInfo("希望です"),
                CreateInfo("輝一", "処断"),
                CreateInfo("菊丸"),
                CreateInfo("宮田", "爆弾噛み名人"),
                CreateInfo("橋本", "6段"),
                CreateInfo("錦糸卵", "５級"),
                CreateInfo("銀の動き覚えました", "初心者"),
                CreateInfo("月曜日です", "祝日付変更"),
                CreateInfo("砂漠"),
                CreateInfo("最強の", "13級"),
                CreateInfo("最強の１２級"),
                CreateInfo("七紫", "処断"),
                CreateInfo("参加者のみなさんおつでした"),
                CreateInfo("主以上"),
                CreateInfo("終盤最強", "１５級"),
                CreateInfo("初心者すぎる家族"),
                CreateInfo("初段"),
                CreateInfo("将棋仮面", "途中からＲＯＭってたわ"),
                CreateInfo("少し進化した名無し", "14級"),
                CreateInfo("少年", "7級"),
                CreateInfo("条件Ｒ５００以下"),
                CreateInfo("寝坊の名無し", "煽り7段"),
                CreateInfo("人狼王", "早く頼みます"),
                CreateInfo("水彩", "15級"),
                CreateInfo("宣誓", "最弱の6段"),
                CreateInfo("宣誓さんの恋人", "12級"),
                CreateInfo("宣誓アンチ", "2級"),
                CreateInfo("煽り勢最強", "人狼初段"),
                CreateInfo("滝", "１１級"),
                CreateInfo("達人に浮気した名無し", "３級"),
                CreateInfo("誰か", "abcからｇｄｇｄ取ったら何が残る？　"),
                CreateInfo("通りすがりの初心者", "自称15級"),
                CreateInfo("藤井武美", "16級"),
                CreateInfo("徳川", "大名"),
                CreateInfo("二段"),
                CreateInfo("入らんけど", "5段"),
                CreateInfo("之びふろい", "西京の重に灸"),
                CreateInfo("不参加", "３級"),
                CreateInfo("仏陀", "4段"),
                CreateInfo("米長邦雄", "15級"),
                CreateInfo("片翼の贖いの天魔王"),
                CreateInfo("某視聴者", "√級　６時間おつーした"),
                CreateInfo("本当は15級", "6段"),
                CreateInfo("本日の参加者なし"),
                CreateInfo("夢の中で応援"),
                CreateInfo("名無し", "一二三１０段"),
                CreateInfo("名無しさん", "煽り6段"),
                CreateInfo("明", "棋力竜王"),
                CreateInfo("梟", "3級"),
                CreateInfo("ส้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้（＾ω＾）ส้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้้ ", "テスト"),
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
            x.AddRange(list);
            voterList.JoinedVoterList.AddRange(x);

            voterList.DonorViewList.AddRange(
                list.OrderBy(_ => Guid.NewGuid())
                    .Take(20).Select(_ => _.Name));

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
