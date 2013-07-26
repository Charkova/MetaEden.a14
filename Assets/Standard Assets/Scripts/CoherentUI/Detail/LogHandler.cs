#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif
#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
namespace Coherent.UI
#elif UNITY_IPHONE
namespace Coherent.UI.Mobile
#endif
{
	class UnityLogHandler : ILogHandler
	{
		public override void WriteLog(Severity severity, string message, uint length)
		{
			UnityEngine.Debug.Log(string.Format("[Coherent UI] ({0}) {1}", severity, message));
		}
	
		public override void Assert(string message)
		{
			// Do nothing; The log will be written by WriteLog with severity level AssertFailure
		}
	}
}
