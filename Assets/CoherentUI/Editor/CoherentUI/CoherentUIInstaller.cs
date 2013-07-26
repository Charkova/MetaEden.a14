using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class CoherentUIInstaller : Editor
{
	public static void MoveAll(DirectoryInfo source, DirectoryInfo target, bool moveAsAssets = true)
	{
		if (source.FullName.ToLower() == target.FullName.ToLower())
		{
			return;
		}

		if (!Directory.Exists(target.FullName))
		{
			if (moveAsAssets)
			{
				var assetsDirInfo = new DirectoryInfo(Application.dataPath);
				if (!target.FullName.StartsWith(assetsDirInfo.FullName))
				{
					Debug.Log("Trying to create a directory asset outside the Assets folder!");
					return;
				}
				string assetName = "Assets" + target.FullName.Substring(assetsDirInfo.FullName.Length).Replace('\\', '/');
				int folderNameStartIndex = assetName.LastIndexOf('/');
				if (folderNameStartIndex == -1)
				{
					Debug.Log("Invalid name supplied when trying to create a directory in the Assets folder!");
					return;
				}
				AssetDatabase.CreateFolder(assetName.Substring(0, folderNameStartIndex), assetName.Substring(folderNameStartIndex + 1));
			}
			else
			{
				Directory.CreateDirectory(target.FullName);
			}
		}

		foreach (FileInfo fi in source.GetFiles())
		{
			string targetFile = Path.Combine(target.FullName, fi.Name);
			if (moveAsAssets)
			{
				if (fi.Extension == ".meta")
				{
					// Ignore meta files
					continue;
				}

				var assetsDirInfo = new DirectoryInfo(Application.dataPath);
				if (!fi.FullName.StartsWith(assetsDirInfo.FullName) || !targetFile.StartsWith(assetsDirInfo.FullName))
				{
					Debug.LogError("Trying to move an asset from or to a path outside the Assets folder!");
					continue;
				}

				string sourceAsset = "Assets" + fi.FullName.Substring(assetsDirInfo.FullName.Length).Replace('\\', '/');
				string targetAsset = "Assets" + targetFile.Substring(assetsDirInfo.FullName.Length).Replace('\\', '/');
				if (File.Exists(targetFile))
				{
					AssetDatabase.DeleteAsset(targetAsset);
				}
				string moveError = AssetDatabase.MoveAsset(sourceAsset, targetAsset);
				if (!string.IsNullOrEmpty(moveError))
				{
					Debug.LogError(string.Format("Unable to move asset '{0}' to '{1}': {2}", sourceAsset, targetAsset, moveError));
				}
			}
			else
			{
				if (File.Exists(targetFile))
				{
					File.Delete(targetFile);
				}
				fi.MoveTo(targetFile);
			}
		}

		foreach (DirectoryInfo sourceSubDir in source.GetDirectories())
		{
			DirectoryInfo nextTargetSubDir = new DirectoryInfo(Path.Combine(target.FullName, sourceSubDir.Name));
			MoveAll(sourceSubDir, nextTargetSubDir, moveAsAssets);
		}
	}

	public static void MoveDirectoryOneUp(DirectoryInfo source)
	{
		MoveAll(source, new DirectoryInfo(Path.Combine(source.Parent.Parent.FullName, source.Name)));
		Directory.Delete(source.FullName, true);
	}

	static void ReorganizeSamples(string workingDirectory)
	{
		string resourcesFolder = "."; // Project root
		string sourceResourcesDir = Path.Combine(workingDirectory, "Samples/UIResources");
		if (Directory.Exists(sourceResourcesDir))
		{
			resourcesFolder = resourcesFolder + "/UIResources";
			DirectoryInfo srcResDir = new DirectoryInfo(sourceResourcesDir);
			MoveAll(srcResDir, new DirectoryInfo(resourcesFolder), false);
			Directory.Delete(srcResDir.FullName, true);
			PlayerPrefs.SetString("CoherentUIResources", resourcesFolder);

			// Rename the .jstxt files back to .js
			string[] foldersWithJstxts = new string[] { resourcesFolder, Path.Combine(workingDirectory, "Samples") };
			DirectoryInfo assetsDirInfo = new DirectoryInfo(Application.dataPath);
			foreach (string folderWithJstxts in foldersWithJstxts)
			{
				if (!Directory.Exists(folderWithJstxts))
				{
					continue;
				}

				string[] jsFiles = Directory.GetFiles(folderWithJstxts, "*.jstxt", SearchOption.AllDirectories);
				foreach (string js in jsFiles)
				{
					string sourceName = new FileInfo(js).FullName;
					string targetName = sourceName.Substring(0, sourceName.Length - 3);
					if (!targetName.StartsWith(assetsDirInfo.FullName))
					{
						// Move as file
						if (File.Exists(targetName))
						{
							File.Delete(targetName);
						}
						File.Move(js, targetName);
					}
					else
					{
						// Move as asset
						string sourceAsset = "Assets" + sourceName.Substring(assetsDirInfo.FullName.Length).Replace('\\', '/');
						string targetAsset = "Assets" + targetName.Substring(assetsDirInfo.FullName.Length).Replace('\\', '/');
						if (File.Exists(targetName))
						{
							AssetDatabase.DeleteAsset(targetAsset);
						}
						string moveError = AssetDatabase.MoveAsset(sourceAsset, targetAsset);
						if (!string.IsNullOrEmpty(moveError))
						{
							Debug.LogError(moveError);
						}
					}
				}
			}

			string[] foldersWithCoherentJscripts = new string[] { Path.Combine(workingDirectory, "Samples/Scenes") };
			foreach (string folderWithJs in foldersWithCoherentJscripts)
			{
				if (!Directory.Exists(folderWithJs))
				{
					continue;
				}

				Regex regx = new Regex(@"#if.*#else(.*)#endif", RegexOptions.Singleline);
				string[] jsFiles = Directory.GetFiles(folderWithJs, "*.js", SearchOption.AllDirectories);
				foreach (string js in jsFiles)
				{
					string contents = File.ReadAllText(js);
					Match m = regx.Match(contents);
					if (m.Success)
					{
						File.WriteAllText(js, m.Groups[1].Value);
					}
				}
			}
		}
	}

	static void InstallCoherentUIImpl()
	{
		string workingDirectory = System.IO.Path.Combine(Application.dataPath, "CoherentUI");

		foreach (string name in ResourcesToBeMovedOneUp.Resources.NameList)
		{
			string resource = System.IO.Path.Combine(workingDirectory, name);

			if (File.Exists(resource))
			{
				FileInfo fi = new FileInfo(resource);
				fi.MoveTo(Path.Combine(fi.Directory.Parent.FullName, fi.Name));
			}
			else if (Directory.Exists(resource))
			{
				MoveDirectoryOneUp(new DirectoryInfo(resource));
			}
		}

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
		ReorganizeSamples(workingDirectory);

		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();

		Debug.Log("Coherent UI installation complete.");
		PlayerPrefs.SetInt("CoherentUI_Installed", 1);
	}

	[MenuItem("Assets/Coherent UI/Install Coherent UI")]
	static void InstallCoherentUI()
	{
		InstallCoherentUIImpl();
	}
}

namespace ResourcesToBeMovedOneUp
{
	static class Resources
	{
		public static readonly string[] NameList = new string[]
		{
			"Standard Assets",
			"StreamingAssets",
			"Plugins",
		};
	}
}