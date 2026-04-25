using MatrixUtils.DependencyInjection;
using UnityEngine;

public class PlayerScoreHandler : MonoBehaviour
{
    [Inject] IScoreManager m_scoreManager;
    Vector2 m_lastPosition;
    void FixedUpdate()
    {
        if (!(Vector3.Distance(transform.position, m_lastPosition) > 1)) return;
        m_lastPosition = transform.position;
        m_scoreManager.AddScore(1);
    }
}
