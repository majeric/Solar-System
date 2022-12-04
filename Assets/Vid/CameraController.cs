using UnityEngine;

public class CameraController : MonoBehaviour {

	public Transform pivot;

	[Header ("Rotation (alt-drag)")]
	public float rotSpeed = 6;
	public float rotSmoothing = 10;

	[Header ("Zoom - (alt-ctrl-drag)")]
	public float zoomSpeed = 6;
	public float zoomSmoothing = 10;

	// Rotation state
	private Vector2 rotInput;

	// Zoom state
	private float targetZoomDst;
	private float currentZoomDst;

	private void Start () {
		rotInput = (Vector2) transform.eulerAngles;
		targetZoomDst = (transform.position - pivot.position).magnitude;
		currentZoomDst = targetZoomDst;
	}

	private void LateUpdate () {
		if (Input.GetKey (KeyCode.LeftAlt) && Input.GetMouseButton (0)) {
			Vector2 mouseInput = new Vector2 (Input.GetAxisRaw ("Mouse X"), Input.GetAxisRaw ("Mouse Y"));

			if (Input.GetKey (KeyCode.LeftControl)) {
				HandleZoomInput (mouseInput);
			} else {
				HandleRotationInput (mouseInput);
			}
		}

		UpdateRotation ();
		UpdateZoom ();
	}

	private void HandleRotationInput (Vector2 mouseInput) {
		rotInput += new Vector2 (-mouseInput.y, mouseInput.x) * rotSpeed;

	}

	private void UpdateRotation () {
		Quaternion targetRot = Quaternion.Euler (rotInput.x, rotInput.y, 0);
		Quaternion rotation = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * rotSmoothing);
		Vector3 position = rotation * Vector3.forward * -(pivot.position - transform.position).magnitude + pivot.position;

		transform.rotation = rotation;
		transform.position = position;
	}

	private void HandleZoomInput (Vector2 mouseInput) {
		float zoomDir = -Mathf.Sign (mouseInput.x);
		targetZoomDst += mouseInput.magnitude * zoomSpeed * zoomDir;
	}

	private void UpdateZoom () {
		currentZoomDst = Mathf.Lerp (currentZoomDst, targetZoomDst, Time.deltaTime * zoomSmoothing);
		Vector3 dirToPivot = (pivot.transform.position - transform.position).normalized;
		transform.position = pivot.transform.position - dirToPivot * currentZoomDst;
	}
}