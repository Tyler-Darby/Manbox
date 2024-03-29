﻿using System;
using System.Threading;

using Rant;
using Rant.Vocabulary;

namespace Manbox
{
    public class _
    {
        public static readonly bool IsMono;
        
        private static readonly RantDictionary VocabClean;
        private static readonly RantDictionary VocabDirty;

        public static RantEngine CreateEngine(bool nsfw = false)
        {
            return new RantEngine(nsfw ? VocabDirty : VocabClean);
        }

        public static T Execute<T>(Func<T> func, int timeout)
        {
            T result;
            TryExecute(func, timeout, out result);
            return result;
        }

        public static bool TryExecute<T>(Func<T> func, int timeout, out T result)
        {
            var t = default(T);
            var thread = new Thread(() => t = func());
            thread.Start();
            var completed = thread.Join(timeout);
            if (!completed) thread.Abort();
            result = t;
            return completed;
        }

        static _()
        {
            IsMono = Type.GetType("Mono.Runtime") != null;
            RantEngine.MaxStackSize = 128;
            VocabClean = RantDictionary.FromDirectory("dictionary", NsfwFilter.Disallow);
            VocabDirty = RantDictionary.FromDirectory("dictionary", NsfwFilter.Allow);
        } 
    }
}