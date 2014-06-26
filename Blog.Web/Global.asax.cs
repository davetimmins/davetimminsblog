using Blog.Web.Model;
using Funq;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.Logging;
using ServiceStack.MiniProfiler;
using ServiceStack.MiniProfiler.Data;
using ServiceStack.OrmLite;
using ServiceStack.Razor;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Blog.Web
{
    // TODO : add tags service
    // add post data model
    // allow summary as html / rss from service
    // change posts to be via service by id and route as data / title in service request


    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            new AppHost().Init();
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Add("X-Frame-Options", "DENY");
        }

        protected void Application_BeginRequest(object src, EventArgs e)
        {
            if (Request.IsLocal)
                ServiceStack.MiniProfiler.Profiler.Start();
        }

        protected void Application_EndRequest(object src, EventArgs e)
        {
            ServiceStack.MiniProfiler.Profiler.Stop();
        }
    }

    public class AppHost : AppHostBase
    {
        public AppHost() : base("Blog", typeof(AppHost).Assembly) { }

        public override void Configure(Container container)
        {
            container.Register<IDbConnectionFactory>(new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider)
            {
                ConnectionFilter = x => new ProfiledDbConnection(x, Profiler.Current)
            });

            using (var db = container.TryResolve<IDbConnectionFactory>().Open())
            {
                if (!db.TableExists("BlogPost"))
                {
                    db.CreateTableIfNotExists<BlogPost>();
                    db.InsertAll(BlogPostData.SeedData());
                }
            }

            Plugins.RemoveAll(x => x is RequestInfoFeature);
            Plugins.RemoveAll(x => x is MetadataFeature);
            Plugins.RemoveAll(x => x is PredefinedRoutesFeature);

            Plugins.Add(new RazorFormat());
            RssFormat.Register(this);

            CustomErrorHttpHandlers.Clear();
            CustomErrorHttpHandlers.Add(HttpStatusCode.NotFound, new RazorHandler("/404"));
            CustomErrorHttpHandlers.Add(HttpStatusCode.InternalServerError, new RazorHandler("/oops"));

            SetConfig(new HostConfig
            {
                GlobalResponseHeaders = new Dictionary<String, String>()
            });
        }
    }

    public class RssFormat
    {
        const string AtomContentType = "application/rss+xml";

        public static void Register(IAppHost appHost)
        {
            appHost.ContentTypes.Register(AtomContentType, SerializeToStream, DeserializeFromStream);
        }

        static void SerializeToStream(ServiceStack.Web.IRequest requestContext, object dto, ServiceStack.Web.IResponse httpRes)        
        {
            var syndicationFeed = dto as SyndicationFeed;
            if (syndicationFeed == null) return;

            using (var xmlWriter = System.Xml.XmlWriter.Create(httpRes.OutputStream))
            {
                var atomFormatter = new Atom10FeedFormatter(syndicationFeed);
                atomFormatter.WriteTo(xmlWriter);
            }
        }

        static object DeserializeFromStream(Type type, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}