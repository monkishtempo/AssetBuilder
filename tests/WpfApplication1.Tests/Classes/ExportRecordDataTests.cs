using System.Diagnostics.CodeAnalysis;
using AssetBuilder.Classes;
using Xunit;

namespace Asset_Builder.Tests.Classes
{
    [ExcludeFromCodeCoverage]
    public class ExportRecordDataTests
    {
        private ExportRecordData _testItem;

        private const string JiraPlaceholder = "Enter related JIRA Ticket number(s)";

        private const string ReasonPlaceholder = "Reason for the export?";

        private const string ValidText = "ANY";
        
        [Theory]
        [InlineData("TKT-101", "Bob", "Export", "")]
        [InlineData("TKT-102", "Amy", "Experiment", "https://blackhole.com/gravitysucks")]
        [InlineData(".", ".", ".", "")]
        public void ValidEnteredData_IsValid(string tickets, string author, string reason, string noteLink)
        {
            _testItem = new ExportRecordData(tickets, author, reason, noteLink);

            Assert.True(_testItem.IsValid);
        }

        [Fact]
        public void NoteLink_IsOptional()
        {
            _testItem = new ExportRecordData(ValidText, ValidText, ValidText, string.Empty);

            Assert.True(_testItem.IsValid);
        }

        [Fact]
        public void UsingDefaultJiraPlaceholder_IsNotValid()
        {
            _testItem = new ExportRecordData(JiraPlaceholder, ValidText, ValidText, ValidText);

            Assert.False(_testItem.IsValid);
        }

        [Fact]
        public void UsingDefaultReasonPlaceholder_IsNotValid()
        {
            _testItem = new ExportRecordData(ValidText, ValidText, ReasonPlaceholder, ValidText);

            Assert.False(_testItem.IsValid);
        }

        [Fact]
        public void ConstructorSuppliedDefaultValues_IsNotValid()
        {
            _testItem = new ExportRecordData(ValidText);

            Assert.False(_testItem.IsValid);
        }

        [Fact]
        public void ConstructorSuppliedDefaultValues_ArePresent()
        {
            _testItem = new ExportRecordData(ValidText);

            Assert.Equal(ValidText, _testItem.ExportedBy);
            Assert.Equal(string.Empty, _testItem.AlgoReleaseNoteLink);
            // Null if watermark used:
            Assert.Null(_testItem.JiraReferences);
            Assert.Null(_testItem.Reason);
        }
    }
}