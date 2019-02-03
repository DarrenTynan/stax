using UnityEngine;
using System.Collections;

/// <summary>
/// Plane rainbow - helper class attached to MainCamera.
/// </summary>
public class PlaneRainbow : MonoBehaviour
{
	private float duration = 1.0f;
	public SpriteRenderer sRender;


	/// <summary>
	/// Screen refresh
	void Update ()
	{
		float lerp = Mathf.PingPong(Time.time, duration) / duration;
		sRender.material.SetColor("_Color", Color.Lerp(HexTooColor("fe805e"),HexTooColor("fa8856"),  lerp)); //bottom
		sRender.material.SetColor("_Color2", Color.Lerp(HexTooColor("527fc1"),HexTooColor("7ae0ec"),  lerp)); //top
	}

	/// <summary>
	/// Convert hex string representation of color to Color32.
	/// </summary>
	/// <returns>The to color.</returns>
	/// <param name="hex">Hex.</param>
	Color HexTooColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
		return new Color32(r,g,b, 255);
	}

}