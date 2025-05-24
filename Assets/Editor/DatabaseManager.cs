using UnityEngine;
using Mono.Data.Sqlite;
using UnityEditor.MemoryProfiler;

public class DatabaseManager : MonoBehaviour
{
    private string dbPath;

    void Start()
    {
        dbPath = "URI=file" + Application.persistentDataPath + "/gameDatabase.db";
        CreateDatabase();
    }

    void CreateDatabase()
    {
        SqliteConnection connection = new SqliteConnection(dbPath);
        connection.Open();
        SqliteCommand command = connection.CreateCommand();
        command.CommandText = "CREATE TABLE IF NOT EXISTS players (id INTEGER PRIMARY KEY, name TEXT, password TEXT, balance DOUBLE)";
        command.ExecuteNonQuery();
        connection.Close();
    }
}
