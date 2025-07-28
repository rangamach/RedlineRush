using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerView1 : MonoBehaviour
{
    [Header("Car")]
    [SerializeField] private float slipAllowance = 0.5f;
    [SerializeField] private float minSpeedToSmoke = 1f;
    [SerializeField] private float speed;
    [SerializeField] private float brake;
    [SerializeField] private WheelColliders colliders;
    [SerializeField] private WheelTransforms transforms;
    [SerializeField] private WheelParticles particles;
    [SerializeField] private ParticleSystem smokeParticlePrefab;
    [SerializeField] private AnimationCurve steeringCurve;
    [SerializeField] private float wheelRotationSpeed;
    private PlayerController playerController;
    private Rigidbody rb;
    private Vector2 currentMovementvector;
    private float slipAngle;
    private float brakeInput;
    private Vector3 originalCameraPosition;

    [Header("Camera")]
    [SerializeField] private float tiltSpeed;
    [SerializeField] private Vector3 cameraPositionOffset;
    [SerializeField] private float cameraMoveSmoothness;
    [SerializeField] private float cameraRotationSmoothness;
    [SerializeField] private Vector3 cameraRotationOffset;
    private Camera mainCamera;
    private float shakeDuration = 0f;
    private float shakeMagnitude;
    private float currentTilt = 0f;

    //InputAction
    private CarDrive carDrive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        carDrive = new CarDrive();

        SetCamera();
        InstantiateSmoke();

        SubscribeInvokeInputs();
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
        carDrive.Movement.Driving.performed += OnMovePerformed;
        carDrive.Movement.Driving.canceled += OnMoveCancelled;
    }
    private void OnEnable() => carDrive.Movement.Enable();
    private void OnDestroy() => carDrive.Movement.Disable();
    private void Start()
    {
        currentMovementvector = Vector2.zero;
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        CheckInput();

        ApplyGas();
        ApplySteering();
        ApplyBrake();
        CheckParticles();
        ApplyWheelMovement();
    }
    private void FixedUpdate()
    {
        CameraFollowCar();
    }
    private void InstantiateSmoke()
    {
        particles = new WheelParticles();

        particles.FRParticle = Instantiate(smokeParticlePrefab, colliders.FRWheelCollider.transform.position - Vector3.up * colliders.FRWheelCollider.radius + new Vector3(-0.15f,0,0), Quaternion.Euler(180f, 0f, 0f), colliders.FRWheelCollider.transform).GetComponent<ParticleSystem>();
        particles.FLParticle = Instantiate(smokeParticlePrefab, colliders.FLWheelCollider.transform.position - Vector3.up * colliders.FLWheelCollider.radius + new Vector3(0.2f, 0, 0), Quaternion.Euler(180f, 0f, 0f), colliders.FLWheelCollider.transform).GetComponent<ParticleSystem>();
        particles.RRParticle = Instantiate(smokeParticlePrefab, colliders.RRWheelCollider.transform.position - Vector3.up * colliders.RRWheelCollider.radius + new Vector3(-0.12f, 0, 0), Quaternion.Euler(180f, 0f, 0f), colliders.RRWheelCollider.transform).GetComponent<ParticleSystem>();
        particles.RLParticle = Instantiate(smokeParticlePrefab, colliders.RLWheelCollider.transform.position - Vector3.up * colliders.RLWheelCollider.radius + new Vector3(0.1f, 0, 0), Quaternion.Euler(180f, 0f, 0f), colliders.RLWheelCollider.transform).GetComponent<ParticleSystem>();
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
    private void OnMoveCancelled(InputAction.CallbackContext ctx) => currentMovementvector = Vector2.zero;
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

        float steeringAngle = currentMovementvector.x * steeringCurve.Evaluate(rb.linearVelocity.magnitude);

        Vector3 velocityForSteering = rb.linearVelocity;

        if(isReversing)
            velocityForSteering = -rb.linearVelocity;

        steeringAngle += Vector3.SignedAngle(transform.forward, velocityForSteering + transform.forward, Vector3.up);
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);

        colliders.FRWheelCollider.steerAngle = steeringAngle;
        colliders.FLWheelCollider.steerAngle = steeringAngle;
    }
    private void UpdateWheels(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Quaternion collider_quaternion;
        Vector3 collider_position;

        wheelCollider.GetWorldPose(out collider_position, out collider_quaternion);

        wheelTransform.position = collider_position;
        wheelTransform.rotation = Quaternion.Slerp(wheelTransform.rotation, collider_quaternion, wheelRotationSpeed * Time.deltaTime);
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

        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position,targetPosition,cameraMoveSmoothness * Time.deltaTime);
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
        this.mainCamera = new GameObject("MainCamera").AddComponent<Camera>();
        this.mainCamera.gameObject.AddComponent<AudioListener>();
        this.mainCamera.tag = "MainCamera";
        this.mainCamera.fieldOfView = 60;
        this.mainCamera.nearClipPlane = 0.1f;
        this.mainCamera.farClipPlane = 5000f;
        this.mainCamera.clearFlags = CameraClearFlags.Skybox;

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

        originalCameraPosition = mainCamera.transform.localPosition;
    }
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