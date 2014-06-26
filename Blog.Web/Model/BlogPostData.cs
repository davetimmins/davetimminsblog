using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Blog.Web.Model
{
    public static class BlogPostData
    {
        const String Author = "Dave Timmins";

        public static List<BlogPost> SeedData()
        {
            return new List<BlogPost>
            {
                new BlogPost
                { 
                    Author = Author,
                    Title = "This Blog",
                    PostId = "ThisBlog",
                    FriendlyPathName = "ThisBlog",
                    Tags = new List<String>{ "Blog", "Development", "OSS", "ServiceStack" },
                    DatePublished = new DateTime(2014, 6, 27, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<i class='fa fa-edit fa-mega post-avatar pure-img summary-img'></i>",
                    Summary = "A look under the hood of this blog."
                },
                new BlogPost
                { 
                    Author = Author,
                    Title = "ArcGIS.PCL Updates",
                    PostId = "ArcGISPCLUpdate",
                    FriendlyPathName = "ArcGIS-PCL-Update",
                    Tags = new List<String>{ "ArcGIS", "Development", "OSS", "PCL" },
                    DatePublished = new DateTime(2014, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<img alt='ArcGIS.PCL' class='post-avatar pure-img summary-img' src='/Assets/gateway.svg' />",
                    Summary = "Recent updates to the ArcGIS.PCL project including the better utilisation of it being a portable class library by taking advantage of the Bait & Switch trick."                    
                },
                new BlogPost
                { 
                    Author = Author,
                    Title = "Sneaky searching with a Fake ArcGIS Locator and Nancy",
                    PostId = "Fakeator",
                    FriendlyPathName = "Fakeator",
                    Tags = new List<String>{ "ArcGIS Online", "Nancy", "Development" },
                    DatePublished = new DateTime(2014, 4, 11, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<img alt='Fakeator' class='post-avatar pure-img summary-img' src='/Assets/logofakeator.png' />",
                    Summary = "Building a fake locator service using Nancy that works with the ArcGIS JS geocode dijit."
                },
                new BlogPost
                { 
                    Author = Author,
                    Title = "Generating Test Data for Hosted Feature Services",
                    PostId = "Randomer",
                    FriendlyPathName = "Randomer",
                    Tags = new List<String>{ "ArcGIS Online", "OSS", "Development" },
                    DatePublished = new DateTime(2014, 4, 2, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<img alt='Randomer' class='post-avatar pure-img summary-img' src='/Assets/logorandomer.png' />",
                    Summary = "A look at automatically generating random data for hosted feature services on ArcGIS Online."
                },
                new BlogPost
                { 
                    Author = Author,
                    Title = "Esri Dev Summit 2014",
                    PostId = "EsriDevSummit2014",
                    FriendlyPathName = "Esri-DevSummit-2014",
                    Tags = new List<String>{ "ArcGIS", "Learning", "Conference" },
                    DatePublished = new DateTime(2014, 3, 31, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<img alt='DevSummit 2014' class='post-avatar pure-img summary-img' src='/Assets/logodevsummit.png' />",
                    Summary = "My thoughts on the 9th Esri Internationl Developer Summit in Palm Springs."
                },
                new BlogPost
                { 
                    Author = Author,
                    Title = "TechEd NZ 2013",
                    PostId = "TechEdNZ2013",
                    FriendlyPathName = "Tech-Ed-NZ-2013",
                    Tags = new List<String>{ "TechEd", "Learning", "Conference" },
                    DatePublished = new DateTime(2013, 9, 15, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<img alt='TechEd 2013' class='post-avatar pure-img summary-img' src='/Assets/logoteched.png' />",
                    Summary = "With TechEd 2013 having just finished here in New Zealand I thought I'd take the opportunity to summarize my thoughts on the conference this year."
                },
                new BlogPost
                {
                    Author = Author,
                    Title = "ArcGIS.PCL - The What, Why & How",
                    PostId = "ArcGISPCL",
                    FriendlyPathName = "ArcGIS-PCL",
                    Tags = new List<String>{ "ArcGIS", "Development", "OSS", "PCL" },
                    DatePublished = new DateTime(2013, 7, 11, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<i class='fa fa-globe fa-mega post-avatar pure-img summary-img' style='color: steelblue'></i>",
                    Summary = "A closer look at ArcGIS.PCL. A Portable class library for .NET for Windows Store apps, .NET framework 4.5, Silverlight 4 and higher & Windows Phone 7.5 and higher that can work with ArcGIS Server types and resources as well as converting between ArcGIS Features and GeoJSON FeatureCollections."
                },
                new BlogPost
                {
                    Author = Author,
                    Title = "ArcGIS Online OAuth with ServiceStack",
                    PostId = "OAuthArcGISOnlineandServiceStack",
                    FriendlyPathName = "OAuth-with-ArcGISOnline-ServiceStack",
                    Tags = new List<String>{ "ArcGIS", "ArcGIS Online", "Development", "OAuth", "ServiceStack" },
                    DatePublished = new DateTime(2013, 4, 12, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<i class='fa fa-lock fa-mega post-avatar pure-img summary-img'></i>",
                    Summary = "Use OAuth from ArcGIS Online by adding a new application to ArcGIS for Developers. In this example we integrate it with ServiceStack to show how it can be used in a real world scenario."
                },
                new BlogPost
                {
                    Author = Author,
                    Title = "Initial Impressions with iOS and XCode",
                    PostId = "InitialImpressionswithiOSandXCode",
                    FriendlyPathName = "Initial-Impressions-with-iOS-and-XCode",
                    Tags = new List<String>{ "iOS", "Development", "Learning" },
                    DatePublished = new DateTime(2012, 7, 18, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<i class='fa fa-apple fa-mega post-avatar pure-img summary-img' style='color: black'></i>",
                    Summary = "I was recently asked if I’d like to spend a couple of weeks working on a prototype iPhone application so I've put together some of my thoughts around the experience."
                },
                new BlogPost
                {
                    Author = Author,
                    Title = "Enhancing GIS WebApps with SignalR",
                    PostId = "GISWebAppswithSignalR",
                    FriendlyPathName = "Enhancing-GIS-WebApps-with-SignalR",
                    Tags = new List<String>{ "ArcGIS", "Development", "SignalR" },
                    DatePublished = new DateTime(2011, 11, 21, 0, 0, 0, 0, DateTimeKind.Utc),
                    SummaryImageMarkup = @"<img alt='MapR' class='post-avatar pure-img summary-img' src='/Assets/mapr.jpg' />",
                    Summary = "A few weeks ago I saw this cool new open source project called SignalR and was wondering about ways to incorporate it into the type of GIS web applications that I usually work on."
                }
            };
        }
    }
}