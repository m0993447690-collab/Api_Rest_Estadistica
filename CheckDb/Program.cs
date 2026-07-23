using System;
using MySqlConnector;

class Program
{
    static void Main(string[] args)
    {
        string connStr = "Server=localhost;Database=Api_GolMundial;User=root;Password=123m;";
        using (var conn = new MySqlConnection(connStr))
        {
            conn.Open();
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM grupos", conn))
            {
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"Grupos count: {count}");
            }
            using (var cmd = new MySqlCommand("SELECT COUNT(*) FROM selecciones", conn))
            {
                var count = cmd.ExecuteScalar();
                Console.WriteLine($"Selecciones count: {count}");
            }
        }
    }
}
