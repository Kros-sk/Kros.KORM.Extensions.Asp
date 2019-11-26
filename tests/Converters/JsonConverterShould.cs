using FluentAssertions;
using Kros.KORM.Converter;
using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests.Converters
{
    public class JsonConverterShould
    {
        [Fact]
        public void ThrowArgumentExceptionWhenSerializationOptionsAreNull()
        {
            Action action = () => new JsonConverter<TestClass>(null);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertJsonToEntity()
        {
            var converter = new JsonConverter<TestClass>();

            TestClass expected = GetSampleClass();
            var actual = converter.Convert(GetSampleJson());

            actual.Should().BeOfType(typeof(TestClass));
            ((TestClass)actual).Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ThrowJsonExceptionWhenTryingToConvertImproperlyFormattedJsonToEntity()
        {
            var converter = new JsonConverter<TestClass>();

            Action action = () => converter.Convert(GetSampleImproperlyFormattedJson());

            action.Should().Throw<JsonException>();
        }

        [Fact]
        public void ConvertImproperlyFormattedJsonToEntityWhenUsingCorrectOptions()
        {
            var converter = new JsonConverter<TestClass>(new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            });

            TestClass expected = GetSampleClass();
            var actual = converter.Convert(GetSampleImproperlyFormattedJson());

            actual.Should().BeOfType(typeof(TestClass));
            ((TestClass)actual).Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ConvertEntityToJson()
        {
            var converter = new JsonConverter<TestClass>();

            string expected = GetSampleJson();
            var actual = converter.ConvertBack(GetSampleClass());

            actual.Should().BeOfType(typeof(string));
            ((string)actual).Should().Equals(expected);
        }

        private TestClass GetSampleClass()
            => new TestClass()
            {
                BoolProperty = true,
                StringProperty = "LoremIpsum",
                DoubleProperty = Math.PI,
                ArrayProperty = new int[] { 4, 8, 15, 16, 23, 42 },
                ObjectProperty = new TestClass() { StringProperty = "DolorSitAmet" }
            };

        private string GetSampleJson()
            => Regex.Replace(
            @"{
                ""BoolProperty"":true,
                ""StringProperty"":""LoremIpsum"",
                ""DoubleProperty"":3.1415926535897931,
                ""ArrayProperty"":[4,8,15,16,23,42],
                ""ObjectProperty"":
                {
                    ""BoolProperty"":false,
                    ""StringProperty"":""DolorSitAmet"",
                    ""DoubleProperty"":0,
                    ""ArrayProperty"":null,
                    ""ObjectProperty"":null
                }
            }", @"\s+", "");

        private string GetSampleImproperlyFormattedJson()
            => Regex.Replace(
            @"{
                ""boolProperty"":true,
                ""stringProperty"":""LoremIpsum"",
                ""doubleProperty"":3.1415926535897931,
                ""arrayProperty"":[4,8,15,16,23,42],
                ""objectProperty"":
                {
                    ""boolProperty"":false,
                    ""stringProperty"":""DolorSitAmet"",
                    ""doubleProperty"":0,
                    ""arrayProperty"":null,
                    ""objectProperty"":null
                },
            }", @"\s+", "");

        private class TestClass
        {
            public bool BoolProperty { get; set; }

            public string StringProperty { get; set; }

            public double DoubleProperty { get; set; }

            public int[] ArrayProperty { get; set; }

            public TestClass ObjectProperty { get; set; }
        }
    }
}
