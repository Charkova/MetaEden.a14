using UnityEngine;
using System.Collections;

public class BlurControl : MonoBehaviour {
	
	float value; 
	
	// Use this for initialization
	void Start () {
		value = 0.0f;
		transform.renderer.material.SetFloat("_blurSizeXY",value);
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetButton("Up"))
		{
			value = value + 0.01f;
			if (value>10f) value = 10f;
			transform.renderer.material.SetFloat("_blurSizeXY",value);
			Debug.Log (value);
		}
		else if(Input.GetButton("Down"))
		{
			value = (value - 0.01f) % 10.0f;
			if (value<0f) value = 0f;
			transform.renderer.material.SetFloat("_blurSizeXY",value);
		}		
	}
	
	void OnGUI () {
		GUI.TextArea(new Rect(10f,10f,200f,50f), "Press the 'Up' and 'Down' arrows \nto interact with the blur plane\nCurrent value: "+value);
		}
}
