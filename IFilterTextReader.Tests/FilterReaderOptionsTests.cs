using FluentAssertions;
using IFilterTextReader;
using Xunit;

namespace IFilterTextReader.Tests
{
    public class FilterReaderOptionsTests
    {
        [Fact]
        public void DefaultValues_AreExpected()
        {
            var opts = new FilterReaderOptions();

            opts.DisableEmbeddedContent.Should().BeFalse();
            opts.IncludeProperties.Should().BeFalse();
            opts.ReadIntoMemory.Should().BeFalse();
            opts.ReaderTimeout.Should().Be(FilterReaderTimeout.NoTimeout);
            opts.Timeout.Should().Be(-1);
            opts.DoCleanUpCharacters.Should().BeTrue();
            opts.WordBreakSeparator.Should().Be("-");
            opts.ChunkTypeSeparator.Should().Be(" ");
            // New option
            // If property is missing the test will fail (draws attention)
            opts.GetType().GetProperty("UseEncodingDetection").Should().NotBeNull();
            // default should be false
            var useDetect = (bool)opts.GetType().GetProperty("UseEncodingDetection").GetValue(opts);
            useDetect.Should().BeFalse();
        }
    }
}
