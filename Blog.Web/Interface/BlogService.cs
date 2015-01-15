using Blog.Web.Model;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;

namespace Blog.Web.Interface
{
    public class BlogService : Service
    {
        List<BlogPost> _data = BlogPostData.SeedData();

        [DefaultView("BlogPostEntry")]
        public object Get(BlogPostEntry request)
        {
            string cacheKey = UrnId.CreateWithParts<BlogPost>(request.Year.ToString(), request.Month, request.FriendlyPathName);
            return base.Request.ToOptimizedResultUsingCache(this.Cache, cacheKey, TimeSpan.FromMinutes(5), () =>
            {
                BlogPost blogpost = null;

                var posts = _data.OrderByDescending(b => b.DatePublished).ToList();
                for (int i = 0; i < posts.Count; i++)
                {
                    var itemToCheck = posts[i];
                    if (string.Equals(itemToCheck.FriendlyPathName, request.FriendlyPathName, StringComparison.OrdinalIgnoreCase))
                    {
                        blogpost = posts[i];
                        return new BlogPostEntryModel(blogpost, (i < (posts.Count - 1))
                            ? posts[i + 1].RelativePostUrl
                            : string.Empty, (i > 0) ? posts[i - 1].RelativePostUrl : string.Empty);
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
                return base.Request.ToOptimizedResultUsingCache(this.Cache, UrnId.Create<BlogPostsModel>(string.Join("-", request.Tags)), TimeSpan.FromMinutes(5), () =>
                {
                    return new BlogPostsModel
                    {
                        Posts = _data.Where<BlogPost>(p => request.Tags.Intersect(p.Tags.Select(s => s.ToLowerInvariant()))
                            .Any()).OrderByDescending(b => b.DatePublished).ToList()
                    };
                });
            }
            return base.Request.ToOptimizedResultUsingCache(this.Cache, UrnId.Create<BlogPostsModel>("all"), TimeSpan.FromMinutes(5), () =>
            {
                return new BlogPostsModel { Posts = _data.OrderByDescending(b => b.DatePublished).ToList() };
            });
        }

        [AddHeader(ContentType = "application/rss+xml")]
        public object Get(BlogPostFeed request)
        {
            var feed = new SyndicationFeed("davetimmins blog", "Feed of blog posts from davetimmins.com", new Uri("http://davetimmins.com"));
            feed.Authors.Add(new SyndicationPerson("davetimminsblog@outlook.com", "Dave Timmins", "http://davetimmins.com"));
            var feedItems = new List<SyndicationItem>();
            var posts = _data.OrderByDescending(b => b.DatePublished);
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

    [Route("/blogposts", Verbs = "GET")]
    [Route("/blogposts/{Tags}", Verbs = "GET")]
    public class BlogPosts : IReturn<BlogPostsModel>
    {
        public int? Id { get; set; }
        public string PostId { get; set; }
        public string[] Tags { get; set; }
    }

    [Route("/post/{Year}/{Month}/{FriendlyPathName}", Verbs = "GET")]
    public class BlogPostEntry : IReturn<BlogPostEntryModel>
    {
        public int Year { get; set; }
        public string Month { get; set; }
        public string FriendlyPathName { get; set; }
    }

    [Route("/feed", Verbs = "GET")]
    public class BlogPostFeed : IReturn<SyndicationFeed>
    { }
}