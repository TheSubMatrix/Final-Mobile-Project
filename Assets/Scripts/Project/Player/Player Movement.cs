using MatrixUtils.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, RequiredField] InputActionReference m_diveAction;
    [SerializeField] float m_diveForce = 1000;
    [SerializeField] float m_minimumHorizontalSpeed = 0.5f;
    [SerializeField] float m_coyoteTime = 0.1f;
    
    [Header("Ground")]
    public UnityEvent OnLanded;
    public UnityEvent OnLeftGround;
    [SerializeField] LayerMask m_groundLayer;
    
    bool m_isDiving;
    Rigidbody2D m_rb;

    int m_groundContactCount;
    bool m_isGrounded;
    float m_groundedLostTimer;

    void OnEnable()
    {
        m_diveAction.action.Enable();
        m_diveAction.action.performed += OnDivePerformed;
        m_diveAction.action.canceled += OnDiveCanceled;
    }

    void OnDisable()
    {
        m_diveAction.action.performed -= OnDivePerformed;
        m_diveAction.action.canceled -= OnDiveCanceled;
        m_diveAction.action.Disable();
    }

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }

    void OnDivePerformed(InputAction.CallbackContext ctx) => m_isDiving = true;
    void OnDiveCanceled(InputAction.CallbackContext ctx) => m_isDiving = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!IsGround(col)) return;
        m_groundContactCount++;
        if (m_isGrounded) return;
        m_isGrounded = true;
        m_groundedLostTimer = 0;
        OnLanded.Invoke();
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (!IsGround(col)) return;
        m_groundContactCount = Mathf.Max(0, m_groundContactCount - 1);
    }

    void FixedUpdate()
    {
        HandleGroundedState();
        if (m_isDiving) m_rb.AddForce(Vector2.down * m_diveForce, ForceMode2D.Force);
        Vector2 velocity = m_rb.linearVelocity;
        velocity = new(Mathf.Max(m_minimumHorizontalSpeed, velocity.x), velocity.y);
        m_rb.linearVelocity = velocity;
    }

    void HandleGroundedState()
    {
        if (m_groundContactCount > 0)
        {
            m_groundedLostTimer = 0;
            return;
        }
        if (!m_isGrounded) return;
        m_groundedLostTimer += Time.fixedDeltaTime;
        if (!(m_groundedLostTimer >= m_coyoteTime)) return;
        m_isGrounded = false;
        OnLeftGround.Invoke();
    }

    bool IsGround(Collision2D col) => ((1 << col.gameObject.layer) & m_groundLayer) != 0;
}