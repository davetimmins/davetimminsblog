[ArcGIS.PCL](https://github.com/davetimmins/ArcGIS.PCL) is a portable class library for calling ArcGIS Server resources. It is currently built to target .NET for Windows Store apps, .NET framework 4.5, Silverlight 4 and higher & Windows Phone 7.5 and higher. It provides a mechanism to call an ArcGIS Server REST endpoint and to work with the corresponding response.

I created this project after having several small projects that required some form of communication directly to ArcGIS Server. Since there is no nice way of using an existing Esri SDK from a server side web application or console application I found myself having to write calls directly. Whilst not particularly hard, I found that the code across these projects was similar and so I decided to create a separate project to wrap these calls. It is not intended to be a replacement for any of the Esri SDKs but can be used in situations where they are either overkill or not readily accessible.

The project uses the concept of a [gateway](https://github.com/davetimmins/ArcGIS.PCL/blob/master/ArcGIS.ServiceModel/IPortalGateway.cs) to broker the calls to ArcGIS. This gateway is responsible for determining the call and formatting the request and response types. Requests can be to either secure or insecure resources and each call is referred to as an operation. The gateway caters for secure calls automatically if it is created with user credentials (this assumes that ArcGIS Server token based security is used). Once the gateway is created it can be thought of as the root site for an ArcGIS Server instance.

The current typed operations supported are:

* Generate token 
  * uses the token service 
  * automatically generated if an ITokenProvider is passed to the gateway
  * auto appended to each request
  * automatically regenerated if the token expires
* Query
  * attribute or spatial query
* Apply Edits 
  * add, update, delete for a layer in a feature service
* Single Input Geocode 
* Reverse Geocode
* Project
* Simplify

in addition there are two gateway methods used for diagnostics (I could see these being useful when used in a scripting context)

* Ping
  * determine if a Url can be reached
* Describe Site 
  * returns all resource endpoints for an ArcGIS Server instance 
  * useful for discovery, validating environments are the same (DEV, TEST, PROD), checking access for different user logons, producing documentation ([example site](https://arcgissitedescriptor.azurewebsites.net/))


Each time you want to call an operation it is just a case of knowing the endpoint Url for it. Since the gateway already has the root Url, these Url's are relative to the root for each resource. The interface [IEndpoint](https://github.com/davetimmins/ArcGIS.PCL/blob/master/ArcGIS.ServiceModel/Common/IEndpoint.cs) is used for this and for the operations defined it is in the context of an ArcGISServerEndpoint which handles the endpoint Url construction.

There are also some common objects used for operations that are included. The most notable of these is a [Feature](https://github.com/davetimmins/ArcGIS.PCL/blob/master/ArcGIS.ServiceModel/Common/Feature.cs). This contains [geometry](https://github.com/davetimmins/ArcGIS.PCL/blob/master/ArcGIS.ServiceModel/Common/IGeometry.cs) and associated attributes and is used for most common operations. Since it can be serialized as JSON it can also be used to feed into an existing Esri SDK or Web API.

Due to the project being a portable class library I have split the de/serialization out so that it is specific to each implementation. Each gateway needs an ISerializer implementation that will be used when calling resources. The test project has ServiceStack.Text and Json.NET [examples](https://github.com/davetimmins/ArcGIS.PCL/blob/master/ArcGIS.Test/ISerializer.cs).

In addition to being able to used the typed operations above, you can also call any resource and get the data back by requesting a dynamic object as the return type. There is an example in the tests that shows this where the return is defined as
<pre><code>public class AgsObject : ServiceStack.Text.JsonObject, IPortalResponse
{
    [System.Runtime.Serialization.DataMember(Name = "error")]
    public ArcGISError Error { get; set; }
}
</code></pre>

by using this type, the return value is created as a dictionary containing the results, handy if you don't want to create a class for the result type (though using the VS Paste Special -> Paste JSON as Classes makes this pretty quick).

If you are still reading then hopefully you want to get the code and start using it. The quickest way to get started is to install the library using NuGet. Search for [ArcGIS.PCL](https://www.nuget.org/packages/ArcGIS.PCL/) in the package manager and install it, or you can get the code directly from [GitHub](https://github.com/davetimmins/ArcGIS.PCL/releases). Once you have the code the first thing to do is create your gateway and ISerializer implementation (or use an existing example).

Some typical uses could be
<pre><code>// ArcGIS Server with non secure resources
public class ArcGISGateway : PortalGateway
{
    public ArcGISGateway(ISerializer serializer)
        : base("http://sampleserver3.arcgisonline.com/ArcGIS/", serializer)
    { }
}
 
... new ArcGISGateway(serializer);
 
// ArcGIS Server with secure resources
public class SecureGISGateway : SecureArcGISServerGateway
{
    public SecureGISGateway(ISerializer serializer)
        : base("http://serverapps10.esri.com/arcgis", "user1", "pass.word1", serializer)
    { }
}
 
... new SecureGISGateway(serializer);
 
// ArcGIS Server with secure resources and token service at different location
public class SecureTokenProvider : TokenProvider
{
    public SecureTokenProvider(ISerializer serializer)
        : base("http://serverapps10.esri.com/arcgis", "user1", "pass.word1", serializer)
    { }
}
 
public class SecureGISGateway : PortalGateway
{
    public SecureGISGateway(ISerializer serializer, ITokenProvider tokenProvider)
        : base("http://serverapps10.esri.com/arcgis", serializer, tokenProvider)
    { }
}
 
... new SecureGISGateway(serializer, new SecureTokenProvider(serializer));
 
// ArcGIS Online either secure or non secure
... new ArcGISOnlineGateway(serializer);
 
... new ArcGISOnlineGateway(serializer, new ArcGISOnlineTokenProvider("user", "pass", serializer));
</code></pre>

hopefully you find it easy to work with and any feedback is welcome.