using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Nancy;
using Nancy.Helpers;
using Nancy.Responses;

using Newtonsoft.Json.Linq;

using Manbox.CmdLine;

using Rant;

namespace Manbox
{
    public class Webserver : NancyModule
    {
        private static readonly SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

        public static string KillKey = Property("killkey", GenerateKey());

        private static readonly Thread killThread = new Thread(() =>
        {
            Thread.Sleep(5000);
            Environment.Exit(0);
        });

        public const int MaxManboxExecutionTime = 5000;

        public const int MaxOutputSize = 32768;

        private static readonly RantEngine _rant = new RantEngine("dictionary");

        public Webserver()
        {
            Get["/"] = p => File.ReadAllText("pages/home.html");

            foreach (var path in Directory.GetFiles("pages", "*.html"))
            {
                string path1 = path;
                Get["/" + Path.GetFileNameWithoutExtension(path)] = p => File.ReadAllText(path1);
            }

            Get["/kill/" + KillKey] = p =>
            {
                Environment.Exit(0);
                return "";
            };

            Post["/kill/" + KillKey] = p =>
            {
                killThread.Start();
                return "Killing in 5 seconds...";
            };

            Post["/manbot_incoming"] = p =>
            {
                string message = HttpUtility.HtmlDecode(Request.Form.text);
                if (message == null) return "";
                var parts = message.Split(new[] {':'}, 2);
                if (parts.Length != 2) return "";
                var response = new JObject();
                response["text"] = Bot.RunPat(parts[1]);
                return response.ToString();
            };

            Post["/rantbox/run"] = p =>
            {
                return RunPat(Request.Form.code, (Request.Form.nsfw ?? "false").ToString().ToLower() == "true");
            };

            Post["/rantbox/fetch"] = p => FetchPage(Request.Form.Hash);
        }

        private static string GenerateKey()
        {
            RNG rng = new RNG();
            var sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                sb.Append("0123456789abcdefghijklmnopqrstuvwxyz"[rng.Next(36)]);
            }
            return sb.ToString();
        }

        private static string FetchPage(string hash)
        {
            hash = hash.StartsWith("#") ? hash.Substring(1) : hash;
            try
            {
                return File.ReadAllText("jobs/" + hash + ".mh");
            }
            catch
            {
                return "";
            }
        } 

        private static string RunPat(string pattern, bool nsfw = false)
        {
            const string OK = "ok";
            const string FAIL = "fail";

            var o = new JObject();

            var hash =
                sha1.ComputeHash(Encoding.UTF8.GetBytes(pattern ?? ""))
                    .Select(b => String.Format("{0:X2}", b))
                    .Aggregate((current, next) => current + next);

            o["hash"] = hash;
            o["success"] = FAIL;

            try
            {
                var filename = String.Concat("jobs/", hash, ".mh");

                if (!File.Exists(filename)) File.WriteAllText(filename, pattern);

                string result = "";
                bool timeout = false;
                bool error = false;

                timeout = !_.TryExecute(() =>
                {
                    try
                    {
                        var engine = _.CreateEngine(nsfw);
                        return engine.Do(pattern, MaxOutputSize);
                    }
                    catch (Exception ex)
                    {
                        error = true;
                        return ex.Message;
                    }
                }, MaxManboxExecutionTime, out result);

                if (timeout)
                {
                    result = "Interpreter timed out.";
                }

                o["result"] = result;
                
                o["success"] = timeout || error ? FAIL : OK;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex);
                Console.ResetColor();
                o["result"] = "<Server error>";
            }

            return o.ToString();
        }
    }
}