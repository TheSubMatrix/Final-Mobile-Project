using MatrixUtils.Attributes;
using UnityEngine;
using UnityEngine.U2D;
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField, RequiredField] SpriteShapeController m_shapeController;
    [Range(3, 100), SerializeField] int m_terrainResolution = 50;
    [SerializeField] Vector2 m_minimums = new(16, 6);
    [SerializeField] Vector2 m_maximums = new(34, 12);
    [SerializeField] float m_floorDistance = 10;
    [SerializeField] float m_maxHeight = 10;
    [SerializeField] float m_minHeight = 0;
    [SerializeField] float m_tangentScale = 1/3f;
    [SerializeField] int m_seed = 0;
    void OnValidate()
    {
        if(m_shapeController == null) return;
        Random.InitState(m_seed);
        m_shapeController.spline.Clear();
        Vector2[] points = new Vector2[m_terrainResolution];
        float directionalSign = -1f;
        float currentYPosition = transform.position.y;
        points[0] = new(transform.position.x, transform.position.y);
        for (int i = 1; i < m_terrainResolution; i++)
        {
            Vector2 steps = new(Random.Range(m_minimums.x, m_maximums.x), Random.Range(m_minimums.y, m_maximums.y));
            points[i].x = points[i - 1].x + steps.x;
            points[i].y = Mathf.Clamp(currentYPosition + steps.y * directionalSign, transform.position.y + m_minHeight, transform.position.y + m_maxHeight);
            currentYPosition = points[i].y;
            directionalSign *= -1f;
        }

        for (int i = 0; i < m_terrainResolution; i++)
        {
            m_shapeController.spline.InsertPointAt(i, points[i]);
            if(i <= 0 || i >= m_terrainResolution - 1) continue;
            Vector2 nextPoint = points[i + 1];
            Vector2 previousPoint = points[i - 1];
            float slope = (nextPoint.y - previousPoint.y) / (nextPoint.x - previousPoint.x);
            float xStep = (nextPoint.x - previousPoint.x) / 2f;
            Vector2 tangent = new(xStep * m_tangentScale, slope * m_tangentScale);
            m_shapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            m_shapeController.spline.SetLeftTangent(i, -tangent);
            m_shapeController.spline.SetRightTangent(i, tangent);
        }
        int lastIndex = m_terrainResolution;
        Vector2 lastPoint = points[m_terrainResolution - 1];
        m_shapeController.spline.InsertPointAt(lastIndex,     new Vector2(lastPoint.x, transform.position.y - m_floorDistance));
        m_shapeController.spline.InsertPointAt(lastIndex + 1, new Vector2(transform.position.x, transform.position.y - m_floorDistance));
    }
}
