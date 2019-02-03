using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Level manager - handles the scene transitions.
/// </summary>
public class LevelManager : MonoBehaviour
{
	enum Fade {In, Out};
	float fadeTime = 1.2f;
	string sceneToo;

	/// <summary>
	/// Raises the button click event.
	/// </summary>
	/// <param name="sceneName">Scene name.</param>
	public void OnButtonClick(string sceneName)
	{
//		Debug.Log("Are we working?");
		sceneToo = sceneName;
		StartCoroutine(FadeAudio(fadeTime, Fade.Out));
	}

	/// <summary>
	/// Fades the audio.
	/// </summary>
	/// <returns>The audio.</returns>
	/// <param name="timer">Timer.</param>
	/// <param name="fadeType">Fade type.</param>
	IEnumerator FadeAudio (float timer, Fade fadeType)
	{
		float currentVolume = GetComponent<AudioSource>().volume;

		float start = fadeType == Fade.In? 0.0f : currentVolume;
		float end = fadeType == Fade.In? currentVolume : 0.0f;
		float i = 0.0f;
		float step = 1.0f / timer;

		while (i <= 1.0f)
		{
			i += step * Time.deltaTime;
			GetComponent<AudioSource>().volume = Mathf.Lerp(start, end, i);
			yield return new WaitForSeconds(step * Time.deltaTime);
		}

//		Debug.Log("Going to scene");
		SceneManager.LoadScene(sceneToo);
	}

}
