using API.Services;

namespace Tests.Services
{
    public class ClockServiceTests
    {
        [Fact]
        public void GetUtcNow_ShouldBeCloseToSystemUtcNow()
        {
            ClockService clock = new ClockService();

            DateTime before = DateTime.UtcNow;
            DateTime value = clock.GetUtcNow();
            DateTime after = DateTime.UtcNow;

            Assert.True(value >= before.AddSeconds(-1));
            Assert.True(value <= after.AddSeconds(1));
            Assert.Equal(DateTimeKind.Utc, value.Kind);
        }
    }
}