namespace SignatureApp
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    public class Program
    {
        private static SignatureGenerator _generator = new SignatureGenerator();

        private static int Main()
        {
            SignatureGenerator.GenerationEvent += delegate (object sender, GenerationArgs e)
            {
                Console.WriteLine("n: {0,-10} hash: {1}", e.ChunkNumber, e.HashValue);
            };

            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                _generator.Cancel();
            };

            Console.WriteLine("Exiting system due to external CTRL-C...");

            do
            {
                Console.Write("File path: ");
                var chosenFile = Console.ReadLine();

                if (File.Exists(chosenFile))
                {
                    Console.Write("Block size: ");

                    if (!int.TryParse(Console.ReadLine(), out int size))
                    {
                        Console.WriteLine("\nFile size has invalid format!");
                        continue;
                    }
                    try
                    {
                        using (var stream = new FileStream(chosenFile, FileMode.Open, FileAccess.Read))
                        {
                            _generator.ChunkSize = size;
                            _generator.ComputeHash<MD5CryptoServiceProvider>(stream);
                        }

                        Console.WriteLine("\nDo you want to continue (Y/N)?");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nAn error has occurred: " + ex.Message);
                        // Logger.Log(ex);
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine("\nFile does not exist!");
                    continue;
                }
            }
            while (Console.ReadKey(true).Key != ConsoleKey.N);

            return 0;
        }
    }
}
