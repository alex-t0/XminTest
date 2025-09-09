using System.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;
using XminTest.Entities;

namespace XminTest;

public class AwesomeDbContext : DbContext
{
    public DbSet<SuperHero> Heroes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseNpgsql(Configuration.ConnectionString)
            .LogTo(Console.WriteLine, LogLevel.Debug)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
    }

    private static volatile int _xminSize;

    private static int GetXminSize(DatabaseFacade database)
    {
        if (_xminSize > 0) return _xminSize;

        using (var connection = new NpgsqlConnection(Configuration.ConnectionString))
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using (var command = new NpgsqlCommand("""
                                                   	SELECT typlen
                                                     FROM pg_type
                                                     WHERE oid = 'xid'::regtype
                                                   """, connection))
            {
                var result = Convert.ToInt32(command.ExecuteScalar());

                Interlocked.CompareExchange(ref _xminSize, result, default);

                return _xminSize;
            }
        }
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var byteArrayValueComparer = new ValueComparer<byte[]>(
            (x,y) => y != null && x.SequenceEqual(y),
            x => x.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            x => x.ToArray());

        Expression<Func<byte[], uint>> convertToUint = to => BitConverter.ToUInt32(to);
        Expression<Func<uint, byte[]>> convertFromUint = from => BitConverter.GetBytes(from);

        Expression<Func<byte[], ulong>> convertToUlong = to => BitConverter.ToUInt64(to);
        Expression<Func<ulong, byte[]>> convertFromUlong = from => BitConverter.GetBytes(from);

        var xminSize = GetXminSize(Database);

        Console.WriteLine("Init: xid size is " + xminSize);
        
        var entityBuilder = modelBuilder.Entity<SuperHero>();
        
        if (xminSize == sizeof(uint) || Configuration.MakeCustomMappingForPostgresPro == false)
        {
            if (Configuration.MakeCustomMappingForPostgresPro == false)
            {
                Console.WriteLine("Init: vanilla Postgres configuration forced");
            }
            
            Console.WriteLine("Init: Using vanilla Postgres mapping");

            if (!Configuration.UseLongForXmin)
            {
                entityBuilder.Property(x => x.Timestamp)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .HasConversion(
                        convertToUint,
                        convertFromUint,
                        byteArrayValueComparer);

                entityBuilder.Ignore(x => x.Timestamp2);
            }
            else
            {
                entityBuilder.Ignore(x => x.Timestamp);
                
                entityBuilder.Property(x => x.Timestamp2)
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .IsRowVersion();
            }
        }
        else
        {
            // Here we are for PostgresPro
            
            Console.WriteLine("Init: Using PostgresPro mapping");

            if (!Configuration.UseLongForXmin)
            {
                entityBuilder.Property(x => x.Timestamp)
                    .HasColumnName("xmin")
                    .HasColumnType("xid8") // no effect: Npgsql use xid anyway
                    .HasConversion(
                        convertToUlong,
                        convertFromUlong,
                        byteArrayValueComparer);
                
                entityBuilder.Ignore(x => x.Timestamp2);
            }
            else
            {
                entityBuilder.Ignore(x => x.Timestamp);
                
                entityBuilder.Property(x => x.Timestamp2)
                    .HasColumnName("xmin")
                    .HasColumnType("xid8")
                    .IsRowVersion();
            }
            
        }
    }
}