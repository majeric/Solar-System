using UnityEngine;

[ExecuteInEditMode]
public class PlanetTest : MonoBehaviour {
	public bool useRadius;
	public float radius;

	public bool spawnPlayer = true;
	public bool spawnShip = true;
	public bool showPlanet = true;
	public PlayerController playerPrefab;
	public Ship shipPrefab;

	public Vector3 shipOffset;

	public int lodIndex;
	public GameObject sun;
	public GameObject skySphere;
	public Light testLight;
	private CelestialBodyGenerator[] bodies;
	public float timeOfDay;

	private void Awake () {
		if (Application.isPlaying) {
			skySphere.SetActive (false);
			if (spawnPlayer) {
				Destroy (FindObjectOfType<Camera> ().gameObject);
				Instantiate (playerPrefab, Vector3.up * radius * 1.2f, Quaternion.identity);
			}
			if (spawnShip) {
				var ship = Instantiate (shipPrefab, Vector3.up * radius * 1.2f + Vector3.forward * 20 + shipOffset, Quaternion.identity);
				ship.ToggleHatch ();
			}
			if (sun) {
				sun.gameObject.SetActive (true);
				testLight.gameObject.SetActive (false);
			}
			bodies = FindObjectsOfType<CelestialBodyGenerator> ();
		}
	}

	private void Update () {
		if (Application.isPlaying && showPlanet) {
			foreach (var body in bodies) {
				body.SetLOD (0);
			}
		}

		if (!Application.isPlaying) {
			//sun.transform.position = new Vector3 (Mathf.Cos (timeOfDay), Mathf.Sin (timeOfDay), 0) * 1500;
			//sun.transform.LookAt (Vector3.zero);
		}
	}

	private void OnValidate () {
		if (!Application.isPlaying) {
			//sun.SetActive (false);
		}
		var body = GetComponent<CelestialBody> ();
		body.radius = radius;
		body.RecalculateMass ();

		if (useRadius) {
			FindObjectOfType<CelestialBodyGenerator> ().transform.localScale = Vector3.one * radius;
		} else {
			FindObjectOfType<CelestialBodyGenerator> ().transform.localScale = Vector3.one;
		}
	}
}