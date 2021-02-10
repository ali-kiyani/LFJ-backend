using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using LFJ.Authorization.Roles;
using LFJ.Authorization.Users;
using LFJ.MultiTenancy;

namespace LFJ.EntityFrameworkCore
{
    public class LFJDbContext : AbpZeroDbContext<Tenant, Role, User, LFJDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public DbSet<Agents.Agents> Agents { get; set; }
        
        public LFJDbContext(DbContextOptions<LFJDbContext> options)
            : base(options)
        {
        }
    }
}
