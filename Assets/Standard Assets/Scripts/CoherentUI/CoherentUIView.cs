#define COHERENT_UI_PRO_UNITY3D
#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
using Coherent.UI;
using Coherent.UI.Binding;
#elif UNITY_IPHONE
using Coherent.UI.Mobile;
using Coherent.UI.Mobile.Binding;
#endif

/// <summary>
/// Component containing a Coherent UI view.
/// </summary>
public class CoherentUIView : MonoBehaviour {
	[HideInInspector]
	[SerializeField]
	private string m_Page = "http://google.com";

	/// <summary>
	/// Gets or sets the URL of the view
	/// </summary>
	/// <value>
	/// The loaded URL of view
	/// </value>
	[CoherentExposeProperty]
	public string Page
	{
		get {
			return m_Page;
		}
		set {
			if(m_Page == value || value == null)
				return;
			m_Page = value;
			var view = View;
			if(view != null)
			{
				view.Load(m_Page);
			}
		}
	}

	[HideInInspector]
	[SerializeField]
	private int m_Width = 1024;
	/// <summary>
	/// Gets or sets the width of the view.
	/// </summary>
	/// <value>
	/// The width.
	/// </value>
	[CoherentExposeProperty]
	public int Width
	{
		get {
			return m_Width;
		}
		set {
			if(m_Width == value)
				return;
			m_Width = value;
			Resize(m_Width, m_Height);
		}
	}

	[HideInInspector]
	[SerializeField]
	private int m_Height = 512;
	/// <summary>
	/// Gets or sets the height of the view.
	/// </summary>
	/// <value>
	/// The height.
	/// </value>
	[CoherentExposeProperty]
	public int Height
	{
		get {
			return m_Height;
		}
		set {
			if(m_Height == value)
				return;
			m_Height = value;
			Resize(m_Width, m_Height);
		}
	}
	
	[HideInInspector]
	[SerializeField]
	private int m_XPos = 0;
	/// <summary>
	/// Gets or sets the X position of the overlay view.
	/// </summary>
	/// <value>
	/// The X position.
	/// </value>
	[CoherentExposePropertyiOS]
	public int XPos
	{
		get {
			return m_XPos;
		}
		set {
			if(m_XPos == value)
				return;
			m_XPos = value;
			#if UNITY_IPHONE && !UNITY_EDITOR
			Move(m_XPos, m_YPos);
			#endif
		}
	}
	
	[HideInInspector]
	[SerializeField]
	private int m_YPos = 0;
	/// <summary>
	/// Gets or sets the Y position of the overlay view.
	/// </summary>
	/// <value>
	/// The Y position.
	/// </value>
	[CoherentExposePropertyiOS]
	public int YPos
	{
		get {
			return m_YPos;
		}
		set {
			if(m_YPos == value)
				return;
			m_YPos = value;
			#if UNITY_IPHONE && !UNITY_EDITOR
			Move(m_XPos, m_YPos);
			#endif
		}
	}
	
	[HideInInspector]
	[SerializeField]
	private bool m_Show = true;
	/// <summary>
	/// Gets or sets if the overlay view should be visible.
	/// </summary>
	/// <value>
	/// The flag.
	/// </value>
	[CoherentExposePropertyiOS]
	public bool Show
	{
		get {
			return m_Show;
		}
		set {
			if(m_Show == value)
				return;
			m_Show = value;
			#if UNITY_IPHONE && !UNITY_EDITOR
			SetShow(m_Show);
			#endif
		}
	}
	
	public enum CoherentViewInputState {
		TakeAll = 0,
		TakeNone,
		Transparent
	}
	[HideInInspector]
	[SerializeField]
	private CoherentViewInputState m_ViewInputState = CoherentViewInputState.Transparent;
	/// <summary>
	/// Gets or sets the inbput state of the overlay view.
	/// </summary>
	/// <value>
	/// The new input state.
	/// </value>
	[CoherentExposePropertyiOS]
	public CoherentViewInputState InputState
	{
		get {
			return m_ViewInputState;
		}
		set {
			if(m_ViewInputState == value)
				return;
			m_ViewInputState = value;
			#if UNITY_IPHONE && !UNITY_EDITOR
			SetInputState(m_ViewInputState);
			#endif
		}
	}
		
	[HideInInspector]
	[SerializeField]
	private bool m_IsTransparent = false;
	/// <summary>
	/// Gets or sets a value indicating whether the view is transparent.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is transparent; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[CoherentExposeProperty]
	public bool IsTransparent
	{
		get {
			return m_IsTransparent;
		}
		set {
			if(m_IsTransparent == value)
				return;
			#if !UNITY_EDITOR
			if(View != null) {
				throw new System.ApplicationException("The transparency of a View can't be modified if it's already been created");
			}
			#endif
			m_IsTransparent = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private bool m_SupportClickThrough = false;
	/// <summary>
	/// Gets or sets a value indicating whether this view <see cref="CoherentUIView"/> supports click through.
	/// </summary>
	/// <value>
	/// <c>true</c> if supports click through; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[CoherentExposeProperty]
	public bool SupportClickThrough
	{
		get {
			return m_SupportClickThrough;
		}
		set {
			if(m_SupportClickThrough == value)
				return;
			#if !UNITY_EDITOR
			if(View != null) {
				throw new System.ApplicationException("The click-through support of a View can't be modified if it's already been created");
			}
			#endif
			m_SupportClickThrough = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private float m_ClickThroughAlphaThreshold = 0;
	/// <summary>
	/// Gets or sets the alpha threshold for click through checks.
	/// </summary>
	/// <value>
	/// The alpha threshold for click through checks.
	/// </value>
	[CoherentExposePropertyStandalone]
	public float ClickThroughAlphaThreshold
	{
		get {
			return m_ClickThroughAlphaThreshold;
		}
		set {
			if(m_ClickThroughAlphaThreshold == value)
				return;
			m_ClickThroughAlphaThreshold = value;
			var view = View;
			if(view != null)
			{
				#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
				view.SetClickThroughAlphaThreshold(m_ClickThroughAlphaThreshold);
				#endif
			}
		}
	}

	[HideInInspector]
	[SerializeField]
	private bool m_ClickToFocus = false;
	/// <summary>
	/// When enabled, allows a view to take input focus when clicked with the left mouse button.
	/// </summary>
	/// <value>
	/// <c>true</c> if this view takes input focus when clicked; otherwise, <c>false</c>.
	/// </value>
	[CoherentExposePropertyStandalone]
	public bool ClickToFocus
	{
		get {
			return m_ClickToFocus;
		}
		set {
			m_ClickToFocus = value;
		}
	}

	#if COHERENT_UI_PRO_UNITY3D && (UNITY_EDITOR || COHERENT_UNITY_STANDALONE)
	[HideInInspector]
	[SerializeField]
	private bool m_IsOnDemand = false;
	/// <summary>
	/// Gets or sets a value indicating whether this view is an "on demand" view
	/// </summary>
	/// <value>
	/// <c>true</c> if this view is an "on demand" view; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[CoherentExposePropertyStandalone]
	public bool IsOnDemand
	{
		get {
			return m_IsOnDemand;
		}
		set {
			if(m_IsOnDemand == value)
				return;
			#if !UNITY_EDITOR
			if(View != null) {
				throw new System.ApplicationException("The type of a View can't be modified if it's already been created");
			}
			#endif
			m_IsOnDemand = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private int m_TargetFramerate = 60;
	/// <summary>
	/// Gets or sets the target framerate for the view.
	/// </summary>
	/// <value>
	/// The target framerate.
	/// </value>
	[CoherentExposePropertyStandalone]
	public int TargetFramerate
	{
		get {
			return m_TargetFramerate;
		}
		set {
			if(m_TargetFramerate == value)
				return;
			m_TargetFramerate = value;
			var view = View;
			if(view != null)
			{
				view.SetTargetFramerate(m_TargetFramerate);
			}
		}
	}
	#endif

	[HideInInspector]
	[SerializeField]
	private bool m_DrawAfterPostEffects = false;
	/// <summary>
	/// Gets or sets a value indicating whether this view is drawn after post effects.
	/// </summary>
	/// <value>
	/// <c>true</c> if the view is drawn after post effects; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the application exception.
	/// </exception>
	[CoherentExposePropertyStandalone]
	public bool DrawAfterPostEffects
	{
		get {
			return m_DrawAfterPostEffects;
		}
		set {
			if(m_DrawAfterPostEffects == value)
				return;
			#if !UNITY_EDITOR
			if(View != null) {
				throw new System.ApplicationException("The draw order of a View can't be modified if it's already been created");
			}
			#endif
			m_DrawAfterPostEffects = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private bool m_FlipY = false;
	/// <summary>
	/// Gets or sets a value indicating whether the Y axis of this view should be flipped.
	/// </summary>
	/// <value>
	/// <c>true</c> if the Y axis is flipped; otherwise, <c>false</c>.
	/// </value>
	[CoherentExposePropertyStandalone]
	public bool FlipY
	{
		get {
			return m_FlipY;
		}
		set {
			if(m_FlipY == value)
				return;
			m_FlipY = value;

			if(m_Listener != null)
			{
				m_Listener.ViewRenderer.FlipY = ForceInvertY() ? !m_FlipY : m_FlipY;
			}
		}
	}
	
	// Invert flipping on camera-attached views on OpenGL
	internal bool ForceInvertY()
	{
		if (GetComponent<Camera>() != null && SystemInfo.graphicsDeviceVersion.Contains("Open"))
		{
			return true;
		}
		return false;
	}

	[HideInInspector]
	[SerializeField]
	private bool m_ReceivesInput = false;

	/// <summary>
	/// Gets or sets a value indicating whether this view receives input.
	/// All automatic processing and reading of this property is done in the
	/// `LateUpdate()` / `OnGUI()` callbacks in Unity, letting you do all your logic
	/// for View focus in `Update()`.
	/// </summary>
	/// <value>
	/// <c>true</c> if this view receives input; otherwise, <c>false</c>.
	/// </value>
	public bool ReceivesInput
	{
		get {
			return m_ReceivesInput;
		}
		set {
			if (m_System != null && !m_System.IsUpdating && this.ClickToFocus) {
				Debug.LogWarning("You're setting the CoherentUIView.ReceivesInput property on a view that manages input focus on its own. " +
					"To avoid this, before setting the ReceivesInput property, check if the ClickToFocus property is set to false.");
			}
			m_ReceivesInput = value;
		}
	}

	protected UnityViewListener m_Listener;
	protected CoherentUISystem m_System;

	[HideInInspector]
	[SerializeField]
	private bool m_InterceptAllEvents = false;
	/// <summary>
	/// Gets or sets a value indicating whether this view intercepts all events and sends a message for each event.
	/// </summary>
	/// <value>
	/// <c>true</c> if view intercepts all events; otherwise, <c>false</c>.
	/// </value>
	/// <exception cref='System.ApplicationException'>
	/// Is thrown when the property is modified and the view has already been created
	/// </exception>
	[CoherentExposeProperty]
	public bool InterceptAllEvents
	{
		get {
			return m_InterceptAllEvents;
		}
		set {
			if(m_InterceptAllEvents == value)
				return;
			#if !UNITY_EDITOR
			if(View != null) {
				throw new System.ApplicationException("Intercepting all events can't be changed if the view has already been created");
			}
			#endif
			m_InterceptAllEvents = value;
		}
	}

	[HideInInspector]
	[SerializeField]
	private bool m_EnableBindingAttribute = false;
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="CoherentUIView"/> enables usage of the CoherentMethod attribute
	/// in components in the host GameObject.
	/// When true, the all components in the host GameObject are inspected for the CoherentMethod attribute (in the Awake() function)
	/// and the decorated methods are automatically bound when the ReadyForBindings event is received.
	/// When false, the attribute does nothing.
	/// </summary>
	/// <value>
	/// <c>true</c> if usage of the CoherentMethod is enabled; otherwise, <c>false</c>.
	/// </value>
	[CoherentExposeProperty]
	public bool EnableBindingAttribute
	{
		get {
			return m_EnableBindingAttribute;
		}
		set {
			m_EnableBindingAttribute = value;
		}
	}

	private Camera m_Camera;

	#if UNITY_EDITOR
	private bool m_QueueCreateView = false;
	#endif

	internal int MouseX { get; set; }
	internal int MouseY { get; set; }
	
	private List<CoherentMethodBindingInfo> m_CoherentMethods;
	
	/// <summary>
	/// Sets the mouse position. Note Coherent UI (0,0) is the upper left corner of the screen.
	/// </summary>
	/// <param name='x'>
	/// X coordinate of the mouse.
	/// </param>
	/// <param name='y'>
	/// Y coordinate of the mouse.
	/// </param>
	public void SetMousePosition(int x, int y)
	{
		MouseX = x;
		MouseY = y;
	}

	void Awake () {
		MouseX = -1;
		MouseY = -1;

		m_System = CoherentUISystem.Instance;
		
		m_CoherentMethods = new List<CoherentMethodBindingInfo>();
		if (EnableBindingAttribute)
		{
			RegisterMethodsForBinding();
		}
		m_Listener = new UnityViewListener(this, this.m_Width, this.m_Height);
		m_Listener.ReadyForBindings += this.ReadyForBindings;

		m_Camera = GetComponent<Camera>();

		m_System.UISystemDestroying += OnDestroy;
		m_System.SystemReady += OnSystemReady;
		m_System.AddView(this);
	}

	private void RegisterMethodsForBinding()
	{
		List<CoherentMethodBindingInfo> methods = CoherentMethodHelper.GetCoherentMethodsInGameObject(this.gameObject);
		m_CoherentMethods.AddRange(methods);
	}

	private void ReadyForBindings(int frame, string url, bool isMain)
	{
		if (isMain)
		{
			foreach (var item in m_CoherentMethods)
			{
				if (item.IsEvent)
				{
					View.RegisterForEvent(item.ScriptEventName, item.BoundFunction);
				}
				else
				{
					View.BindCall(item.ScriptEventName, item.BoundFunction);
				}
			}

			if (m_InterceptAllEvents)
			{
				View.RegisterForEvent("all", (System.Action<string, Value[]>)this.InterceptEvent);
			}
		}
	}

	private void InterceptEvent(string name, Value[] arguments)
	{
		SendMessage(name, arguments, SendMessageOptions.DontRequireReceiver);
	}

	private void SendCreateView()
	{
		if(Page == null || Page == "") {
			throw new System.ApplicationException("The Page of a view must not be null or empty.");
		}
				
		var viewInfo = new ViewInfo();
		viewInfo.Width = this.m_Width;
		viewInfo.Height  = this.m_Height;
		viewInfo.IsTransparent = this.m_IsTransparent;
		#if COHERENT_UI_PRO_UNITY3D && (UNITY_EDITOR || COHERENT_UNITY_STANDALONE)
		viewInfo.IsOnDemand = this.m_IsOnDemand;
		viewInfo.TargetFrameRate = (int)this.m_TargetFramerate;
		#endif
		viewInfo.SupportClickThrough = this.m_SupportClickThrough;

		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		viewInfo.ClickThroughAlphaThreshold = this.m_ClickThroughAlphaThreshold;
		viewInfo.UsesSharedMemory = !m_System.DeviceSupportsSharedTextures;
		#elif UNITY_IPHONE
		viewInfo.X = this.m_XPos;
		viewInfo.Y = this.m_YPos;
		viewInfo.ShowImmediately = this.m_Show;
		viewInfo.InitialInputState = (ViewInputState)this.m_ViewInputState;
		#endif
		
		m_System.UISystem.CreateView(viewInfo, Page, m_Listener);
		
		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		CoherentUIViewRenderer.WakeRenderer();
		#endif
	}

	private void OnSystemReady()
	{
		if (this.enabled)
		{
			SendCreateView();
		}
		else
		{
			#if UNITY_EDITOR
			m_QueueCreateView = true;
			#endif
		}
	}

	void Update () {
		#if UNITY_EDITOR
		if (m_QueueCreateView && this.enabled)
		{
			SendCreateView();
			m_QueueCreateView = false;
		}
		#endif

		var view = View;
		if(view != null)
		{
			#if COHERENT_UI_PRO_UNITY3D && (UNITY_EDITOR || COHERENT_UNITY_STANDALONE)
			if(IsOnDemand)
			{
				view.RequestFrame();
			}
			#endif
			if (m_Camera != null)
			{
				var width = (int)m_Camera.pixelWidth;
				var height = (int)m_Camera.pixelHeight;
				if (width != m_Width || height != m_Height)
				{
					Resize(width, height);
				}
			}
		}
	}
	
	void OnDisable()
	{
		if(m_Listener != null && m_Listener.ViewRenderer != null)
		{
			m_Listener.ViewRenderer.IsActive = false;
		}
	}
	
	void OnEnable()
	{
		if(m_Listener != null && m_Listener.ViewRenderer != null)
		{
			m_Listener.ViewRenderer.IsActive = true;
		}
	}
	
	void OnDestroy()
	{
		if(m_Listener != null)
		{
			m_Listener.Destroy();
			m_Listener.Dispose();
			m_Listener = null;
		}

		m_System.RemoveView(this);
		
		if (OnViewDestroyed != null)
		{
			OnViewDestroyed();
		}
	}

	/// <summary>
	/// Destroy this view. Destroys the Coherent UI view and removes the CoherentUIView
	/// component from its game object. Any usage of the view after this method is
	/// undefined behaviour.
	/// </summary>
	public void DestroyView()
	{
		OnDestroy();
		UnityEngine.Object.Destroy(this);
	}

	/// <summary>
	/// Handler for ViewDestroyed event.
	/// </summary>
	public delegate void ViewDestroyedHandler();

	/// <summary>
	/// Occurs when the view has been destroyed and the CoherentUIView component
	/// is going to be removed from the game object.
	/// </summary>
	public event ViewDestroyedHandler OnViewDestroyed;


	/// <summary>
	/// Resize the view to the specified width and height.
	/// </summary>
	/// <param name='width'>
	/// New width for the view.
	/// </param>
	/// <param name='height'>
	/// New height for the view.
	/// </param>
	public void Resize(int width, int height)
	{
		m_Width = width;
		m_Height = height;
		var view = View;
		if(view != null)
		{
			view.Resize((uint)m_Width, (uint)m_Height);
			if(m_Listener.RTTexture != null)
			{
				m_Listener.RTTexture.width = m_Width;
				m_Listener.RTTexture.height = m_Height;
			}
		}
	}
	
	#if UNITY_IPHONE && !UNITY_EDITOR
	/// <summary>
	/// Move the overlay view to the specified x and y position.
	/// </summary>
	/// <param name='x'>
	/// New X position of the view.
	/// </param>
	/// <param name='y'>
	/// New Y position of the view.
	/// </param>
	public void Move(int x, int y)
	{
		m_XPos = x;
		m_YPos = y;
		var view = View;
		if(view != null)
		{
			view.Move((uint)m_XPos, (uint)m_YPos);
		}
	}
	
	/// <summary>
	/// Show/hide the overlay view.
	/// </summary>
	/// <param name='show'>
	/// Whether to show or to hide the view
	/// </param>
	public void SetShow(bool show)
	{
		m_Show = show;
		var view = View;
		if(view != null) 
		{
			view.SetShow(m_Show);
		}
	}
	
	/// <summary>
	/// Sets the input state of the overlay view.
	/// </summary>
	/// <param name='state'>
	/// The new state of the input
	/// </param>
	public void SetInputState(CoherentViewInputState state)
	{
		m_ViewInputState = state;
		var view = View;
		if(view != null) 
		{
			view.SetInputState((ViewInputState)m_ViewInputState);
		}
	}
		
	#endif
	
	/// <summary>
	/// Gets the underlying Coherent.UI.View instance.
	/// </summary>
	/// <value>
	/// The underlying Coherent.UI.View instance.
	/// </value>
	public View View
	{
		get {
			if(m_Listener != null) {
				return m_Listener.View;
			}
			else {
				return null;
			}
		}
	}
	
	#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE		
	/// <summary>
	/// Request redraw of this view.
	/// </summary>
	public void Redraw()
	{
		var view = View;
		if(view != null)
		{
			view.Redraw();
		}
	}
	#endif

	/// <summary>
	/// Request reload of this view.
	/// </summary>
	/// <param name='ignoreCache'>
	/// Ignore cache for the reload.
	/// </param>
	public void Reload(bool ignoreCache)
	{
		var view = View;
		if(view != null)
		{
			view.Reload(ignoreCache);
		}
	}

	/// <summary>
	/// Occurs when the underlying Coherent.UI.View is created
	/// </summary>
	public event UnityViewListener.CoherentUI_OnViewCreated OnViewCreated {

		add {
			m_Listener.ViewCreated += value;
		}

		remove {
			m_Listener.ViewCreated -= value;
		}
	}

	/// <summary>
	/// Occurs when the view bindings should be registered.
	/// </summary>
	public event UnityViewListener.CoherentUI_OnReadyForBindings OnReadyForBindings {

		add {
			m_Listener.ReadyForBindings += value;
		}

		remove {
			m_Listener.ReadyForBindings -= value;
		}
	}

	/// <summary>
	/// Gets the underlying UnityViewListener for this view.
	/// </summary>
	/// <value>
	/// The listener.
	/// </value>
	public UnityViewListener Listener {
		get { return m_Listener; }
	}
}





