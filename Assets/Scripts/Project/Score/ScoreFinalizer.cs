using MatrixUtils.DependencyInjection;
using UnityEngine;

public class ScoreFinalizer : MonoBehaviour
{
    [Inject] IScoreWriter m_scoreWriter;
    public void FinalizeScore()
    {
        m_scoreWriter.CommitScore();
    }
}
