using Microsoft.EntityFrameworkCore;

public class Context:DbContext
{

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //not use user and password
        optionsBuilder.UseSqlServer("Data Source=.;initial Catalog=test;MultipleActiveResultSets=true;integrated security=true;trust server certificate=true");
    }
    
}