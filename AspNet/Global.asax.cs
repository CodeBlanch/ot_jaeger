using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace OpenTelemetry.Exporter.AspNet
{
    public class WebApiApplication : HttpApplication
    {
        private static TracerFactory tracerFactory;

        protected void Application_Start()
        {
            tracerFactory = TracerFactory.Create(builder =>
            {
                builder
                     .UseJaeger(c =>
                     {
                         c.ServiceName = "huehuehue";
                         c.AgentHost = "localhost";
                         c.AgentPort = 6831;
                     })
                    .AddRequestCollector()
                    .AddDependencyCollector();
            });

            GlobalConfiguration.Configure(WebApiConfig.Register);

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_End()
        {
            tracerFactory?.Dispose();
        }

        protected void Application_BeginRequest()
        {
            var tracer = tracerFactory.GetTracer("huehuehue");
            using (tracer.StartActiveSpan("Main", out var span))
            {
                span.SetAttribute("BeginRequest", Context.Request.Url.AbsoluteUri);
                // Simulate some work.
                try
                {
                    Console.WriteLine("Doing busy work");
                    Thread.Sleep(1000);
                }
                catch (ArgumentOutOfRangeException er)
                {
                    // Set status upon error
                    span.Status = Status.Internal.WithDescription(er.ToString());
                }

                // Annotate our span to capture metadata about our operation
                var attributes = new Dictionary<string, object> { { "use", "demo" } };
                span.AddEvent(new Event("Invoking DoWork", attributes));
            }
        }
    }
}
