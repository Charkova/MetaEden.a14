using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class CoherentUIViewRenderer : MonoBehaviour
{
	CoherentUIViewRenderer()
	{
		ViewId = -1;
		DrawAfterPostEffects = false;
		IsActive = true;
	}

#if UNITY_STANDALONE_WIN
	[DllImport ("CoherentUI_Native", EntryPoint="DummyUnityCall")]
	private static extern void DummyUnityCall32();

	[DllImport("CoherentUI64_Native", EntryPoint = "DummyUnityCall")]
	private static extern void DummyUnityCall64();

	IEnumerator Start () {
		if (System.IntPtr.Size == 4) {
			DummyUnityCall32();
		} else {
			DummyUnityCall64();
		}
		
		if(DrawAfterPostEffects) {
			yield return StartCoroutine("DrawAtEndOfFrames");
		}
		yield break;
	}
	
	public static void WakeRenderer() {
		if (System.IntPtr.Size == 4) {
			DummyUnityCall32();
		} else {
			DummyUnityCall64();
		}
		
		GL.IssuePluginEvent(MakeEvent(CoherentUISystem.CoherentRenderingEventType.WakeRenderer, 0));
	}
#else
	[DllImport("CoherentUI_Native")]
	private static extern void DummyUnityCall();

	IEnumerator Start()
	{
		DummyUnityCall();

		if (DrawAfterPostEffects)
		{
			yield return StartCoroutine("DrawAtEndOfFrames");
		}
		yield break;
	}

	public static void WakeRenderer()
	{
		DummyUnityCall();

		GL.IssuePluginEvent(MakeEvent(CoherentUISystem.CoherentRenderingEventType.WakeRenderer, 0));
	}
#endif

	private static int MakeEvent(CoherentUISystem.CoherentRenderingEventType evType, short viewId)
	{
		int eventId = CoherentUISystem.COHERENT_PREFIX << 24;
		eventId |= ((int)evType) << 16;
		eventId |= viewId;

		return eventId;
	}

	void OnPostRender()
	{
		if (!DrawAfterPostEffects)
		{
			Draw();
		}
	}

	private IEnumerator DrawAtEndOfFrames()
	{
		while (true)
		{
			yield return new WaitForEndOfFrame();
			Draw();
		}
	}

	private void Draw()
	{
		if(!IsActive) return;
		
		int eventId = 0;
		if (!FlipY)
		{
			eventId = MakeEvent(CoherentUISystem.CoherentRenderingEventType.DrawView, ViewId);
		}
		else
		{
			eventId = MakeEvent(CoherentUISystem.CoherentRenderingEventType.DrawFlipYView, ViewId);
		}

		GL.IssuePluginEvent(eventId);
	}

	internal short ViewId
	{
		get;
		set;
	}

	internal bool DrawAfterPostEffects
	{
		get;
		set;
	}
	
	internal bool IsActive
	{
		get;
		set;
	}

	internal bool FlipY;
}
