using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;

namespace MinesweeperBot
{
    public abstract class DBObject
    {
        public string Type;
        public string ID;

        public abstract string Serialize();
        public abstract void Deserialize(string s);

        public void SaveToDatabase(SQLiteConnection sqlite)
        {
            if (ID == null || ID.Length < 1) ID = FormatHelper.hash(Serialize());

            SQLiteCommand cmd = sqlite.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Objects WHERE `ID` == @id";
            cmd.Parameters.Add(new SQLiteParameter("@id", ID));
            SQLiteDataReader reader = cmd.ExecuteReader();
            reader.Read();
            bool ID_exists = reader.GetInt32(0) > 0;

            if (ID_exists)
            {
                cmd = sqlite.CreateCommand();
                cmd.CommandText = "UPDATE Objects SET `Data` = @data WHERE `ID` == @id";
                cmd.Parameters.Add(new SQLiteParameter("@id", ID));
                cmd.Parameters.Add(new SQLiteParameter("@data", Serialize()));
                int count = cmd.ExecuteNonQuery();
            }
            else
            {
                cmd = sqlite.CreateCommand();
                cmd.CommandText = "INSERT INTO Objects (`ID`, `Type`, `Data`) VALUES (@id, @type, @data)";
                cmd.Parameters.Add(new SQLiteParameter("@id", ID));
                cmd.Parameters.Add(new SQLiteParameter("@data", Serialize()));
                cmd.Parameters.Add(new SQLiteParameter("@type", Type));
                int count = cmd.ExecuteNonQuery();
            }
        }
    }
}
