using UnityEngine;
using System.Collections; // Required for Coroutines
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // Keep this if StarterAssetsInputs uses it
#endif

namespace StarterAssets // Keep the same namespace if your Inputs script uses it
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(StarterAssetsInputs))] // We rely on this for input values
    public class CorridorRunnerMovement : MonoBehaviour
    {
         #region PowerUp Variables

        [Header("PowerUp Settings")]
        // Potion
        public float potionSpeedMultiplier = 1.5f;
        public float potionJumpMultiplier = 1.3f;
        public float potionDuration = 8.0f;
        // Apple
        public float intangibilityDuration = 5.0f; // Duration from assignment
        // Coin
        public float cameraAnglePowerUpValue = 0.0f; 
        public float cameraAnglePowerUpDuration = 10.0f;


        // Internal PowerUp State
        public float invulnerabilityDuration = 5.0f; 
        private bool isPotionActive = false;
        private float originalSpeed;
        private float originalJumpHeight;
        private bool isCollisionProof = false; // Tracks apple effect

     
        private bool isCameraPowerUpActive = false;
        private float originalCameraAngleOverride;

        #endregion

        #region Public Variables (Inspector)

        [Header("Runner Movement")]
        public float ForwardSpeed = 5.0f;
        public float SidewaysSpeed = 4.0f;
        public float SprintSpeedMultiplier = 1.5f;
        public float SidewaysSpeedChangeRate = 10.0f;
        // [Tooltip("Multiplier for forward speed when moving backwards input is held (S key)")] // Optional Braking
        // public float BrakingSpeedMultiplier = 0.5f;


        [Header("Jump and Gravity")]
        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;


        [Header("Cinemachine")]
        public GameObject CinemachineCameraTarget;
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        [Header("Audio")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        #endregion

        #region Private Variables

        // Component References
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private Animator _animator;
        private GameManager _gameManager;

        // Cinemachine / Camera Control
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private const float _threshold = 0.01f;

        // Player Movement State
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _currentSidewaysSpeed;

        // Timeouts
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // Animation IDs
        private bool _hasAnimator;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDSpeed;

        #endregion

        #region Unity Methods (Awake, Start, Update, LateUpdate)

        void Awake()
        {
              _gameManager = FindFirstObjectByType<GameManager>(); // <<< THE CHANGED LINE

    // Keep the null check, it's still useful
    if (_gameManager == null) {
        Debug.LogWarning("CorridorRunnerMovement: GameManager not found!", this);
    }
        }

        void Start()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _hasAnimator = TryGetComponent(out _animator);

            if (_controller == null || _input == null) {
                Debug.LogError("Missing CharacterController or StarterAssetsInputs on player!", this);
                enabled = false; return;
            }
            if (CinemachineCameraTarget == null) {
                 Debug.LogError("Cinemachine Camera Target not assigned!", this);
                 enabled = false; return;
            }

            AssignAnimationIDs();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        }

        void Update()
        {
             if (_gameManager != null && _gameManager.currentState != GameManager.GameState.Playing)
             {
                 HandleAnimationState(0f, 0f);
                 return;
             }
             if (!_controller.enabled) return;

            GroundedCheck();
            JumpAndGravity();
            MovePlayer();
        }

        void LateUpdate()
        {
             if (_gameManager != null && _gameManager.currentState == GameManager.GameState.Playing)
             {
               CameraRotation();
             }
        }

        #endregion

        #region Core Logic Methods (Move, Jump/Gravity, GroundedCheck, Camera)

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

               private void GroundedCheck()
        {
            // Calculate the sphere's position
            Vector3 spherePosition = new Vector3(
                transform.position.x,
                transform.position.y + GroundedOffset, // Use '+' assumes GroundedOffset is negative
                transform.position.z
            );

            // Perform the check and store the result in a temporary variable
            bool isGroundedNow = Physics.CheckSphere(
                spherePosition,
                GroundedRadius,
                GroundLayers,
                QueryTriggerInteraction.Ignore
            );

            // --- DEBUG LOG: Log only when the Grounded state CHANGES ---
            if (Grounded != isGroundedNow) // Compare current state to the new result
            {
                Debug.Log($"[GroundedCheck] State Changed. Grounded = {isGroundedNow} at Y={transform.position.y:F3}");
            }
            // --- END DEBUG LOG ---

            // Update the main Grounded variable for the rest of the script to use
            Grounded = isGroundedNow;

            // Update the animator parameter
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

private void JumpAndGravity()
{
     if (Grounded)
    {
        _fallTimeoutDelta = FallTimeout;
        if (_hasAnimator) { _animator.SetBool(_animIDJump, false); _animator.SetBool(_animIDFreeFall, false); }

        // --- MODIFY AND ADD LOG ---
        if (_verticalVelocity < 0.0f)
        {
            float previousVertVel = _verticalVelocity; // Store previous value for logging
            _verticalVelocity = -2f; // Prevent accumulating negative velocity
            // Log only if it actually changed significantly (avoid spam)
            if (Mathf.Abs(previousVertVel - _verticalVelocity) > 0.1f)
                Debug.Log($"[JumpAndGravity - Grounded] Reset VertVel from {previousVertVel:F3} to: {_verticalVelocity:F3}");
        }
        // --- END MODIFY ---

        if (_input.jump && _jumpTimeoutDelta <= 0.0f) {
            _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
             Debug.Log($"[JumpAndGravity - Grounded] JUMPING! Set VertVel to: {_verticalVelocity:F3}"); // Log jump impulse
            if (_hasAnimator) { _animator.SetBool(_animIDJump, true); }
        }
        if (_jumpTimeoutDelta >= 0.0f) { _jumpTimeoutDelta -= Time.deltaTime; }
    }
    else // Not Grounded (Falling)
    {
        _jumpTimeoutDelta = JumpTimeout;
        if (_fallTimeoutDelta >= 0.0f) {
            _fallTimeoutDelta -= Time.deltaTime;
        } else {
            if (_hasAnimator) { _animator.SetBool(_animIDFreeFall, true); }
        }
        _input.jump = false;

        // --- ADD LOG FOR GRAVITY ---
        float previousVertVelGravity = _verticalVelocity; // Store for logging comparison
        // Apply gravity
        if (_verticalVelocity < _terminalVelocity) {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
        // Log periodically while falling
        if(Time.frameCount % 15 == 0) // Log roughly 4 times per second while falling
            Debug.Log($"[JumpAndGravity - Falling] Applying Gravity. VertVel: {_verticalVelocity:F3} (Delta: {_verticalVelocity-previousVertVelGravity:F4})");
        // --- END ADD ---
    }
}
               private void MovePlayer()
        {
            // --- Calculate Forward Speed ---
            float currentForwardSpeed = ForwardSpeed;
            if (_input.sprint) { currentForwardSpeed *= SprintSpeedMultiplier; }

            // Optional Braking Logic (keep commented out unless needed)
            // if (_input.move.y < -_threshold)
            // {
            //     currentForwardSpeed *= BrakingSpeedMultiplier;
            // }


            // --- Calculate Sideways Speed ---
            float targetSidewaysSpeed = _input.move.x * SidewaysSpeed;
            _currentSidewaysSpeed = Mathf.Lerp(_currentSidewaysSpeed, targetSidewaysSpeed, Time.deltaTime * SidewaysSpeedChangeRate);


            // --- Combine Movement Vectors ---
            Vector3 forwardMovement = Vector3.forward * currentForwardSpeed;
            Vector3 sidewaysMovement = Vector3.right * _currentSidewaysSpeed;
            Vector3 totalHorizontalMovement = forwardMovement + sidewaysMovement;


            // --- CALCULATE FINAL DELTA VECTOR & ADD LOG (Lines Added Here) ---
            // Calculate the full movement vector including vertical velocity for this frame
            Vector3 finalMoveDelta = (totalHorizontalMovement + new Vector3(0.0f, _verticalVelocity, 0.0f)) * Time.deltaTime;

            // Log the vertical component being applied just before moving
            // Log periodically (e.g., every 10 frames) to avoid spamming the console too much
            if(Time.frameCount % 10 == 0)
            {
                // Also log the CharacterController's internal isGrounded for comparison
                bool ccIsGrounded = _controller.isGrounded;
                Debug.Log($"[MovePlayer] Applying Y Move: {finalMoveDelta.y:F4} (From VertVel: {_verticalVelocity:F3}) | Current Pos Y: {transform.position.y:F3} | Script Grounded: {Grounded} | CC.isGrounded: {ccIsGrounded}");
            }
            // --- END ADDED LINES ---


            // --- Apply Movement ---
            // Use the calculated finalMoveDelta vector
            _controller.Move(finalMoveDelta);


            // --- Force Player Rotation ---
            transform.rotation = Quaternion.LookRotation(Vector3.forward);


            // --- Update Animator ---
            HandleAnimationState(currentForwardSpeed, Mathf.Abs(_input.move.x));
        }
         private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;
                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
        }

        #endregion

        #region Helper Methods

        private void HandleAnimationState(float finalForwardSpeed, float sidewaysMagnitude)
        {
            if (_hasAnimator)
            {
                 float speedRatio = ForwardSpeed > 0 ? finalForwardSpeed / ForwardSpeed : 0;
                _animator.SetFloat(_animIDSpeed, speedRatio);
                float effectiveMotionSpeed = Mathf.Max(speedRatio, sidewaysMagnitude);
                _animator.SetFloat(_animIDMotionSpeed, effectiveMotionSpeed);
                // Debug.Log($"Animator Params - Grounded: {Grounded}, SpeedRatio: {speedRatio:F2}, MotionSpeed (Effective): {effectiveMotionSpeed:F2}, SidewaysInputMag: {sidewaysMagnitude:F2}");
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                PlayerInput playerInput = GetComponent<PlayerInput>();
                 return playerInput != null && playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        #endregion

        #region Animation Event Handlers

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudioClip != null) {
                     if (_controller != null) {
                        AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
                     }
                }
            }
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips != null && FootstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, FootstepAudioClips.Length);
                    if (_controller != null) {
                       AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
                    }
                }
            }
        }

        #endregion


         #region PowerUp Activation Methods

        // --- POTION ---
        public void ActivateSpeedJumpBoost()
        {
            if (!isPotionActive) // Prevent stacking multiple potions right now
            {
                StartCoroutine(SpeedJumpBoostCoroutine());
            }
            // Optional: Could reset timer if already active
        }

        private IEnumerator SpeedJumpBoostCoroutine()
        {
            isPotionActive = true;
            // Store original values
            originalSpeed = ForwardSpeed;
            originalJumpHeight = JumpHeight;

            // Apply boost
            ForwardSpeed *= potionSpeedMultiplier;
            JumpHeight *= potionJumpMultiplier;
            Debug.Log($"POTION Activated! Speed: {ForwardSpeed}, Jump: {JumpHeight}");

            // TODO: Add optional visual/audio feedback for potion start

            // Wait for the duration
            yield return new WaitForSeconds(potionDuration);

            // Restore original values
            ForwardSpeed = originalSpeed;
            JumpHeight = originalJumpHeight;
            isPotionActive = false;
            Debug.Log("POTION Deactivated. Speed/Jump restored.");

            // TODO: Add optional visual/audio feedback for potion end
        }


        // --- APPLE ---
        public bool IsCollisionProof => isCollisionProof; // Read-only property

public void ActivateCollisionProof() // Renamed activation method
{
     // Allow restarting the timer if already active
     // Stop any previous coroutine first to prevent conflicts
     StopCoroutine(nameof(CollisionProofCoroutine));
     StartCoroutine(CollisionProofCoroutine());
}

private IEnumerator CollisionProofCoroutine()
{
    isCollisionProof = true;
    Debug.Log("APPLE Activated! Collision consequences ignored.");

     if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateResistanceON(); // Tell UI effect is active
        }
        else { Debug.LogWarning("UIManager instance not found when trying to call UpdateResistanceON."); }
        // -----------------

    // TODO: Add visual feedback (e.g., shield effect, color tint?)

    yield return new WaitForSeconds(invulnerabilityDuration); // Use the duration variable

    isCollisionProof = false;
    Debug.Log("APPLE Deactivated. Collisions are deadly again.");
     if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateResistanceOFF(); // Tell UI effect is inactive
        }
         else { Debug.LogWarning("UIManager instance not found when trying to call UpdateResistanceOFF."); }

    // TODO: Remove visual feedback
}
        
        // --- COIN ---
         public void ActivateCameraAngleChange()
         {
             if (!isCameraPowerUpActive)
             {
                StartCoroutine(CameraAngleCoroutine());
             }
         }

         private IEnumerator CameraAngleCoroutine()
         {
            isCameraPowerUpActive = true;
            originalCameraAngleOverride = CameraAngleOverride; // Store current override
            float rnumber= Random.value;
            if(rnumber> 0.5f){
            CameraAngleOverride += cameraAnglePowerUpValue; 
            }
            else{
                CameraAngleOverride -= cameraAnglePowerUpValue; 
            }
            // Or set directly: CameraAngleOverride = cameraAnglePowerUpValue;
            Debug.Log($"COIN Activated! Camera Angle Override now: {CameraAngleOverride}");

            // TODO: Add camera transition effect? Sound?

            yield return new WaitForSeconds(cameraAnglePowerUpDuration);

            CameraAngleOverride = originalCameraAngleOverride; // Restore original override
            isCameraPowerUpActive = false;
            Debug.Log("COIN Deactivated. Camera Angle restored.");
         }

        #endregion








        #region Gizmos


private void OnDrawGizmosSelected() // Or OnDrawGizmos
{
    // Determine color based on the Grounded state (only reliable at runtime)
    Color gizmoColor;
    if (Application.isPlaying) // Check runtime state
        gizmoColor = Grounded ? new Color(0.0f, 1.0f, 0.0f, 0.35f) : new Color(1.0f, 0.0f, 0.0f, 0.35f);
    else // Default to green if not playing (Grounded variable isn't updated)
        gizmoColor = new Color(0.0f, 1.0f, 0.0f, 0.35f);

    Gizmos.color = gizmoColor;

    // --- VERIFY THIS CALCULATION ---
    // Calculate sphere position using current offset/radius values
    // Ensure GroundedOffset is negative in the Inspector (e.g., -0.14f)
    Vector3 spherePosition = new Vector3(
        transform.position.x,
        transform.position.y + GroundedOffset, // Use '+' because GroundedOffset is negative
        transform.position.z
    );
    // --- END VERIFICATION ---

    // --- VERIFY RADIUS VALUE ---
    // Ensure GroundedRadius has a sensible value in the Inspector (e.g., 0.2f to 0.3f)
    // If GroundedRadius is 0 or very small, the sphere will be invisible.
    if (GroundedRadius > 0.01f) // Add a check to prevent drawing tiny spheres
    {
         Gizmos.DrawSphere(spherePosition, GroundedRadius);
    }
    else if (Application.isPlaying) // Only warn when playing
    {
         // Optional: Warn if radius is too small to be seen
         // Debug.LogWarning("GroundedRadius is very small, gizmo may not be visible.");
    }
    // --- END VERIFICATION ---
}

        #endregion
    }
}