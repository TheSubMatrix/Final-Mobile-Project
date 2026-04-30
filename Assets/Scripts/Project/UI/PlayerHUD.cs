using System.Collections;
using JetBrains.Annotations;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using MatrixUtils.Extensions;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour, IDependencyProvider
{
    static readonly WaitForSeconds s_waitForSeconds08 = new(0.8f);

    [Provide, UsedImplicitly] IScoreManager GetScoreManager() => m_scoreManager;
    [SerializeField, RequiredField] TMP_Text m_scoreText;
    [SerializeField, RequiredField] TMP_Text m_bonusScoreText;
    [SerializeField, RequiredField] CanvasGroup m_bonusCanvasGroup;
    [SerializeField] string m_scorePrefix;
    [ClassSelector, SerializeReference] IScoreManager m_scoreManager;

    RoutineQueue m_bonusRoutineQueue;
    void OnEnable()
    {
        m_scoreManager.Score.AddListener(OnScoreChanged);
        m_scoreManager.OnBonusEarned.AddListener(OnBonusEarned);
        m_bonusScoreText.text = "";
        m_bonusRoutineQueue = new(this);
        m_bonusCanvasGroup.alpha = 0;
    }

    void OnDisable()
    {
        m_scoreManager.Score.RemoveListener(OnScoreChanged);
        m_scoreManager.OnBonusEarned.RemoveListener(OnBonusEarned);
    }
    void OnScoreChanged(uint score)
    {
        m_scoreText.text = m_scorePrefix + score;
    }

    void OnBonusEarned(uint bonus)
    {
        m_bonusScoreText.text = $"+{bonus}";
        Debug.Log($"Bonus Earned: {bonus} points!");
        m_bonusRoutineQueue.QueueRoutine(BonusDisplayCoroutine());
    }
    IEnumerator BonusDisplayCoroutine()
    {
        yield return m_bonusCanvasGroup.FadeToOpacity(1, 0.2f);
        yield return s_waitForSeconds08;
        yield return m_bonusCanvasGroup.FadeToOpacity(0, 0.2f);
    }
}