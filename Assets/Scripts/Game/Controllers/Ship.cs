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

	private Rigidbody _rb;
	private Quaternion _targetRot;
	private Quaternion _smoothedRot;

	private Vector3 _thrusterInput;
	private PlayerController _pilot;
	private bool _shipIsPiloted;
	private int _numCollisionTouches;
	private bool _hatchOpen;

	private readonly KeyCode _ascendKey = KeyCode.Space;
	private readonly KeyCode _descendKey = KeyCode.LeftShift;
	private readonly KeyCode _rollCounterKey = KeyCode.Q;
	private readonly KeyCode _rollClockwiseKey = KeyCode.E;
	private readonly KeyCode _forwardKey = KeyCode.W;
	private readonly KeyCode _backwardKey = KeyCode.S;
	private readonly KeyCode _leftKey = KeyCode.A;
	private readonly KeyCode _rightKey = KeyCode.D;

	private void Awake () {
		InitRigidbody ();
		_targetRot = transform.rotation;
		_smoothedRot = transform.rotation;
		inputSettings.Begin ();
	}

	private void Update () {
		if (_shipIsPiloted) {
			HandleMovement ();
		}

		// Animate hatch
		float hatchTargetAngle = (_hatchOpen) ? hatchAngle : 0;
		hatch.localEulerAngles = Vector3.right * Mathf.LerpAngle (hatch.localEulerAngles.x, hatchTargetAngle, Time.deltaTime);

		HandleCheats ();
	}

	private void HandleMovement () {
		// Thruster input
		int thrustInputX = GetInputAxis (_leftKey, _rightKey);
		int thrustInputY = GetInputAxis (_descendKey, _ascendKey);
		int thrustInputZ = GetInputAxis (_backwardKey, _forwardKey);
		_thrusterInput = new Vector3 (thrustInputX, thrustInputY, thrustInputZ);

		// Rotation input
		float yawInput = Input.GetAxisRaw ("Mouse X") * rotSpeed * inputSettings.mouseSensitivity / 100f;
		float pitchInput = Input.GetAxisRaw ("Mouse Y") * rotSpeed * inputSettings.mouseSensitivity / 100f;
		float rollInput = GetInputAxis (_rollCounterKey, _rollClockwiseKey) * rollSpeed * Time.deltaTime;

		// Calculate rotation
		if (_numCollisionTouches == 0) {
			var yaw = Quaternion.AngleAxis (yawInput, transform.up);
			var pitch = Quaternion.AngleAxis (-pitchInput, transform.right);
			var roll = Quaternion.AngleAxis (-rollInput, transform.forward);

			_targetRot = yaw * pitch * roll * _targetRot;

			_smoothedRot = Quaternion.Slerp (transform.rotation, _targetRot, Time.deltaTime * rotSmoothSpeed);
		} else {
			_targetRot = transform.rotation;
			_smoothedRot = transform.rotation;
		}
	}

	private void FixedUpdate () {
		// Gravity
		Vector3 gravity = NBodySimulation.CalculateAcceleration (_rb.position);
		_rb.AddForce (gravity, ForceMode.Acceleration);

		// Thrusters
		Vector3 thrustDir = transform.TransformVector (_thrusterInput);
		_rb.AddForce (thrustDir * thrustStrength, ForceMode.Acceleration);

		if (_numCollisionTouches == 0) {
			_rb.MoveRotation (_smoothedRot);
		}
	}

	private void TeleportToBody (CelestialBody body) {
		_rb.velocity = body.velocity;
		var normalized = (transform.position - body.transform.position).normalized;
		_rb.MovePosition (body.transform.position + normalized * (body.radius * 2));
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
		_rb = GetComponent<Rigidbody> ();
		_rb.interpolation = RigidbodyInterpolation.Interpolate;
		_rb.useGravity = false;
		_rb.isKinematic = false;
		_rb.centerOfMass = Vector3.zero;
		_rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
	}

	public void ToggleHatch () {
		_hatchOpen = !_hatchOpen;
	}

	public void TogglePiloting () {
		if (_shipIsPiloted) {
			StopPilotingShip ();
		} else {
			PilotShip ();
		}
	}

	public void PilotShip () {
		_pilot = FindObjectOfType<PlayerController> ();
		_shipIsPiloted = true;
		_pilot.Camera.transform.parent = camViewPoint;
		_pilot.Camera.transform.localPosition = Vector3.zero;
		_pilot.Camera.transform.localRotation = Quaternion.identity;
		_pilot.gameObject.SetActive (false);
		_hatchOpen = false;
		window.SetActive (false);

	}

	private void StopPilotingShip () {
		_shipIsPiloted = false;
		_pilot.transform.position = pilotSeatPoint.position;
		_pilot.transform.rotation = pilotSeatPoint.rotation;
		_pilot.Rigidbody.velocity = _rb.velocity;
		_pilot.gameObject.SetActive (true);
		window.SetActive (true);
		_pilot.ExitFromSpaceship ();
	}

	private void OnCollisionEnter (Collision other) {
		if (groundedMask == (groundedMask | (1 << other.gameObject.layer))) {
			_numCollisionTouches++;
		}
	}

	private void OnCollisionExit (Collision other) {
		if (groundedMask == (groundedMask | (1 << other.gameObject.layer))) {
			_numCollisionTouches--;
		}
	}

	public void SetVelocity (Vector3 velocity) {
		_rb.velocity = velocity;
	}

	public bool ShowHUD => _shipIsPiloted;

	public bool HatchOpen => _hatchOpen;

	public bool IsPiloted => _shipIsPiloted;

	public Rigidbody Rigidbody => _rb;
}