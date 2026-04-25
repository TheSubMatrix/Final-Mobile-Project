using JetBrains.Annotations;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour, IDependencyProvider
{
    [Provide, UsedImplicitly] IScoreManager GetScoreManager() => m_scoreManager;
    [SerializeField, RequiredField] TMP_Text m_scoreText;
    [SerializeField] string m_scorePrefix;
    [ClassSelector, SerializeReference] IScoreManager m_scoreManager;

    void OnEnable()
    {
        m_scoreManager.Score.AddListener(OnScoreChanged);
    }

    void OnDisable()
    {
        m_scoreManager.Score.RemoveListener(OnScoreChanged);
    }
    void OnScoreChanged(uint score)
    {
        m_scoreText.text = m_scorePrefix + score;
    }
}