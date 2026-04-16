using MatrixUtils.Attributes;
using UnityEngine;
using UnityEngine.U2D;
[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField, RequiredField] SpriteShapeController m_shapeController;
    [Range(3, 100), SerializeField] int m_levelPoints;
    [SerializeField] float m_xScale;
    [SerializeField] float m_yScale;
    [SerializeField] float m_smoothness;
    [SerializeField] float m_bottom;
    void OnValidate()
    {
        Vector2 position = transform.position;
        Vector2 lastPoint = new();
        m_shapeController.spline.Clear();
        for (int i = 0; i < m_levelPoints; i++)
        {
            lastPoint =  position + new Vector2(i * m_xScale, Mathf.PerlinNoise(i * m_xScale, i) * m_yScale);
            m_shapeController.spline.InsertPointAt(i, lastPoint);
            if (i <= 0 || i >= m_levelPoints) continue;
            m_shapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
            m_shapeController.spline.SetLeftTangent(i, Vector2.left * m_xScale * m_smoothness);
            m_shapeController.spline.SetRightTangent(i, Vector2.right * m_xScale * m_smoothness);
        }
        m_shapeController.spline.InsertPointAt(m_levelPoints, new Vector2(lastPoint.x, transform.position.y - m_bottom));
        m_shapeController.spline.InsertPointAt(m_levelPoints + 1, new Vector2(transform.position.x, transform.position.y - m_bottom));
    }
}
