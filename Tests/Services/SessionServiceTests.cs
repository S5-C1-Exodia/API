using Api.Managers.InterfacesDao;
using API.Managers.InterfacesServices;
using Api.Models;
using API.Services;
using Moq;

namespace Tests.Services
{
    /// <summary>
    /// Unit tests for <see cref="SessionService"/>.
    /// </summary>
    public class SessionServiceTests
    {
        /// <summary>
        /// Tests that <see cref="SessionService.CreateSessionAsync(string, DateTime, DateTime)"/> inserts a session with the expected fields and returns the session ID.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task CreateSessionAsync_ShouldInsert_WithExpectedFields_AndReturnSessionId()
        {
            Mock<ISessionDao> dao = new Mock<ISessionDao>();
            Mock<IIdGenerator> ids = new Mock<IIdGenerator>();

            string expectedId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            ids.Setup(i => i.NewSessionId()).Returns(expectedId);

            AppSession captured = null;
            dao.Setup(d => d.InsertAsync(It.IsAny<AppSession>()))
                .Callback<AppSession>(s => captured = s)
                .Returns(Task.CompletedTask);

            SessionService service = new SessionService(dao.Object, ids.Object);

            DateTime created = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            DateTime expires = created.AddHours(1);

            string returned = await service.CreateSessionAsync("Android Pixel 7", created, expires);

            Assert.Equal(expectedId, returned);
            Assert.NotNull(captured);
            Assert.Equal(expectedId, captured.SessionId);
            Assert.Equal("Android Pixel 7", captured.DeviceInfo);
            Assert.Equal(created, captured.CreatedAt);
            Assert.Equal(created, captured.LastSeenAt);
            Assert.Equal(expires, captured.ExpiresAt);

            dao.Verify(d => d.InsertAsync(It.IsAny<AppSession>()), Times.Once);
        }

        /// <summary>
        /// Tests that <see cref="SessionService.GetSessionAsync(string)"/> retrieves a session from the DAO.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Fact]
        public async Task GetSessionAsync_ShouldReturnFromDao()
        {
            Mock<ISessionDao> dao = new Mock<ISessionDao>();
            Mock<IIdGenerator> ids = new Mock<IIdGenerator>();

            AppSession session = new AppSession("sid123", "dev", DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow.AddHours(1));
            dao.Setup(d => d.GetAsync("sid123")).ReturnsAsync(session);

            SessionService service = new SessionService(dao.Object, ids.Object);

            AppSession got = await service.GetSessionAsync("sid123");

            Assert.Same(session, got);
            dao.Verify(d => d.GetAsync("sid123"), Times.Once);
        }
    }
}