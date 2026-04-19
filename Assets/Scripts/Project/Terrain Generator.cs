using UnityEngine;
using UnityEngine.U2D;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteShapeController))]
public class TerrainSegmentGenerator : MonoBehaviour
{
    SpriteShapeController m_shapeController;
    [Range(3, 100), SerializeField] int m_terrainResolution = 50;
    [SerializeField] Vector2 m_minimums = new(16, 6);
    [SerializeField] Vector2 m_maximums = new(34, 12);
    [SerializeField] float m_floorDistance = 10;
    [SerializeField] float m_maxHeight = 10;
    [SerializeField] float m_minHeight;
    [SerializeField] float m_tangentScale = 1/3f;

    void OnEnable()
    {
        m_shapeController = GetComponent<SpriteShapeController>();
    }
    
    void OnValidate()
    {
        GenerateTerrain();
    }
    
    TerrainConnectionPoint GenerateTerrain(TerrainConnectionPoint? leftConnectionPoint = null)
    {
        if (m_shapeController == null) return leftConnectionPoint ?? default;
        m_shapeController.spline.Clear();
        Vector2[] points = GenerateTerrainKeyPoints(leftConnectionPoint);
        for (int i = 0; i < m_terrainResolution; i++)
        {
            m_shapeController.spline.InsertPointAt(i, points[i]);
            if (i <= 0 || i >= m_terrainResolution - 1) continue;
            Vector2 tangent = ComputeCatmullRomTangent(points[i - 1], points[i + 1]);
            m_shapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            m_shapeController.spline.SetLeftTangent(i,  -tangent);
            m_shapeController.spline.SetRightTangent(i,  tangent);
        }
        Vector2 lastPoint = points[m_terrainResolution - 1];
        m_shapeController.spline.InsertPointAt(m_terrainResolution,new Vector2(lastPoint.x, transform.position.y - m_floorDistance));
        m_shapeController.spline.InsertPointAt(m_terrainResolution + 1, new Vector2(points[0].x, transform.position.y - m_floorDistance));
        Vector2 phantomNext = lastPoint + (lastPoint - points[m_terrainResolution - 2]);
        Vector2 edgeTangent = ComputeCatmullRomTangent(points[m_terrainResolution - 2], phantomNext);
        return new(lastPoint, edgeTangent);
    }

    Vector2[] GenerateTerrainKeyPoints(TerrainConnectionPoint? startingEdge = null)
    {
        Vector2[] points = new Vector2[m_terrainResolution];
        float directionalSign = -1f;
        float currentYPosition = transform.position.y;
        points[0] = startingEdge?.Position ?? new Vector2(transform.position.x, transform.position.y);
        for (int i = 1; i < m_terrainResolution; i++)
        {
            Vector2 steps = new(Random.Range(m_minimums.x, m_maximums.x), Random.Range(m_minimums.y, m_maximums.y));
            points[i].x = points[i - 1].x + steps.x;
            points[i].y = Mathf.Clamp(currentYPosition + steps.y * directionalSign, transform.position.y + m_minHeight, transform.position.y + m_maxHeight);
            currentYPosition = points[i].y;
            directionalSign *= -1f;
        }
        return points;
    }
    
    Vector2 ComputeCatmullRomTangent(Vector2 previous, Vector2 next)
    {
        float slope = (next.y - previous.y) / (next.x - previous.x);
        float xStep = (next.x - previous.x) / 2f;
        return new(xStep * m_tangentScale, slope * m_tangentScale);
    }
}

[System.Serializable]
public struct TerrainConnectionPoint
{
    [field:SerializeField] public Vector2 Position { get; private set;}
    [field:SerializeField] public Vector2 Tangent { get; private set;}
    public TerrainConnectionPoint(Vector2 position, Vector2 tangent)
    {
        Position = position;
        Tangent = tangent;
    }
}