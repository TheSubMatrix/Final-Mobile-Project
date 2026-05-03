using MatrixUtils.DependencyInjection;
using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    [Inject] IScoreReaderWriter m_scoreManager;

    [Header("Scoring Settings")]
    [SerializeField] int m_airtimeMultiplier = 10;
    [SerializeField] int m_heightMultiplier = 5;

    Vector2 m_startPosition;
    bool m_isAirborne;
    float m_airtimeTimer;
    float m_maxHeightInJump;
    float m_jumpStartY;

    void FixedUpdate()
    {
        HandleDistanceScoring();
        if (m_isAirborne) TrackAirStats();
    }

    void HandleDistanceScoring()
    {
        m_scoreManager.UpdateDistance(Vector2.Distance(m_startPosition, transform.position));
    }

    void TrackAirStats()
    {
        m_airtimeTimer += Time.fixedDeltaTime;
        if (transform.position.y <= m_maxHeightInJump) return;
        m_maxHeightInJump = transform.position.y;
    }

    public void OnLeftGround()
    {
        m_isAirborne = true;
        m_airtimeTimer = 0;
        m_jumpStartY = transform.position.y;
        m_maxHeightInJump = transform.position.y;
    }

    public void OnLanded()
    {
        if (!m_isAirborne) return;
        ApplyAirBonus();
        m_isAirborne = false;
    }

    void ApplyAirBonus()
    {
        float totalHeight = Mathf.Max(0, m_maxHeightInJump - m_jumpStartY);
        int heightBonus = Mathf.RoundToInt(totalHeight * m_heightMultiplier);
        int airtimeBonus = Mathf.RoundToInt(m_airtimeTimer * m_airtimeMultiplier);
        if (heightBonus <= 0 && airtimeBonus <= 0) return;
        uint totalBonus = (uint)heightBonus + (uint)airtimeBonus;
        m_scoreManager.UpdateBonusPoints(m_scoreManager.GetCurrentScore().BonusPoints + totalBonus);
    }
}