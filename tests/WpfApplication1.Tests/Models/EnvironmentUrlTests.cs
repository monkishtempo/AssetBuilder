using System;
using System.Diagnostics.CodeAnalysis;
using AssetBuilder.Models;
using Xunit;

namespace Asset_Builder.Tests.Models
{
    [ExcludeFromCodeCoverage]
    public class EnvironmentUrlTests
    {
        [Theory]
        [InlineData("Agnes", "Zephyrus", ComparisonResult.LeftBeforeRight)]
        [InlineData("Solitude", "Abandonment", ComparisonResult.RightBeforeLeft)]
        [InlineData("isolation", "isolation", ComparisonResult.Equal)]
        [InlineData("isolation", "ISOLATION", ComparisonResult.Equal)]
        public void Compare_UsesNames_IsCaseInsensitive(string env1, string env2, ComparisonResult compareResult)
        {
            const string baseUrl = "https://idontcare.com";
            var lhs = new EnvironmentUrl {BaseUrl = baseUrl, Name = env1};
            var rhs = new EnvironmentUrl {BaseUrl = baseUrl, Name = env2};

            var actual = lhs.CompareTo(rhs);

            switch (compareResult)
            {
                case ComparisonResult.LeftBeforeRight:
                    Assert.True(actual < 0, $"LHS: {env1}, RHS: {env2}, Expected: {compareResult} but was {(ComparisonResult)actual}");
                    break;
                case ComparisonResult.RightBeforeLeft:
                    Assert.True(actual > 0, $"LHS: {env1}, RHS: {env2}, Expected: {compareResult} but was {(ComparisonResult)actual}");
                    break;
                case ComparisonResult.Equal:
                    Assert.True(actual == 0, $"LHS: {env1}, RHS: {env2}, Expected: {compareResult} but was {(ComparisonResult)actual}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compareResult), compareResult, null);
            }
        }

        [Fact]
        public void CompareToNull_NullComesFirst()
        {
            const string baseUrl = "https://samaritans.org";
            const ComparisonResult expected = ComparisonResult.RightBeforeLeft;
            var lhs = new EnvironmentUrl {BaseUrl = baseUrl, Name = "LHS"};

            var actual = lhs.CompareTo(null);

            Assert.True(actual > 0, $"LHS: LHS, RHS: null, Expected: {expected} but was {(ComparisonResult)actual}");
        }

        [Fact]
        public void CompareReferenceOther_AreEqual()
        {
            const string baseUrl = "https://rethink.org";
            const ComparisonResult expected = ComparisonResult.Equal;
            var rhs = new EnvironmentUrl { BaseUrl = baseUrl, Name = "RHS" };
            var lhs = rhs;

            var actual = lhs.CompareTo(rhs);

            Assert.True(actual == 0, $"LHS: Reference to RHS, RHS: RHS, Expected: {expected} but was {(ComparisonResult)actual}");
        }

        public enum ComparisonResult
        {
            LeftBeforeRight = -1,
            Equal = 0,
            RightBeforeLeft = 1
        }
    }
}