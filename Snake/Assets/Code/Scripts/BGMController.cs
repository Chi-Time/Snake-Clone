using UnityEngine;
using System.Collections;

public class BGMController : MonoBehaviour
{
//	// Static singleton property
//	public static BGMController Instance { get; private set; }
//	
//	void Awake()
//	{
//		// First we check if there are any other instances conflicting
//		if(Instance != null && Instance != this)
//		{
//			// If that is the case, we destroy other instances
//			Destroy(gameObject);
//		}
//		
//		// Here we save our singleton instance
//		Instance = this;
//		
//		// Furthermore we make sure that we don't destroy between scenes (this is optional)
//		DontDestroyOnLoad(gameObject);
//	}

	private static BGMController instance = null;

	public static BGMController Instance 
	{
		get { return instance; }
	}

	void Awake() 
	{
		if (instance != null && instance != this) 
		{
			Destroy(this.gameObject);
			return;
		} 
		else 
		{
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);
	}
}

