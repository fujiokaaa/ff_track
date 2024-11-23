using SQLite;

namespace AltTrack;

[Table("Characters")]
public class DBCharacter
{
    [PrimaryKey, AutoIncrement, Column("id")]
    public int Id { get; set; }

    [Column("accountid")]
    public ulong AccountId { get; set; }

    [Column("contentid")]
    public ulong ContentId { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("homeworld")]
    public uint HomeWorld { get; set; }
}

public class Database : IDisposable
{
    private SQLiteConnection _conn;

    public Database(string path)
    {
        _conn = new(path);
        _conn.CreateTable<DBCharacter>();
        Service.Log.Info($"foo");
    }

    public void Dispose()
    {
        _conn.Dispose();
    }

    public void Update(ulong accountId, ulong contentId, string name, uint homeworld)
    {
        var entries = _conn.Query<DBCharacter>("SELECT * FROM Characters WHERE contentid = ?", contentId);
        var latestEntry = entries.LastOrDefault();
        if (latestEntry != null && latestEntry.Name == name && latestEntry.HomeWorld == homeworld)
            return; // already up-to-date
        Service.Log.Info($"New entry: aid={accountId:X}, cid={contentId:X}, name={name}, hw={homeworld}");
        _conn.Insert(new DBCharacter() { AccountId = accountId, ContentId = contentId, Name = name, HomeWorld = homeworld });
    }

    public List<DBCharacter> Entries(ulong accountId)
    {
        return _conn.Query<DBCharacter>("SELECT * FROM Characters WHERE accountid = ?", accountId);
    }
}
