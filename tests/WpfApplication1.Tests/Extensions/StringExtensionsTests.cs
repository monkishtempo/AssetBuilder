using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AssetBuilder.Extensions;
using Xunit;

namespace Asset_Builder.Tests.Extensions
{
    /// <summary>
    /// Note: This test code file must be saved as Unicode
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class StringExtensionsTests
    {
        private const string UnicodeString = @"Here is some random text (₡₢₣ ≥)";

        [Fact]
        public void UnicodeStringDefaultReplacement()
        {
            const string expected = @"Here is some random text (â¡â¢â£ â¥)";

            var actual = UnicodeString.ReplaceChars();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData((char)8239, ' ')]
        [InlineData((char)0x2018, '\'')]
        [InlineData((char)0x2019, '\'')]
        [InlineData((char)0x201C, '\"')]
        [InlineData((char)0x201D, '\"')]
        [InlineData((char)0x2013, '-')]
        public void SwapCharactersGreaterThan255_UsingBaseReplacements(char input, char expected)
        {
            var actual = input.ToString().ReplaceChars();

            Assert.Equal(expected, actual[0]);
        }

        [Fact]
        public void SwapCharsGreaterThan255_UsingReplacementOverload()
        {
            var replacements = new Dictionary<char, char> {{ 'დ', ' '}, { 'ݾ', ' '}};
            const string input = @"Howდnow brownݾcow";
            const string expected = @"How now brown cow";

            var actual = input.ReplaceChars(replacements);

            Assert.Equal(expected, actual);
        }
    }
}
