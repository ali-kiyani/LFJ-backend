using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using LFJ.Configuration;
using LFJ.Web;

namespace LFJ.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class LFJDbContextFactory : IDesignTimeDbContextFactory<LFJDbContext>
    {
        public LFJDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<LFJDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            LFJDbContextConfigurer.Configure(builder, configuration.GetConnectionString(LFJConsts.ConnectionStringName));

            return new LFJDbContext(builder.Options);
        }
    }
}
