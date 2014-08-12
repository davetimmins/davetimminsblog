Esri recently released support for authenticating users and applications using OAuth 2 on their new developer site. In this post we will look at adding a provider to an existing web site built using [ServiceStack](http://www.servicestack.net/) to see how you can integrate your ArcGIS Online (AGO) users with another web application.
The ArcGIS [documentation](https://developers.arcgis.com/en/authentication/) covers user and app logins. We will be focusing on user logins since app logins will most likely be transparent to end users via a proxy or the like. To get started you should [sign in](https://developers.arcgis.com/en/sign-in/) to ArcGIS for Developers with your subscription or developer account and in your applications choose new application and set the details.

As we want to use OAuth we also need to set the Redirect URI, this should match the location of the provider. For testing my application looks like 

<img src="/Assets/oauth1.jpg" alt="new application" class="pure-img"/>

once created we can select API Access from the sidebar to get the Client ID and Secret to use when authenticating. Make sure to note these down but do not share them publically

<img src="/Assets/oauth2.jpg" alt="API access" class="pure-img"/>

Now that we have our application configured with AGO we are ready to code the OAuth provider. To speed testing up I used the ServiceStack [Social Bootstrap template](https://github.com/ServiceStack/SocialBootstrapApi) as this already has similar functionality so it is just a matter of adding the new provider. ServiceStack makes this very easy thanks to its excellent documentation and wealth of sample code. 

The details for parameters and urls to call are on the ArcGIS website so I'll let you look at the code rather than waffling on about it but in a nutshell we are getting verification from AGO that the user is who they say they are and then getting some additional user information via the ArcGIS Portal API.

There is already an OAuthProvider base class for us to inherit from so we'll base our implementation on that. The only other steps we need to follow are to register our new authentication provider with ServiceStack by including it as an authentication method in our AppHost

<pre><code class='cs'>public class ArcGISAuthProvider : OAuthProvider ...
 
//Register all Authentication methods you want to enable for this web app.            
Plugins.Add(new AuthFeature(() => new CustomUserSession(), //Use your own typed Custom UserSession type
    new IAuthProvider[] {
        new CredentialsAuthProvider(),              //HTML Form post of UserName/Password credentials
        new TwitterAuthProvider(appSettings),       //Sign-in with Twitter
        new FacebookAuthProvider(appSettings),      //Sign-in with Facebook
        new DigestAuthProvider(appSettings),        //Sign-in with Digest Auth
        new BasicAuthProvider(),                    //Sign-in with Basic Auth
        new GoogleOpenIdOAuthProvider(appSettings), //Sign-in with Google OpenId
        new YahooOpenIdOAuthProvider(appSettings),  //Sign-in with Yahoo OpenId
        new OpenIdOAuthProvider(appSettings),       //Sign-in with Custom OpenId
        new ArcGISAuthProvider(appSettings)         // Sign-in using our AGO Id
    }));
</code></pre>

then add the configuration application settings to our web.config to pass through the Client ID and Client Secret that we created earlier. The convention the ServiceStack uses for these is 

<pre><code class='xml'>&lt;add key="oauth.arcgis.ConsumerKey" value=""/>
&lt;add key="oauth.arcgis.ConsumerSecret" value=""/>
</code></pre>

where arcgis is the name defined in our provider. Though as with pretty much everything in ServiceStack the key names can be configured. 

Finally you will need some UI to show the user a link to click and choose to sign in with ArcGIS. This will be a link to points to the same place as the redirect url we configured when adding our application to AGO. Hopefully someone will make a nice icon for people to embed in their sites.

In Index.cshtml under Views/Shared

<pre><code class='html'>&lt;div id="arcgis-signin">
    &lt;a href="~/api/auth/arcgis">Sign in&lt;/a>
&lt;/div>
</code></pre>

When you run the app and click on the sign in with ArcGIS link you will be redirected to AGO where you can now enter your AGO account credentials.

<img src="/Assets/oauth3.jpg" alt="ArcGIS Online login" class="pure-img"/>

The full code for the ArcGISAuthProvider is listed below. Thanks for reading.

<pre><code class='cs'>using ServiceStack.Configuration;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;
using ServiceStack.ServiceInterface.Auth;
using ServiceStack.Common;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
 
public class ArcGISAuthProvider : OAuthProvider
{
    public const string Name = "arcgis";
    public static string Realm = "https://www.arcgis.com/sharing/oauth2/";
 
    public ArcGISAuthProvider(IResourceManager appSettings)
        : base(appSettings, Realm, Name) { }
 
    public override object Authenticate(IServiceBase authService, IAuthSession session, ServiceStack.ServiceInterface.Auth.Auth request)
    {
        var tokens = Init(authService, ref session, request);
 
        var error = authService.RequestContext.Get<IHttpRequest>().QueryString["error"];
        var isPreAuthError = !error.IsNullOrEmpty();
        if (isPreAuthError) return authService.Redirect(session.ReferrerUrl);
 
        var code = authService.RequestContext.Get<IHttpRequest>().QueryString["code"];
        var isPreAuthCallback = !code.IsNullOrEmpty();
        if (!isPreAuthCallback)
        {
            var preAuthUrl = Realm + "authorize?response_type=code&client_id={0}&redirect_uri={1}";
            preAuthUrl = preAuthUrl.Fmt(ConsumerKey, CallbackUrl.UrlEncode());
 
            authService.SaveSession(session, SessionExpiry);
            return authService.Redirect(preAuthUrl);
        }
 
        var accessTokenUrl = Realm +
                             "token?grant_type=authorization_code&code={0}&redirect_uri={1}&client_id={2}&client_secret={3}";
        accessTokenUrl = accessTokenUrl.Fmt(code, CallbackUrl.UrlEncode(), ConsumerKey, ConsumerSecret);
 
        // get the access token and store the result
        var contents = accessTokenUrl.GetStringFromUrl();
        var authInfo = JsonObject.Parse(contents);
        tokens.AccessToken = authInfo["access_token"];
 
        if (tokens.AccessToken.IsNullOrEmpty())
            return authService.Redirect(session.ReferrerUrl.AddHashParam("f", "AccessTokenFailed"));
 
        tokens.RefreshToken = authInfo["refresh_token"];
        tokens.RefreshTokenExpiry = DateTime.UtcNow.AddSeconds(Double.Parse(authInfo["expires_in"]));
        tokens.UserName = authInfo["username"];
 
        session.IsAuthenticated = true;
 
        OnAuthenticated(authService, session, tokens, authInfo.ToDictionary());
        authService.SaveSession(session, SessionExpiry);
 
        return authService.Redirect(session.ReferrerUrl.AddHashParam("s", "1"));
    }
 
    protected override void LoadUserAuthInfo(AuthUserSession userSession, IOAuthTokens tokens, Dictionary<string, string> authInfo)
    {
        var url = "https://www.arcgis.com/sharing/rest/community/users/{0}?f=json";
        url = url.Fmt(authInfo["username"]);
        var json = url.GetStringFromUrl();
 
        var data = JsonObject.Parse(json);
        tokens.DisplayName = data.Get("fullName");
        tokens.FullName = data.Get("fullName");
        // todo : get more data if available
    }
 
    public override void LoadUserOAuthProvider(IAuthSession authSession, IOAuthTokens tokens)
    {
        var userSession = authSession as AuthUserSession;
        if (userSession == null) return;
 
        userSession.DisplayName = tokens.DisplayName ?? userSession.DisplayName;
        userSession.FullName = tokens.FullName ?? userSession.DisplayName;
        userSession.FirstName = tokens.FirstName ?? userSession.FirstName;
        userSession.LastName = tokens.LastName ?? userSession.LastName;
        userSession.PrimaryEmail = tokens.Email ?? userSession.PrimaryEmail ?? userSession.Email;
    }
}
</code></pre>