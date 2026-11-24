using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public CharacterController controller;
    public Animator anim;

    [Header("Movement")]
    public float walkSpeed = 6f;
    public float sprintSpeed = 10f;

    [Header("Jump & Gravity")]
    public float gravity = -15f;
    public float jumpHeight = 1.5f;

    [Header("Stamina")]
    public float maxStamina = 10f;
    public float staminaDrainRate = 1f;
    public float staminaRechargeRate = 2f;
    public float staminaRechargeDelay = 1f;

    private Vector3 velocity;
    private float currentStamina;
    private float rechargeTimer;

    public bool IsGrounded { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsSprinting { get; private set; }

    void Start()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        currentStamina = maxStamina;

        if (controller != null)
        {
            controller.Move(Vector3.down * 0.1f);
            IsGrounded = controller.isGrounded;
        }
    }

    void Update()
    {
        if (controller == null) return;

        if (IsGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move = Vector3.ClampMagnitude(move, 1f);

        IsMoving = move.sqrMagnitude > 0.01f;

        bool wantsToSprint = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                             && IsMoving
                             && currentStamina > 0f;

        if (wantsToSprint)
        {
            IsSprinting = true;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            rechargeTimer = staminaRechargeDelay;
        }
        else
        {
            IsSprinting = false;

            if (rechargeTimer > 0f)
            {
                rechargeTimer -= Time.deltaTime;
            }
            else
            {
                currentStamina += staminaRechargeRate * Time.deltaTime;
                currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            }
        }

        if (Input.GetButtonDown("Jump") && IsGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            if (anim != null)
            {
                anim.SetTrigger("jump");
            }
        }

        velocity.y += gravity * Time.deltaTime;

        float speed = IsSprinting ? sprintSpeed : walkSpeed;
        Vector3 finalMove = (move * speed) + velocity;
        controller.Move(finalMove * Time.deltaTime);

        IsGrounded = controller.isGrounded;

        if (anim != null)
        {
            anim.SetFloat("moveX", x, 0.08f, Time.deltaTime);
            anim.SetFloat("moveZ", z, 0.08f, Time.deltaTime);
            anim.SetFloat("yVelocity", velocity.y);
            anim.SetBool("isGrounded", IsGrounded);
            anim.SetBool("isMoving", IsMoving);
            anim.SetBool("isSprinting", IsSprinting);
        }
    }
}