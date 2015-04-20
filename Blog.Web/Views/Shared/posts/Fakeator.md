[Geocode services](http://resources.arcgis.com/en/help/arcgis-rest-api/#/Geocode_Service/02r3000000q9000000/) are great for searching against one or more spatial datasets. Setting one up requires creating a locator or for more advanced scenarios composite locators which comprise of multiple locators with rules around search order and execution. What happens if you don't have access to the underlying data that you want to search though, or if there are already ArcGIS Server services published that you want to search against. Well here is a solution using a fake locator (fakeator) that works with the Esri JS [geocoder dijit](https://developers.arcgis.com/javascript/jsapi/geocoder-amd.html) for both custom applications and using ArcGIS Online. I'm using [Nancy](https://github.com/NancyFx/Nancy) here but I could easily use another web framework if needed.

If you want to try out the example it's at [fakeator.azurewebsites.net](http://fakeator.azurewebsites.net/index.html) and the source is on [GitHub](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects/tree/master/UnifiedSearch.Nancy.Sample)

The features I want to provide are
 
 - Work with the geocoder dijit
 - Work with ArcGIS online / portal
 - Searching of published ArcGIS server services via query and find operations

####Geocoder Dijit

In order to work with the geocoder dijit I need to make sure that I am providing the same interface that a standard published Geocode service provides. This is easy to find via the documentation and looking at that in combination with requests that the geocode dijit makes I can see that I only really need the [find address candidates](http://resources.arcgis.com/en/help/arcgis-rest-api/#/Find_Address_Candidates/02r3000000wv000000/) operation.

So in my search service I'll define a route for that

<pre><code class='cs'>Get[@"/GeocodeServer/findAddressCandidates", true] = async (x, ct) =>
{
	return await EsriSearch();
};</code></pre>

As well as having my service I need to tell the client how to use it. To do this I can configure the geocoder dijit to use a custom locator with the following

<pre><code class='js'>require(["esri/map", "esri/dijit/Geocoder", "dojo/domReady!"], function (Map, Geocoder)
{
	var map = new Map("map", {
		basemap: "topo",
		center: [-117.19, 34.05], // lon, lat
		zoom: 13
	});

	var myGeocoders = [{
		url: "http://fakeator.azurewebsites.net/GeocodeServer",
		name: "Unified Search",
		singleLineFieldName: "SingleLine"
	}];
	var geocoder = new Geocoder({
		map: map,
		autoComplete: true,
		arcgisGeocoder: false,
		geocoders: myGeocoders,
		value: "100 willis",
		searchDelay: 10
	}, "search");
	geocoder.startup();
});</code></pre>

Using this dijit also has the advantage of providing type ahead searching by default.

####ArcGIS Online Integration

You may have noticed that I am including `/GeocodeServer` in the url paths above. This is to make sure that I can use the implementation with ArcGIS Online. If you log in to your organization account and go to the settings you have the option to add geocoders under the utility services section. In order for your service to be added it must be identified by ArcGIS Online as a valid Esri Locator service so I achieve this by providing the expected response when the service is interrogated via the `/GeocodeServer` endpoint. 

<img src="/Assets/configure fakeator.png" alt="configure fakeator" class="pure-img"/>

The search service implementation just returns the response from an existing valid Locator service.

<pre><code class='cs'>Get[@"/GeocodeServer"] = _ =>
{
	var content = new HttpClient().GetStringAsync("http://geocode.arcgis.com/arcgis/rest/services/World/geocodeserver?f=json").Result;

	return Response.AsText(content).WithContentType("application/json");
};</code></pre>

####Searching using Query and Find Operations

To provide some flexibility with the searching I allow for both query and find operations to be performed. Query works against a single layer in a service whereas find can work across one or more layers and fields. Take a look at the [code](https://github.com/davetimmins/ArcGIS.PCL-Sample-Projects/blob/master/UnifiedSearch.Nancy/Interface/SearchService.cs#L47) for this yourself if you are interested. 

So that the search can be easily changed / configured I have a basic JSON configuration file with my search options which for the published example looks like

<pre><code class='json'>{
"querySearches":[
{
	"endpoint":"http://s3.demos.eaglegis.co.nz/ArcGIS/rest/services/LINZ/crs/MapServer/1",
	"expression":"STREET_ADDRESS LIKE '{0}%'",
	"type":"esriGeometryPoint"
},
{
	"endpoint":"http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/1",
	"expression":"ZONE = {0}",
	"type":"esriGeometryPolygon",
	"regex":"(-?[0-9]|[0-9]\\d|13)$"
}],
"findSearches":[
{
	"endpoint":"http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer",
	"searchFields":["CITY_NAME","CONTINENT"],
	"layerIds":[0,1]
}],
"returnFields":["CITY_NAME","CONTINENT","STREET_ADDRESS","ZONE"],
"outputWkid":102100
}</code></pre>

With all this in place the final step is to ensure that I am returning the correct reponse type. Again this is a case of checking the [REST API documentation](http://resources.arcgis.com/en/help/arcgis-rest-api/#/Find_Address_Candidates/02r3000000wv000000/) and then converting the results from the various requests into the correct type. In this case I want a result that returns a list of candidate locations and an associated spatial reference.

<pre><code class='json'>{
"spatialReference": {"wkid" : 4326},
"candidates" : [
  {
  "address" : "1 MASON ST",
  "location" : { "x" : -122.408951, "y" : 37.783206 },
  "score" : 75,
  "attributes" : {"StreetName" : "MASON", "StreetType" : "ST"}
  },
  {
  "address" : "49 MASON ST",
  "location" : { "x" : -122.408986, "y" : 37.783460 },
  "score" : 27,
  "attributes" : {"StreetName" : "MASON", "StreetType" : "ST"}
  }
]
}
</code></pre>

Now when I run the application I can search using the data configured and if I need to add or change where I want to search I just need to update my configuration.

<img src="/Assets/fakeator search.png" alt="fakeator search" class="pure-img"/>
<br />
