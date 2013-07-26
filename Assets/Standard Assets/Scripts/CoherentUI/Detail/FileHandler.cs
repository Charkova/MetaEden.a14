#if UNITY_STANDALONE || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
#define COHERENT_UNITY_STANDALONE
#endif
using UnityEngine;
using System;
using System.IO;
using System.Runtime.InteropServices;

#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
namespace Coherent.UI
#elif UNITY_IPHONE
namespace Coherent.UI.Mobile
#endif
{
	class UnityFileHandler : FileHandler
	{
		private string GetFilepath(string url)
		{
			var asUri = new Uri(url);
			string cleanUrl;
			if(asUri.Scheme != "file") {
#if UNITY_EDITOR
			cleanUrl = asUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
			// Read resources from the project folder
			var uiResources = PlayerPrefs.GetString("CoherentUIResources");
			if (uiResources == string.Empty)
			{
				Debug.LogError("Missing path for Coherent UI resources. Please select path to your resources via Edit -> Project Settings -> Coherent UI -> Select UI Folder");
			}
			cleanUrl = cleanUrl.Insert(0, uiResources + '/');
#else
			cleanUrl = asUri.GetComponents(UriComponents.Host | UriComponents.Path, UriFormat.Unescaped);
			// Read resources from the <executable>_Data folder
			cleanUrl = Application.dataPath + '/' + cleanUrl;
#endif
			}
			else {
				cleanUrl = asUri.GetComponents(UriComponents.Path, UriFormat.Unescaped);
			}
			return cleanUrl;
		}
		
		public override void ReadFile(string url, ResourceResponse response)
		{
			string cleanUrl = GetFilepath(url);
			
			if (!File.Exists(cleanUrl))
			{
				response.SignalFailure();
				return;
			}

			byte[] bytes = File.ReadAllBytes(cleanUrl);

			IntPtr buffer = response.GetBuffer((uint)bytes.Length);
			if (buffer == IntPtr.Zero)
			{
				response.SignalFailure();
				return;
			}

			Marshal.Copy(bytes, 0, buffer, bytes.Length);

			response.SignalSuccess();
		}
		
		#if UNITY_EDITOR || COHERENT_UNITY_STANDALONE
		public override void WriteFile(string url, ResourceData resource)
		{
			IntPtr buffer = resource.GetBuffer();
			if (buffer == IntPtr.Zero)
			{
				resource.SignalFailure();
				return;
			}

			byte[] bytes = new byte[resource.GetSize()];
			Marshal.Copy(buffer, bytes, 0, bytes.Length);

			string cleanUrl = GetFilepath(url);
			
			try
			{
				File.WriteAllBytes(cleanUrl, bytes);
			}
			catch (IOException ex)
			{
				Console.Error.WriteLine(ex.Message);
				resource.SignalFailure();
				return;
			}

			resource.SignalSuccess();
		}
		#endif
	}
}
