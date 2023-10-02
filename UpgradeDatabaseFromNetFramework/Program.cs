using System;
using LiteDB;

namespace UpgradeDatabaseFromNetFramework
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var db = new LiteDatabase($"Filename=c:\\temp\\pim\\Pim.db; Upgrade=true; Password=;");
                Console.WriteLine("Version: {0}", db.GetCollection("Note")?.Count());
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
            Console.ReadLine();
        }
    }
}