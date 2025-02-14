#if DEBUG
using System.Threading.Tasks;
using Xunit;

namespace Litium.Accelerator.Controllers
{
    public class ApiControllerTest : TestBase
    {
        private readonly ApplicationFixture _app;

        public ApiControllerTest(ApplicationFixture app)
        {
            _app = app;
        }

        [Fact]
        public async Task TestPingEndpoint_get()
        {
            // arrange
            var httpClient = await _app.CreateClientAsync();

            // act
            var result = await httpClient.GetAsync("/api/test/ping");

            // assert
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal("pong", await result.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task TestPingEndpoint_post_as_everyone()
        {
            // arrange
            var httpClient = await _app.CreateClientAsync();

            // act
            var result = await httpClient.PostAsync("/api/test/ping", null);

            // assert
            Assert.False(result.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.Forbidden, result.StatusCode);
        }

        [Fact]
        public async Task TestPingEndpoint_post_as_system()
        {
            // arrange
            using var _ = _app.ActAsSystem();
            var httpClient = await _app.CreateClientAsync();

            // act
            var result = await httpClient.PostAsync("/api/test/ping", null);

            // assert
            Assert.True(result.IsSuccessStatusCode);
            Assert.Equal("pong", await result.Content.ReadAsStringAsync());
        }
    }
}
#endif
