using MatrixUtils.DependencyInjection;
using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    [Inject] IScoreManager m_scoreManager;

    [Header("Scoring Settings")]
    [SerializeField] float m_groundCheckDistance = 1.1f;
    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] int m_airtimeMultiplier = 10;
    [SerializeField] int m_heightMultiplier = 5;

    Vector2 m_lastPosition;
    bool m_isAirborne;
    float m_airtimeTimer;
    float m_maxHeightInJump;
    float m_jumpStartY;

    void FixedUpdate()
    {
        HandleDistanceScoring();
        HandleAirBonus();
    }

    void HandleDistanceScoring()
    {
        if (!(Vector3.Distance(transform.position, m_lastPosition) > 1)) return;
        m_lastPosition = transform.position;
        m_scoreManager.AddScore(1);
    }

    void HandleAirBonus()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, m_groundCheckDistance, m_groundLayer);
        bool isGrounded = hit.collider;
        if (!isGrounded)
        {
            if (!m_isAirborne)
            {
                m_isAirborne = true;
                m_airtimeTimer = 0;
                m_jumpStartY = transform.position.y;
                m_maxHeightInJump = transform.position.y;
                Debug.Log("<color=cyan>Airborne Started!</color>");
            }

            m_airtimeTimer += Time.fixedDeltaTime;
            if (transform.position.y > m_maxHeightInJump)
            {
                m_maxHeightInJump = transform.position.y;
            }
        }
        else if (m_isAirborne)
        {
            Debug.Log($"<color=yellow>Landed! Airtime: {m_airtimeTimer:F2}s, Peak: {m_maxHeightInJump - m_jumpStartY:F2}m</color>");
            ApplyAirBonus();
            m_isAirborne = false;
        }
    }

    void ApplyAirBonus()
    {
        float totalHeight = Mathf.Max(0, m_maxHeightInJump - m_jumpStartY);
        int heightBonus = Mathf.RoundToInt(totalHeight * m_heightMultiplier);
        int airtimeBonus = Mathf.RoundToInt(m_airtimeTimer * m_airtimeMultiplier);
        if (heightBonus <= 0 && airtimeBonus <= 0) return;
        uint totalBonus = (uint)heightBonus + (uint)airtimeBonus;
        m_scoreManager.AddAirtimeBonus(totalBonus);
    }
}