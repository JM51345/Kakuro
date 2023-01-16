using System;
using System.IO;
using static System.Console;

namespace Kakuro
{
    class Program
    {
        static void Main()
        {
            TestRun();
        }

        public static void TestRun()
        {
            string input;
            do
            {
                try
                {
                    Clear();
                    Title = "Kakuro";
                    WriteLine("Save files in Data-folder:");
                    DirectoryInfo d = new DirectoryInfo(Environment.CurrentDirectory + "\\Data");
                    FileInfo[] Files = d.GetFiles("*.txt");

                    foreach (FileInfo file in Files)
                    {
                        WriteLine(file.Name);
                    }
                    WriteLine("");

                    Write("Select save number(quit to exit): ");
                    input = ReadLine();
                    if (input != null)
                    {
                        if (input.Equals("quit"))
                        {
                            break;
                        }
                        else if (int.Parse(input) > 0 && int.Parse(input) < 30)
                        {
                            ApplicationPlay.RunGame(short.Parse(input));
                        }
                    } 
                }
                catch (Exception e)
                {
                    Clear();
                    WriteLine(e.Message);
                    Write("Enter to continue...");
                    ReadLine();
                }

            } while (true);
        }
    }
}
