using System.Data;
using System.Reflection;
using System.Text.Json;
using MySqlConnector;

namespace SecureForms.Data;

public record DatabaseSettings(string Server, uint Port, string Database, string User, string Password)
{
    public static string SettingsPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CelineSecureFormsLab3", "database.json");
    public static DatabaseSettings Load() => File.Exists(SettingsPath)
        ? JsonSerializer.Deserialize<DatabaseSettings>(File.ReadAllText(SettingsPath)) ?? Defaults()
        : Defaults();
    public static DatabaseSettings Defaults() => new("127.0.0.1", 3306, "secure_forms_lab3", "root", "");
    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
        File.WriteAllText(SettingsPath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }
    public string ConnectionString(bool includeDatabase = true)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = Server, Port = Port, UserID = User, Password = Password,
            Database = includeDatabase ? Database : "", ConnectionTimeout = 8,
            DefaultCommandTimeout = 20
        };
        return builder.ConnectionString;
    }
}

public sealed class Database
{
    private readonly DatabaseSettings settings;
    public Database(DatabaseSettings settings) => this.settings = settings;
    public MySqlConnection Open()
    {
        var connection = new MySqlConnection(settings.ConnectionString());
        try { connection.Open(); return connection; }
        catch { connection.Dispose(); throw; }
    }
    public void Initialize()
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(settings.Database, "^[a-zA-Z0-9_]{1,64}$"))
            throw new InvalidOperationException("Use only letters, digits, and underscores in the database name.");
        using (var connection = new MySqlConnection(settings.ConnectionString(false)))
        {
            connection.Open();
            using var command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{settings.Database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci", connection);
            command.ExecuteNonQuery();
        }
        var assembly = Assembly.GetExecutingAssembly();
        string resource = assembly.GetManifestResourceNames().Single(n => n.EndsWith("schema.sql"));
        using var stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        using var c = Open();
        foreach (string sql in reader.ReadToEnd().Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            if (string.IsNullOrWhiteSpace(sql)) continue;
            using var command = new MySqlCommand(sql, c);
            command.ExecuteNonQuery();
        }
    }
    public static MySqlCommand Command(MySqlConnection c, MySqlTransaction? tx, string sql, params (string, object?)[] values)
    {
        var command = new MySqlCommand(sql, c, tx);
        foreach (var (name, value) in values) command.Parameters.AddWithValue(name, value ?? DBNull.Value);
        return command;
    }
    public static int Execute(MySqlConnection c, MySqlTransaction? tx, string sql, params (string, object?)[] values)
    {
        using var command = Command(c, tx, sql, values);
        return command.ExecuteNonQuery();
    }
    public static long Insert(MySqlConnection c, MySqlTransaction tx, string sql, params (string, object?)[] values)
    {
        using var command = Command(c, tx, sql, values);
        command.ExecuteNonQuery(); return command.LastInsertedId;
    }
    public static void Audit(MySqlConnection c, MySqlTransaction tx, int? user, string action, string details) =>
        Execute(c, tx, "INSERT INTO audit_logs(user_id,action,details) VALUES(@u,@a,@d)", ("@u", user), ("@a", action), ("@d", details));
}
