using MatrixUtils.Attributes;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField, RequiredField] Rigidbody2D m_rb;
    void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, Mathf.Atan2(m_rb.linearVelocity.y, m_rb.linearVelocity.x) * Mathf.Rad2Deg), Time.deltaTime * 10);
    }
}
