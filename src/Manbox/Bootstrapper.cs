using System;
using Nancy;
using Nancy.Conventions;

namespace Manbox
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            Console.Write("Configuring server conventions... ");
            base.ConfigureConventions(nancyConventions);
            nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("static", "static"));
            Console.WriteLine("Done.");
        }
    }
}