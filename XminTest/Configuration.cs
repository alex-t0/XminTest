namespace XminTest;

public static class Configuration
{
    public const string ConnectionString = "Host=10.2.100.156;Port=5432;Username=postgres;Password=postgres;Database=XminTest";
    
    /// <summary>
    /// allow to disable custom mapping to ulong, even if postgresPro used
    /// </summary>
    public const bool MakeCustomMappingForPostgresPro = true;
    
    /// <summary>
    /// try to use real ulong field
    /// </summary>
    public const bool UseLongForXmin = false;
}