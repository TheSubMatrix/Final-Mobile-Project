using MatrixUtils.DependencyInjection;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class Coin : Collectible
{
    [SerializeField] uint m_scoreToGive = 10;
    [Inject] IScoreWriter m_scoreManager;

    protected override void OnCollected(Collider2D other)
    {
        m_scoreManager.UpdateExtraPoints(m_scoreToGive);
    }
}
