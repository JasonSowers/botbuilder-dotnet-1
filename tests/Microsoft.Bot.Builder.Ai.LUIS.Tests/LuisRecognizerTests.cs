
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.Cognitive.LUIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Ai.LUIS.Tests
{
    [TestClass]
    /*
     * The LUIS application used in these unit tests is in TestData/TestLuistApp
     */
    public class LuisRecognizerTests
    {

        private readonly string _luisAppId = TestUtilities.GetKey("LUISAPPID");
        private readonly string _subscriptionKey = TestUtilities.GetKey("LUISAPPKEY");
        private readonly string _luisUriBase = TestUtilities.GetKey("LUISURIBASE");


        [TestMethod]
        public async Task SingleIntent_SimplyEntity()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true);
            var result = await luisRecognizer.Recognize("My name is Emad", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNull(result.AlteredText);
            Assert.AreEqual("My name is Emad", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.AreEqual(1, result.Intents.Count);
            Assert.IsNotNull(result.Intents["SpecifyName"]);
            Assert.IsTrue((double)result.Intents["SpecifyName"] > 0 && (double)result.Intents["SpecifyName"] <= 1);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Name"]);
            Assert.AreEqual("emad", (string)result.Entities["Name"].First);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Name"]);
            Assert.AreEqual(11, (int)result.Entities["$instance"]["Name"].First["startIndex"]);
            Assert.AreEqual(14, (int)result.Entities["$instance"]["Name"].First["endIndex"]);
            AssertScore(result.Entities["$instance"]["Name"].First["score"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntity()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver February 2nd 2001", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.AreEqual("Please deliver February 2nd 2001", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsTrue(result.Intents.Count > 1);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsTrue((double)result.Intents["Delivery"] > 0 && (double)result.Intents["Delivery"] <= 1);
            Assert.AreEqual("Delivery", result.GetTopScoringIntent().Item1);
            Assert.IsTrue(result.GetTopScoringIntent().Item2 > 0);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["builtin_number"]);
            Assert.AreEqual(2001, (int)result.Entities["builtin_number"].First);
            Assert.IsNotNull(result.Entities["builtin_ordinal"]);
            Assert.AreEqual(2, (int)result.Entities["builtin_ordinal"].First);
            Assert.IsNotNull(result.Entities["builtin_datetimeV2_date"].First);
            Assert.AreEqual("2001-02-02", (string)result.Entities["builtin_datetimeV2_date"].First.First);
            Assert.IsNotNull(result.Entities["$instance"]["builtin_number"]);
            Assert.AreEqual(28, (int)result.Entities["$instance"]["builtin_number"].First["startIndex"]);
            Assert.AreEqual(31, (int)result.Entities["$instance"]["builtin_number"].First["endIndex"]);
            Assert.AreEqual("2001", (string)result.Entities["$instance"]["builtin_number"].First["text"]);
            Assert.IsNotNull(result.Entities["$instance"]["builtin_datetimeV2_date"]);
            Assert.AreEqual(15, (int)result.Entities["$instance"]["builtin_datetimeV2_date"].First["startIndex"]);
            Assert.AreEqual(31, (int)result.Entities["$instance"]["builtin_datetimeV2_date"].First["endIndex"]);
            Assert.AreEqual("february 2nd 2001", (string)result.Entities["$instance"]["builtin_datetimeV2_date"].First["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_PrebuiltEntitiesWithMultiValues()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver February 2nd 2001 in room 201", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver February 2nd 2001 in room 201", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["builtin_number"]);
            Assert.AreEqual(2, result.Entities["builtin_number"].Count());
            Assert.IsTrue(result.Entities["builtin_number"].Any(v => (int)v == 201));
            Assert.IsTrue(result.Entities["builtin_number"].Any(v => (int)v == 2001));
            Assert.IsNotNull(result.Entities["builtin_datetimeV2_date"].First);
            Assert.AreEqual("2001-02-02", (string)result.Entities["builtin_datetimeV2_date"].First.First);
        }

        [TestMethod]
        public async Task MultipleIntents_ListEntityWithSingleValue()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("I want to travel on united", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("I want to travel on united", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Travel"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Airline"]);
            Assert.AreEqual("United", result.Entities["Airline"][0][0]);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Airline"]);
            Assert.AreEqual(20, result.Entities["$instance"]["Airline"][0]["startIndex"]);
            Assert.AreEqual(25, result.Entities["$instance"]["Airline"][0]["endIndex"]);
            Assert.AreEqual("united", result.Entities["$instance"]["Airline"][0]["text"]);
        }

        [TestMethod]
        public async Task MultipleIntents_ListEntityWithMultiValues()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("I want to travel on DL", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("I want to travel on DL", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Travel"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNotNull(result.Entities["Airline"]);
            Assert.AreEqual(2, result.Entities["Airline"][0].Count());
            Assert.IsTrue(result.Entities["Airline"][0].Any(airline => (string)airline == "Delta"));
            Assert.IsTrue(result.Entities["Airline"][0].Any(airline => (string)airline == "Virgin"));
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNotNull(result.Entities["$instance"]["Airline"]);
            Assert.AreEqual(20, result.Entities["$instance"]["Airline"][0]["startIndex"]);
            Assert.AreEqual(21, result.Entities["$instance"]["Airline"][0]["endIndex"]);
            Assert.AreEqual("dl", result.Entities["$instance"]["Airline"][0]["text"]);
        }

        [TestMethod]
        public async Task MultipleIntens_CompositeEntity()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Please deliver it to 98033 WA", CancellationToken.None);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Text);
            Assert.AreEqual("Please deliver it to 98033 WA", result.Text);
            Assert.IsNotNull(result.Intents);
            Assert.IsNotNull(result.Intents["Delivery"]);
            Assert.IsNotNull(result.Entities);
            Assert.IsNull(result.Entities["builtin_number"]);
            Assert.IsNull(result.Entities["State"]);
            Assert.IsNotNull(result.Entities["Address"]);
            Assert.AreEqual(98033, result.Entities["Address"][0]["builtin_number"][0]);
            Assert.AreEqual("wa", result.Entities["Address"][0]["State"][0]);
            Assert.IsNotNull(result.Entities["$instance"]);
            Assert.IsNull(result.Entities["$instance"]["builtin_number"]);
            Assert.IsNull(result.Entities["$instance"]["State"]);
            Assert.IsNotNull(result.Entities["$instance"]["Address"]);
            Assert.AreEqual(21, result.Entities["$instance"]["Address"][0]["startIndex"]);
            Assert.AreEqual(28, result.Entities["$instance"]["Address"][0]["endIndex"]);
            AssertScore(result.Entities["$instance"]["Address"][0]["score"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]["builtin_number"]);
            Assert.AreEqual(21, result.Entities["Address"][0]["$instance"]["builtin_number"][0]["startIndex"]);
            Assert.AreEqual(25, result.Entities["Address"][0]["$instance"]["builtin_number"][0]["endIndex"]);
            Assert.AreEqual("98033", result.Entities["Address"][0]["$instance"]["builtin_number"][0]["text"]);
            Assert.IsNotNull(result.Entities["Address"][0]["$instance"]["State"]);
            Assert.AreEqual(27, result.Entities["Address"][0]["$instance"]["State"][0]["startIndex"]);
            Assert.AreEqual(28, result.Entities["Address"][0]["$instance"]["State"][0]["endIndex"]);
            Assert.AreEqual("wa", result.Entities["Address"][0]["$instance"]["State"][0]["text"]);
            AssertScore(result.Entities["Address"][0]["$instance"]["State"][0]["score"]);
        }

        [TestMethod]
        public async Task MultipleDateTimeEntities()
        {
            if (!EnvironmentVariablesDefined())
            {
                Assert.Inconclusive("Missing Luis Environment variables - Skipping test");
                return;
            }

            var luisRecognizer = GetLuisRecognizer(verbose: true, luisOptions: new LuisRequest { Verbose = true });
            var result = await luisRecognizer.Recognize("Book a table on Friday or tomorrow at 5 or tomorrow at 4", CancellationToken.None);
            Assert.IsNotNull(result.Entities["builtin_datetimeV2_date"]);
            Assert.AreEqual(1, result.Entities["builtin_datetimeV2_date"].Count());
            Assert.AreEqual(1, result.Entities["builtin_datetimeV2_date"][0].Count());
            Assert.AreEqual("XXXX-WXX-5", (string)result.Entities["builtin_datetimeV2_date"][0][0]);
            Assert.AreEqual(2, result.Entities["builtin_datetimeV2_datetime"].Count());
            Assert.AreEqual(2, result.Entities["builtin_datetimeV2_datetime"][0].Count());
            Assert.AreEqual(2, result.Entities["builtin_datetimeV2_datetime"][1].Count());
            Assert.IsTrue(((string)result.Entities["builtin_datetimeV2_datetime"][0][0]).EndsWith("T05"));
            Assert.IsTrue(((string)result.Entities["builtin_datetimeV2_datetime"][0][1]).EndsWith("T17"));
            Assert.IsTrue(((string)result.Entities["builtin_datetimeV2_datetime"][1][0]).EndsWith("T04"));
            Assert.IsTrue(((string)result.Entities["builtin_datetimeV2_datetime"][1][1]).EndsWith("T16"));
            Assert.AreEqual(1, result.Entities["$instance"]["builtin_datetimeV2_date"].Count());
            Assert.AreEqual(2, result.Entities["$instance"]["builtin_datetimeV2_datetime"].Count());
        }

        private void AssertScore(JToken scoreToken)
        {
            var score = (double) scoreToken;
            Assert.IsTrue(score >= 0);
            Assert.IsTrue(score <= 1);
        }

        private bool EnvironmentVariablesDefined()
        {
            return _luisAppId != null && _subscriptionKey != null && _luisUriBase != null;
        }

        private IRecognizer GetLuisRecognizer(bool verbose = false, ILuisOptions luisOptions = null)
        {
            var luisRecognizerOptions = new LuisRecognizerOptions { Verbose = verbose };
            var luisModel = new LuisModel(_luisAppId, _subscriptionKey, new Uri(_luisUriBase), LuisApiVersion.V2);
            return new LuisRecognizer(luisModel, luisRecognizerOptions, luisOptions);
        }
    }
}
