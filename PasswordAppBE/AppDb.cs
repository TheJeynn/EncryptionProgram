using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PasswordApp;
using System.Collections.Generic;

public class AppUser : IdentityUser { }

public class AppDb : IdentityDbContext<AppUser>
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) { }

    public DbSet<Vault> Vaults { get; set; }
    public DbSet<SecretItem> SecretItems { get; set; }
}
