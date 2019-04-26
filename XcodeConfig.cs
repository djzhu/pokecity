using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;

//================================================================
//Automates some build settings for the produced Xcode project
//Automatically Generates the Apple watch app and adds references
//To the specified files
//================================================================
public class XcodeConfig
{
	//Display Name for the Watch app
	public static string watchAppName = "WatchApp";
	//Bundle Id for the Watch app (must not have any spaces)
	public static string watchAppBundleId = UnityEditor.PlayerSettings.applicationIdentifier + "." + watchAppName;
	//Display name for the Watch app extension
	public static string watchAppExtensionName = watchAppName + " Extension";
	//Bundle Id for the Watch app extension (must not have any spaces)
	public static string watchAppExtensionBundleId = watchAppBundleId + "." + watchAppName + "Extension";
    
	//Method gets called automatically when building an Xcode project from Unity
	[PostProcessBuild]
    static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

		ConfigureProject (buildPath);

		//ConfigurePhonePlistFile (buildPath);
    }

	/// <summary>
	/// Adds the WatchConnectivity framework and sets the Enable Bitcode build property for the phone app.
	/// runs the method to also create and configure the Watch App
	/// </summary>
	/// <param name="buildPath">the path that leads to the Xcode project.</param>
	static void ConfigureProject(string buildPath)
	{
		//Create the project file
		PBXProject proj = new PBXProject ();
		string projPath = PBXProject.GetPBXProjectPath(buildPath);

		//Setup the proj file
		proj.ReadFromFile (projPath);

		//Get the target GUID
		string mainTarget = proj.TargetGuidByName (PBXProject.GetUnityTargetName());

		//Create and Configure the WatchApp
		CreateAndConfigureWatchApp (buildPath, mainTarget, proj);

		//Update the proj file
		proj.WriteToFile(projPath);

		//Set the Enable Bitcode build property to false
		proj.SetBuildProperty (mainTarget, "ENABLE_BITCODE", "false");
		//Add the WatchConnectivitiy framework to the phone Target if it hasn't already been added, this allows the phone to communicate with the watch
		if (!proj.ContainsFramework (mainTarget, "WatchConnectivity.framework")) 
		{
			proj.AddFrameworkToProject (mainTarget, "WatchConnectivity.framework", false);
		}
		//Write everything to the file
		File.WriteAllText (projPath, proj.WriteToString ());
	}

	// /// <summary>
	// /// Add fields to the Phone Plist document.
	// /// </summary>
	// /// <param name="buildPath">the path that leads to the Xcode project.</param>
	// static void ConfigurePhonePlistFile(string buildPath)
	// {
	// 	//Create and read the Plist file
	// 	var plist = new PlistDocument();
	// 	var path = Path.Combine(buildPath, "Info.plist");
	// 	plist.ReadFromFile(path);

	// 	//Add two plist entries to handle HealthKit plist Requirements
	// 	string healthShareDescription = "Health share description";
	// 	plist.root.SetString("NSHealthShareUsageDescription", healthShareDescription);
	// 	Debug.Log(string.Format("Set NSHealthShareUsageDescription as \"{0}\"", healthShareDescription));

	// 	string healthUpdateDescription = "Health update description";
	// 	plist.root.SetString("NSHealthUpdateUsageDescription", healthUpdateDescription);
	// 	Debug.Log(string.Format("Set NSHealthUpdateUsageDescription as \"{0}\"", healthUpdateDescription));

	// 	//Add the NSMotionUsageDescription entry to the plist to handle pedometer plist requirements
	// 	//You can tweak the description shown by tweaking the description variable below
	// 	string motionUsageDescription = "Motion usage description";
	// 	plist.root.SetString("NSMotionUsageDescription", motionUsageDescription);
	// 	Debug.Log(string.Format("Set NSMotionUsageDescription as \"{0}\"", motionUsageDescription));

	// 	//Update the plist file
	// 	plist.WriteToFile(path);
	// }

	/// <summary>
	/// Creates and configures the watch app and watch extension app.
	/// </summary>
	/// <param name="buildPath">The path that leads to the Xcode project.</param>
	/// <param name="mainTarget">The GUID for the main target (the Phone).</param>
	/// <param name="proj">Reference to the PBXProject.</param>
	static void CreateAndConfigureWatchApp (string buildPath, string mainTarget, PBXProject proj)
	{
		//Create the Watch extension and store the Watch Extension's GUID
		string watchExtensionTargetGuid = PBXProjectExtensions.AddWatchExtension(proj,mainTarget, watchAppExtensionName,
			watchAppExtensionBundleId,
			watchAppExtensionName + "/Info.plist");
		//Create the Watch App and Store the Watch app GUID
		string watchAppTargetGuid = PBXProjectExtensions.AddWatchApp(proj, mainTarget, watchExtensionTargetGuid,
			watchAppName, watchAppBundleId, watchAppName + "/Info.plist");

		//Copy all files from the WatchApp folder in Assets to the location of the WatchApp in the Created XCode project
		FileUtil.CopyFileOrDirectory("Assets/AppleWatchKit/Plugins/WatchAppFiles/WatchApp", Path.Combine(buildPath, watchAppName));
		//Copy all files from the folder WatchApp Extension in Assets to the location of the WatchApp Extension in the Created XCode project
		FileUtil.CopyFileOrDirectory("Assets/AppleWatchKit/Plugins/WatchAppFiles/WatchApp Extension", Path.Combine(buildPath, watchAppExtensionName));

		//Establish the proper project paths for the files in the folder that were just added to WatchApp.
		//Any new files that you wish to add to the WatchApp must have a reference to their filename
		//Included below and the file itself must be included in Plugins/WatchAppFiles/WatchApp
		List<string> filesToBuild = new List<string>
		{
			watchAppName + "/Interface.storyboard",
			watchAppName + "/Info.plist",
			watchAppName + "/Assets.xcassets",
			watchAppName + "/Arimo-Regular.ttf",
		};

		//For each path in filesToBuild
		for (int i = 0; i < filesToBuild.Count; i++)
		{
			//Add a reference to the file at the path location to proj and store the GUID of the file
			string fileGuid = proj.AddFile (filesToBuild [i], filesToBuild [i]);
			//Add the file to the Watch App Target
			proj.AddFileToBuild (watchAppTargetGuid, fileGuid);
		}

		//Establish the proper project paths for the files in the folder that were just added to WatchApp Extension.
		//Any new files that you wish to add to the WatchApp Extension must have a reference to their filename
		//Included below and the file itself must be included in Plugins/WatchAppFiles/WatchApp Extension
		filesToBuild = new List<string>
		{
			watchAppExtensionName + "/Assets.xcassets",
			watchAppExtensionName + "/Info.plist",
			watchAppExtensionName + "/ExtensionDelegate.h",
			watchAppExtensionName + "/ExtensionDelegate.m",
			watchAppExtensionName + "/InterfaceController.h",
			watchAppExtensionName + "/InterfaceController.m",
			watchAppExtensionName + "/Background1.png",
			watchAppExtensionName + "/Background2.png",
		};

		//For each path in filesToBuild
		for (int i = 0; i < filesToBuild.Count; i++)
		{
			//Add a reference to the file at the path location to proj and store the GUID of the file
			string fileGuid = proj.AddFile(filesToBuild[i], filesToBuild[i]);
			//Add the file to the Watch App Extension Target
			proj.AddFileToBuild(watchExtensionTargetGuid, fileGuid);
		}

		//Add the WatchConnectivity, UIKit, and CoreMotion(used for the pedometer) frameworks to the Watch App Extension Target
		if (!proj.ContainsFramework (watchExtensionTargetGuid, "WatchConnectivity.framework")) 
		{
			proj.AddFrameworkToProject (watchExtensionTargetGuid, "WatchConnectivity.framework", false);
		}

		if (!proj.ContainsFramework (watchExtensionTargetGuid, "UIKit.framework")) 
		{
			proj.AddFrameworkToProject (watchExtensionTargetGuid, "UIKit.framework", false);
		}

		if (!proj.ContainsFramework (watchExtensionTargetGuid, "CoreMotion.framework")) 
		{
			proj.AddFrameworkToProject (watchExtensionTargetGuid, "CoreMotion.framework", false);
		}

		//Configure the plist file for the Watch App
		ConfigureWatchPlist (buildPath);
		//Configure the plist file for the Watch App Extension
		ConfigureWatchExtensionPlist (buildPath);
	}

	/// <summary>
	/// Configures the watch app plist file.
	/// </summary>
	/// <param name="buildPath">Build path for the project.</param>
	static void ConfigureWatchPlist(string buildPath)
	{
		//Setup the Watch App Plist file
		PlistDocument plistWatchApp = new PlistDocument ();
		string plistPath = Path.Combine (buildPath, watchAppName + "/Info.plist");
		plistWatchApp.ReadFromFile (plistPath);

		//Please note: these lines can be commented out if you hard code these values into the provided Plist file
		//in the WatchAppFiles/WatchApp folder of the Unity project although it is recommended not to do so

		//Sets the bundle display name
		plistWatchApp.root.SetString ("CFBundleDisplayName", watchAppName);
		//Sets the WKCompanionAppBundleIdentifier (This must match the applicationIdentifier)
		plistWatchApp.root.SetString ("WKCompanionAppBundleIdentifier", UnityEditor.PlayerSettings.applicationIdentifier);
		//Sets the Bundle Short Version (must match the version for the phone app)
		plistWatchApp.root.SetString ("CFBundleShortVersionString", UnityEditor.PlayerSettings.bundleVersion);
		//Sets the Bundle version (must match the version for the phone app)
		plistWatchApp.root.SetString ("CFBundleVersion", UnityEditor.PlayerSettings.iOS.buildNumber);

		//The below code can be uncommented to see the print out of the plist file for the Watch App
//		IEnumerator<KeyValuePair<string,PlistElement>> plistWatchAppEnumerator = plistWatchApp.root.values.GetEnumerator ();
//		while (plistWatchAppEnumerator.MoveNext ()) 
//		{
//			try
//			{
//				Debug.Log("Key: " + plistWatchAppEnumerator.Current.Key + " Value: " + plistWatchAppEnumerator.Current.Value.AsString());
//			}
//			catch(Exception e) 
//			{
//				Debug.Log ("Value does not work: but key is: " + plistWatchAppEnumerator.Current.Key);
//			}
//		}
		//Update the watch app plist file
		plistWatchApp.WriteToFile (plistPath);
	}

	/// <summary>
	/// Configures the watch app extension plist file.
	/// </summary>
	/// <param name="buildPath">Build path for the project.</param>
	static void ConfigureWatchExtensionPlist(string buildPath)
	{
		//Setup the Watch App Extension Plist file
		PlistDocument plistWatchAppExtension = new PlistDocument ();
		string plistPathExtension = Path.Combine (buildPath, watchAppExtensionName + "/Info.plist");
		plistWatchAppExtension.ReadFromFile (plistPathExtension);

		//Please note: these lines can be commented out if you hard code these values into the provided Plist file
		//in the WatchAppFiles/WatchApp Extension folder of the Unity project although it is recommended not to do so

		//Sets the bundle display name
		plistWatchAppExtension.root.SetString ("CFBundleDisplayName", watchAppExtensionName);
		//Sets the Bundle Short Version (must match the version for the phone app)
		plistWatchAppExtension.root.SetString ("CFBundleShortVersionString", UnityEditor.PlayerSettings.bundleVersion);
		//Sets the Bundle version (must match the version for the phone app)
		plistWatchAppExtension.root.SetString ("CFBundleVersion", UnityEditor.PlayerSettings.iOS.buildNumber);
		//Find the WKAppBundleIdentifier in the NSExtension Dictionary of the plist file and set its value to the WatchAppBundleId
		plistWatchAppExtension.root.values["NSExtension"].AsDict().values["NSExtensionAttributes"].AsDict().SetString("WKAppBundleIdentifier",watchAppBundleId);

		//The below code can be uncommented to see the print out of the plist file for the Watch App Extension
//		IEnumerator<KeyValuePair<string,PlistElement>> plistWatchAppExtensionEnumerator = plistWatchAppExtension.root.values.GetEnumerator ();
//		while (plistWatchAppExtensionEnumerator.MoveNext ()) 
//		{
//			try
//			{
//				Debug.Log("Key for Extension: " + plistWatchAppExtensionEnumerator.Current.Key + " Value for Extension: " + plistWatchAppExtensionEnumerator.Current.Value.AsString());
//			}
//			catch(Exception e) 
//			{
//				Debug.Log ("Value does not work: but key is for Extension: " + plistWatchAppExtensionEnumerator.Current.Key);
//			}
//		}
		//Update the watch app extension plist file
		plistWatchAppExtension.WriteToFile (plistPathExtension);
	}
}