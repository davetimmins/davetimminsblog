With the release of [Xamarin 3](http://blog.xamarin.com/announcing-xamarin-3/), Xamarin added [Xamarin Forms](https://xamarin.com/forms); a way of building native UIs for iOS, Android and Windows Phone from a single, shared C# codebase. Totally awesome right! Well yes it is and added to that was Xamarin.Forms.Maps which provides a map control you can use in these applications. Now it's still early days for this so the functionality is pretty basic, especially if you are coming from other mapping / GIS SDKs but what you get is a map control and the ability to overlay point data. It assumes that the data is in WGS84 (lat / long). If you want to get started with taking a look then the [Mobile CRM sample](https://github.com/xamarin/xamarin-forms-samples/tree/master/MobileCRM) is a good place to start.

What we are going to look at here is adding data from ArcGIS Server using a NuGet package I created called [ArcGIS.PCL.XamarinMaps](https://www.nuget.org/packages/ArcGIS.PCL.XamarinMaps/). This package provides a bunch of extension methods for converting ArcGIS Features to and from Xamarin features (Pins, MapSpan). The data retrieval is really done by another package that this one depends on called [ArcGIS.PCL](https://github.com/davetimmins/ArcGIS.PCL) so check that out first if you want more detail. 

Since we can only add point data lets add some points! To speed things up I'll use the same Mobile CRM sample and for the data I'll use a sample service hosted at `http://sampleserver3.arcgisonline.com/ArcGIS/rest/services/Earthquakes/EarthquakesFromLastSevenDays/MapServer/0` to get the data. As you might expect this is going to give us recent earthquakes from around the world.

To start we need to add the ArcGIS.PCL.XamarinMaps NuGet package which will tell you something simliar to what is below in its `readme` file. Then in the shared code project we can hook into the `MapViewModel` and load our data in the `LoadPins()` function rather than what is currently there. The code we need is just a few lines

<pre><code>public async Task&lt;List&lt;Pin&gt;&gt; LoadPins()
{
    ExecuteLoadModelsCommand();

    var pins = new List&lt;Pin&gt;();

    var gateway = new PortalGateway(
        "http://sampleserver3.arcgisonline.com/ArcGIS",
        new ArcGIS.ServiceModel.Serializers.JsonDotNetSerializer());

    var query = new Query("Earthquakes/EarthquakesFromLastSevenDays/MapServer/0".AsEndpoint())
    {
        Where = "magnitude &gt; 5.5",
        OutFields = new List&lt;string&gt; { "magnitude" }
    };

    var points = await gateway.Query&lt;Point&gt;(query.ToOutputAsGeographic());

    return points.ToPins(PinType.Place, "magnitude");
}</code></pre>

Walking through this we are creating a new `PortalGateway` for the ArcGIS Server we are interested in. This brokers any operation calls we make and we also pass in a serializer for (de)serializing requests and response.

Next we define a query operation to execute for the service we are interested in. In this case the recent earthquakes, but only those with a magnitude of greater than 5.5.

Then we call the operation and just to make sure the data comes back in WGS84 we call `ToOutputAsGeographic()` on the query object.

Finally we convert the result to Pins using the `ToPins` extension method.

If we run the code we get our data nicely displayed on the map control.

<img src="/Assets/xamarinmaps.png" alt="Xamarin Maps with ArcGIS data" class="pure-img"/>

You could also go the other way and post data that was captured on the device to an ArcGSI Server feature service using the `ApplyEdits` operation.

Hopefully this gives you a good starting point to working with these libraries. Feel free to comment here or raise an issue on [GitHub](https://github.com/davetimmins/ArcGIS.PCL.XamarinMaps) if you have any feedback.
