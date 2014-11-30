using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Nancy;
using Nancy.Helpers;

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

        public const int MaxManboxExecutionTime = 3000;

        public const int MaxOutputSize = 32768;

        private const int MaxSatanExecutionTime = 100000;

        private const string LoremIpsum = @"[sync:phrase;cdeck][before:[caps:first]][rep:6][sep:\s]{<noun.plural::=a>... so many <noun.plural::=a>.|{All my life|For [num:2;50] <timenoun.plural>}, I have {endeavored|tried|longed} to <verb> <noun.plural>.|<verb.noun::=a> isn't <verb.noun::=a> without {some|a little|a dash of|a bit of} {<substance>|<noun.plural>|<adj.ness>}.|They say that <verb.noun> is {bad|good} for the <noun-body>... {That is <adj>|They lie|It's the [chance:50]{{fucking|goddamn}\s}truth|I believe it[chance:10]{ <adv>}}.|And that{'s|\swas} when I [chance:50]{{finally|at last}\s}{realized|noticed}, {drinking|consuming|ingesting} <substance> is how I get my {<noun>|<substance>}.|<name> once told me that for every <noun> that <verb.s>, a <noun> gets its <noun.plural>.|I came to this place in hopes of finding the {long-lost|legendary|sacred} [caps:word]<noun>-<verb.er>[caps:none].|After I got <verb.ed>, I was <adv> <adj> with <adj.ness>. It was truly a <verb.ing> <timenoun> in my life.|So here I am, <verb.ing> a <noun>, <verb.ing> my <noun-body> and {checking out|looking at|examining|spying} {this <noun>|my <rel>|some {guy|girl|kid|woman|man|dude}} <verb.ing> {in the corner|across from me}. {Is this what my life has come to|What has my life come to}?|I {like|love|hate} <noun.plural>{.|!}|Do you like {to <verb> <noun.plural>|<verb.ing> <noun.plural>}?|I'm not sure whether my <noun> is <adj>, or I {just|simply} have a <adj> <noun-body>...|[rep:[num:6;14]][caps:first]{a|e|i|o|u|y}{.|?|!}|I would love to <verb> your <noun> right now.|Have you heard of <name> <surname>? [caps:first]<pron.nom::=a>'s a {<adj> <noun><verb.er>|<job>|[num:1;10]-<noun-body> <noun-animal>|the {best|worst} <verb.er> of <noun.plural>}.|For {some reason|reasons unknown}, <name> {likes to <verb>|enjoys <verb.ing>} <noun.plural> <adv>. {I think it's <em> <adj> of <pron> to do|That's <adv> unacceptable|<x>}.|<x>, what is going on with your <noun in body>? It's <em> <adj-appearance>{.|!}|I think it's time to <verb> some <adj> <noun.plural>{.|!}|[rep:[num:8;24]][sep:\sand\s][caps:first]{<noun.plural>|<verb.noun>}...}";

        private static readonly RantEngine _manhood = new RantEngine("dictionary");

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
                response["text"] = RohBot.RunPat(parts[1]);
                return response.ToString();
            };

            //Get["/hell/"] = p => new RedirectResponse("/hell/" + GeneratePageName());

            //Get["/hell/{name}"] = p => RunSatan(p.name);

            Post["/rantbox/run"] = p =>
            {
                return RunPat(Request.Form.code, (Request.Form.nsfw ?? "false").ToString().ToLower() == "true");
            };

            Post["/rantbox/fetch"] = p => FetchPage(Request.Form.Hash);

            Get["/rantbox/lorem_ipsum"] = p => _manhood.Do(LoremIpsum).MainValue;
        }

        public static string GeneratePageName()
        {
            return _manhood.Do(@"[caps lower]<adj without [\\s'-]><noun.plural without [\\s'-]>");
        }

        public static long HellHash(string input)
        {
            long seed = 47;
            foreach (char c in input)
            {
                seed += c + 11;
                seed *= 12345;
            }
            return seed;
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

        private static string RunSatan(string seed)
        {
            try
            {
                var args = String.Concat("dicpath=../dictionary do=", seed);

                var psi = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = "wsfh",
                    FileName = _.IsMono ? "mono" : "wsfh/Satan.exe",
                    Arguments = _.IsMono ? "Satan.exe " + args : args
                };

                var proc = Process.Start(psi);

                if (proc == null)
                {
                    return "Interpreter failed to run.";
                }

                if (!proc.WaitForExit(MaxSatanExecutionTime))
                {
                    proc.Kill();
                    return "Satan could not make your page fast enough. Reload the page and hopefully he will do better.";
                }
                
                var error = proc.StandardError.ReadToEnd();
                var output = proc.StandardOutput.ReadToEnd();
                return error.Any() ? error : output;
            }
            catch (Exception ex)
            {
                return ex.ToString();
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