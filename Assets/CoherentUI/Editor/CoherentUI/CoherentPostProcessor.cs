using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

public partial class CoherentPostProcessor {
	private static readonly HashSet<string> ScenesForMobileOnly = new HashSet<string>()
	{
		"iOSInput",
	};

	private static void CheckExportedScenesPlatformCompatibility()
	{
		foreach (var scene in EditorBuildSettings.scenes)
		{
			if (!scene.enabled)
			{
				continue;
			}

			string sceneName = scene.path;

			int lastSlash = sceneName.LastIndexOf('/');
			int lastDot = sceneName.LastIndexOf('.');
			if (lastSlash != -1 && lastDot != -1 && lastDot > lastSlash + 1)
			{
				sceneName = sceneName.Substring(lastSlash + 1, lastDot - lastSlash - 1);
			}

			bool forMobileOnly = ScenesForMobileOnly.Contains(sceneName);

#if UNITY_IPHONE
			if (!forMobileOnly)
#else
			if (forMobileOnly)
#endif
			{
				Debug.LogError("The scene \"" + scene.path + "\" is not compatible for the target platform! It can be exported for "
					+ (forMobileOnly ? "Mobile" : "Desktop") + " only!");
			}
		}
	}

	private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		DirectoryInfo dir = new DirectoryInfo(sourceDirName);
		DirectoryInfo[] dirs = dir.GetDirectories();

		if (!dir.Exists)
		{
			throw new DirectoryNotFoundException(
				"Source directory does not exist or could not be found: "
				+ sourceDirName);
		}

		if (!Directory.Exists(destDirName))
		{
			Directory.CreateDirectory(destDirName);
		}

		FileInfo[] files = dir.GetFiles();
		foreach (FileInfo file in files)
		{
			string temppath = Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, true);
		}

		if (copySubDirs)
		{
			foreach (DirectoryInfo subdir in dirs)
			{
				string temppath = Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}
	}

	private static void DeleteFileIfExists(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	private static void CleanUpForWindows(BuildTarget target, string dataDirectory)
	{
		string sourceDll64Native =  dataDirectory + "StreamingAssets/CoherentUI64_Native.dll";
		string sourceDll64Managed = dataDirectory + "StreamingAssets/CoherentUINet.dll64";

		string unneededManagedDllInPlugins = dataDirectory + "Plugins/CoherentUINet.dll";
		DeleteFileIfExists(unneededManagedDllInPlugins);

		if (target == BuildTarget.StandaloneWindows64)
		{
			string targetDll64Native = dataDirectory + "Plugins/CoherentUI64_Native.dll";
			string targetDllManaged = dataDirectory + "Managed/CoherentUINet.dll";
			string unneededNativeDllInPlugins = dataDirectory + "Plugins/CoherentUI_Native.dll";

			DeleteFileIfExists(unneededNativeDllInPlugins);
			DeleteFileIfExists(targetDll64Native);
			DeleteFileIfExists(targetDllManaged);

			if (!File.Exists(sourceDll64Native) || !File.Exists(sourceDll64Managed))
			{
				Debug.LogError("Unable to copy essential files for Coherent UI x64 when postprocessing build!");
				return;
			}
			File.Move(sourceDll64Native, targetDll64Native);
			File.Move(sourceDll64Managed, targetDllManaged);
		}
		else
		{
			// Delete the unneeded CoherentUI x64 DLLs
			DeleteFileIfExists(sourceDll64Native);
			DeleteFileIfExists(sourceDll64Managed);
		}

		if (Directory.Exists(dataDirectory + "StreamingAssets/CoherentUI_Host/macosx"))
		{
			Directory.Delete(dataDirectory + "StreamingAssets/CoherentUI_Host/macosx", true);
		}
	}

	private static void CleanUpForMacOSX(string bundle)
	{
		var dataDirectory = bundle + "Contents/Data/StreamingAssets/";
		var pluginsDirectory = bundle + "Contents/Plugins/";
		string[] windowsDlls = {
			dataDirectory + "CoherentUI64_Native.dll",
			dataDirectory + "CoherentUINet.dll64",
			pluginsDirectory + "CoherentUI_Native.dll",
			pluginsDirectory + "CoherentUINet.dll",
		};

		foreach (var file in windowsDlls)
		{
			DeleteFileIfExists(file);
		}

		if (Directory.Exists(dataDirectory + "CoherentUI_Host/windows"))
		{
			Directory.Delete(dataDirectory + "CoherentUI_Host/windows", true);
		}
	}

	private static void CleanUpForiOS(string dataFolder)
	{
		string[] dlls = {
			dataFolder + "CoherentUINet.dll64",
			dataFolder + "CoherentUI64_Native.dll",
		};

		foreach (var file in dlls)
		{
			DeleteFileIfExists(file);
		}

		string hostDir = dataFolder + "CoherentUI_Host";
		if(Directory.Exists(hostDir))
		{
			Directory.Delete(hostDir, true);
		}
	}

	[PostProcessBuild]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
		CheckExportedScenesPlatformCompatibility();

		var outDir = Path.GetDirectoryName(pathToBuiltProject);
		var projName = Path.GetFileNameWithoutExtension(pathToBuiltProject);
		var resourcesFolder = PlayerPrefs.GetString("CoherentUIResources");

		// check for per-project override
		if(string.IsNullOrEmpty(resourcesFolder))
		{
			FieldInfo projectUIResourcesStr = typeof(CoherentPostProcessor).GetField("ProjectUIResources", BindingFlags.Public | BindingFlags.Static);
			if(projectUIResourcesStr != null)
			{
				string projectResFolder = (string)projectUIResourcesStr.GetValue(null);
				Debug.Log(String.Format("[Coherent UI]: Found project resources folder: {0}", projectResFolder));
				resourcesFolder = projectResFolder;
			}
		}

		// copy the UI resources
		if(!string.IsNullOrEmpty(resourcesFolder))
		{
			var lastDelim = resourcesFolder.LastIndexOf('/');
			string folderName = lastDelim != -1 ? resourcesFolder.Substring(lastDelim) : resourcesFolder;

			StringBuilder outputDir = new StringBuilder(outDir);
			string uiResourcesFormat = null;
			switch(target)
			{
			case BuildTarget.StandaloneOSXIntel:
				uiResourcesFormat = "/{0}.app/Contents/{1}";
				break;
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
				uiResourcesFormat = "/{0}_Data/{1}";
				break;
			case BuildTarget.iPhone:
				uiResourcesFormat = "/{0}/Data/{1}";
				break;
			default:
				new System.ApplicationException("Unsupported by Coherent UI build target");
				break;
			}

			outputDir.AppendFormat(uiResourcesFormat, projName, folderName);

			StringBuilder inDir = new StringBuilder(Application.dataPath);
			inDir.AppendFormat("/../{0}", resourcesFolder);

			DirectoryCopy(inDir.ToString(), outputDir.ToString(), true);
		}

		switch (target)
		{
		case BuildTarget.StandaloneWindows:
		case BuildTarget.StandaloneWindows64:
			CleanUpForWindows(target, string.Format("{0}/{1}_Data/", outDir, projName));
			break;
		case BuildTarget.StandaloneOSXIntel:
			CleanUpForMacOSX(string.Format("{0}/{1}.app/", outDir, projName));
			break;
		case BuildTarget.iPhone:
			var outputFolder = string.Format("{0}/{1}/Data/Raw/", outDir, projName);
			IOSPostProcessor.PostProcess(pathToBuiltProject);
			CleanUpForiOS(outputFolder);
			break;
		default:
			new System.ApplicationException("Unsupported by Coherent UI build target");
			break;
		}
	}
}