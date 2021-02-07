using System.Threading.Tasks;
using LFJ.Configuration.Dto;

namespace LFJ.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
