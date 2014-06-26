using Blog.Web.Model;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel.Syndication;

namespace Blog.Web.Interface
{
    public class BlogService : Service
    {
        public BlogService()
        {
            var dbConnection = this.TryResolve<IDbConnectionFactory>();
            if (dbConnection == null || !Db.TableExists("BlogPost"))
                throw new InvalidOperationException("BlogPost database table has not been configured.");
        }

        [DefaultView("BlogPostEntry")]
        public object Get(BlogPostEntry request)
        {
            String cacheKey = UrnId.CreateWithParts<BlogPost>(request.Year.ToString(), request.Month, request.FriendlyPathName);
            return base.Request.ToOptimizedResultUsingCache(this.Cache, cacheKey, TimeSpan.FromMinutes(5), () =>
            {
                BlogPost blogpost = null;

                var posts = Db.Select<BlogPost>().OrderByDescending(b => b.DatePublished).ToList();
                for (int i = 0; i < posts.Count; i++)
                {
                    var itemToCheck = posts[i];
                    if (String.Equals(itemToCheck.FriendlyPathName, request.FriendlyPathName, StringComparison.OrdinalIgnoreCase))
                    {
                        blogpost = posts[i];
                        return new BlogPostEntryModel(blogpost, (i < (posts.Count - 1)) ? posts[i + 1].RelativePostUrl : String.Empty, (i > 0) ? posts[i - 1].RelativePostUrl : String.Empty);
                    }
                }
                if (blogpost == null) throw HttpError.NotFound("Blogpost not found.");
                return null;
            });
        }

        [DefaultView("Summary")]
        public object Get(BlogPosts request)
        {
            if (request.Tags != null && request.Tags.Any())
            {
                return base.Request.ToOptimizedResultUsingCache(this.Cache, UrnId.Create<BlogPostsModel>(String.Join("-", request.Tags)), TimeSpan.FromMinutes(5), () =>
                {
                    return new BlogPostsModel { Posts = Db.Select<BlogPost>().Where<BlogPost>(p => request.Tags.Intersect(p.Tags).Any()).OrderByDescending(b => b.DatePublished).ToList() };
                });
            }
            return base.Request.ToOptimizedResultUsingCache(this.Cache, UrnId.Create<BlogPostsModel>("all"), TimeSpan.FromMinutes(5), () =>
            {
                return new BlogPostsModel { Posts = Db.Select<BlogPost>().OrderByDescending(b => b.DatePublished).ToList() };
            });
        }

        [AddHeader(ContentType = "application/rss+xml")]
        public object Get(BlogPostFeed request)
        {
            var feed = new SyndicationFeed("davetimmins blog", "Feed of blog posts from davetimmins.com", new Uri("http://davetimmins.com"));
            feed.Authors.Add(new SyndicationPerson("davetimminsblog@outlook.com", "Dave Timmins", "http://davetimmins.com"));
            var feedItems = new List<SyndicationItem>();
            var posts = Db.Select<BlogPost>().OrderByDescending(b => b.DatePublished);
            foreach (var post in posts)
            {
                var item = new SyndicationItem(
                    post.Title,
                    post.Summary,
                    new Uri("http://davetimmins.com" + post.RelativePostUrl), post.PostId,
                    post.DateLastUpdated ?? post.DatePublished);
                foreach (var tag in post.Tags)
                    item.Categories.Add(new SyndicationCategory(tag));
                feedItems.Add(item);
            }
            feed.Items = feedItems;
            return feed;
        }
    }

    [Route("/blogposts")]
    [Route("/blogposts/{Tags}")]
    public class BlogPosts : IReturn<BlogPostsModel>
    {
        public int? Id { get; set; }
        public String PostId { get; set; }
        public String[] Tags { get; set; }
    }

    [Route("/post/{Year}/{Month}/{FriendlyPathName}")]
    public class BlogPostEntry : IReturn<BlogPostEntryModel>
    {
        public int Year { get; set; }
        public String Month { get; set; }
        public String FriendlyPathName { get; set; }
    }

    [Route("/feed")]
    public class BlogPostFeed : IReturn<SyndicationFeed>
    { }
}