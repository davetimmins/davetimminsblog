A couple of months ago I saw a comment on twitter from [odoenet](https://twitter.com/odoenet) about how it would be useful to be able to generate some sample data for feature services hosted on ArcGIS Online. As chance would have it I have recently had to do some work with hosted feature services and found myself needing to create sample data for a number of services so rather than having to do this manually and multiple times I decided to write a basic web app that does just this. If you want to try it out it's at [randomer.azurewebsites.net](https://randomer.azurewebsites.net) and the source is on [GitHub](https://github.com/davetimmins/Randomer). It isn't particulary complex, the basic workflow is

* Log in to ArcGIS Online using your developer or organizational account
* A list of your hosted feature services is shown
* Select one and it will query the first layer in the service
* Enter the number of features to create 
* Only attribute fields that are required and are editable will be auto populated (with random data)

It is also pretty limited in its current form as it only works with point features and assumes that you only have one layer in your service. You can go through the code to see how it works but I thought I'd do a quick write up of how I did it in case you find it useful. 

To start I went to [developers.arcgis.com](https://developers.arcgis.com) and logged in with my developer account, went ahead and created a new feature service and then looked at the feature service details. 

<img src="/Assets/create hosted feature service.png" alt="create hosted feature service access" class="pure-img"/>

I am running Fiddler whilst doing this so I can see the queries being used behind the scenes (Note the url, type of request and the parameters used in the request and response). From this information I can now recreate the correct request to query my feature services and start working with them. What this showed me was how to get the feature services for my account and then how to interrogate those services for their service details. 

<img src="/Assets/arcgis online fiddler.png" alt="arcgis online fiddler" class="pure-img"/>

Since I am doing the logic on the server I used one of my other libraries [ArcGIS.PCL](https://github.com/davetimmins/ArcGIS.PCL) and extended it to allow me to have some ArcGIS Online operations. This is a matter of inspecting the json payload of a request and copying it then using Visual Studio and paste as JSON classes in order to create a typed version of the JSON request. Now I can use familiar tooling to write my code and hopefully reduce errors. The classes end up looking like (note that I have removed lots of properties that aren't needed for this solution)

<pre><code class='cs'>[DataContract]
public class SearchHostedFeatureServices : ArcGISServerOperation
{
    public SearchHostedFeatureServices(ArcGISOnlineEndpoint endpoint, String username)
        : base(endpoint, "search")
    {
        Query = String.Format("owner:{0} AND (type:\"Feature Service\")", username);
        SortField = "created";
        SortOrder = "desc";
        NumberToReturn = 100;
        StartIndex = 1;
    }

    [DataMember(Name = "q")]
    public String Query { get; private set; }

    [DataMember(Name = "sortField")]
    public String SortField { get; set; }

    [DataMember(Name = "sortOrder")]
    public String SortOrder { get; set; }

    [DataMember(Name = "num")]
    public int NumberToReturn { get; set; }

    [DataMember(Name = "start")]
    public int StartIndex { get; set; }
}

[DataContract]
public class SearchHostedFeatureServicesResponse : PortalResponse
{
    [DataMember(Name = "results")]
    public HostedFeatureService[] Results { get; set; }
}

[DataContract]
public class HostedFeatureService
{
    [DataMember(Name = "id")]
    public string Id { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }

    [DataMember(Name = "url")]
    public string Url { get; set; }
}</code></pre>

Lastly I know that I want to edit data so I can look at the [ArcGIS REST API documentation](http://resources.arcgis.com/en/help/arcgis-rest-api/#/Apply_Edits_Feature_Service_Layer/02r3000000r6000000/) to find out what request I need to use to add new data. Then using the same technique as above I can easily replicate that functionality. 

Take a look at the [service code](https://github.com/davetimmins/Randomer/blob/master/TestWeb/ServiceInterface/RandomDataService.cs) for how it is all plumbed together.

So for a few hours of work I have saved myself the tedious task of manually creating data, but it may also be useful for other work in the future and as with any automated process I can reuse and abuse as much as I like. I encourage you to fork away and send me pull requests if you are that way inclined or just leave me a comment with your thoughts.
