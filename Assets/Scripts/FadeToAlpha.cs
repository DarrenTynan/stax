using UnityEngine;
using System.Collections;

public class FadeToAlpha : MonoBehaviour
{
	public float fadePerSecond = 0.1f;

	public bool fadeOut;


	private void Update()
	{
		Material material = GetComponent<Renderer>().material;
		Color color = material.color;
		if(color.a > 0)
		{
			material.color = new Color(color.r, color.g, color.b, color.a - (fadePerSecond * Time.deltaTime));	
		} else Destroy(gameObject);
	}
}
