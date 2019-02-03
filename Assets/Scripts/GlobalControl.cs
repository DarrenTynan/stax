using UnityEngine;
using System.Collections;

/// <summary>
/// Global control - home of the global variables.
/// </summary>
public class GlobalControl : MonoBehaviour 
{
	public static GlobalControl Instance;

	public int adCounter = 0;
	public int adMaxCount = 2;

	void Awake ()   
	{
		if (Instance == null)
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
		else if (Instance != this)
		{
			Destroy (gameObject);
		}
	}
}
