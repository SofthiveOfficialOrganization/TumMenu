namespace WebAPI.Logging.Providers.ConfigurationModels;

public class MsSqlConfiguration
{
    /// <summary>
    /// Gets or sets the connection string for the Microsoft SQL Server database.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the table where log entries will be stored in the database.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to automatically create the log table if it does not exist.
    /// </summary>
    public bool AutoCreateSqlTable { get; set; }

    public MsSqlConfiguration()
    {
        ConnectionString = string.Empty;
        TableName = string.Empty;
    }

    public MsSqlConfiguration(string connectionString, string tableName, bool autoCreateSqlTable)
    {
        ConnectionString = connectionString;
        TableName = tableName;
        AutoCreateSqlTable = autoCreateSqlTable;
    }
}