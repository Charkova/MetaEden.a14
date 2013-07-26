#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
using Coherent.UI;
#elif UNITY_IPHONE
using Coherent.UI.Mobile;
#endif

/// <summary>
/// Component controlling the CoherentUI System
/// </summary>
public class CoherentUISystem : MonoBehaviour {

	private static CoherentUISystem m_Instance = null;
	public static CoherentUISystem Instance
	{
		get
		{
			if (m_Instance == null)
			{
				m_Instance = Component.FindObjectOfType(typeof(CoherentUISystem)) as CoherentUISystem;
				if (m_Instance == null)
				{
					m_Instance = CoherentUISystem.Create();
					if (m_Instance == null)
					{
						throw new System.ApplicationException("Unable to create Coherent UI System");
					}
				}
			}
			return m_Instance;
		}
	}

	public const byte COHERENT_PREFIX = 177;
	public enum CoherentRenderingEventType
	{
		DrawView = 1,
		DrawFlipYView = 2,
		WakeRenderer = 3
	};
	
	private UISystem m_UISystem;
	private SystemListener m_SystemListener;
	private ILogHandler m_LogHandler;
	private FileHandler m_FileHandler;

	/// <summary>
	/// Creates the FileHandler instance for the system. Change to allow usage of custom FileHandler
	/// </summary>
	public static System.Func<FileHandler> FileHandlerFactoryFunc = () => { return new UnityFileHandler(); };

	/// <summary>
	/// Creates the SystemListener instance for the system. Change to allow usage of custom EventListener
	/// <remarks>custom OnSystemReady override must call SystemListener.OnSystemReady</remarks>
	/// <para>Action to be given to SystemListener constructor</para>
	/// </summary>
	public static System.Func<System.Action, SystemListener> SystemListenerFactoryFunc;
	
	#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
	private Vector2 m_LastMousePosition = new Vector2(-1, -1);
	private MouseEventData m_MouseMoveEvent = new MouseEventData() { MouseModifiers = new EventMouseModifiersState(), Modifiers = new EventModifiersState() };
	#endif
	
	/// <summary>
	/// Indicates whether one of the views in the system is keeping input focus.
	/// </summary>
	private bool m_SystemHasFocusedView = false;
	
	/// <summary>
	/// Determines whether the Coherent UI System component is currently in its Update() method
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is updating; otherwise, <c>false</c>.
	/// </returns>
	public bool IsUpdating { get; private set; }
	
	public delegate void OnUISystemDestroyingDelegate();
	public event OnUISystemDestroyingDelegate UISystemDestroying;
	
	public delegate void SystemReadyEventHandler();
	
	private SystemReadyEventHandler SystemReadyHandlers;
	
	public event SystemReadyEventHandler SystemReady
	{
		add {
			if (!IsReady())
			{
				SystemReadyHandlers += value;
			}
			else
			{
				m_ReadyHandlers.Add(value);
			}
		}
		remove {
			SystemReadyHandlers -= value;
		}	
	}
	
	private List<SystemReadyEventHandler> m_ReadyHandlers = new List<SystemReadyEventHandler>();
	
	private List<CoherentUIView> m_Views = new List<CoherentUIView>();
	
	internal void AddView(CoherentUIView view)
	{
		m_Views.Add(view);
	}
	
	internal bool RemoveView(CoherentUIView view)
	{
		return m_Views.Remove(view);
	}
	
	public List<CoherentUIView> UIViews
	{
		get
		{
			return m_Views;
		}
	}
	
	public static CoherentUISystem Create()
	{
		if (GameObject.Find("CoherentUISystem") != null)
		{
			Debug.LogWarning("Unable to create CoherentUISystem because a GameObject with the same name already exists!");
			return null;
		}

		var go = new GameObject("CoherentUISystem");
		go.AddComponent("CoherentUISystem");
		return go.GetComponent<CoherentUISystem>();
	}
	
	private static string GetDefaultHostDirectory()
	{
#if UNITY_EDITOR
		return (Application.platform == RuntimePlatform.WindowsEditor)? "StreamingAssets/CoherentUI_Host/windows" : "StreamingAssets/CoherentUI_Host/macosx";
#elif UNITY_STANDALONE_WIN
		return "StreamingAssets/CoherentUI_Host/windows";
#elif UNITY_STANDALONE_OSX
		return "Data/StreamingAssets/CoherentUI_Host/macosx";
#elif UNITY_IPHONE
		return "";
#else
#error Unsupported Unity platform
#endif
	}
	
	/// <summary>
	/// the path to CoherentUI_Host executable
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_HostDirectory = GetDefaultHostDirectory();
	[CoherentExposePropertyStandalone]
	public string HostDirectory
	{
		get {
			return m_HostDirectory;
		}
		set {
			m_HostDirectory = value;
		}
	}
	
	/// <summary>
	/// enable proxy support for loading web pages
	/// </summary>	
	[HideInInspector]
	[SerializeField]
	private bool m_EnableProxy = false;
	[CoherentExposePropertyStandalone]
	public bool EnableProxy
	{
		get {
			return m_EnableProxy;
		}
		set {
			m_EnableProxy = value;
		}
	}

	/// <summary>
	/// allow cookies
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_AllowCookies = true;
	[CoherentExposePropertyStandalone]
	public bool AllowCookies
	{
		get {
			return m_AllowCookies;
		}
		set {
			m_AllowCookies = value;
		}
	}
	
	/// <summary>
	/// URL for storing persistent cookies
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_CookiesResource = "cookies.dat";
	[CoherentExposePropertyStandalone]
	public string CookiesResource
	{
		get {
			return m_CookiesResource;
		}
		set {
			m_CookiesResource = value;
		}
	}
	
	/// <summary>
	/// path for browser cache
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_CachePath = "cui_cache";
	[CoherentExposePropertyStandalone]
	public string CachePath
	{
		get {
			return m_CachePath;
		}
		set {
			m_CachePath = value;
		}
	}
	
	/// <summary>
	/// path for HTML5 localStorage
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private string m_HTML5LocalStoragePath = "cui_app_cache";
	[CoherentExposePropertyStandalone]
	public string HTML5LocalStoragePath
	{
		get {
			return m_HTML5LocalStoragePath;
		}
		set {
			m_HTML5LocalStoragePath = value;
		}
	}
	
	/// <summary>
	/// disable fullscreen for plugins like Flash and Silverlight
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_ForceDisablePluginFullscreen = true;
	[CoherentExposePropertyStandalone]
	public bool ForceDisablePluginFullscreen
	{
		get {
			return m_ForceDisablePluginFullscreen;
		}
		set {
			m_ForceDisablePluginFullscreen = value;
		}
	}
	
	/// <summary>
	/// disable web security like cross-domain checks
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private bool m_DisableWebSecutiry = false;
	[CoherentExposePropertyStandalone]
	public bool DisableWebSecutiry
	{
		get {
			return m_DisableWebSecutiry;
		}
		set {
			m_DisableWebSecutiry = value;
		}
	}
	
	/// <summary>
	/// port for debugging Views, -1 to disable
	/// </summary>
	[HideInInspector]
	[SerializeField]
	private int m_DebuggerPort = 9999;
	[CoherentExposePropertyStandalone]
	public int DebuggerPort
	{
		get {
			return m_DebuggerPort;
		}
		set {
			m_DebuggerPort = value;
		}
	}
	
	/// <summary>
	/// The main camera. Used for obtaining mouse position over the HUD and raycasting in the world.
	/// </summary>
	public Camera m_MainCamera = null;
	
	[HideInInspector]
	public bool DeviceSupportsSharedTextures = false;
	
#if UNITY_EDITOR
	void Awake () {
		if (!PlayerPrefs.HasKey("CoherentUI_Installed"))
		{
			Debug.LogError("Please use the Assets/CoherentUI/Install Coherent UI menu entry first!");
			UnityEditor.EditorApplication.isPlaying = false;
		}
	}
#endif
	
	// Use this for initialization
	void Start () {
		
		if(SystemInfo.graphicsDeviceVersion.StartsWith("Direct3D 11") || SystemInfo.operatingSystem.Contains("Mac"))
		{
			DeviceSupportsSharedTextures = true;
		}
		
		if (m_UISystem == null)
		{
			m_SystemListener = (SystemListenerFactoryFunc != null)? SystemListenerFactoryFunc(this.OnSystemReady) : new SystemListener(this.OnSystemReady);
			m_LogHandler = new UnityLogHandler();
			if (FileHandlerFactoryFunc != null)
			{
				m_FileHandler = FileHandlerFactoryFunc();
			}
			if (m_FileHandler == null)
			{
				Debug.LogWarning("Unable to create file handler using factory function! Falling back to default handler.");
				m_FileHandler = new UnityFileHandler();
			}
			
			#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
			SystemSettings settings = new SystemSettings() { 
				HostDirectory = Path.Combine(Application.dataPath, this.HostDirectory),
				EnableProxy = this.EnableProxy,
				AllowCookies = this.AllowCookies,
				CookiesResource = "file:///" + Application.persistentDataPath + '/' + this.CookiesResource,
				CachePath = Path.Combine(Application.temporaryCachePath, this.CachePath),
				HTML5LocalStoragePath = Path.Combine(Application.temporaryCachePath, this.HTML5LocalStoragePath),
				ForceDisablePluginFullscreen = this.ForceDisablePluginFullscreen,
				DisableWebSecurity = this.DisableWebSecutiry,
				DebuggerPort = this.DebuggerPort,
			};
			#elif UNITY_IPHONE
			SystemSettings settings = new SystemSettings() {
				
			};
			#endif
			if (string.IsNullOrEmpty(Coherent.UI.License.COHERENT_KEY))
			{
				throw new System.ApplicationException("You must supply a license key to start Coherent UI! Follow the instructions in the manual for editing the License.cs file.");
			}
			m_UISystem = CoherentUI_Native.InitializeUISystem(Coherent.UI.License.COHERENT_KEY, settings, m_SystemListener, Severity.Warning, m_LogHandler, m_FileHandler);
			if (m_UISystem == null)
			{
				throw new System.ApplicationException("UI System initialization failed!");
			}
			Debug.Log ("Coherent UI system initialized..");
		}
		
		DontDestroyOnLoad(this.gameObject);
	}
	
	private void OnSystemReady()
	{
		if (SystemReadyHandlers != null)
		{
			SystemReadyHandlers();
		}
	}
	
	/// <summary>
	/// Determines whether this instance is ready.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is ready; otherwise, <c>false</c>.
	/// </returns>
	public bool IsReady() {
		return m_UISystem != null && m_SystemListener.IsReady;
	}
	
	/// <summary>
	/// Determines whether there is an focused Click-to-focus view
	/// </summary>
	/// <value>
	/// <c>true</c> if there is an focused Click-to-focus view; otherwise, <c>false</c>.
	/// </value>
	public bool HasFocusedView {
		get { return m_SystemHasFocusedView; }
	}
	
	public delegate void OnViewFocusedDelegate(bool focused);
	
	/// <summary>
	/// Occurs when a Click-to-focus view gains or loses focus
	/// </summary>
	public event OnViewFocusedDelegate OnViewFocused;
	
	private void SetViewFocused(bool focused)
	{
		m_SystemHasFocusedView = focused;
		if (OnViewFocused != null)
		{
			OnViewFocused(focused);
		}
	}
	
	private void TrackInputFocus() {
		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		if (m_MainCamera == null)
		{
			m_MainCamera = Camera.main;
			if (m_MainCamera == null)
			{
				return;
			}
		}
		
		bool isClick = Input.GetMouseButtonDown(0);
		if (!m_SystemHasFocusedView && !isClick)
		{
			// Do nothing if the left mouse button isn't clicked
			// (and there is no focused view; if there is, we need to track the mouse)
			return;
		}

		CoherentUIView cameraView = m_MainCamera.gameObject.GetComponent<CoherentUIView>();
		if (cameraView && cameraView.ClickToFocus)
		{
			var view = cameraView.View;
			if (view != null)
			{
				var normX = Input.mousePosition.x / cameraView.Width;
				var normY = 1 - Input.mousePosition.y / cameraView.Height;
				if (normX >= 0 && normX <= 1 && normY >= 0 && normY <= 1)
				{
					view.IssueMouseOnUIQuery(normX, normY);
					view.FetchMouseOnUIQuery();
					if (view.IsMouseOnView())
					{
						if (isClick)
						{
							// Reset input processing for all views
							foreach (var item in m_Views)
							{
								item.ReceivesInput = false;
							}
							// Set input to the clicked view
							cameraView.ReceivesInput = true;
							SetViewFocused(true);
						}
						cameraView.SetMousePosition((int)Input.mousePosition.x, cameraView.Height - (int)Input.mousePosition.y);
						
						return;
					}
				}
			}
		}

		// Activate input processing for the view below the mouse cursor
		RaycastHit hitInfo;
		if (Physics.Raycast(m_MainCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
		{
			//Debug.Log (hitInfo.collider.name);

			CoherentUIView viewComponent = hitInfo.collider.gameObject.GetComponent(typeof(CoherentUIView)) as CoherentUIView;
			if (viewComponent == null)
			{
				viewComponent = hitInfo.collider.gameObject.GetComponentInChildren(typeof(CoherentUIView)) as CoherentUIView;
			}

			if (viewComponent != null && viewComponent.ClickToFocus)
			{
				if (isClick)
				{
					// Reset input processing for all views
					foreach (var item in m_Views)
					{
						item.ReceivesInput = false;
					}
					// Set input to the clicked view
					viewComponent.ReceivesInput = true;
					SetViewFocused(true);
				}
				viewComponent.SetMousePosition(
					(int)(hitInfo.textureCoord.x * viewComponent.Width),
					(int)(hitInfo.textureCoord.y * viewComponent.Height));
				
				return;
			}
		}
		
		// If neither the HUD nor an object was clicked, clear the focus
		if (m_SystemHasFocusedView && isClick)
		{
			// Reset input processing for all views
			foreach (var item in m_Views)
			{
				item.ReceivesInput = false;
			}
			SetViewFocused(false);
		}
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		if (m_UISystem != null)
		{
			IsUpdating = true;
			
			m_UISystem.Update();
			if (m_ReadyHandlers.Count > 0)
			{
				foreach (var handler in m_ReadyHandlers)
				{
					handler();
				}
				m_ReadyHandlers.Clear();
			}
			
			TrackInputFocus();
			
			IsUpdating = false;
		}
	}
	
	void LateUpdate() {
		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		// Check if the mouse moved
		Vector2 mousePosition = Input.mousePosition;
		if (mousePosition != m_LastMousePosition)
		{
			if (m_MouseMoveEvent != null && m_Views != null)
			{
				InputManager.GenerateMouseMoveEvent(ref m_MouseMoveEvent);
					
				foreach (var item in m_Views)
				{
					var view = item.View;
					if (item.ReceivesInput && view != null)
					{
						if (item.MouseX != -1 && item.MouseY != -1)
						{
							m_MouseMoveEvent.X = item.MouseX;
							m_MouseMoveEvent.Y = item.MouseY;
						}
						else
						{
							m_MouseMoveEvent.Y = item.Height - m_MouseMoveEvent.Y;
						}
						view.MouseEvent(m_MouseMoveEvent);
					}
				}
			}
			
			m_LastMousePosition = mousePosition;
		}
		#endif
	}
	
	void OnApplicationQuit() {
		if (m_UISystem != null)
		{
			if(UISystemDestroying != null) {
				UISystemDestroying();
			}
			
			m_SystemListener.Dispose();
			m_UISystem.Uninitialize();
			m_UISystem.Dispose();
		}
	}
	
	public virtual void OnGUI()
	{
		if (m_Views == null)
		{
			return;
		}
		
		// Process touch events:
		if (Input.touchCount > 0)
		{
			for (int i = 0; i < Input.touchCount; i++)
			{
				Touch touch = Input.GetTouch(i);
				InputManager.ProcessTouchEvent(touch);
			}
		}
		
		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		MouseEventData mouseEventData = null;
		KeyEventData keyEventData = null;
		KeyEventData keyEventDataChar = null;
		
		switch (Event.current.type)
		{
		case EventType.MouseDown:
			{
				mouseEventData = Coherent.UI.InputManager.ProcessMouseEvent(Event.current);
				mouseEventData.Type = MouseEventData.EventType.MouseDown;
			}
			break;
		case EventType.MouseUp:
			{
				mouseEventData = Coherent.UI.InputManager.ProcessMouseEvent(Event.current);
				mouseEventData.Type = MouseEventData.EventType.MouseUp;
			}
			break;
		case EventType.ScrollWheel:
			{
				mouseEventData = Coherent.UI.InputManager.ProcessMouseEvent(Event.current);
				mouseEventData.Type = MouseEventData.EventType.MouseWheel;
			}
			break;
		case EventType.KeyDown:
			if (Event.current.keyCode != KeyCode.None)
			{
				keyEventData = Coherent.UI.InputManager.ProcessKeyEvent(Event.current);
				keyEventData.Type = KeyEventData.EventType.KeyDown;
				
				if (keyEventData.KeyCode == 0)
				{
					keyEventData = null;
				}
			}
			if (Event.current.character != 0)
			{
				keyEventDataChar = Coherent.UI.InputManager.ProcessCharEvent(Event.current);
			}
			break;
		case EventType.KeyUp:
			{
				keyEventData = Coherent.UI.InputManager.ProcessKeyEvent(Event.current);
				keyEventData.Type = KeyEventData.EventType.KeyUp;
			
				if (keyEventData.KeyCode == 0)
				{
					keyEventData = null;
				}
			}
			break;
		}
		
		foreach (var item in m_Views) {
			var view = item.View;
			if (item.ReceivesInput && view != null)
			{
				if (mouseEventData != null)
				{
					if (item.MouseX != -1 && item.MouseY != -1)
					{
						mouseEventData.X = item.MouseX;
						mouseEventData.Y = item.MouseY;
					}
					view.MouseEvent(mouseEventData);
					Event.current.Use();
				}
				if (keyEventData != null)
				{
					view.KeyEvent(keyEventData);
					Event.current.Use();
				}
				if (keyEventDataChar != null)
				{
					view.KeyEvent(keyEventDataChar);
					Event.current.Use();
				}
			}
		}
		#endif
	}
	
	/// <summary>
	/// Gets the user interface system.
	/// </summary>
	/// <value>
	/// The user interface system.
	/// </value>
	public UISystem UISystem
	{
		get
		{
			return m_UISystem;
		}
	}
	
}
