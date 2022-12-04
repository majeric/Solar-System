using UnityEngine;

public class PlayerController : GravityObject {

	// Exposed variables
	[Header ("Movement settings")]
	public float walkSpeed = 8;
	public float runSpeed = 14;
	public float jumpForce = 20;
	public float vSmoothTime = 0.1f;
	public float airSmoothTime = 0.5f;
	public float stickToGroundForce = 8;

	public float jetpackForce = 10;
	public float jetpackDuration = 2;
	public float jetpackRefuelTime = 2;
	public float jetpackRefuelDelay = 2;

	[Header ("Mouse settings")]
	public float mouseSensitivityMultiplier = 1;
	public float maxMouseSmoothTime = 0.3f;
	public Vector2 pitchMinMax = new Vector2 (-40, 85);
	public InputSettings inputSettings;

	[Header ("Other")]
	public float mass = 70;
	public LayerMask walkableMask;
	public Transform feet;

	// Private
	private Rigidbody _rb;
	//private Ship spaceship;

	private float _yaw;
	private float _pitch;
	private float _smoothYaw;
	private float _smoothPitch;

	private float _smoothYawOld;

	private float _yawSmoothV;
	private float _pitchSmoothV;

	private Vector3 _targetVelocity;
	private Vector3 _cameraLocalPos;
	private Vector3 _smoothVelocity;
	private Vector3 _smoothVRef;

	private bool _isGrounded;

	// Jetpack
	private bool _usingJetpack;
	private float _jetpackFuelPercent = 1;
	private float _lastJetpackUseTime;

	private CelestialBody _referenceBody;

	private Camera _cam;
	private bool _readyToFlyShip;
	private bool _debugPlayerFrozen;
	private Animator _animator;

	private void Awake () {
		_cam = GetComponentInChildren<Camera> ();
		_cameraLocalPos = _cam.transform.localPosition;
		//spaceship = FindObjectOfType<Ship> ();
		InitRigidbody ();

		_animator = GetComponentInChildren<Animator> ();
		inputSettings.Begin ();
	}

	private void InitRigidbody () {
		_rb = GetComponent<Rigidbody> ();
		_rb.interpolation = RigidbodyInterpolation.Interpolate;
		_rb.useGravity = false;
		_rb.isKinematic = false;
		_rb.mass = mass;
	}

	private void Update () {
		if (Time.timeScale == 0) {
			return;
		}
		
		HandleInput();

		// Refuel jetpack
		if (Time.time - _lastJetpackUseTime > jetpackRefuelDelay) {
			_jetpackFuelPercent = Mathf.Clamp01 (_jetpackFuelPercent + Time.deltaTime / jetpackRefuelTime);
		}

		// Handle animations
		float currentSpeed = _smoothVelocity.magnitude;
		float animationSpeedPercent = (currentSpeed <= walkSpeed) ? currentSpeed / walkSpeed / 2 : currentSpeed / runSpeed;
		_animator.SetBool ("Grounded", _isGrounded);
		_animator.SetFloat ("Speed", animationSpeedPercent);
	}

	private void HandleInput()
	{
		HandleEditorInput();

		// Look input
		_yaw += Input.GetAxisRaw ("Mouse X") * inputSettings.mouseSensitivity / 10 * mouseSensitivityMultiplier;
		_pitch -= Input.GetAxisRaw ("Mouse Y") * inputSettings.mouseSensitivity / 10 * mouseSensitivityMultiplier;
		_pitch = Mathf.Clamp (_pitch, pitchMinMax.x, pitchMinMax.y);
		float mouseSmoothTime = Mathf.Lerp (0.01f, maxMouseSmoothTime, inputSettings.mouseSmoothing);
		_smoothPitch = Mathf.SmoothDampAngle (_smoothPitch, _pitch, ref _pitchSmoothV, mouseSmoothTime);
		_smoothYawOld = _smoothYaw;
		_smoothYaw = Mathf.SmoothDampAngle (_smoothYaw, _yaw, ref _yawSmoothV, mouseSmoothTime);

		// Movement input
		_isGrounded = IsGrounded ();
		Vector3 input = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical"));
		bool running = Input.GetKey (KeyCode.LeftShift);
		_targetVelocity = transform.TransformDirection (input.normalized) * ((running) ? runSpeed : walkSpeed);
		_smoothVelocity = Vector3.SmoothDamp (_smoothVelocity, _targetVelocity, ref _smoothVRef, (_isGrounded) ? vSmoothTime : airSmoothTime);
	}

	private void HandleMovement () {
		if (!_debugPlayerFrozen && Time.timeScale > 0) {
			_cam.transform.localEulerAngles = Vector3.right * _smoothPitch;
			transform.Rotate (Vector3.up * Mathf.DeltaAngle (_smoothYawOld, _smoothYaw), Space.Self);
		}

		//bool inWater = referenceBody
		if (_isGrounded) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				_rb.AddForce (transform.up * jumpForce, ForceMode.VelocityChange);
				_isGrounded = false;
			} else {
				// Apply small downward force to prevent player from bouncing when going down slopes
				_rb.AddForce (-transform.up * stickToGroundForce, ForceMode.VelocityChange);
			}
		} else {
			// Press (and hold) spacebar while above ground to engage jetpack
			if (Input.GetKeyDown (KeyCode.Space)) {
				_usingJetpack = true;
			}
		}

		if (_usingJetpack && Input.GetKey (KeyCode.Space) && _jetpackFuelPercent > 0) {
			_lastJetpackUseTime = Time.time;
			_jetpackFuelPercent -= Time.deltaTime / jetpackDuration;
			_rb.AddForce (transform.up * jetpackForce, ForceMode.Acceleration);
		} else {
			_usingJetpack = false;
		}
	}

	private bool IsGrounded () {
		// Sphere must not overlay terrain at origin otherwise no collision will be detected
		// so rayRadius should not be larger than controller's capsule collider radius
		const float rayRadius = .3f;
		const float groundedRayDst = .2f;
		bool grounded = false;

		if (_referenceBody) {
			var relativeVelocity = _rb.velocity - _referenceBody.velocity;
			// Don't cast ray down if player is jumping up from surface
			if (relativeVelocity.y <= jumpForce * .5f) {
				RaycastHit hit;
				Vector3 offsetToFeet = (feet.position - transform.position);
				Vector3 rayOrigin = _rb.position + offsetToFeet + transform.up * rayRadius;
				Vector3 rayDir = -transform.up;

				grounded = Physics.SphereCast (rayOrigin, rayRadius, rayDir, out hit, groundedRayDst, walkableMask);
			}
		}

		return grounded;
	}

	private void FixedUpdate () {
		if (Time.timeScale == 0) {
			return;
		}
		
		HandleMovement();
		
		CelestialBody[] bodies = NBodySimulation.Bodies;
		Vector3 gravityOfNearestBody = Vector3.zero;
		float nearestSurfaceDst = float.MaxValue;

		// Gravity
		foreach (CelestialBody body in bodies) {
			float sqrDst = (body.Position - _rb.position).sqrMagnitude;
			Vector3 forceDir = (body.Position - _rb.position).normalized;
			Vector3 acceleration = forceDir * Universe.gravitationalConstant * body.mass / sqrDst;
			_rb.AddForce (acceleration, ForceMode.Acceleration);

			float dstToSurface = Mathf.Sqrt (sqrDst) - body.radius;

			// Find body with strongest gravitational pull 
			if (dstToSurface < nearestSurfaceDst) {
				nearestSurfaceDst = dstToSurface;
				gravityOfNearestBody = acceleration;
				_referenceBody = body;
			}
		}

		// Rotate to align with gravity up
		Vector3 gravityUp = -gravityOfNearestBody.normalized;
		transform.rotation = Quaternion.FromToRotation (transform.up, gravityUp) * transform.rotation;

		// Move
		_rb.MovePosition (_rb.position + _smoothVelocity * Time.fixedDeltaTime);
	}

	private void HandleEditorInput () {
		if (Application.isEditor) {
			if (Input.GetKeyDown (KeyCode.O)) {
				Debug.Log ("Debug mode: Toggle freeze player");
				_debugPlayerFrozen = !_debugPlayerFrozen;
			}
		}
	}

	public void SetVelocity (Vector3 velocity) {
		_rb.velocity = velocity;
	}

	public void ExitFromSpaceship () {
		_cam.transform.parent = transform;
		_cam.transform.localPosition = _cameraLocalPos;
		_smoothYaw = 0;
		_yaw = 0;
		_smoothPitch = _cam.transform.localEulerAngles.x;
		_pitch = _smoothPitch;
	}
	public Camera Camera => _cam;

	public Rigidbody Rigidbody => _rb;
}