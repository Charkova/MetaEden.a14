#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif
#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
namespace Coherent.UI
#elif UNITY_IPHONE
namespace Coherent.UI.Mobile
#endif
{
	public class SystemListener : EventListener
	{
		internal SystemListener(System.Action onSystemReady)
		{
			m_OnSystemReady = onSystemReady;
		}
		
		public override void SystemReady()
		{
			IsReady = true;
			m_OnSystemReady();
		}
	
		public override void OnError(SystemError arg0)
		{
			UnityEngine.Debug.Log(string.Format("An error occured! {0} (#{1})", arg0.Error, arg0.ErrorCode));
		}
	
		public bool IsReady
		{
			get;
			private set;
		}
		
		private System.Action m_OnSystemReady;
	}
}
