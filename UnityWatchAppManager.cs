using UnityEngine;
using System.Runtime.InteropServices;

public class UnityWatchAppManager : MonoBehaviour
{
	public static UnityWatchAppManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}


	#if UNITY_IOS && !UNITY_EDITOR

	//[DllImport("__Internal")]
	//private static extern void setUsernameAndPassword(string username, string password, string version, string platformid, string packageChannelid, const char* macAddr);

	//[DllImport("__Internal")]
	//private static extern void gain(string msg);

	//[DllImport("__Internal")]
	//private static extern void sendImages(byte[] textures, int width, int height);
    
	#endif

	// iphone -> watch

	public static void InitInfo(string username, string password, string version, string platformid, string packageChannelid, const char* macAddr)
	{
		#if UNITY_IOS && !UNITY_EDITOR

		//setUsernameAndPassword();

		#endif
	}

	public static void Gain(string key)
	{
		#if UNITY_IOS && !UNITY_EDITOR

		//gain(key);

		#endif
	}

	// watch -> iphone

	private void WatchAppInit(string value)
	{
		#if UNITY_IOS && !UNITY_EDITOR

		// // watchapp 启动的时候从 app 中获取图片资源及相关信息用来初始化
		// Texture2D[] textures = GameObject.Find("Canvas").GetComponent<main>().textures;
		// int width = textures[0].width;
		// int height = textures[0].height;
		// // multi images push together
		// List<byte> lst = new List<byte>();
		// for (int i = 0; i < textures.length; i++)
		// {
		// 	lst.AddRange(textures[i].EncodeToPNG());
		// }

		// sendImages(lst.ToArray(), width, height);

		#endif
	}

	private void WatchAppGain(string value)
	{
		// 客户端 gain 逻辑
		Debug.Log("unity WatchAppGain:" + value);
		if (value == "1")
		{
			GameObject.Find("Canvas").GetComponent<main>().OnClickGain1();
		}
		else if (value == "2")
		{
			GameObject.Find("Canvas").GetComponent<main>().OnClickGain2();
		}

		// disable watch button
		Gain(value);
	}

}