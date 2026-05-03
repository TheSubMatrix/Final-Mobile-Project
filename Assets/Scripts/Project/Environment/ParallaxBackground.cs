using System;
using System.Collections.Generic;
using MatrixUtils.Attributes;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    [SerializeField, RequiredField] Camera m_camera;
    [SerializeField] List<ParallaxObject> m_parallaxObjects;
    
    Vector3 m_previousCameraPosition;
    float m_cameraHalfWidth;

    [Serializable]
    class ParallaxObject
    { 
        [SerializeField] PrefabPool<SpriteRenderer> m_parallaxPool;
        [SerializeField] float m_tileWidth;
        [SerializeField] bool m_lockYToCamera = true;
        GameObject m_gameObject;
        public float ParallaxAmount;
        LinkedList<SpriteRenderer> m_active = new();

        Vector2 LeftEdge => new(m_active.First.Value.transform.position.x - m_tileWidth * 0.5f, 0);
        Vector2 RightEdge => new(m_active.Last.Value.transform.position.x + m_tileWidth * 0.5f, 0);

        public void Initialize(float anchorY, float cameraX, float cameraHalfWidth)
        {
            if (m_tileWidth <= 0)
            {
                Debug.LogError("Parallax Object failed to initialize: m_tileWidth must be greater than 0!");
                return;
            }

            m_parallaxPool.Initialize();
            Spawn(cameraX - cameraHalfWidth - m_tileWidth * 0.5f, anchorY);

            while (RightEdge.x < cameraX + cameraHalfWidth + m_tileWidth)
                SpawnRight(anchorY);
        }

        public void ApplyParallax(Vector2 delta, float cameraX, float cameraY, float cameraHalfWidth)
        {
            if (m_tileWidth <= 0) return;

            foreach (SpriteRenderer sr in m_active)
            {
                Vector3 pos = sr.transform.position;
                pos.x -= delta.x * (1f - ParallaxAmount);
                pos.y = m_lockYToCamera ? cameraY : pos.y - delta.y * (1f - ParallaxAmount);
                sr.transform.position = pos;
            }

            while (m_active.First.Value.transform.position.x + m_tileWidth * 0.5f < cameraX - cameraHalfWidth)
            {
                SpriteRenderer tile = m_active.First.Value;
                m_active.RemoveFirst();
                float newX = m_active.Last.Value.transform.position.x + m_tileWidth;
                float newY = m_lockYToCamera ? cameraY : tile.transform.position.y;
                tile.transform.position = new(newX, newY, tile.transform.position.z);
                m_active.AddLast(tile);
            }

            while (m_active.Last.Value.transform.position.x - m_tileWidth * 0.5f > cameraX + cameraHalfWidth)
            {
                SpriteRenderer tile = m_active.Last.Value;
                m_active.RemoveLast();
                float newX = m_active.First.Value.transform.position.x - m_tileWidth;
                float newY = m_lockYToCamera ? cameraY : tile.transform.position.y;
                tile.transform.position = new(newX, newY, tile.transform.position.z);
                m_active.AddFirst(tile);
            }
        }

        SpriteRenderer Spawn(float x, float y)
        {
            SpriteRenderer sr = m_parallaxPool.Get();
            sr.transform.position = new(x, y, sr.transform.position.z);
            m_active.AddLast(sr);
            return sr;
        }

        void SpawnRight(float y) => Spawn(RightEdge.x + m_tileWidth * 0.5f, y);
        void SpawnLeft(float y) => Spawn(LeftEdge.x - m_tileWidth * 0.5f, y);
    }

    void Start()
    {
        m_previousCameraPosition = m_camera.transform.position;
        m_cameraHalfWidth = m_camera.orthographicSize * m_camera.aspect;

        float anchorY = transform.position.y;
        foreach (ParallaxObject obj in m_parallaxObjects)
            obj.Initialize(anchorY, m_camera.transform.position.x, m_cameraHalfWidth);
    }

    void LateUpdate()
    {
        Vector3 delta = m_camera.transform.position - m_previousCameraPosition;
        float cameraX = m_camera.transform.position.x;
        float cameraY = m_camera.transform.position.y;

        foreach (ParallaxObject obj in m_parallaxObjects)
            obj.ApplyParallax(delta, cameraX, cameraY, m_cameraHalfWidth);

        m_previousCameraPosition = m_camera.transform.position;
    }
}