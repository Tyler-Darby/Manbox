using System;
using System.Collections.Generic;
using System.Linq;

namespace Manbox
{
    static class CmdLine
    {
        private static readonly Dictionary<string, string> Arguments = new Dictionary<string, string>();
        private static readonly HashSet<string> Flags = new HashSet<string>();

        static CmdLine()
        {
            foreach (var argKeyVal in Environment.GetCommandLineArgs().Where(arg => arg.StartsWith("-")).Select(arg => arg.TrimStart('-').Split(new[] { '=' }, 2)))
            {
                if (argKeyVal.Length == 2)
                {
                    Arguments[argKeyVal[0].ToLower().Trim()] = argKeyVal[1];
                }
                else
                {
                    Flags.Add(argKeyVal[0]);
                }
            }
        }

        public static string Property(string name, string defaultValue = "")
        {
            string arg = null;
            if (!Arguments.TryGetValue(name.ToLower(), out arg))
            {
                return defaultValue;
            }
            return arg;
        }

        public static bool Flag(string name) => Flags.Contains(name);
    }
}