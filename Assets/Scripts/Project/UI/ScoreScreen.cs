using System;
using JetBrains.Annotations;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using TMPro;
using UnityEngine;

public class ScoreScreen : MonoBehaviour
{
    [SerializeField] TextDisplayData m_highScoreData;
    [SerializeField] TextDisplayData m_latestScoreData;
    IScoreReader m_scoreReader;
    [UsedImplicitly, Inject]
    void OnScoreReaderInjected(IScoreReader scoreReader)
    {
        m_scoreReader = scoreReader;
        UpdateHighScore(scoreReader.GetHighScore());
        UpdateLatestScore(scoreReader.GetLatestScore());
        scoreReader.OnHighScoreUpdated.AddListener(UpdateHighScore);
        scoreReader.OnLatestScoreUpdated.AddListener(UpdateLatestScore);
    }
    void UpdateHighScore(ScoreData data) => m_highScoreData.UpdateText(data.Total);
    void UpdateLatestScore(ScoreData data) => m_latestScoreData.UpdateText(data.Total);
    
    void OnDestroy()
    {
        m_scoreReader.OnHighScoreUpdated.RemoveListener(UpdateHighScore);
        m_scoreReader.OnLatestScoreUpdated.RemoveListener(UpdateLatestScore);
    }
    [Serializable]
    struct TextDisplayData
    {
        [SerializeField, RequiredField] TMP_Text m_text;
        [SerializeField] string Prefix;
        public void UpdateText(uint value) => m_text.text = $"{Prefix}{value}";
    }
}
