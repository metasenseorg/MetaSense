using SQLite;

namespace NodeLibrary.Native
{
    public interface IDatabase
    {
        SQLiteConnection Connection { get; }
    }
}