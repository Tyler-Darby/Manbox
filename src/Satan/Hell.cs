using System;
using System.IO;
using System.Linq;
using Google.API.Search;
using Manhood;

namespace Satan
{
    public static class Hell
    {
        public static string DictionaryPath = "dictionary";

        public const string ErrorPage = @"<!DOCTYPE html>
<html>
    <head>
        <title>Well, shit.</title>
    </head>
    <body>
        <h1>Something bad happened.</h1>
        <b>An internal server error occurred while fetching your content. The admin has been accordingly bombarded with console output.</b>
    </body>
</html>";

        private static ManhoodContext _manhood;
        private static GimageSearchClient _google;

        public static void Load()
        {
            _manhood = new ManhoodContext(DictionaryPath);
            _manhood.StringRequested += StringRequested;

            _manhood.DoFile("lib/css.mh");
            _manhood.DoFile("lib/stanman.mh");
            _manhood.DoFile("lib/wsfhpage.mh");

            _google = new GimageSearchClient("http://berkin.me");
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

        public static string GeneratePage(string name)
        {
            long seed = HellHash(name);
            string html = "";
            string path = String.Format("cache/{0:X16}.html", seed);
            bool cached = File.Exists(path);
            if (cached)
            {
                try
                {
                    html = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Console.Error.Write("Error while fetching cached page: " + ex.Message);
                    return ErrorPage;
                }
            }
            else
            {
                html = _manhood.Do("[$page]", seed);
            }
            if (cached) return html;

            try
            {
                File.WriteAllText(path, html);
            }
            catch (Exception ex)
            {
                Console.Error.Write("Error while caching page: " + ex.Message);
            }
            return html;
        }

        private static string StringRequested(RNG rng, string requestName)
        {
            var args = requestName.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2) return "";
            switch (args[0].ToLower().Trim())
            {
                case "img":
                    {
                        var results = _google.Search(args[1], 1);
                        return results.Any() ? results[0].Url : "";
                    }
                case "img-small":
                    {
                        var results = _google.Search(args[1], 1);
                        return results.Any() ? results[0].TbImage.Url : "";
                    }
            }
            return "";
        }
    }
}