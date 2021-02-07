using System.Threading.Tasks;
using LFJ.Models.TokenAuth;
using LFJ.Web.Controllers;
using Shouldly;
using Xunit;

namespace LFJ.Web.Tests.Controllers
{
    public class HomeController_Tests: LFJWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}