using UnityEngine;

public class Ship : GravityObject {

	public InputSettings inputSettings;
	public Transform hatch;
	public float hatchAngle;
	public Transform camViewPoint;
	public Transform pilotSeatPoint;
	public LayerMask groundedMask;
	public GameObject window;

	[Header ("Handling")]
	public float thrustStrength = 20;
	public float rotSpeed = 5;
	public float rollSpeed = 30;
	public float rotSmoothSpeed = 10;

	[Header ("Interact")]
	public Interactable flightControls;

	private Rigidbody rb;
	private Quaternion targetRot;
	private Quaternion smoothedRot;

	private Vector3 thrusterInput;
	private PlayerController pilot;
	private bool shipIsPiloted;
	private int numCollisionTouches;
	private bool hatchOpen;

	private KeyCode ascendKey = KeyCode.Space;
	private KeyCode descendKey = KeyCode.LeftShift;
	private KeyCode rollCounterKey = KeyCode.Q;
	private KeyCode rollClockwiseKey = KeyCode.E;
	private KeyCode forwardKey = KeyCode.W;
	private KeyCode backwardKey = KeyCode.S;
	private KeyCode leftKey = KeyCode.A;
	private KeyCode rightKey = KeyCode.D;

	private void Awake () {
		InitRigidbody ();
		targetRot = transform.rotation;
		smoothedRot = transform.rotation;
		inputSettings.Begin ();
	}

	private void Update () {
		if (shipIsPiloted) {
			HandleMovement ();
		}

		// Animate hatch
		float hatchTargetAngle = (hatchOpen) ? hatchAngle : 0;
		hatch.localEulerAngles = Vector3.right * Mathf.LerpAngle (hatch.localEulerAngles.x, hatchTargetAngle, Time.deltaTime);

		HandleCheats ();
	}

	private void HandleMovement () {
		// Thruster input
		int thrustInputX = GetInputAxis (leftKey, rightKey);
		int thrustInputY = GetInputAxis (descendKey, ascendKey);
		int thrustInputZ = GetInputAxis (backwardKey, forwardKey);
		thrusterInput = new Vector3 (thrustInputX, thrustInputY, thrustInputZ);

		// Rotation input
		float yawInput = Input.GetAxisRaw ("Mouse X") * rotSpeed * inputSettings.mouseSensitivity / 100f;
		float pitchInput = Input.GetAxisRaw ("Mouse Y") * rotSpeed * inputSettings.mouseSensitivity / 100f;
		float rollInput = GetInputAxis (rollCounterKey, rollClockwiseKey) * rollSpeed * Time.deltaTime;

		// Calculate rotation
		if (numCollisionTouches == 0) {
			var yaw = Quaternion.AngleAxis (yawInput, transform.up);
			var pitch = Quaternion.AngleAxis (-pitchInput, transform.right);
			var roll = Quaternion.AngleAxis (-rollInput, transform.forward);

			targetRot = yaw * pitch * roll * targetRot;

			smoothedRot = Quaternion.Slerp (transform.rotation, targetRot, Time.deltaTime * rotSmoothSpeed);
		} else {
			targetRot = transform.rotation;
			smoothedRot = transform.rotation;
		}
	}

	private void FixedUpdate () {
		// Gravity
		Vector3 gravity = NBodySimulation.CalculateAcceleration (rb.position);
		rb.AddForce (gravity, ForceMode.Acceleration);

		// Thrusters
		Vector3 thrustDir = transform.TransformVector (thrusterInput);
		rb.AddForce (thrustDir * thrustStrength, ForceMode.Acceleration);

		if (numCollisionTouches == 0) {
			rb.MoveRotation (smoothedRot);
		}
	}

	private void TeleportToBody (CelestialBody body) {
		rb.velocity = body.velocity;
		rb.MovePosition (body.transform.position + (transform.position - body.transform.position).normalized * body.radius * 2);
	}

	private int GetInputAxis (KeyCode negativeAxis, KeyCode positiveAxis) {
		int axis = 0;
		if (Input.GetKey (positiveAxis)) {
			axis++;
		}
		if (Input.GetKey (negativeAxis)) {
			axis--;
		}
		return axis;
	}

	private void HandleCheats () {
		if (Universe.cheatsEnabled) {
			if (Input.GetKeyDown (KeyCode.Return) && IsPiloted && Time.timeScale != 0) {
				var shipHud = FindObjectOfType<ShipHUD> ();
				if (shipHud.LockedBody) {
					TeleportToBody (shipHud.LockedBody);
				}
			}
		}
	}

	private void InitRigidbody () {
		rb = GetComponent<Rigidbody> ();
		rb.interpolation = RigidbodyInterpolation.Interpolate;
		rb.useGravity = false;
		rb.isKinematic = false;
		rb.centerOfMass = Vector3.zero;
		rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	}

	public void ToggleHatch () {
		hatchOpen = !hatchOpen;
	}

	public void TogglePiloting () {
		if (shipIsPiloted) {
			StopPilotingShip ();
		} else {
			PilotShip ();
		}
	}

	public void PilotShip () {
		pilot = FindObjectOfType<PlayerController> ();
		shipIsPiloted = true;
		pilot.Camera.transform.parent = camViewPoint;
		pilot.Camera.transform.localPosition = Vector3.zero;
		pilot.Camera.transform.localRotation = Quaternion.identity;
		pilot.gameObject.SetActive (false);
		hatchOpen = false;
		window.SetActive (false);

	}

	private void StopPilotingShip () {
		shipIsPiloted = false;
		pilot.transform.position = pilotSeatPoint.position;
		pilot.transform.rotation = pilotSeatPoint.rotation;
		pilot.Rigidbody.velocity = rb.velocity;
		pilot.gameObject.SetActive (true);
		window.SetActive (true);
		pilot.ExitFromSpaceship ();
	}

	private void OnCollisionEnter (Collision other) {
		if (groundedMask == (groundedMask | (1 << other.gameObject.layer))) {
			numCollisionTouches++;
		}
	}

	private void OnCollisionExit (Collision other) {
		if (groundedMask == (groundedMask | (1 << other.gameObject.layer))) {
			numCollisionTouches--;
		}
	}

	public void SetVelocity (Vector3 velocity) {
		rb.velocity = velocity;
	}

	public bool ShowHUD => shipIsPiloted;

	public bool HatchOpen => hatchOpen;

	public bool IsPiloted => shipIsPiloted;

	public Rigidbody Rigidbody => rb;
}