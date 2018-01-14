using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OGame.Services.UnitTests
{
    public class IdProviderTests
    {
        [Fact]
        public async Task NewId_Many_AreDistinct()
        {
            var count = 1000;
            var service = new IdProvider();
            var ids = await Task.WhenAll(Enumerable.Range(0, count).Select(x => service.NewId()));

            Assert.DoesNotContain(ids, id => id == Guid.Empty);
            Assert.Equal(ids.Length, ids.Distinct().Count());
        }
    }
}
