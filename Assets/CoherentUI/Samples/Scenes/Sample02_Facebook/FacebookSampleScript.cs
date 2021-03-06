using UnityEngine;
using System.Collections;
using Coherent.UI;

#if !UNITY_IPHONE || UNITY_EDITOR
class FacebookSampleScript : MonoBehaviour
{
	public const string FacebookAppURL = "http://www.coherent-labs.com/sample.html";
	public const string LocalAppURL = "coui://UIResources/FacebookSample/facebook.html";
	
	public void Start()
	{
		var viewComponent = GetComponent<CoherentUIView>();
		viewComponent.OnViewCreated += (view) => { view.InterceptURLRequests(true); };
		viewComponent.Listener.URLRequest += OnURLRequestHandler;
	}
	
	void OnURLRequestHandler (string url, URLResponse response)
	{
		if (url.StartsWith(FacebookAppURL))
		{
			// change the url, keeping all parameters intact
			string redirectURL = LocalAppURL + url.Substring(FacebookAppURL.Length);
			response.RedirectRequest(redirectURL);
			return;
		}
		response.AllowRequest();
	}
}
#endif