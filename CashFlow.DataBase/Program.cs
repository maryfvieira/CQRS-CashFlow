namespace CashFlow.DataBase;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Forneça a connection string como argumento!");
            return;
        }

        string connectionString = args[0];
        DatabaseMigrator.Migrate(connectionString);
        
        Console.WriteLine("Migrations aplicadas com sucesso!");
    }
}