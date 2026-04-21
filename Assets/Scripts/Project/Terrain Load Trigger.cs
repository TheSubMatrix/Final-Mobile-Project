using System;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D))]
public class TerrainLoadTrigger : MonoBehaviour
{
    public event Action OnThresholdCrossed;

    BoxCollider2D m_collider;

    void Awake()
    {
        m_collider = GetComponent<BoxCollider2D>();
        m_collider.isTrigger = true;
    }

    public void SetBounds(float x, float height)
    {
        transform.position = new(x, 0f, 0f);
        m_collider.size = new(1f, height);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        OnThresholdCrossed?.Invoke();
    }
}
