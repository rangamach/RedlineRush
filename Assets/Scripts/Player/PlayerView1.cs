using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerView1 : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;

    private bool hasReversedThruGoal = false;

    private bool finishedRace = false;

    [Header("Car")]
    [SerializeField] private float slipAllowance = 0.5f;
    [SerializeField] private float minSpeedToSmoke = 1f;
    [SerializeField] private float speed;
    [SerializeField] private float brake;
    [SerializeField] private WheelColliders colliders;
    [SerializeField] private WheelTransforms transforms;
    [SerializeField] private WheelParticles particles;
    [SerializeField] private CarExplosion explosion;
    [SerializeField] private ParticleSystem smokeParticlePrefab;
    [SerializeField] private ParticleSystem explosionParticlePrefab;
    [SerializeField] private AnimationCurve steeringCurve;
    [SerializeField] private float wheelRotationSpeed;
    private PlayerController playerController;
    private Rigidbody rb;
    private Vector2 currentMovementvector;
    private float slipAngle;
    private float brakeInput;
    private int lap = -1;

    [Header("Car Stability Settings")]
    [SerializeField] private float antiRollForce = 6000f; // Reduced from 8000
    [SerializeField] private float centerOfMassY = -0.9f; // Lowered center of mass
    [SerializeField] private float suspensionSpringStrength = 35000f; // Softer
    [SerializeField] private float suspensionDamperStrength = 5000f;
    [SerializeField] private float rearDownforceMultiplier = 30f; // Pushes rear down

    [Header("Camera")]
    [SerializeField] private float tiltSpeed;
    [SerializeField] private Vector3 cameraPositionOffset;
    [SerializeField] private float cameraMoveSmoothness;
    [SerializeField] private float cameraRotationSmoothness;
    [SerializeField] private Vector3 cameraRotationOffset;
    private Camera mainCamera;
    private float shakeDuration = 0f;
    [SerializeField] private float shakeMagnitude;
    private float currentTilt = 0f;
    private Vector3 currentCameraVelocity;

    //InputAction
    private CarDrive carDrive;
    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialScale = transform.localScale;


        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0f, centerOfMassY, 0f);
        carDrive = new CarDrive();

        SetCamera();
        InstantiateSmoke();
        InstantiateExplosionParticleEffect();
    }
    private void SetupSuspension()
    {
        SetupWheelSuspension(colliders.FRWheelCollider, true);
        SetupWheelSuspension(colliders.FLWheelCollider, true);
        SetupWheelSuspension(colliders.RRWheelCollider, false);
        SetupWheelSuspension(colliders.RLWheelCollider, false);
    }

    private void SetupWheelSuspension(WheelCollider wheel, bool isFront)
    {
        JointSpring spring = wheel.suspensionSpring;

        spring.spring = 32000f;
        spring.damper = 8000f;

        wheel.suspensionDistance = 0.14f;
    }
    public PlayerView1 Spawn(Vector3 position, Vector3 rotation, Vector3 scale)
    {
        GameObject view = Instantiate(this.gameObject, position,Quaternion.Euler(rotation));
        view.transform.localScale = scale;

        var playerView = view.GetComponent<PlayerView1>();

        return playerView;
    }
        private void SubscribeInvokeInputs()
        {
            carDrive.GameAction.Pause.performed += CheckPauseInput;
        }
        private void OnEnable()
        {
            carDrive.GameAction.Enable();
            carDrive.Movement.Enable();

            SubscribeInvokeInputs();
        }
        private void OnDisable()
        {
            carDrive.GameAction.Pause.performed -= CheckPauseInput;

            carDrive.GameAction.Disable();
            carDrive.Movement.Disable();
        }
    private void OnDestroy()
    {
        GameService.Instance.EventService.OnPlayerDeath.RemoveListener(PlayerDied);
    }
    private void Start()
    {
        GameService.Instance.EventService.OnPlayerDeath.AddListener(PlayerDied);

        SetupCarFriction();

        currentMovementvector = Vector2.zero;
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        currentMovementvector = carDrive.Movement.Driving.ReadValue<Vector2>();

        ApplyDeadZone();

        CheckInput();

        CheckParticles();
        ApplyWheelMovement();
        UpdateCarEngineAudio();
    }
    private void FixedUpdate()
    {
        ApplyGas();
        ApplySteering();
        ApplyBrake();

        CameraFollowCar();
    }
    private void ApplyDeadZone()
    {
        float deadzone = 0.05f;
        if (Mathf.Abs(currentMovementvector.x) < deadzone) currentMovementvector.x = 0;
        if (Mathf.Abs(currentMovementvector.y) < deadzone) currentMovementvector.y = 0;

    }
    private void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if(impactForce > 5f)
        {
            playerController.TakeDamage(10);
            if (GameService.Instance.GameState != GameState.Gameover)
            {
                shakeDuration = 0.5f;
                InstantGripRecovery();
            }
            else
            {
                return;
            }
            GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.CarCrash);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Goal"))
        {
            Vector3 velDir = rb.linearVelocity.normalized;
            Vector3 goalDir = other.transform.right;

            float dot = Vector3.Dot(goalDir,velDir);

            if(dot < 0)
            {
                hasReversedThruGoal = true;
                return;
            }
            if (!hasReversedThruGoal)
            {
                lap++;
                if(lap == 1)
                {
                    finishedRace = true;
                    GameService.Instance.SetGameState(GameState.Gameover);
                }
            }
        }
    }
    private void StabilityControl()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        float angle = Mathf.Atan2(localVelocity.x, localVelocity.z) * Mathf.Rad2Deg;

        if (Mathf.Abs(angle) > 15f) // Too much slip
        {
            float brakeForce = Mathf.Abs(angle) / 15f * 1500f;
            colliders.FRWheelCollider.brakeTorque = brakeForce;
            colliders.FLWheelCollider.brakeTorque = brakeForce;
        }
        else
        {
            colliders.FRWheelCollider.brakeTorque = 0f;
            colliders.FLWheelCollider.brakeTorque = 0f;
        }
    }
    private void SetupCarFriction()
    {
        SetupWheelFriction(colliders.FRWheelCollider, 2.0f, 2.3f);
        SetupWheelFriction(colliders.FLWheelCollider, 2.0f, 2.3f);
        SetupWheelFriction(colliders.RRWheelCollider, 2.0f, 3f); // Slightly more rear grip
        SetupWheelFriction(colliders.RLWheelCollider, 2.0f, 3f);
    }
    private void SetupWheelFriction(WheelCollider wheel, float forwardStiffness, float sidewaysStiffness)
    {
        WheelFrictionCurve forwardFriction = wheel.forwardFriction;
        forwardFriction.extremumSlip = 0.4f;
        forwardFriction.extremumValue = 1f;
        forwardFriction.asymptoteSlip = 0.8f;
        forwardFriction.asymptoteValue = 0.5f;
        forwardFriction.stiffness = forwardStiffness;
        wheel.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = wheel.sidewaysFriction;
        sidewaysFriction.extremumSlip = 0.3f;
        sidewaysFriction.extremumValue = 1f;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 0.75f;
        sidewaysFriction.stiffness = sidewaysStiffness;
        wheel.sidewaysFriction = sidewaysFriction;
    }
    private void InstantGripRecovery()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        colliders.RRWheelCollider.motorTorque = 0f;
        colliders.RLWheelCollider.motorTorque = 0f;

        colliders.FRWheelCollider.brakeTorque = brake * 2f;
        colliders.FLWheelCollider.brakeTorque = brake * 2f;
        colliders.RRWheelCollider.brakeTorque = brake * 2f;
        colliders.RLWheelCollider.brakeTorque = brake * 2f;

        colliders.FRWheelCollider.steerAngle = 0f;
        colliders.FLWheelCollider.steerAngle = 0f;

        ClearSmoke();

        Invoke(nameof(ReleaseBrakes),0.1f);
    }
    private void ReleaseBrakes()
    {
        colliders.FRWheelCollider.brakeTorque = 0;
        colliders.FLWheelCollider.brakeTorque = 0;
        colliders.RRWheelCollider.brakeTorque = 0;
        colliders.RLWheelCollider.brakeTorque = 0;
    }
    private void ClearSmoke()
    {
        if (particles == null) return;

        particles.FRParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.FLParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.RRParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.RLParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    private void InstantiateSmoke()
    {
        particles = new WheelParticles();

        particles.FRParticle = Instantiate(smokeParticlePrefab, transforms.FRTransform.transform.position - Vector3.up * colliders.FRWheelCollider.radius, Quaternion.Euler(180f, 75f, 0f), colliders.FRWheelCollider.transform).GetComponent<ParticleSystem>();
        particles.FLParticle = Instantiate(smokeParticlePrefab, transforms.FLTransform.transform.position - Vector3.up * colliders.FLWheelCollider.radius, Quaternion.Euler(180f, 75f, 0f), colliders.FLWheelCollider.transform).GetComponent<ParticleSystem>();
        particles.RRParticle = Instantiate(smokeParticlePrefab, transforms.RRTransform.transform.position - Vector3.up * colliders.RRWheelCollider.radius, Quaternion.Euler(180f, 75f, 0f), colliders.RRWheelCollider.transform).GetComponent<ParticleSystem>();
        particles.RLParticle = Instantiate(smokeParticlePrefab, transforms.RLTransform.transform.position - Vector3.up * colliders.RLWheelCollider.radius, Quaternion.Euler(180f, 75f, 0f), colliders.RLWheelCollider.transform).GetComponent<ParticleSystem>();
    }
    private void InstantiateExplosionParticleEffect()
    {
        explosion = new CarExplosion();

        explosion.WholeCarExplosion = Instantiate(explosionParticlePrefab);
        explosion.WholeCarExplosion.transform.SetParent(null);
        explosion.WholeCarExplosion.transform.localPosition = Vector3.zero + new Vector3(0f, 0.75f, -0.25f);
        explosion.WholeCarExplosion.transform.localRotation = Quaternion.identity;
        explosion.WholeCarExplosion.transform.localScale = new Vector3(1f, 1f, 1f);

        var mod = explosion.WholeCarExplosion.main;
        mod.loop = false;

        explosion.WholeCarExplosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    private void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        bool[] Grounded = new bool[4];

        Grounded[0] = colliders.FRWheelCollider.GetGroundHit(out wheelHits[0]);
        Grounded[1] = colliders.FLWheelCollider.GetGroundHit(out wheelHits[1]);
        Grounded[2] = colliders.RRWheelCollider.GetGroundHit(out wheelHits[2]);
        Grounded[3] = colliders.RLWheelCollider.GetGroundHit(out wheelHits[3]);

        float carSpeed = rb.linearVelocity.magnitude;

        HandleSmokeParticles(Grounded[0], wheelHits[0], particles.FRParticle, carSpeed);
        HandleSmokeParticles(Grounded[1], wheelHits[1], particles.FLParticle, carSpeed);
        HandleSmokeParticles(Grounded[2], wheelHits[2], particles.RRParticle, carSpeed);
        HandleSmokeParticles(Grounded[3], wheelHits[3], particles.RLParticle, carSpeed);
    }

    private void HandleSmokeParticles(bool isGrounded, WheelHit hit, ParticleSystem particle, float speed)
    {
        float slip = Mathf.Abs(hit.sidewaysSlip) + Mathf.Abs(hit.forwardSlip);

        if (isGrounded && (slip > slipAllowance || speed > minSpeedToSmoke))
        {
            if (!particle.isPlaying)
            {
                particle.Play();
            }
        }
        else
        {
            if (particle.isPlaying)
            {
                particle.Stop();
            }
        }
    }
    private void ApplyWheelMovement()
    {
        UpdateWheels(colliders.FRWheelCollider, transforms.FRTransform);
        UpdateWheels(colliders.FLWheelCollider, transforms.FLTransform);
        UpdateWheels(colliders.RRWheelCollider, transforms.RRTransform);
        UpdateWheels(colliders.RLWheelCollider, transforms.RLTransform);
    }
    private void OnMovePerformed(InputAction.CallbackContext ctx) => currentMovementvector = ctx.ReadValue<Vector2>();
    private void OnMoveCancelled(InputAction.CallbackContext ctx)
    {
        currentMovementvector = Vector2.zero;
    }
    private void CheckPauseInput(InputAction.CallbackContext ctx)
    {
        if(GameService.Instance.GameState == GameState.Gameplay || GameService.Instance.GameState == GameState.Gamepaused)
        {
            switch(GameService.Instance.GameState)
            {
                case GameState.Gameplay:
                    GameService.Instance.SoundService.NonBGMAudios(true);
                    Time.timeScale = 0f;
                    GameService.Instance.SetGameState(GameState.Gamepaused);
                    break;
                case GameState.Gamepaused:
                    GameService.Instance.SoundService.NonBGMAudios(false);
                    Time.timeScale = 1f;
                    GameService.Instance.SetGameState(GameState.Gameplay);
                    break;
            }
        }
    }
    private void CheckInput()
    {
        float slipAngle = Vector3.Angle(transform.forward, rb.linearVelocity - transform.forward);

        if (slipAngle < 120f)
        {
            if (currentMovementvector.y < 0)
            {
                brakeInput = Mathf.Abs(currentMovementvector.y);
            }
        }
        else
            brakeInput = 0;
    }

    private void ApplyGas()
    {
        colliders.RRWheelCollider.motorTorque = speed * currentMovementvector.y;
        colliders.RLWheelCollider.motorTorque = speed * currentMovementvector.y;
    }

    private void ApplyBrake()
    {
        float frontBrake = brakeInput * brake * 0.7f;
        float rearBrake = brakeInput * brake * 0.3f;

        colliders.FRWheelCollider.brakeTorque = frontBrake;
        colliders.FLWheelCollider.brakeTorque = frontBrake;
        colliders.RLWheelCollider.brakeTorque = rearBrake;
        colliders.RRWheelCollider.brakeTorque = rearBrake;
    }

    private void ApplySteering()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);
        bool isReversing = localVelocity.z < -0.1;

        float speedkmph = rb.linearVelocity.magnitude * 3.6f;

        float maxSteerAngle = steeringCurve.Evaluate(speedkmph);
        float steeringAngle = currentMovementvector.x * maxSteerAngle;

        if (isReversing)
        {
            steeringAngle *= -1;
        }

        colliders.FRWheelCollider.steerAngle = steeringAngle;
        colliders.FLWheelCollider.steerAngle = steeringAngle;
    }
    private void UpdateWheels(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Quaternion collider_quaternion;
        Vector3 collider_position;

        wheelCollider.GetWorldPose(out collider_position, out collider_quaternion);

        wheelTransform.position = collider_position;
        wheelTransform.rotation = Quaternion.Slerp(wheelTransform.rotation, collider_quaternion, wheelRotationSpeed * 2f * Time.deltaTime);
    }
    private void CameraFollowCar()
    {
        CameraMovementUpdate();
        CameraRotationUpdate();
    }
    private void CameraMovementUpdate()
    {
        Vector3 targetPosition = new Vector3();
        targetPosition = transform.TransformPoint(cameraPositionOffset);

        if (shakeDuration > 0)
        {
            targetPosition = CameraShake(targetPosition);
        }

        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, targetPosition, ref currentCameraVelocity, 1f/cameraMoveSmoothness);
    }
    private Vector3 CameraShake(Vector3 targetPosition)
    {
        shakeDuration -= Time.deltaTime;
        return targetPosition += Random.insideUnitSphere * shakeMagnitude;
    }
    private void CameraRotationUpdate()
    {
        var direction = transform.position - mainCamera.transform.position;
        var rotation = new Quaternion();

        rotation = Quaternion.LookRotation(direction + cameraRotationOffset,Vector3.up);

        Quaternion finalRotation = rotation * CameraTilt();

        mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, finalRotation, cameraRotationSmoothness * Time.deltaTime);

    }
    private Quaternion CameraTilt()
    {
        float targetTilt = -currentMovementvector.x * tiltSpeed;
        currentTilt = Mathf.Lerp(currentTilt,targetTilt,Time.deltaTime * cameraRotationSmoothness);
        Quaternion tiltRotation = Quaternion.Euler(0f, 0f, currentTilt);

        return tiltRotation;
    }
    public void SetController(PlayerController playerController) => this.playerController = playerController;
    private void SetCamera()
    {
        if (this.mainCamera == null)
        {
            this.mainCamera = new GameObject("MainCamera").AddComponent<Camera>();
            this.mainCamera.gameObject.AddComponent<AudioListener>();
            this.mainCamera.tag = "MainCamera";
            this.mainCamera.fieldOfView = 60;
            this.mainCamera.nearClipPlane = 0.1f;
            this.mainCamera.farClipPlane = 5000f;
            this.mainCamera.clearFlags = CameraClearFlags.Skybox;
        }

        SetCameraInitialTransform();
    }
    private void SetCameraInitialTransform()
    {
        //Initial Position
        Vector3 startPos = transform.TransformPoint(cameraPositionOffset);
        mainCamera.transform.position = startPos;

        //Initial Rotation
        Vector3 direction = transform.position - startPos;
        Quaternion startRot = Quaternion.LookRotation(direction + cameraRotationOffset, Vector3.up);
        mainCamera.transform.rotation = startRot;

        initialCameraPosition = startPos;
        initialCameraRotation = startRot;
    }
    public void PlayerDied()
    {
        GameService.Instance.SetGameState(GameState.Gameover);
        transform.GetChild(0).gameObject.SetActive(false);

        GameService.Instance.SoundService.PlaySFXMusic(SoundTypes.CarExplosion);

        explosion.WholeCarExplosion.transform.position = transform.position + new Vector3(0f,0.75f,0.25f);
        explosion.WholeCarExplosion.transform.rotation = Quaternion.identity;

        explosion.WholeCarExplosion.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        explosion.WholeCarExplosion.Play();
    }
    public void ResetPlayer()
    {
        VelocityRemover();
        ResetTransform();
        ResetLap();
        transform.GetChild(0).gameObject.SetActive(true);
    }
    private void ResetTransform()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        transform.localScale = initialScale;

        mainCamera.transform.position = initialCameraPosition;
        mainCamera.transform.rotation = initialCameraRotation;
    }
    private void ResetLap()
    {
        finishedRace = false;
        lap = -1;
    }
    private void VelocityRemover()
    {
        currentMovementvector = Vector2.zero;

        InstantGripRecovery();
    }
    private void UpdateCarEngineAudio()
    {
        GameService.Instance.SoundService.UpdateCarEnginePitch(rb.linearVelocity.magnitude * 3.6f);
    }
    public bool GetFinishedRace() => finishedRace;
}
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheelCollider;
    public WheelCollider FLWheelCollider;
    public WheelCollider RRWheelCollider;
    public WheelCollider RLWheelCollider;
}
[System.Serializable]
public class WheelTransforms
{
    public Transform FRTransform;
    public Transform FLTransform;
    public Transform RRTransform;
    public Transform RLTransform;
}
public class WheelParticles
{
    public ParticleSystem FRParticle;
    public ParticleSystem FLParticle;
    public ParticleSystem RRParticle;
    public ParticleSystem RLParticle;
}
public class CarExplosion
{
    public ParticleSystem WholeCarExplosion;
}