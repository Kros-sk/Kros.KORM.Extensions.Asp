using FluentAssertions;
using Kros.KORM.Converter;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace Kros.KORM.Extensions.Api.UnitTests.Converters
{
    public class JsonConverterShould
    {
        [Fact]
        public void ThrowArgumentExceptionWhenEntityTypeIsNotSet()
        {
            Action action = () => new JsonConverter(null);

            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConvertJsonToEntity()
        {
            var converter = new JsonConverter(typeof(TestClass));

            TestClass expected = GetSampleClass();
            var actual = converter.Convert(GetSampleJson());

            actual.Should().BeOfType(typeof(TestClass));
            ((TestClass)actual).BoolProperty.Should().Equals(expected.BoolProperty);
            ((TestClass)actual).StringProperty.Should().Equals(expected.StringProperty);
            ((TestClass)actual).DoubleProperty.Should().Equals(expected.DoubleProperty);
            ((TestClass)actual).ArrayProperty.Should().BeEquivalentTo(expected.ArrayProperty);
            ((TestClass)actual).ObjectProperty.Should().NotBeNull();
            ((TestClass)actual).ObjectProperty.StringProperty.Equals(expected.ObjectProperty.StringProperty);
        }

        [Fact]
        public void ConvertEntityToJson()
        {
            var converter = new JsonConverter(typeof(TestClass));

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
