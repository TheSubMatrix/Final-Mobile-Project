using System.Collections;
using JetBrains.Annotations;
using MatrixUtils.Attributes;
using MatrixUtils.DependencyInjection;
using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour, IDependencyProvider
{
    static readonly WaitForSeconds s_waitForSeconds08 = new(0.8f);

    [Provide, UsedImplicitly] IScoreManager GetScoreManager() => m_scoreManager;
    [SerializeField, RequiredField] TMP_Text m_scoreText;
    [SerializeField, RequiredField] TMP_Text m_bonusScoreText;
    [SerializeField] string m_scorePrefix;
    [ClassSelector, SerializeReference] IScoreManager m_scoreManager;

    void OnEnable()
    {
        m_scoreManager.Score.AddListener(OnScoreChanged);
        m_scoreManager.OnBonusEarned.AddListener(OnBonusEarned);
        m_bonusScoreText.text = "";
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
        StartCoroutine(BonusDisplayCoroutine());
    }
    IEnumerator BonusDisplayCoroutine()
    {
        m_bonusScoreText.gameObject.SetActive(true);
        yield return s_waitForSeconds08;
        m_bonusScoreText.gameObject.SetActive(false);
    }
}