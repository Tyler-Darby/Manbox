using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Helpers;
using Newtonsoft.Json;
using WebSocketSharp;

using Manbox.CmdLine;

namespace Manbox
{
    public static class Bot
    {
        private static readonly SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
        private static WebSocket ws = null;

        public static string RohBotUsername = Property("rohuser");
        public static string RohBotPassword = Property("rohpass");

        public static string RunPat(string pattern)
        {
            var test = new String(pattern.ToLower().Where(c => c == ' ' || Char.IsLetterOrDigit(c)).ToArray());
            return Regex.Replace(_RunPat(pattern), @"^\s*(\/|manbot:\s*)+", "");
        }

        private static string _RunPat(string pattern)
        {
            string output = "";

            try
            {
                bool timeout = false;

                timeout = !_.TryExecute(() =>
                {
                    try
                    {
                        var engine = _.CreateEngine();
                        return engine.Do(pattern, Webserver.MaxOutputSize);
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                }, Webserver.MaxManboxExecutionTime, out output);

                if (timeout)
                {
                    output = "Interpreter timed out.";
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex);
                Console.ResetColor();
                output = "<Server error>";
            }

            return output;
        }

        public static async void ConnectRohBot()
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Logging into RohBot...");
                ws = new WebSocket("ws://fpp.literallybrian.com:12000/ws/");
                
                ws.OnOpen += (sender, args) =>
                {
                    var msg = new
                    {
                        Type = "auth",
                        Method = "login",
                        Username = RohBotUsername,
                        Password = RohBotPassword
                    };

                    ws.Send(JsonConvert.SerializeObject(msg));
                    ws.Send(JsonConvert.SerializeObject(new
                    {
                        Type = "sendMessage",
                        Target = "home",
                        Content = "/join manbot"
                    }));
                };

                ws.OnClose += (sender, args) =>
                {
                    Console.WriteLine("Lost connection to RohBot.");
                    Thread.Sleep(5000);
                    ConnectRohBot();
                };

                ws.OnError += (sender, args) => Console.WriteLine("FUCK SHIT PISS " + args.Exception.ToString());

                ws.OnMessage += (sender, args) =>
                {
                    var msg = args.Data;
                    dynamic obj = JsonConvert.DeserializeObject(msg);

                    if (obj.Type == "sysMessage")
                    {
                        Console.WriteLine("(RohBot) \{obj.Content}");
                        return;
                    }

                    if (obj.Type != "message")
                        return;

                    var content = HttpUtility.HtmlDecode((string)obj.Line.Content);

                    if (content.Length > 32768)
                        return;

                    var parts = content.Split(new[] {':'}, 2);

                    if (parts.Length != 2) return;
                    if (parts[0].ToLower().Trim() != "manbot") return;

                    var response = new
                    {
                        Type = "sendMessage",
                        Target = "manbot",
                        Content = RunPat(parts[1])
                    };

                    ws.Send(JsonConvert.SerializeObject(response));
                };

                ws.Connect();
            });
        }
    }
}