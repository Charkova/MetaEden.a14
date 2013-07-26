using UnityEngine;
using System.Collections;


public class TestWWWConnector : MonoBehaviour {
	
	public string url;

	// Use this for initialization
	void Start() {
        //testConn = new MyPhotonClient();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void FetchTestURL() {
		Debug.Log ("FetchTestURL called");
		StartCoroutine("GetWebData");
		DoSFS();
		
	}
	
	IEnumerator GetWebData() {
		Debug.Log("GetWebData called");
		WWW www = new WWW(url);		
        yield return www;
		Debug.Log(www.text);	
		
    }
	
	public void DoSFS() {
		
		this.gameObject.AddComponent ("MySFSConnect");
		
		
		
		
	}
	
	
}
