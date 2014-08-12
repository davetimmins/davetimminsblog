using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Blog.Web.Model
{
    public class BlogPost
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public String Title { get; set; }

        [Required]
        [StringLength(50)]
        public String PostId { get; set; }

        [Required]
        [StringLength(200)]
        [Index(true)]
        public String FriendlyPathName { get; set; }

        [Required]
        [Index]
        public DateTime DatePublished { get; set; }

        public DateTime? DateLastUpdated { get; set; }

        [Index]
        public List<String> Tags { get; set; }

        [Required]
        [StringLength(100)]
        public String Author { get; set; }

        public String SummaryImageMarkup { get; set; }

        [Required]
        public String Summary { get; set; }

        public String FriendlyDate { get { return DatePublished.ToString("MMMM dd, yyyy"); } }

        public String RelativePostUrl { get { return String.Format("/post/{0:yyyy}/{0:MMMM}/{1}/", DatePublished, FriendlyPathName).ToLower(); } }

        public String RelativePostCommentsUrl { get { return RelativePostUrl + "#disqus_thread"; } }
    }

    public class BlogPostEntryModel
    {
        public BlogPostEntryModel(BlogPost blogpost, String previous, String next)
        {
            Previous = previous;
            Next = next;
            Title = blogpost.Title;
            Summary = blogpost.Summary;
            Tags = blogpost.Tags;
            PostId = blogpost.PostId;
            FriendlyDate = blogpost.DatePublished.ToString("MMMM dd, yyyy");
            FriendlyPathName = blogpost.FriendlyPathName.ToLower();
        }

        public String Title { get; set; }

        public String Summary { get; set; }

        public List<String> Tags { get; set; }

        public String Previous { get; set; }

        public String Next { get; set; }

        public String PostId { get; set; }

        public String FriendlyDate { get; set; }

        public String FriendlyPathName { get; set; }
    }

    public class BlogPostsModel
    {
        public List<BlogPost> Posts { get; set; }
    }
}