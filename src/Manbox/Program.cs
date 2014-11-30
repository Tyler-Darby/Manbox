using System;
using System.IO;
using System.Net;
using System.Threading;
using Nancy;
using Nancy.Hosting.Self;

using Manbox.CmdLine;

namespace Manbox
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            
            Console.Title = "Manbox";

            Console.WriteLine("Hello!");

            var watcher =
                    new FileSystemWatcher(
                        Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));

            watcher.Changed += WatcherOnChanged;
            watcher.EnableRaisingEvents = true;

            string url = Property("url", "http://localhost:8080");

            StaticConfiguration.DisableErrorTraces = false;
            ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) => true;

            RohBot.Connect();

            Console.WriteLine("Starting server...");

            using (var nancyHost = new NancyHost(
                new HostConfiguration { UrlReservations = new UrlReservations() { CreateAutomatically = true } },
                new Uri(url)))
            {
                nancyHost.Start();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Ready!");
                Console.WriteLine("Hosting on: {0}", url);
                Console.WriteLine("Kill URL: /kill/{0}", Webserver.KillKey);
                Console.ResetColor();

                Thread.Sleep(Timeout.Infinite);
            }
        }

        private static void WatcherOnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.Name.ToLower() == "manbox.exe")
            {
                Environment.Exit(0);
            }
        }
    }
}
