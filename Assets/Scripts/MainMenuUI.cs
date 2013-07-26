using UnityEngine;
using System.Collections;
using Coherent.UI;
using Coherent.UI.Binding;


public class MainMenuUI : MonoBehaviour {


    public CoherentUIView ViewComponent;

	// Use this for initialization
	void Start ()
	{
		ViewComponent = GetComponent<CoherentUIView>();
		if (ViewComponent)
		{
			ViewComponent.OnReadyForBindings += this.RegisterBindings;
		}
	
		ViewComponent.ReceivesInput = true;
	}
	
	void Update ()
	{
		
	}	
	
	private void RegisterBindings(int frame, string url, bool isMain)
	{
		if (isMain)
		{
			var view = ViewComponent.View;
			if (view != null)
			{
				TestWWWConnector testConnector = GetComponent<TestWWWConnector>();
				view.BindCall("MenuConnect", (System.Action)testConnector.FetchTestURL);
				view.BindCall("MenuExit", (System.Action)this.MenuExit);
			}
		}
	}
	
	
	private void MenuConnect()
	{
		Debug.Log("connect command");
		//Application.LoadLevel(1);
						
	}
	
	private void MenuExit()
	{
		Debug.Log("exit command");
		Application.Quit();
	}
	
	public void TriggerSomeUIEvent(string msg)
	{
		Debug.Log("triggering event on UI");

		ViewComponent = GetComponent<CoherentUIView>();
		var view = ViewComponent.View;
		if (view != null)
		{
			view.TriggerEvent("myJavascriptFunction", msg);
			Debug.Log(msg);
		}
				
	}	
	
		
	// old...
	IEnumerator LoadGameScene()
	{
		// Display a loading screen
		var viewComponent = GetComponent<CoherentUIView>();
		viewComponent.View.Load("coui://UIResources/MenuAndHUD/loading/loading.html");
		// The game level is very simple and loads instantly;
		// Add some artificial delay so we can display the loading screen.
		yield return new WaitForSeconds(2.5f);
		
		// Load the game level
		//Application.LoadLevelAsync("game");
	}
}
