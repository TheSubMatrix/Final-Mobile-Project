using MatrixUtils.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent( typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, RequiredField] InputActionReference m_diveAction;
    [SerializeField] float m_diveForce = 1000;
    [SerializeField] float m_minimumHorizontalSpeed = 0.5f;
    bool m_isDiving;
    Rigidbody2D m_rb;

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
    void OnDivePerformed(InputAction.CallbackContext ctx)
    {
        m_isDiving = true;
    }

    void OnDiveCanceled(InputAction.CallbackContext ctx)
    {
        m_isDiving = false;
    }

    void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
    }
    void FixedUpdate()
    {
        if (m_isDiving) m_rb.AddForce(Vector2.down * m_diveForce, ForceMode2D.Force);
        Vector2 velocity = m_rb.linearVelocity;
        velocity = new(Mathf.Max(m_minimumHorizontalSpeed, velocity.x), velocity.y);
        m_rb.linearVelocity = velocity;
    }
}