A common request for web map applications is to have some sort of bookmark function. Bookmarks are just a quick way of jumping to a saved extent or view for your map and they are usually presented as a list of meaningful names. This is fine when you are using an application but what if you just want a way of sharing an extent with someone or creating a permalink of your extent / view. Here's a very quick and easy way to do it for the [Esri JS API](https://developers.arcgis.com/javascript/) using the `window.location.hash`.

We only need to do 2 things for this. First, since we want to know the extent we need to hook into the map `extent-change` event and update the `location.hash`. Second, when we start the app we need to check the `location.hash` and if an extent is there then set the map extent accordingly. That's it!

Here's the entire code for the functionality and a [live demo](http://joosh.azurewebsites.net) 

<pre><code>define([
    'esri/geometry/Extent',
    'dojo/_base/lang',
    'dojo/_base/declare',
    'dojo/on',
    'dojo/_base/json',
    'dojo/dom'],
    function (
        Extent,
        lang,
        declare,
        on,
        dojo,
        dom
    ) {
        return declare('joosh.Bookmarker', null, {
            map: null,

            constructor: function (params) {
                lang.mixin(this, params);

                if (this.map) {

                    this.map.on('load', function () {
                        if (window.location.hash) {
                            var hashExtent = dojo.fromJson('{' + window.location.hash.replace('#', '') + '}');
                            var savedExtent = new Extent(
                            {
                                xmin: hashExtent.xmin,
                                ymin: hashExtent.ymin,
                                xmax: hashExtent.xmax,
                                ymax: hashExtent.ymax,
                                spatialReference: this.map.spatialReference
                            });
                            this.map.setExtent(savedExtent);
                        }

                        this.map.on('extent-change', function (e) {
                            window.location.hash =
                                "xmin:" + this.map.extent.xmin +
                                ",ymin:" + this.map.extent.ymin +
                                ",xmax:" + this.map.extent.xmax +
                                ",ymax:" + this.map.extent.ymax;
                        });
                    });
                }
            }
        });
    });
</code></pre>

Now to enable this you just need

<pre><code>this.map = new Map('map', config.options);

require(['joosh/Bookmarker'], function (Bookmarker) {
    new Bookmarker({ map: this.map});
});
</code></pre>