A few weeks ago I saw this cool new open source project called [SignalR](http://signalr.net/) and was wondering about ways to incorporate it into the type of GIS web applications that I usually work on. SignalR is an asynchronous signalling library for ASP.NET. There are a number of great write ups about it so I recommend you search for these and check them out along with the project on GitHub. The main usage I’d seen for SignalR had been to create chat applications and whilst this was a good demonstration of the technology it isn’t typically that useful for my scenario so in this demo I’m going to be applying it to a simple problem. I have a web page where I want to display the locations of any users that are currently accessing it. Whenever someone connects to the page it should update the connection for everyone else currently connected and it should display the number of other people connected. If someone disconnects from the page then they should be removed from all the other still active connections. So basically I want to sync the state of everyone accessing the page. You may have seen a demo where each time a browser window is opened a counter is incremented and this is taking this and applying it to a different visualization.

To start you can create a new web application in Visual Studio and add the SignalR reference using NuGet.

Now there are 2 parts that we need to code, the server side and client side implementation of our logic. On the server we need to add the ability to add and remove locations. These methods will be invoked from JavaScript on the client and can also call JavaScript functions on the client from the server side, very cool. All this is really doing is passing messages between connected clients using the proxy that SignalR generates for these operations.

Add a new class that inherits the abstract Microsoft.AspNet.SignalR.Hub class.

<pre><code>public class LocationHub : Hub
{
    static ConcurrentDictionary<string, object> _graphics = new ConcurrentDictionary<string, object>();

    public void Add(object json)
    {          
        foreach (var graphic in _graphics)
            Clients.Caller.addGraphic(graphic.Key, graphic.Value);

        Clients.Others.addGraphic(Context.ConnectionId, json);

        _graphics.TryAdd(Context.ConnectionId, json);
    }

    public void Update(string id, object json)
    {
        _graphics.AddOrUpdate(id, json, (key, oldValue) => json);
        Clients.Others.updateGraphic(id, json);
    }

    public override Task OnDisconnected()
    {
        object removed;
        _graphics.TryRemove(Context.ConnectionId, out removed);
        return Clients.All.leave(Context.ConnectionId);
    }
}
</code></pre>

Very simple. Now we need to add the client side code to call and handle these methods. Start by creating a new html page and add references to the jQuery and SignalR scripts.

<pre><code>&lt;script src="Scripts/jquery-1.6.4.min.js">&lt;/script>
&lt;script src="Scripts/jquery.signalR-1.0.1.min.js">&lt;/script>
&lt;script src="signalr/hubs">&lt;/script>
</code></pre>

In order to use the methods we just defined we need to create the proxy reference. Then we can either invoke or handle the various methods.

<pre><code>var connection = $.hubConnection();
locationConnection = connection.createHubProxy('locationHub');
</code></pre>

Now we have the proxy we can call the methods defined for it. So for the above example, if I want to call the Add method for the LocationHub on the server I would use

<pre><code>locationConnection.invoke('add', graphic.toJson());
</code></pre>

where I’m passing the graphic as a Json string value I want to use (the parameter binding is done automatically but you need to define the correct type). Now to handle what happens when the Add method is called I need to add the addGraphic handler on the client since that is called from the server, once again making sute that the definition matches what was used on the server. The convention we can use is

<pre><code>locationConnection.on('addGraphic', function (id, json) {
    ....
});</code></pre>

This will be called for any page that defines the handler but in this example it is in the same page. So now we have seen how to wire up the calls we need to apply it to our initial requirement. Thinking about the requirement we can identify the steps that we need to follow once our proxy is created which in a nutshell are

 - First, lookup the user location using the W3C geolocation API  
 - Do a reverse geocode of the location to get the address
 - Call the add method so that the new location is added to any other open client 
 - When we close the page or navigate away we need to notify other clients that we have left so that they can remove the location

To do this I’m using the [ArcGIS JavaScript API](https://developers.arcgis.com/en/javascript/). This allows me to reverse geocode the location from the browser and add it to the map display as well as adding a maptip for the address data. You can run the sample page yourself or you can create your own app using the code on this post. If you run the sample and get some of your friends to access it at the same time then you should see the map markers appear and display the address if you click on them. Note that you will need to be using a browser that supports geolocation. There is some additional logic for when the user hovers over a graphic too but you can look at the source yourself to see how that is handled.

The sample itself looks like

<iframe src="http://mapr.azurewebsites.net" width="100%" height="400px" frameborder="0" seamless></iframe>

but to really appreciate what is happening it's best to have more than one browser open at the same time. Try <a target="_blank" href="http://mapr.azurewebsites.net">opening</a> / closing more browser windows.

This is a very simple example of enhancing your application but there are many other scenarios where SignalR could be utilised such as 

 - Shared redlining / editing 
 - Controlling user sessions 
 - Real time data updates 
 - etc

Remember this is just a sample to demonstrate the concept. Thanks for reading, as always feedback is appreciated and I’d be interested to hear what you would find it useful for.



