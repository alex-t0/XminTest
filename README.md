## Problem

Our application requires the use of optimistic locking and must maintain compatibility with MS SQL, vanilla PostgreSQL, and now PostgresPro as well.

However, in PostgresPro, there is an issue: the data type xid has been changed — it is now 8 bytes in size. Meanwhile, 
in PostgreSQL (and PostgresPro), there is an xid8 data type that is exactly 8 bytes long. Due to this change in an existing data type, 
libraries like Npgsql do not understand this modification and crash with errors. Npgsql relies on the type's Oid to determine which .NET CLR 
type should be used. And since the Oid remains the same — 28, instead of 5069 (which is for xid8) — the following exception occurs:

```
exception: The read on this field has not consumed all of its bytes (pos: 4, len: 8)
 Stacktrace: 
   at Npgsql.Internal.PgReader.ThrowNotConsumedExactly()
   at Npgsql.Internal.PgReader.EndRead()
   at Npgsql.NpgsqlDataReader.GetFieldValueCore[T](Int32 ordinal)
   at Npgsql.NpgsqlDataReader.GetFieldValue[T](Int32 ordinal)
   at lambda_method39(Closure, DbDataReader, Int32)
   at Microsoft.EntityFrameworkCore.RelationalPropertyExtensions.GetReaderFieldValue(IProperty property, RelationalDataReader relationalReader, Int32 ordinal, Boolean detailedErrorsEnabled)
   at Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal.NpgsqlModificationCommand.PropagateResults(RelationalDataReader relationalReader)
   at Npgsql.EntityFrameworkCore.PostgreSQL.Update.Internal.NpgsqlModificationCommandBatch.Consume(RelationalDataReader reader, Boolean async, CancellationToken cancellationToken)
```

## Demo App

- Create empty database;
- Use XminTest/SQL/SuperHero.sql to create SuperHero table;
- In Configuration.cs change connection string to your correct;
- Also in Configuration.cs you may change MakeCustomMappingForPostgresPro value (but if it equals true another exception occurs: can't map ulong to xid)
- Build in Rider and run (or simple `dotnet run`).

## Other variants

Using property with ulong type (Timestamp2 in this example) also will not work - for same reasons. You can switch to this approach by setting UseLongForXmin to true.
