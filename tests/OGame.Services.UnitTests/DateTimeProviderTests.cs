using System;
using System.Threading.Tasks;
using Xunit;

namespace OGame.Services.UnitTests
{
    public class DateTimeProviderTests
    {
        [Fact]
        public async Task Now_ReturnsCurrentUtcDateTime()
        {
            var service = new DefaultDateTimeProvider();
            var expectedLow = DateTime.UtcNow;

            var actual = await service.Now();
            Assert.True(actual >= expectedLow);
            Assert.True(actual <= DateTime.UtcNow);
        }
    }
}
