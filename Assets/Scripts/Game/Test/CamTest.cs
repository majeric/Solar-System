using UnityEngine;

public class CamTest : MonoBehaviour {

	public bool orient;
	public bool setTimestep;
	public float physicsStep = 0.2f;
	public Vector3 initial;

	private void Start () {
		GetComponent<Rigidbody> ().velocity = initial;
		if (setTimestep) {

		}
	}

	private void FixedUpdate () {
		Debug.Log (GetComponent<Rigidbody> ().velocity);
		GetComponent<Rigidbody> ().position += GetComponent<Rigidbody> ().velocity * Time.deltaTime;
	}

}