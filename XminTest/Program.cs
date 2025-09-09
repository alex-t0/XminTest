// See https://aka.ms/new-console-template for more information

using System.Transactions;
using XminTest;
using XminTest.Entities;

Console.WriteLine("Creating new SuperHero...");

try
{
    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
    {
        using (var context = new AwesomeDbContext())
        {
            context.Heroes.Add(new SuperHero
            {
                Id = 1,
                Name = "Kapitoshka"
            });
            
            context.SaveChanges();
        }
        
        scope.Complete();
    }
    
    Console.WriteLine("SuperHero created.");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine("Stacktrace: ");
    Console.WriteLine(ex.StackTrace);
    
    var nestedException = ex.InnerException;
    while (nestedException != null)
    {
        Console.WriteLine("Nested exception: " + nestedException.Message);
        Console.WriteLine("Stacktrace: ");
        Console.WriteLine(nestedException.StackTrace);
        
        nestedException = nestedException.InnerException;
    }
}
