using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace Web.Dal
{
    //public class EfContextFactory : IDesignTimeDbContextFactory<EfContext>
    //{
    //    public EfContext CreateDbContext(string[] args)
    //    {
    //        return Create("Server=localhost;Database=mydb;Username=postgres;Password=p@ssw0rd");
    //    }

    //    private EfContext Create(string connectionString)
    //    {
    //        if (string.IsNullOrEmpty(connectionString))
    //        {
    //            throw new ArgumentException($"{nameof(connectionString)} is null or empty", nameof(connectionString));
    //        }
    //        var optionsBuilder = new DbContextOptionsBuilder<EfContext>();
    //        return Configure(connectionString, optionsBuilder);
    //    }

    //    protected virtual EfContext Configure(string connectionString, DbContextOptionsBuilder<EfContext> builder)
    //    {
    //        builder.UseNpgsql(connectionString);

    //        EfContext db = new EfContext(builder.Options);
    //        return db;
    //    }
    //}
}