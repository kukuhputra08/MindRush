using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Refs")]
    public CharacterController controller;
    public Transform cameraTransform;          // drag Main Camera
    public Animator animator;                  // drag Animator karakter

    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 10f;

    [Header("Smoothing")]
    public float turnSmoothTime = 0.15f;      // halus belok
    public float speedSmoothTime = 0.10f;      // halus percepatan
    private float turnSmoothVel;
    private float speedSmoothVel;
    private float currentSpeedSmoothed;

    [Header("Jump / Gravity")]
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public float groundedBufferTime = 0.1f;
    private float groundedTimer;
    private Vector3 velocityY;                 // hanya Y

    [Header("Animator (Blend: 0=idle, 0.5=walk, 1=run)")]
    public float blendDamp = 0.1f;

    void Awake()
    {
        if (!controller) controller = GetComponent<CharacterController>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        if (!animator) animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        // --- grounded helper (coyote) ---
        bool groundedNow = controller.isGrounded;
        if (groundedNow) groundedTimer = groundedBufferTime;
        else groundedTimer -= Time.deltaTime;

        if (groundedTimer > 0f && velocityY.y < 0f) velocityY.y = -2f;

        // --- WASD manual (camera-relative) ---
        float ix = 0f, iz = 0f;
        if (Input.GetKey(KeyCode.A)) ix -= 1f;
        if (Input.GetKey(KeyCode.D)) ix += 1f;
        if (Input.GetKey(KeyCode.S)) iz -= 1f;
        if (Input.GetKey(KeyCode.W)) iz += 1f;

        Vector2 input2 = new Vector2(ix, iz);
        float inputMag = Mathf.Clamp01(input2.magnitude);
        if (input2.sqrMagnitude > 1f) input2.Normalize(); // diagonal normalisasi
        ix = input2.x; iz = input2.y;

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float targetSpeed = (isRunning ? runSpeed : walkSpeed) * inputMag;

        // --- arah relatif kamera ---
        Vector3 camF = cameraTransform.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = cameraTransform.right; camR.y = 0f; camR.Normalize();
        Vector3 moveDir = camF * iz + camR * ix;   // inilah inti: WASD relatif kamera

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            // rotasi halus menghadap arah gerak
            float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
            float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref turnSmoothVel, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, smoothYaw, 0f);
        }

        // --- percepatan halus ---
        currentSpeedSmoothed = Mathf.SmoothDamp(currentSpeedSmoothed, targetSpeed, ref speedSmoothVel, speedSmoothTime);

        // --- lompat & gravitasi ---
        if (Input.GetKeyDown(KeyCode.Space) && groundedTimer > 0f)
            velocityY.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        velocityY.y += gravity * Time.deltaTime;

        // --- satu kali Move per frame (horizontal + vertical) ---
        Vector3 finalMove = moveDir.normalized * currentSpeedSmoothed + new Vector3(0f, velocityY.y, 0f);
        controller.Move(finalMove * Time.deltaTime);

        // --- animator params (untuk nested Walk2D/Run2D) ---
        if (animator)
        {
            // untuk 2D tree, kirim input relatif kamera (ix, iz)
            animator.SetFloat("MoveX", ix, 0.05f, Time.deltaTime);
            animator.SetFloat("MoveZ", iz, 0.05f, Time.deltaTime);

            // level locomotion
            float blendTarget = (isRunning ? 1f : 0.5f) * inputMag; // 0..1
            animator.SetFloat("Blend", blendTarget, blendDamp, Time.deltaTime);
        }
    }
}
