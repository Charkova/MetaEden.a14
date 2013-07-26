using UnityEngine;
using System;
using System.Collections;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Requests;
using Sfs2X.Logging;
using Coherent.UI;
using Coherent.UI.Binding;

public class SFSChat : MonoBehaviour {
	
	public CoherentUIView ViewComponent;
	private SmartFox smartFox; // the smartfox server
		
	// Use this for initialization
	void Start () {
		smartFox = SmartFoxConnection.Connection;
		
		if(smartFox == null){
			smartFox = new SmartFox();
		}
		
		smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
		
		Debug.Log("The last room you joined was:");
		//Debug.Log(smartFox.LastJoinedRoom.Name);
		
		
		ViewComponent = GetComponent<CoherentUIView>();
		if (ViewComponent)
		{
			ViewComponent.OnReadyForBindings += this.RegisterBindings;
		}
	
		ViewComponent.ReceivesInput = true;
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void FixedUpdate() {
		smartFox.ProcessEvents(); //this is the code that runs the server communication loop, must have
	}
	
	
	private void RegisterBindings(int frame, string url, bool isMain)
	{
		if (isMain)
		{
			var view = ViewComponent.View;
			if (view != null)
			{
				var msg = "";
				view.BindCall("ChatMessageEntry", (Action<string>)this.SendChatMessage);
			}
		}
	}
		
	
	void SendChatMessage(string msg) {
		Debug.Log(msg);
		smartFox.Send( new PublicMessageRequest(msg) );
		
	}
	
	void OnPublicMessage(BaseEvent evt) {
		User sender = (User)evt.Params["sender"];
		
		if(sender == smartFox.MySelf) {
			Debug.Log ("I said " + (string)evt.Params["message"]);
		} else {
			Debug.Log ("User " + sender.Name + " said: " + (string)evt.Params["message"]);
		}
		
		ViewComponent = GetComponent<CoherentUIView>();
		var view = ViewComponent.View;
		if (view != null)
		{
			view.TriggerEvent("PostChatMessage", (string)evt.Params["message"]);
			Debug.Log("message sent to UI");
		}
	}
}
