using Funq;
using ServiceStack;
using ServiceStack.Razor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.ServiceModel.Syndication;

namespace Blog.Web
{
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
            SetConfig(new HostConfig
            {
                GlobalResponseHeaders = new Dictionary<string, string>(),
                WebHostUrl = "http://davetimmins.com", //for sitemap.xml urls
            });

            Plugins.RemoveAll(x => x is RequestInfoFeature);
            Plugins.RemoveAll(x => x is MetadataFeature);
            Plugins.RemoveAll(x => x is PredefinedRoutesFeature);
            Plugins.RemoveAll(x => x is NativeTypesFeature);

            Plugins.Add(new RazorFormat());
            Plugins.Add(new SitemapFeature
            {
                UrlSet = Blog.Web.Model.BlogPostData.SeedData()
                    .ConvertAll(x => new SitemapUrl
                    {
                        Location = new Blog.Web.Interface.BlogPostEntry
                        {
                            FriendlyPathName = x.FriendlyPathName,
                            Month = string.Format("{0:MMMM}", x.DatePublished).ToLower(),
                            Year = x.DatePublished.Year
                        }.ToAbsoluteUri(),
                        LastModified = x.DatePublished,
                        ChangeFrequency = SitemapFrequency.Weekly,
                    })
            });
            RssFormat.Register(this);

            CustomErrorHttpHandlers.Clear();
            CustomErrorHttpHandlers.Add(HttpStatusCode.NotFound, new RazorHandler("/404"));
            CustomErrorHttpHandlers.Add(HttpStatusCode.InternalServerError, new RazorHandler("/oops"));
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