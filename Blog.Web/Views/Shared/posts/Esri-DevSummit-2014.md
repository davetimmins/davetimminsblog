This was held once again in Palm Springs and was my 5th time attending. Every year this event gets bigger and there were around 1800 attendees comprised of Esri staff, distributors and customers. I personally like the venue as it can easily accomodate everyone and is easy to get around including the expo area. The only real gripe would be the food which was pretty average.

I noticed a clear difference in the messaging this year. Previously there has been a focus on cloud technology or configurable tools but this time there were 2 main themes that stood out: Platform and SDKs. Platform is obviously looking at the big picture and encompassing the entire Esri suite whereas the SDKs are really about giving developers a tool to build a great experience for their customers on any technology they want. This is great that we are getting the same set of functionality across the various stacks and it is all achieved by having a common core c++ runtime that each of them uses. The available SDKs at time of writing are:

 - JavaScript
 - iOS
 - Android
 - .NET (WPF, WP, WinStore)
 - Java
 - Qt
 - Silverlight
 - Flex
 - OSX
 
That's a pretty impressive list and if you can't find an SDK to use from those then I'd be interested to hear why. If you are unfamiliar with the Esri SDKs and want to know more then the best place to start is at [developers.arcgis.com](https://developers.arcgis.com)

These messages were highlighted during the plenary but were also completmented by the keynote which this year was given by Chris Wanstroth the GitHub CEO. Chris delivered a highly entertaining talk about his history and of course about GitHub which Esri is now using extensively. A particular quote of his that resonated with me was
>Collaborate to build valuable software where the customer is the user

This is always a challenge in software development, actually making something that people want to use and is an area of constant improvement.

As a final note about the conference as a whole, this is the best forum for meeting other developers and core Esri staff. Also, I suck at dodgeball.

####Technology

#####Offline! 
Finally we have the capability to use data offline. This has been requested for what seems like forever and with the 10.2.2 release of ArcGIS Server and the runtime SDKs we can now do this. The deoms that were shown of this all worked well and if you ar efamiliar with the way it worked with ArcGIS Mobile (or ArcGIS for Windows Mobile) then the concept should be straightforward. There was talk of being able to take entire maps offline in the future rather than just basemaps or layers so I can only see this whole experience getting better and better. 

#####Vector Basemaps
I have been saying for a while now (to anyone who'll listen) that tiled/cached basemaps will not be around for long. They are a pain to maintain, take up lots of storage, can take a long time to generate / copy around and don't look great on high dpi devices since they are usually built at 96dpi. This last point is becoming increasingly relevant with retina and QHD displays that are commonplace now. Vector basemaps solve lots of these issues and are already used in google / apple maps so it's good to see Esri working on it. It should see the light of day around the middle of the year and from what was shown the performance is fantastic.

#####JS API v4
We are currently on v3.8 of the JavaScript API with 3.9 and 3.10 likely to come before v4 but Esri teased a few features that are coming for that release. 

 * 3D (3D is actually coming to all the runtimes)
 * Client side projections
 * Animation
 * View padding

#####JSO
JSO or the JavaScript Optimizer allows custom builds of the JS API. This is another highly sought after feature and will allow you to pick the modules you need and then generate a build of the JS API continaing those that you can either download and host yourself or host online. Very handy for speeding up page loads. It isn't live yet but I believe it'll be at [jso.arcgis.com](http://jso.arcgis.com) once it is.

#####Cross Platform
This always gets raised and with the increase in popularity of Xamarin there is always the call for support there. This isn't being actively developed but if you want it I suggest voting up the [idea](http://ideas.arcgis.com/apex/ideaSearchResults?s=xamarin&searchButton=search) on the ideas website. What Esri do have for cross platform development is Qt. With Qt you can write c++ applications that run across pretty much any device. Later this year the Esri Qt SDK will be getting support for QML which is a declarative language for writing the UI.

#####Server / Portal 10.3
Portal is a product that I am seeing gain in popularity due to the features it has and the business functions it meets. If you aren't familiar with Portal think of it as ArcGIS Online in your organisation behind your firewall. Some of the upcoming features are:

* Activity dashboard for portal
* Service stats in Server Manager
* High availability portal configurations
* Control group membership via AD for portal

#####EMF
Esri Maps Framework is a JavaScript based framework for integrating Esri with BI products such as SAP, SharePoint, Dynamics etc. It can also be used to create your own integrations but perhaps more useful is the ability to use its extensibility to enhance the current integrations, this is coming to Office and SharePoint initially. The EMF is available to Esri distributors and business partners

#####Web App Builder
Yet another highly requested product has been a HTML/JS equivalent of the Flex and Silverlight builders and now we have it. The public beta is available at [betacommunity.esri.com](http://betacommunity.esri.com/) and I encourage you to check it out for yourself. In a nutshell tools like this are great as they reduce the amount of boilerplate work and allow us to get into crafting workflow specific tools and a unique user experience.

####Wrapping Up
If you are interested in seeing more then I recommend checking out the [videos online](http://video.esri.com/series/166/2014-esri-international-developer-summit-plenary) and lots of the presentation code is on [GitHub](http://esri.github.io/). 

I'd love to hear your feedback if you attended or if you have any other queries / suggestions.