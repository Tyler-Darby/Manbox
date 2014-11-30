using System;
using System.Linq;

namespace Satan
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) => Environment.FailFast("yolo");
            try
            {
                var dicPathArg = args.FirstOrDefault(arg => arg.StartsWith("dicpath="));
                if (dicPathArg != null)
                {
                    var parts = dicPathArg.Split('=');
                    Hell.DictionaryPath = parts[1];
                }

                var seedArg = args.FirstOrDefault(arg => arg.StartsWith("do="));
                if (seedArg != null)
                {
                    var parts = seedArg.Split('=');
                    var seed = parts[1];
                    Hell.Load();
                    Console.Write(Hell.GeneratePage(seed));
                }
                else
                {
                    Console.Write("No seed specified!");
                }
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex);
            }
            finally
            {
                Environment.Exit(0);
            }
        }
    }
}
