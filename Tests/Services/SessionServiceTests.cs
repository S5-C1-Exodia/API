using API.Services;
using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;
using Moq;
using MySqlConnector;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="SessionService"/>.
    /// </summary>
    public class SessionServiceTests
    {
        private readonly Mock<ISessionDao> _dao = new();
        private readonly Mock<IIdGenerator> _ids = new();
        private readonly Mock<IClockService> _clock = new();

        [Fact]
        public async Task CreateSessionAsync_ShouldGenerateId_AndInsert()
        {
            _ids.Setup(i => i.NewSessionId()).Returns("session123");
            var now = DateTime.UtcNow;
            var svc = new SessionService(_dao.Object, _ids.Object, _clock.Object);

            string sid = await svc.CreateSessionAsync("deviceX", now, now.AddMinutes(30));

            Assert.Equal("session123", sid);
            _dao.Verify(d => d.InsertAsync(It.Is<AppSession>(s => s.SessionId == "session123")), Times.Once);
        }

        [Fact]
        public async Task GetSessionAsync_ShouldCallDao()
        {
            var expected = new AppSession("sid", "dev", DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1));
            _dao.Setup(d => d.GetAsync("sid")).ReturnsAsync(expected);
            var svc = new SessionService(_dao.Object, _ids.Object, _clock.Object);

            var res = await svc.GetSessionAsync("sid");

            Assert.Equal(expected, res);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDao()
        {
            var svc = new SessionService(_dao.Object, _ids.Object, _clock.Object);

            await svc.DeleteAsync("sid");

            _dao.Verify(d => d.DeleteAsync("sid"), Times.Once);
        }
        
    }
}