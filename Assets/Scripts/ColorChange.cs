using UnityEngine;
using System.Collections;

/// <summary>
/// Color change - helper class.
/// </summary>
public class ColorChange : MonoBehaviour
{
	public Color[] colors;

	public int currentIndex = 0;
	private int nextIndex;

	public float changeColourTime = 2.0f;

	private float timer = 0.0f;

	void Start()
	{
		if (colors == null || colors.Length < 2)
			Debug.Log ("Need to setup colors array in inspector");

		nextIndex = (currentIndex + 1) % colors.Length;    
	}

	/// <summary>
	/// Frame refresh.
	/// </summary>
	void Update()
	{
		timer += Time.deltaTime;

		if (timer > changeColourTime)
		{
			currentIndex = (currentIndex + 1) % colors.Length;
			nextIndex = (currentIndex + 1) % colors.Length;
			timer = 0.0f;

		}

		Color newColor = GetComponent<Renderer>().material.color;
		newColor.a = Mathf.Lerp(colors[currentIndex].a, colors[nextIndex].a, timer / changeColourTime);
		GetComponent<Renderer>().material.color = newColor;
	}
}