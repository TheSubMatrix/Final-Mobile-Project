using System.Collections;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [Inject] IScoreReader m_scoreManager;
    static readonly WaitForSeconds s_waitForSeconds08 = new(0.8f);
    [SerializeField, RequiredField] TMP_Text m_scoreText;
    [SerializeField, RequiredField] TMP_Text m_bonusScoreText;
    [SerializeField, RequiredField] CanvasGroup m_bonusCanvasGroup;
    [SerializeField] string m_scorePrefix;
    float m_lastBonusTotal;

    RoutineQueue m_bonusRoutineQueue;
    void OnEnable()
    {
        m_scoreManager.OnCurrentScoreUpdated.AddListener(OnScoreChanged);
        m_scoreManager.OnCurrentScoreUpdated.AddListener(OnBonusEarned);
        m_bonusScoreText.text = "";
        m_bonusRoutineQueue = new(this);
        m_bonusCanvasGroup.alpha = 0;
    }

    void OnDisable()
    {
        m_scoreManager.OnCurrentScoreUpdated.RemoveListener(OnScoreChanged);
        m_scoreManager.OnCurrentScoreUpdated.RemoveListener(OnBonusEarned);
    }
    void OnScoreChanged(ScoreData score)
    {
        m_scoreText.text = m_scorePrefix + score.Total;
    }

    void OnBonusEarned(ScoreData score)
    {
        if (score.BonusPoints <= m_lastBonusTotal) return;
        m_bonusScoreText.text = $"+{score.BonusPoints - m_lastBonusTotal}";
        m_lastBonusTotal = score.BonusPoints;
        m_bonusRoutineQueue.QueueRoutine(BonusDisplayCoroutine());
    }
    IEnumerator BonusDisplayCoroutine()
    {
        yield return m_bonusCanvasGroup.FadeToOpacity(1, 0.2f);
        yield return s_waitForSeconds08;
        yield return m_bonusCanvasGroup.FadeToOpacity(0, 0.2f);
    }
}