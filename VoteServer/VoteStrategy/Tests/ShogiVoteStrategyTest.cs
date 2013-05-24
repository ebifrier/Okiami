#if TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Ragnarok.Shogi;

namespace VoteSystem.Server.VoteStrategy.Tests
{
    /// <summary>
    ///ShogiVoteStrategyTest のテスト クラスです。すべての
    ///ShogiVoteStrategyTest 単体テストをここに含めます
    ///</summary>
    [TestFixture()]
    internal class ShogiVoteStrategyTest
    {
        private void PVTest(string text, string expMoveText,
                            string expNote, int expId, int expNextId)
        {
            string note = string.Empty;
            int id = -1;
            int nextId = -1;

            var expMoveList = (
                string.IsNullOrEmpty(expMoveText) ?
                new List<Move>() :
                BoardExtension.MakeMoveList(expMoveText));
            var moveList = ShogiVoteStrategy.ParseVariation(
                text, out note, out id, out nextId);
            if (moveList == null)
            {
                Assert.IsTrue(!expMoveList.Any());
                return;
            }

            Assert.IsTrue(expMoveList.SequenceEqual(moveList));
            Assert.AreEqual(note, expNote);
            Assert.AreEqual(id, expId);
            Assert.AreEqual(nextId, expNextId);
        }

        /// <summary>
        /// ParseVariation のテスト
        /// </summary>
        [Test()]
        public void ParseVariationTest()
        {
            PVTest(
                @"76歩34歩56歩54歩",
                @"76歩34歩56歩54歩",
                string.Empty, -1, -1);
            PVTest(
                @"76歩34歩56歩54歩 これ大丈夫か？",
                @"76歩34歩56歩54歩",
                "これ大丈夫か？", -1, -1);
            PVTest(
                @"76歩34歩56歩54歩　$234　",
                @"76歩34歩56歩54歩",
                string.Empty, -1, 234);

            PVTest(
                @"$234 86歩 24歩 85歩　$235　",
                @"86歩 24歩 85歩",
                string.Empty, 234, 235);
            PVTest(
                @"$234 86歩 24歩 85歩 まあ ええか",
                @"86歩24歩 85歩",
                "まあ ええか", 234, -1);
            PVTest(
                @"$235 86歩 45馬 85歩 これだ！　$334",
                @"86歩45馬 85歩",
                "これだ！", 235, 334);
            PVTest(
                @"$234 きたああああああ",
                string.Empty,
                "きたああああああ", 234, -1);

            // 馬打は不可能
            PVTest(
                @"$235 86歩 45馬打 85歩 これだ！　$334",
                @"86歩",
                "45馬打 85歩 これだ！", 235, 334);
            PVTest(
                @"$23525歩 まあええか",
                string.Empty,
                "まあええか", -1, -1);
            PVTest(
                @"$235$ 25歩 まあええか",
                string.Empty,
                "まあええか", -1, -1);
            PVTest(
                @"$235 25歩34歩56歩54歩うーむ",
                @"25歩34歩56歩54歩",
                string.Empty, 235, -1);
        }
    }
}
#endif
