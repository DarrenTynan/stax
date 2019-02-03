using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Advertisements;

/// <summary>
/// Game - main class.
/// </summary>
public class Game : MonoBehaviour
{
	[SerializeField] private GameObject[] theStack;
	public int scoreCount = 0;
	public int stackIndex;

	private const float BOUNDS = 3.5f;
	private const float STACK_MOVING_SPEED = 2.5f;
	private const float ERROR_MARGIN = 0.1f;
	private const int COMBO_COUNT = 3-1;

	private float tileTransition = 0.0f;
	private float tileSpeed = 1.6f;
	private bool isMovingOnX = true;
	private float secondaryPosition;
	public int combo = 0;
	private bool gameOver = false;

	private Vector3 desiredPosition;
	private Vector3 lastTilePosition;
	private Vector2 stackBounds = new Vector2(BOUNDS, BOUNDS);
	public Text scoreText;
	public Text bestText;
	public GameObject endPanel;
	public Color32[] gameColors = new Color32[4];
	public Material tileMaterial;

	public AudioClip blockFallSFX;
	public AudioClip pigSFX;
	public AudioClip scrapeSFX;
	public AudioClip[] scaleSFX;

	private AudioSource audioSource = new AudioSource();
	public ParticleSystem particle;

	public GameObject stackAnimPrefab;

	private bool isDirectHit = false;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start ()
	{
		bestText.text = PlayerPrefs.GetInt("score").ToString();

		// Get the audio componant.
		audioSource = GetComponent<AudioSource>();

		// Create array of go tiles.
		theStack = new GameObject[transform.childCount];

		// Fill array with positions of go's.
		for(int i = 0; i < transform.childCount; i++)
		{
			theStack[i] = transform.GetChild(i).gameObject;
			ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
		}

		// Start by moving the tile at top of stack.
		stackIndex = 0;
	}

	/// <summary>
	/// Update this instance frame.
	/// </summary>
	void Update ()
	{
		// Check state machine.
		if(gameOver) return;

		/// LMB?
		if(Input.GetMouseButtonDown(0))
		{
			if(PlaceTile())
			{
				SpawnTile();		
				if(isDirectHit) InstantiateRing();
				scoreCount ++;
				scoreText.text = scoreCount.ToString();
			}
			else
			{
				EndGame();
			}
		}

		// Move the top tile back and forth.
		MoveTile();

		// Move the stack down.
		transform.position = Vector3.Lerp(transform.position, desiredPosition, STACK_MOVING_SPEED * Time.deltaTime);

	}

	/// <summary>
	/// Instantiate the 'holy rings of fire!" object.
	/// </summary>
	private void InstantiateRing()
	{
		// Instantiate explosion.
		GameObject go = (GameObject)Instantiate(stackAnimPrefab);
		go.transform.parent = theStack[stackIndex].transform.parent;

		go.transform.position = new Vector3(lastTilePosition.x, 0.5f, lastTilePosition.z);

		float xPerc = (stackBounds.x / 100) * 10;
		float yPerc = (stackBounds.y / 100) * 10;
		go.transform.localScale = new Vector3(stackBounds.x + xPerc, 0.1f, stackBounds.y + yPerc);

	}

	/// <summary>
	/// Creates the rubble.
	/// </summary>
	/// <param name="pos">Position.</param>
	/// <param name="scale">Scale.</param>
	private void CreateRubble(Vector3 pos, Vector3 scale)
	{
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.transform.localPosition = pos;
		go.transform.localScale = scale;
		// Make it fall!
		go.AddComponent<Rigidbody>();

		// Set material to same as tile.
		go.GetComponent<MeshRenderer>().material = tileMaterial;
		// And set the rubble color to the same as cut tile.
		go.GetComponent<MeshFilter>().mesh = theStack[stackIndex].GetComponent<MeshFilter>().mesh;

		// Audio for rubble.
		audioSource.PlayOneShot(blockFallSFX, 0.7F);

	}

	/// <summary>
	/// Moves the tile back and forth using sin.
	/// </summary>
	private void MoveTile()
	{
		// Keep track of the deltatime * tile speed.
		tileTransition += Time.deltaTime * tileSpeed;

		// Are we moving tile on x or z?
		if(isMovingOnX)
		{
			theStack[stackIndex].transform.localPosition = new Vector3(Mathf.Sin(tileTransition) * BOUNDS, scoreCount, secondaryPosition);
		}
		else
		{
			theStack[stackIndex].transform.localPosition = new Vector3(secondaryPosition, scoreCount, Mathf.Sin(tileTransition) * BOUNDS);
		}

	}

	/// <summary>
	/// Places the tile. Togle the isMovingOnX and set the transform direction.
	/// </summary>
	/// <returns><c>true</c>, if tile was placed, <c>false</c> otherwise.</returns>
	private bool PlaceTile()
	{
		isDirectHit = false;
		// Get our transform of the current moving tile.
		Transform t = theStack[stackIndex].transform;

		if(isMovingOnX)
		{
			float deltaX = lastTilePosition.x - t.position.x;

			// We have missed placed.
			if(Mathf.Abs(deltaX) > ERROR_MARGIN)
			{
				// Reset combo count.
				combo = 0;

				// Cut the tile. Get the absolute value, not negative.
				stackBounds.x -= Mathf.Abs(deltaX);

				if(stackBounds.x <= 0) return false;

				float middle = lastTilePosition.x + (t.transform.localPosition.x / 2);
				t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

				// (condition) ? [true path] : [false path]
				CreateRubble
				(
					new Vector3(
						(t.position.x > 0)
						? t.position.x + (t.localScale.x / 2)
						: t.position.x - (t.localScale.x / 2),
						t.position.y,
						t.position.z), 
					new Vector3(Mathf.Abs(deltaX), 1, t.localScale.z)
				);

				t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);

				// We have misplaced but we still play scrapeSFX
				if(scoreCount > 0) audioSource.PlayOneShot(scrapeSFX, 0.7F);
			}

			// Hit
			else
			{
				if(scoreCount > 0) audioSource.PlayOneShot(scrapeSFX, 0.7F);
				// Audio for scale.
				audioSource.PlayOneShot(scaleSFX[combo], 0.2F);

				combo ++;
				if(combo > COMBO_COUNT)
				{
					particle.Play();
					audioSource.PlayOneShot(pigSFX, 0.7F);

					// Reset combo count.
					combo = 0;
					stackBounds.x += 0.25f;
					if(stackBounds.x > BOUNDS) stackBounds.x = BOUNDS;
					float middle = lastTilePosition.x + (t.transform.localPosition.x / 2);
					t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3(middle - (lastTilePosition.x / 2), scoreCount, lastTilePosition.z);
				}

				t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);

				isDirectHit = true;
			}
		}

		// We have missed placed.
		if(!isMovingOnX)
		{
			float deltaZ = lastTilePosition.z - t.position.z;

			// We have misplaced.
			if(Mathf.Abs(deltaZ) > ERROR_MARGIN)
			{
				// Reset combo count.
				combo = 0;

				// Get the absolute value, not negative.
				stackBounds.y -= Mathf.Abs(deltaZ);

				if(stackBounds.y <= 0) return false;

				float middle = lastTilePosition.z + (t.transform.localPosition.z / 2);
				t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

				// (condition) ? [true path] : [false path]
				CreateRubble
				(
					new Vector3(t.position.x, t.position.y, 
						(t.position.z > 0) 
						? t.position.z + (t.localScale.z / 2)
						: t.position.z - (t.localScale.z / 2)),
					new Vector3(t.localScale.x, 1, Mathf.Abs(deltaZ))
				);

				t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));

				// We have misplaced but still play the scrapeSFX.
				if(scoreCount > 0) audioSource.PlayOneShot(scrapeSFX, 0.7F);
			}

			// Hit
			else
			{
				if(scoreCount > 0) audioSource.PlayOneShot(scrapeSFX, 0.7F);
				// Audio for scale.
				audioSource.PlayOneShot(scaleSFX[combo], 0.2F);

				// Tile placed so increase combo.
				combo ++;

				if(combo > COMBO_COUNT)
				{
					particle.Play();
					audioSource.PlayOneShot(pigSFX, 0.7F);

					// Reset combo count.
					combo = 0;
					stackBounds.y += 0.25f;
					if(stackBounds.y > BOUNDS) stackBounds.y = BOUNDS;
					float middle = lastTilePosition.z + (t.transform.localPosition.z / 2);
					t.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);
					t.localPosition = new Vector3(lastTilePosition.x, scoreCount, middle - (lastTilePosition.z / 2));
				}

				t.localPosition = new Vector3(lastTilePosition.x, scoreCount, lastTilePosition.z);

				isDirectHit = true;
			}
		}

		// Set the secondaryPosition to .x OR .z
		secondaryPosition = (isMovingOnX) 
			? t.localPosition.x 
			: t.localPosition.z;

		// Invert bool
		isMovingOnX = !isMovingOnX;

		return true;

	}

	/// <summary>
	/// Spawns the tile. Loop the stackIndex, and set the desiredPosition based on the scoreCount.
	/// </summary>
	private void SpawnTile()
	{
		// Save position of old tile.
		lastTilePosition = theStack[stackIndex].transform.localPosition;

		// Update stackIndex.
		stackIndex --;
		// Loop the index over child count.
		if(stackIndex < 0) stackIndex = transform.childCount - 1;

		desiredPosition = (Vector3.down) * scoreCount;
		theStack[stackIndex].transform.localPosition = new Vector3(0, scoreCount, 0);
		theStack[stackIndex].transform.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

		ColorMesh(theStack[stackIndex].GetComponent<MeshFilter>().mesh);

	}

	/// <summary>
	/// Colors the mesh.
	/// Itterate over colors array and change the mesh vertices.
	/// </summary>
	/// <param name="mesh">Mesh.</param>
	private void ColorMesh(Mesh mesh)
	{
		Vector3[] vertices = mesh.vertices;
		Color32[] colors = new Color32[vertices.Length];
		float f = Mathf.Sin(scoreCount * 0.25f);

		for(int i = 0; i < vertices.Length; i++)
		{
			colors[i] = Lerp4(gameColors[0], gameColors[1], gameColors[2], gameColors[3], f);
		}

		mesh.colors32 = colors;

	}

	/// <summary>
	/// Lerp4 the specified a, b, c, d colour according to trans.
	/// </summary>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="c">C.</param>
	/// <param name="d">D.</param>
	/// <param name="trans">Trans.</param>
	private Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float trans)
	{
		if(trans < 0.33f) return Color.Lerp (a, b, trans / 0.33f);
		else if (trans < 0.66f) return Color.Lerp(b, c, (trans - 0.33f) / 0.33f);
		else return Color.Lerp(c, d, (trans - 0.66f) / 0.66f);

	}

	/// <summary>
	/// Ends the game.
	/// If the current score is greater than saved score then make a new saved score.
	/// Else just display the saved score.
	/// Set gameOver flag to true.
	/// Enable GameOver menu.
	/// </summary>
	private void EndGame()
	{
		if(PlayerPrefs.GetInt("score") < scoreCount)
		{
			PlayerPrefs.SetInt("score", scoreCount);
			bestText.text = scoreCount.ToString();

		}
		else bestText.text = PlayerPrefs.GetInt("score").ToString();

		gameOver = true;

		endPanel.SetActive(true);
		theStack[stackIndex].AddComponent<Rigidbody>();

	}

	/// <summary>
	/// Helper method to change screen's.
	/// </summary>
	/// <param name="sceneName">Scene name.</param>
	public void OnButtonClick(string sceneName)
	{
		GlobalControl.Instance.adCounter += 1;
		if(GlobalControl.Instance.adCounter == GlobalControl.Instance.adMaxCount)
		{
			ShowAd();
			GlobalControl.Instance.adCounter = 0;
		}

		SceneManager.LoadScene(sceneName);

	}

	/// <summary>
	/// Shows the ad.
	/// </summary>
	public void ShowAd()
	{
		if (Advertisement.IsReady())
		{
			Advertisement.Show();
		}

	}

}
