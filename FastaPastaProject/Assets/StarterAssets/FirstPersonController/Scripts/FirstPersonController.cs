using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        public float smoothTime = 2f;
        public float SpeedHardCap;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.1f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.5f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        [Header("Sliding")]
        public bool slideBoost = false;
        private float MaxSlideSpeed;
        public float VelocityOnFlat;
        public float VelocityRegular;
        public float SlideBoostFromJumpMultiply;
        public float SlideBoostFromJumpMultiplyZeroVelocity;

        [Tooltip("The maximum speed the player can reach while sliding")]


        [Header("NOT CHENGABLE")]
        private Vector3 inputdir;
        public Vector3 previousPosition;
        public float _speed;
        public float _verticalVelocity;

        [Header("WallRampJumpForce")]
        public float rightwardJumpSpeed = 100.0f;

        // cinemachine
        private float _cinemachineTargetPitch;

        // player
        private Vector3 _transferredVelocity;
        public float temporarySwingBoost;

        private float _rotationVelocity;

        private float _terminalVelocity = 53.0f;
        private float targetspeed;
        private float treshold = 0.01f;
        private float slopeAngle;
        private Vector3 hitNormal;
        private Vector3 slideDirection;
        private float checkDistance;
        public float currentHorizontalSpeed;
        private float speedOffset;
        private float inputMagnitude;
        private bool isSlide;
        private bool coorutineStarted;
        private SwingMechanic swingMechanic;
        private WallrunMechanic wallrunMechanic;
        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;


        private Vector3 rightwardJumpTarget;
        private Vector3 leftwardJumpTarget;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private bool wallJumpWasPressedLeft;
        private bool wallJumpWasPressedRight;
        public bool _hasDoubleJumped = true;

        private const float _threshold = 0.01f;
        public bool _isAirborneFromSwing = false;
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        private void Awake()
        {
           _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }

        private void Start()
        {
            swingMechanic = gameObject.GetComponent<SwingMechanic>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            wallrunMechanic = gameObject.GetComponent<WallrunMechanic>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
            Debug.LogError("Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {

            JumpAndGravity();
            GroundedCheck();
            Move();
            MaxSlideSpeed = currentHorizontalSpeed * 3;
            MovementChangeVelocityCOntrol();
            WallJumpController();

        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            bool wasGrounded = Grounded;
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            if (!wasGrounded && Grounded)
            {
                _hasDoubleJumped = true;
            }
        }

        private void CameraRotation()
        {
            // if there is an input
            if (_input.look.sqrMagnitude >= _threshold)
            {
                // Don't multiply mouse input by Time.deltaTime
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetPitch += _input.look.y * RotationSpeed * deltaTimeMultiplier;
                _rotationVelocity = _input.look.x * RotationSpeed * deltaTimeMultiplier;

                // clamp our pitch rotation
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

                // rotate the player left and right
                transform.Rotate(Vector3.up * _rotationVelocity);
            }
        }

        private void Move()
        {

            if (_isAirborneFromSwing)
            {
                if (!Grounded)
                {
                    targetspeed = 1f;
                    if (_input.move == Vector2.zero)
                    {
                        _speed = Mathf.Lerp(_speed, targetspeed * inputMagnitude, (Time.deltaTime/10));
                    }

                }
                else
                {
                    temporarySwingBoost = 0;
                    _isAirborneFromSwing = false;
                }
            }
            targetspeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero)
            {
                targetspeed = 0.0f;
            }
            speedOffset = 0.1f;
            inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
            _speed = Mathf.Lerp(_speed, targetspeed, Time.deltaTime);

            if (Grounded)
            {

                currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

                if (currentHorizontalSpeed < targetspeed - speedOffset || currentHorizontalSpeed > targetspeed + speedOffset)
                {
                    _speed = Mathf.Lerp(currentHorizontalSpeed, targetspeed * inputMagnitude, Time.deltaTime * smoothTime);
                    _speed = Mathf.Round(_speed * 1000) / 1000;
                }
            }
            if (Mathf.Abs(_speed) < treshold)
            {
                _speed = 0;
            }
            SlopeController();
            if (_input.move != Vector2.zero)
            {
                if (wallrunMechanic.isWallRunning && wallrunMechanic.disableADKeys)
                {
                    inputdir = transform.forward * _input.move.y;  // Disable sideways movement
                }
                else
                {
                    inputdir = transform.right * _input.move.x + transform.forward * _input.move.y;
                }
            }


            if (_speed >= SpeedHardCap)
            {
                _speed = SpeedHardCap;
                currentHorizontalSpeed = SpeedHardCap;
            }
            if (!wallJumpWasPressedLeft || !wallJumpWasPressedRight)
            {
                _controller.Move(inputdir.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

            //Debug.Log("Current Speed: " + _speed);
        }
        private void JumpAndGravity()
		{
			if (Grounded)
			{
				_fallTimeoutDelta = FallTimeout;
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}
                if (_input.jump && _hasDoubleJumped && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    _hasDoubleJumped = false;
                    _input.jump = false;
                }
                if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
                }
			}

			if (!Grounded && !wallrunMechanic.isWallRunning && _hasDoubleJumped)
			{

                _jumpTimeoutDelta = JumpTimeout;
				if (_fallTimeoutDelta >= 0.0f)
				{
                    _fallTimeoutDelta -= Time.deltaTime;
				}

			}
            if (!Grounded && !wallrunMechanic.isWallRunning)
            {
                if (_input.jump && !_hasDoubleJumped && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    _hasDoubleJumped = true; // Mark that double jump is used
                    _input.jump = false;
                    Debug.Log("doubleJump");
                }
                if (_hasDoubleJumped)
                {
                    _input.jump = false;
                }
            }

            if (!Grounded)
            {
                LeftWallRunJump();
                RightWallRunJump();
                slideBoost = true;
                coorutineStarted = false;
            }
            else if (Grounded && slideBoost && !coorutineStarted)
            {
                coorutineStarted = true;
                StartCoroutine(JumpSlopeBoost());
            }
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
		}

        private void WallJumpController()
        {

            if (wallJumpWasPressedRight && !Grounded)
            {
                // Smoothly move towards the target rightward position at a faster speed
                transform.position = Vector3.MoveTowards(transform.position, leftwardJumpTarget, rightwardJumpSpeed * Time.deltaTime);
                // Check if the target position is reached
                if (Vector3.Distance(transform.position, leftwardJumpTarget) < 0.1f)
                {
                    wallrunMechanic.isRightwardJump = false;
                    wallJumpWasPressedRight = false;
                }

            }
            if (wallJumpWasPressedLeft && !Grounded)
            {
                // Smoothly move towards the target rightward position at a faster speed
                transform.position = Vector3.MoveTowards(transform.position, rightwardJumpTarget, rightwardJumpSpeed * Time.deltaTime);
                // Check if the target position is reached
                if (Vector3.Distance(transform.position, rightwardJumpTarget) < 0.1f)
                {
                    wallrunMechanic.isLeftwardJump = false;
                    wallJumpWasPressedLeft = false;
                }

            }
            if (Grounded || swingMechanic.isSwinging)
            {
                wallJumpWasPressedLeft = false;
                wallJumpWasPressedRight = false;
            }
        }

        public void LeftWallRunJump()
        {
            if (_input.jump && _jumpTimeoutDelta <= 0.0f && wallrunMechanic.isLeftwardJump)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                Vector3 verticalJumpForceLeft = transform.up * _verticalVelocity;

                _controller.Move(verticalJumpForceLeft * Time.deltaTime);

                rightwardJumpTarget = transform.position + transform.right * wallrunMechanic.Offset;
                wallJumpWasPressedLeft = true;
               wallrunMechanic.JumpOffWall();
                Debug.Log("left");
            }
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        public void RightWallRunJump()
        {
            if (_input.jump && _jumpTimeoutDelta <= 0.0f && wallrunMechanic.isRightwardJump)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                Vector3 verticalJumpForceRight = transform.up * _verticalVelocity;

                _controller.Move(verticalJumpForceRight * Time.deltaTime);

                leftwardJumpTarget = transform.position + -transform.right * wallrunMechanic.Offset;
                wallJumpWasPressedRight = true;
                wallrunMechanic.JumpOffWall();
                Debug.Log("right");
            }
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        private void MovementChangeVelocityCOntrol()
        {
            if (Grounded)
            {
                smoothTime = 2f;
            }
            else if (!Grounded && !swingMechanic.isSwinging)
            {
                //_speed += 1;
                smoothTime = 0.1f;
            }
            else if (!Grounded && swingMechanic.isSwinging)
            {
                smoothTime = 0f;
            }
        }

        public void ResetVerticalVelocity()
        {
            _verticalVelocity = 0.0f;
        }

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

        IEnumerator JumpSlopeBoost()
        {
            yield return new WaitForSeconds(0.2f);
            slideBoost = false;
            coorutineStarted = false;
        }

        private bool IsOnSlope(out Vector3 hitNormal, out float slopeAngle)
        {
            RaycastHit hit;
            checkDistance = _controller.height / 2 + GroundedOffset;
            bool hitDetected = Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance, GroundLayers);
            hitNormal = Vector3.up;
            slopeAngle = 0f;

            if (hitDetected)
            {
                hitNormal = hit.normal;
                slopeAngle = Vector3.Angle(Vector3.up, hitNormal);
                return true;
            }

            return false;
        }
        public void SlopeController()
        {

            if (Grounded && _input.slide)
            {
                isSlide = true;


                if (IsOnSlope(out hitNormal, out slopeAngle))
                {
                    _speed = Mathf.Lerp(currentHorizontalSpeed, MaxSlideSpeed, (Time.deltaTime * VelocityRegular));

                    if (slopeAngle <= 2 && !slideBoost)
                    {
                        _speed = Mathf.Lerp(currentHorizontalSpeed, 0, (Time.deltaTime * VelocityOnFlat));

                    }
                    else if (slopeAngle <= 2 && slideBoost)
                    {
                        SlopeBoostedSlide();
                    }
                    if (slopeAngle >= 60)
                    {
                        _speed = Mathf.Lerp(currentHorizontalSpeed, MaxSlideSpeed, (Time.deltaTime * VelocityOnFlat));
                    }

                    if (Mathf.Abs(_speed) < treshold)
                    {
                        _speed = 0;
                    }
                    slideDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                    _controller.Move(slideDirection.normalized * _speed * Time.deltaTime);
                }
            }
            else
            {
                isSlide = false;
                _speed = Mathf.Lerp(_speed, (_input.move == Vector2.zero) ? 0f : targetspeed, Time.deltaTime * smoothTime);
                _controller.Move(inputdir.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            }

          //  Debug.Log("Sliding Speed: " + _speed);
        }

        private void SlopeBoostedSlide()
        {
            if (slideBoost)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed * SlideBoostFromJumpMultiply, 0, (Time.deltaTime * VelocityOnFlat));

                if (currentHorizontalSpeed <= 2)
                {
                    _speed = Mathf.Lerp(SlideBoostFromJumpMultiplyZeroVelocity, 0, Time.deltaTime);
                }
            }
        }

        public void ApplyStoredVelocity(Vector3 velocity)
        {
            // Set the transferred velocity
            _transferredVelocity = velocity;

            // Set the initial speed to the magnitude of the transferred velocity
            _speed += (_transferredVelocity.magnitude / 1.2f);

            // Mark the player as airborne due to swinging
            _isAirborneFromSwing = true;
        }


        private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}