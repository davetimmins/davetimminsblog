Inspired by a recent blog post by [@paulcbetts](https://twitter.com/paulcbetts) I decided to review the ArcGIS.PCL project and see how I can use the [Bait & Switch trick](http://log.paulbetts.org/the-bait-and-switch-pcl-trick/) to improve it. Previously I was just using the all in approach to package up the functionality so no matter what platform you were targeting, the same code and dependencies would be used. This was working fine to some degree but I realised that I was including more than I needed to for certain platforms and I had also been wondering on the best way to implement some code that wasn't available to a portable class library. Moving to this updated approach has allowed me to do both of these things better and with very little effort so I'll explain what I did.

The original solution looked like this.

<img src="/Assets/arcgispclsolution original.png" alt="Original solution" class="pure-img"/>

With just the portable class library and some tests. Whereas the current solution is

<img src="/Assets/arcgispclsolution current.png" alt="Current solution" class="pure-img"/>

This has the same portable class library with almost all of the same code in it but now there are platform specific projects which allow for platform specific code to be written for each of them is required. The portable class library project now only references the .NET Portable Subset too. Since I left the majority of the code in the PCL, I also reference the PCL from the platform specific projects.

In addition to using the PCL in the other projects I have a couple of other classes that are the same code but since their platform dependencies are different I am not including them in the PCL. Rather than copy the code for each project I just add them as linked files, so if we look at the iOS and .NET versions you can see that they both have 2 linked files and then just 1 platform specific file

<img src="/Assets/arcgispcl project comp.png" alt="Project comparison" class="pure-img"/>

So what I've had to do so far is 

 - Create new projects, 1 for each supported platform
 - Remove a couple of classes from the PCL
 - Add those classes as linked files to the platform specific projects
 - Add any platform specific code to each project

And now my final step is to make sure that NuGet knows what to do when a reference is added to this package. I can do this by adding platform specific dependencies in my .nuspec file and what I ended up with is this

<pre><code class='xml'>&lt;dependencies&gt;
	&lt;group targetFramework="net40"&gt;
		&lt;dependency id="Microsoft.Bcl.Async" version="1.0.168" /&gt;
		&lt;dependency id="Microsoft.Net.Http" version="2.2.22" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="sl5"&gt;
		&lt;dependency id="Microsoft.Bcl.Async" version="1.0.168" /&gt;
		&lt;dependency id="Microsoft.Net.Http" version="2.2.22" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="wp8"&gt;
		&lt;dependency id="Microsoft.Net.Http" version="2.2.22" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="net45"&gt;
		&lt;dependency id="Microsoft.Net.Http" version="2.2.22" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="netcore45"&gt;
		&lt;dependency id="Microsoft.Net.Http" version="2.2.22" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="MonoAndroid1"&gt;
		&lt;dependency id="modernhttpclient" version="1.2.2" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="MonoTouch1"&gt;
		&lt;dependency id="modernhttpclient" version="1.2.2" /&gt;
	&lt;/group&gt;
	&lt;group targetFramework="portable-net4+sl5+netcore45+wp8+MonoAndroid1+MonoTouch1"&gt;
	&lt;/group&gt;
&lt;/dependencies&gt;</code></pre>

Pretty simple right. Now when you add ArcGIS.PCL from NuGet you will get the version that targets your platform.

One other thing I did as part of this was to try and make it easier to get up and running with the code. Since most of the time you are working with this library is going to involve calling ArcGIS Server resources, some form of serialization is involved and this used to require creating your own implementation of `ISerializer` to facilitate this (albeit usually just a copy paste from the read me). To this end I created `Json.NET` and `ServiceStack.Text` (version 3) `ISerializer` [NuGet packages](https://www.nuget.org/packages?q=arcgis.pcl). Since `Json.NET` is also a PCL it means that the `ArcGIS.PCL.JsonDotNetSerializer` package is also a PCL so now you can see one benefit of this approach. You can still use your own serializer if you want though.

### Other Updates

[ArcGIS.PCL](https://components.xamarin.com/view/arcgis.pcl) is now on the Xamarin Component Store!

There has been some new functionality added since the previous blog post (in addition to fixing stuff :)) but rather than waffle on about it I'll just be lazy and list it 

 - Added `ArcGISOnlineAppLoginOAuthProvider` for creating OAuth tokens to use with ArcGIS Online services.
 - Added ArcGIS Online `IEndpoint`
 - Added `ObjectIds` property to `Query` operation
 - Added `QueryForIds` operation
 - Added `QueryForCount` operation
 - Token generation works better for various scenarios and the requests are encrypted if supported on the server

and finally I am now using a [Grunt](http://gruntjs.com/) task to build the NuGet packages, it is super simple and very easy to understand so take a look on [GitHub](https://github.com/davetimmins/ArcGIS.PCL/blob/master/gruntfile.js) if you are interested. To test these I have a [MyGet](https://www.myget.org/) feed setup and use [AppVeyor](https://ci.appveyor.com) as a build server.