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

public class MEUIChatBox : MonoBehaviour {
	
	private SmartFox smartFox;
	
	void Start () {
		// get the current smartfox connection
		smartFox = SmartFoxConnection.Connection;
		
		// log an error and just quit the script if no smartfox connection
		if(smartFox == null){
			Debug.Log ("MEUIChatBox - Init Failed: There was no smartfox connection.");
			return;
		}
		
		//smartFox.AddEventListener(SFSEvent.PUBLIC_MESSAGE, OnPublicMessage);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
