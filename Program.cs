using System;
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace TBPTServer
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            Server.Start(new IPEndPoint(IPAddress.Any, 5555), 10);

            while (true)
            {
                Console.Write("\n>$ ");
                RunCommand(Console.ReadLine());
            }
        }

        public static void RunCommand(string c)
        {
            c = c.ToLower();

            if (c == "createpackage")
            {
                Console.Write("Name: ");
                string name = Console.ReadLine();

                Console.Write("Version: ");
                string version = Console.ReadLine();

                Console.Write("Repository: ");
                string repository = Console.ReadLine();

                Console.Write("Zip file: ");
                string zipFile = Console.ReadLine();

                File.Create(Server.PackagesFolder + name + ".json").Close();
                File.WriteAllText(Server.PackagesFolder + name + ".json", JsonConvert.SerializeObject(new TPackage()
                {
                    Name = name,
                    Version = version,
                    Repository = repository,
                    Data = File.ReadAllBytes(zipFile)
                }));
            }
            else if (c == "clear")
            {
                Console.Clear();
            }
        }
    }
}
