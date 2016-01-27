using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AcmeScore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AcmeScoreTests
{
    [TestClass]
    public class AcmeScoreTests
    {
        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        private static void AssertRecordValues(ScoreRecord target, int contactId, Event channel, float score)
        {
            Assert.AreEqual(target?.Event, channel);
            Assert.AreEqual(target?.ContactId, contactId);
            Assert.AreEqual(target?.Score, score);
        }

        [TestMethod]
        public void ScoreRecord_Parse_Test()
        {
            int contactId = 1;
            string @event = "web";
            float score = 34.3f;

            // Check that white-space variations succeed
            var variousWhitespaceFormats = new List<string>
            {
                $"{contactId}, {@event}, {score}",
                $"{contactId},{@event},{score}",
                $" {contactId}, {@event}, {score} ",
                $"  {contactId},    {@event},   {score}",
            };

            variousWhitespaceFormats.ForEach(format =>
            {
                TestContext.WriteLine($"Parsing: {format}");
                AssertRecordValues(ScoreRecord.Parse(format), contactId, Event.Web, score);
            });

            // Ignore trailing fields
            var target = ScoreRecord.Parse("1,web,34.3,1,1,1,asdf");
            AssertRecordValues(target, 1, Event.Web, 34.3f);
        }

        [TestMethod]
        public void ScoreRecord_Parse_InvalidLineReturnsNull_Test()
        {
            Assert.IsNull(ScoreRecord.Parse(string.Empty));
            Assert.IsNull(ScoreRecord.Parse("1"));
            Assert.IsNull(ScoreRecord.Parse("1,web"));
        }

        /// <summary>
        ///     In any case where exactly 2 contact IDs exist, and have appreciably different scores,
        ///     we expect the higher to normalize to 100, and the lower to 0
        /// </summary>
        [TestMethod]
        public void Calculate_TwoContactsBroadScoresShouldNormalizeTo100And0_Test()
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            Program.CalculateScores(new string[] {"1,web,0.1", "2,email,3.3", "2,webinar,2"}, stringWriter);
            var textResponse = stringBuilder.ToString();
            TestContext.WriteLine(textResponse);

            // Test console output, expecting lines with designated values
            Assert.IsTrue(textResponse.IndexOf("1, bronze, 0") >= 0);
            Assert.IsTrue(textResponse.IndexOf("2, platinum, 100") >= 0);
        }

        /// <summary>
        ///     When the scores across all contacts doesn't reflect any significant delta, we can't provide meaningful results
        /// </summary>
        [TestMethod]
        public void Calculate_SmallScores_Test()
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            var result =Program.CalculateScores(new string[] { "1,web,0.01", "2,web,0.02" }, stringWriter);
            var textResponse = stringBuilder.ToString();
            TestContext.WriteLine(textResponse);

            Assert.IsTrue(result.First().NormalizedScore == 100 && result.First().ContactId == 2);
        }

        /// <summary>
        ///     When a single contact is provided, normalization can't provide meaningful results
        /// </summary>
        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public void Calculate_SingleContactThrows_Test()
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            Program.CalculateScores(new [] { "1,web,0.1"}, stringWriter);
        }

        /// <summary>
        ///     When the scores across all contacts doesn't reflect any significant delta, we can't provide meaningful results
        /// </summary>
        [TestMethod, ExpectedException(typeof(ApplicationException))]
        public void Calculate_ScoresTooSimilarThrows_Test()
        {
            var stringBuilder = new StringBuilder();
            var stringWriter = new StringWriter(stringBuilder);
            Program.CalculateScores(new string[] { "1,web,1", "2,web,1" }, stringWriter);
        }

    }
}
