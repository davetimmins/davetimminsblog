This is a bit overdue given that this blog has been around for a year or so but I thought I'd do a post on this blog and how it works. It should come as no surprise that there isn't much to it!

I started out using WordPress for blogging around 3 years ago (I still have my [old posts](http://davetimmins.wordpress.com/) on there) but decided I wanted to move to something different and preferrably using Markdown for authoring content. I looked at some existing blog engines but in the end decided to write my own since I only have very basic requirements and what you see is the end result.

The basic functionality I wanted was

 - Show a summary of my posts on the home page
 - Allow posts to be authored in Markdown
 - Support tags
 - Support comments
 - Support social sharing
 - Have navigation between posts
 - Have a feed
 - Keep it basic! I don't blog very often anyway so the simpler the better.

I'm a big fan of [ServiceStack](https://servicestack.net/) so I started out by creating a new website using that. Next I created a model class for my blog posts and I ended up with this

<pre><code>public class BlogPost
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

	public String RelativePostUrl { get { return String.Format("/post/{0:yyyy}/{0:MMMM}/{1}/", DatePublished, FriendlyPathName); } }

	public String RelativePostCommentsUrl { get { return RelativePostUrl + "#disqus_thread"; } }
}</code></pre>

I use this as an in memory data model but it could be persisted to a database if needed by changing the registered `IDbConnectionFactory` in my `AppHost`.

Trying to follow ServiceStack's design philosophy I then needed my API to retrieve posts. Thinking of what requests I would need to handle I defined the following routes

<pre><code>[Route("/blogposts")]
[Route("/blogposts/{Tags}")]
public class BlogPosts : IReturn&lt;BlogPostsModel&gt;
{
	public int? Id { get; set; }
	public String PostId { get; set; }
	public String[] Tags { get; set; }
}

[Route("/post/{Year}/{Month}/{FriendlyPathName}")]
public class BlogPostEntry : IReturn&lt;BlogPostEntryModel&gt;
{
	public int Year { get; set; }
	public String Month { get; set; }
	public String FriendlyPathName { get; set; }
}

[Route("/feed")]
public class BlogPostFeed : IReturn&lt;SyndicationFeed&gt;
{ }</code></pre>

The first request DTO returns blog posts which can be filtered by tags. The next gets a blog post entry by its date and name and the last gets the feed.

The entire [service code](https://github.com/davetimmins/davetimminsblog/blob/master/Blog.Web/Interface/BlogService.cs) is only around 100 lines including the request DTOs so if you're interested check out the whole thing. Looking at the method to return post summaries you can see that if tags are specified then the results are filtered, otherwise all post summaries are returned.

<pre><code>[DefaultView("Summary")]
public object Get(BlogPosts request)
{
	if (request.Tags != null && request.Tags.Any())
	{
		return base.Request.ToOptimizedResultUsingCache(
			this.Cache, UrnId.Create&lt;BlogPostsModel&gt;(String.Join("-", request.Tags)), 
			TimeSpan.FromMinutes(5), () =&gt;
		{
			return new BlogPostsModel 
			{ 
				Posts = Db.Select<BlogPost>()
					.Where<BlogPost>(p =&gt;> request.Tags.Intersect(p.Tags).Any())
					.OrderByDescending(b => b.DatePublished).ToList() 
			};
		});
	}
	return base.Request.ToOptimizedResultUsingCache(
		this.Cache, UrnId.Create&lt;BlogPostsModel&gt;("all"), 
		TimeSpan.FromMinutes(5), () =&gt;
	{
		return new BlogPostsModel 
		{ 
			Posts = Db.Select<BlogPost>()
				.OrderByDescending(b =&gt; b.DatePublished).ToList() 
		};
	});
}</code></pre>

The other thing to note here is the default view attribute. This means that the route at `/blogposts/{Tags}` will render the view called `Summary`. That view is just a razor view that displays the list of returned post summaries. I mentioned earlier that I wanted my posts as Markdown though remember and another great feature of ServiceStack is that this is supported when using the Razor view engine. All you need to do is register the Razor format plugin in your `AppHost` along with the [other code](https://github.com/davetimmins/davetimminsblog/blob/master/Blog.Web/Global.asax.cs#L55) used to initialise the site and now you can use `.cshtml` and/or `.md` files for content.

For comments I added [Disqus](https://disqus.com/) as it looked the most common and mature offering. It was pretty trivial to add and their documentation is very clear and easy to follow.

Social sharing is implemented using basic links to the various sharing url's available from Twitter, Facebook and Google+.

For the feed I had to do a bit of extra work in order to return a custom media type of `application/rss+xml`. Thankfully this is another area that ServiceStack makes simple and a quick search on the internet gave me the [solution](https://github.com/davetimmins/davetimminsblog/blob/master/Blog.Web/Global.asax.cs#L93).

All told it only took a few hours to get everything up and running (not including authoring content) and thanks to some fantastic free offerings from the folks at [GitHub](https://github.com/), [Microsoft Azure](http://azure.microsoft.com/) and [AppVeyor](http://www.appveyor.com/) I now have what you see here being built, deployed and run seamlessly.

Feel free to take a look at this project on [my GitHub repository](https://github.com/davetimmins/davetimminsblog) and any feedback is welcome.
