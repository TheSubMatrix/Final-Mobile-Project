using System;
using MatrixUtils.DependencyInjection;
using UnityEngine;
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    [SerializeField] uint m_scoreToGive = 10;
    [Inject] IScoreManager m_scoreManager;
    Action m_coinCollectedAction;
    
    void Initialize(Action onCoinCollected)
    {
        m_coinCollectedAction = onCoinCollected;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        m_scoreManager.AddScore(m_scoreToGive);
        m_coinCollectedAction?.Invoke();
    }
}
