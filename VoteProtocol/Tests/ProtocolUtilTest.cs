#if TESTS
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

using Ragnarok.Shogi;

namespace VoteSystem.Protocol.Tests
{
    using Vote;

    /// <summary>
    ///</summary>
    [TestFixture()]
    internal class ProtocolUtilTest
    {
        private void MyTest(VoteState state, DateTime startTime,
                            TimeSpan span, TimeSpan progressSpan)
        {
            VoteState state2;
            DateTime startTime2;
            TimeSpan span2;
            TimeSpan progressSpan2;

            ProtocolUtil.WriteTotalVoteSpan(
                state, startTime, span, progressSpan);
            ProtocolUtil.ReadTotalVoteSpan(
                out state2, out startTime2, out span2, out progressSpan2);

            Assert.AreEqual(state, state2);
            Assert.AreEqual(startTime, startTime2);
            Assert.AreEqual(span, span2);
            Assert.AreEqual(progressSpan, progressSpan2);
        }

        /// <summary>
        /// Read/Write TotalVoteSpan のテスト
        /// </summary>
        [Test()]
        public void TotalVoteSpanTest()
        {
            MyTest(VoteState.Voting, DateTime.Now, TimeSpan.FromHours(2), TimeSpan.Zero);
            MyTest(VoteState.Pause, DateTime.Now, TimeSpan.FromHours(2), TimeSpan.Zero);
            MyTest(VoteState.End, DateTime.Now, TimeSpan.FromHours(2), TimeSpan.Zero);
            MyTest(VoteState.Stop, DateTime.Now, TimeSpan.FromHours(2), TimeSpan.Zero);

            MyTest(VoteState.Voting, DateTime.Now, TimeSpan.MaxValue, TimeSpan.Zero);
            MyTest(VoteState.Pause, DateTime.Now, TimeSpan.MaxValue, TimeSpan.Zero);
            MyTest(VoteState.End, DateTime.Now, TimeSpan.MaxValue, TimeSpan.Zero);
            MyTest(VoteState.Stop, DateTime.Now, TimeSpan.MaxValue, TimeSpan.Zero);
        }
    }
}
#endif
