using MatrixUtils.DependencyInjection;
using UnityEngine;

public class DistanceScoreProxy : MonoBehaviour
{
    [Inject] IScoreWriter m_scoreWriter;
    float m_distance;
    public void UpdateDistance(float distance)
    {
        m_distance = distance;
    }
    public void GameEnded()
    {
        m_scoreWriter.UpdateDistance(m_distance);
        m_distance = 0f;
    }
}
