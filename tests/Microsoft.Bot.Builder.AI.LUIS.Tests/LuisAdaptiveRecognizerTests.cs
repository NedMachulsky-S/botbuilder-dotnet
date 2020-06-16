﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AdaptiveExpressions.Converters;
using AdaptiveExpressions.Properties;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.Luis.Testing;
using Microsoft.Bot.Builder.Dialogs.Declarative.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class LuisAdaptiveRecognizerTests
    {
        private const string DynamicListJSon = @"[
                {
                    'entity': 'alphaEntity',
                    'list': [
                        {
                            'canonicalForm': 'a',
                            'synonyms': [
                                'a',
                                'aa'
                            ]
                        },
                        {
                            'canonicalForm': 'b',
                            'synonyms': [
                                'b',
                                'bb'
                            ]
}
                    ]
                },
                {
                    'entity': 'numberEntity',
                    'list': [
                        {
                            'canonicalForm': '1',
                            'synonyms': [
                                '1',
                                'one'
                            ]
                        },
                        {
                            'canonicalForm': '2',
                            'synonyms': [
                                '2',
                                'two'
                            ]
                        }
                    ]
                }
            ]";

        private const string RecognizerJson = @"{
            '$kind': 'Microsoft.LuisRecognizer',
            'applicationId': '=settings.luis.DynamicLists_test_en_us_lu',
            'endpoint': '=settings.luis.endpoint',
            'endpointKey': '=settings.luis.endpointKey', 'dynamicLists': " + DynamicListJSon + "}";

        private static readonly string DynamicListsDirectory = PathUtils.NormalizePath(@"..\..\..\tests\LuisAdaptiveRecognizerTests");
        
        public static ResourceExplorer ResourceExplorer { get; set; }

        public static IConfiguration Configuration { get; set; }

        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Configuration = new ConfigurationBuilder()
                .UseMockLuisSettings(DynamicListsDirectory, "TestBot")
                .Build();
            
            ResourceExplorer = new ResourceExplorer()
                .AddFolder(Path.Combine(TestUtils.GetProjectPath(), "tests", nameof(LuisAdaptiveRecognizerTests)), monitorChanges: false)
                .RegisterType(LuisAdaptiveRecognizer.Kind, typeof(MockLuisRecognizer), new MockLuisLoader(Configuration));
        }

        [TestMethod]
        public async Task DynamicLists()
        {
            await TestUtils.RunTestScript(ResourceExplorer, configuration: Configuration);
        }

        [TestMethod]
        public async Task DynamicListsExpression()
        {
            await TestUtils.RunTestScript(ResourceExplorer, configuration: Configuration);
        }

        [TestMethod]
        public async Task ExternalEntities()
        {
            await TestUtils.RunTestScript(ResourceExplorer, configuration: Configuration);
        }

        [TestMethod]
        public void DeserializeDynamicList()
        {
            var dl = JsonConvert.DeserializeObject<List<DynamicList>>(DynamicListJSon);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }

        [TestMethod]
        public void DeserializeSerializedDynamicList()
        {
            var ol = JsonConvert.DeserializeObject<List<DynamicList>>(DynamicListJSon);
            var json = JsonConvert.SerializeObject(ol);
            var dl = JsonConvert.DeserializeObject<List<DynamicList>>(json);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }

        [TestMethod]
        public void DeserializeArrayExpression()
        {
            var ae = JsonConvert.DeserializeObject<ArrayExpression<DynamicList>>(DynamicListJSon, new ArrayExpressionConverter<DynamicList>());
            var dl = ae.GetValue(null);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }

        [TestMethod]
        public void DeserializeLuisAdaptiveRecognizer()
        {
            var recognizer = JsonConvert.DeserializeObject<LuisAdaptiveRecognizer>(RecognizerJson, new ArrayExpressionConverter<DynamicList>());
            var dl = recognizer.DynamicLists.GetValue(null);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }
    }
}
