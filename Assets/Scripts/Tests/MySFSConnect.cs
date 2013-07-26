using UnityEngine;
using System.Collections;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Requests;
using Sfs2X.Logging;

public class MySFSConnect : MonoBehaviour {
	
	public string serverName = "99.9.138.92";
	//public string serverName = "localhost";
	//public string serverName = "24.49.9.98";
	//public string serverName = "127.0.0.1";
	//public string serverName = "192.168.0.11";
	public int serverPort = 9933;
	public string zone = "BasicExamples";
	public bool debug = true;
	
	private SmartFox smartFox; // the smartfox server
	
	private bool shuttingDown = false;
	
	private string username = "demz";
	private string loginErrorMessage = "";
	
	
	// Use this for initialization
	void Awake() {
		
		Application.runInBackground = true; // so unity doesn't timeout when switching focus
		
		// initialize smartfox with debug flag
		smartFox = new SmartFox(debug);
		
		// Register callback delegate
		smartFox.AddEventListener(SFSEvent.CONNECTION, OnConnection);
		smartFox.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
		smartFox.AddEventListener(SFSEvent.LOGIN, OnLogin);
		smartFox.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);
		smartFox.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
		smartFox.AddEventListener(SFSEvent.ROOM_JOIN_ERROR, OnRoomJoinError);
		smartFox.AddEventListener(SFSEvent.ROOM_ADD, OnRoomAdded);

		smartFox.AddLogListener(LogLevel.DEBUG, OnDebugMessage);

		smartFox.Connect(serverName, serverPort);
	}
	
	void Start () {
				
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void FixedUpdate() {
		smartFox.ProcessEvents(); //this is the code that runs the server communication loop, must have
	}
	
	
	/***************
	 * smart fox server callback handlers
	 * *************/
	
	void OnApplicationQuit() {
		shuttingDown = true;
	}
	
	public void OnConnection(BaseEvent evt) {
		bool success = (bool)evt.Params["success"];
		string error = (string)evt.Params["error"];
		
		Debug.Log("On Connection callback got: " + success + " (error : <" + error + ">)");
		
		loginErrorMessage = "";
		if (success) {
			SmartFoxConnection.Connection = smartFox;
			Debug.Log ("You are connected!!!");
			
			smartFox.Send( new LoginRequest(username, "", zone) );
			
		} else {
			loginErrorMessage = error;
		}
		
		
	}

	public void OnConnectionLost(BaseEvent evt) {
		loginErrorMessage = "Connection lost / no connection to server";
	}

	public void OnDebugMessage(BaseEvent evt) {
		string message = (string)evt.Params["message"];
		Debug.Log("[SFS DEBUG] " + message);
	}

	public void OnLogin(BaseEvent evt) {
		bool success = true;
		if (evt.Params.ContainsKey("success") && !(bool)evt.Params["success"]) {
			// Login failed - lets display the error message sent to us
			loginErrorMessage = (string)evt.Params["errorMessage"];
			Debug.Log("Login error: "+loginErrorMessage);
		} else {
			// Startup up UDP
			Debug.Log("Login ok");
			//SmartFoxConnection.Connection = smartFox;
			smartFox.InitUDP(serverName, serverPort);
			
			
			
			
			
		}			
	}

	public void OnUdpInit(BaseEvent evt) {
		if (evt.Params.ContainsKey("success") && !(bool)evt.Params["success"]) {
			loginErrorMessage = (string)evt.Params["errorMessage"];
			Debug.Log("UDP error: "+loginErrorMessage);
		} else {
			Debug.Log("UDP ok");
		
			// On to the lobby
			loginErrorMessage = "";
			
			
			var numMaxUsers = 5;
			var roomName = "My Room";
			RoomSettings settings = new RoomSettings(roomName);
			//settings.GroupId = "game";
			//settings.IsGame = true;
			//settings.MaxUsers = (short)numMaxUsers;
			//settings.MaxSpectators = 0;
			smartFox.Send(new CreateRoomRequest(settings));
			smartFox.Send( new JoinRoomRequest(roomName) );
			
			
		}
	}
	
	
	public void OnRoomJoin (BaseEvent evt) {
		Debug.Log ("You have joined the room: " + smartFox.LastJoinedRoom.Name);
		//Debug.Log (evt.Params);
		Application.LoadLevel(1);
	}
	
	
	public void OnRoomJoinError (BaseEvent evt) {
		Debug.Log ("ERROR JOINING ROOM");
	}
	
	
	public void OnRoomAdded (BaseEvent evt) {
		Debug.Log("A ROOM WAS CREATED");
		//Debug.Log (evt.Params["room"]);
	}
	
	
	
	
}
