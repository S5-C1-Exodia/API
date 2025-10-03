using API.Services;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ClockService"/>.
    /// </summary>
    public class ClockServiceTests
    {
        /// <summary>
        /// Tests that <see cref="ClockService.GetUtcNow"/> returns a value close to <see cref="DateTime.UtcNow"/>.
        /// </summary>
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