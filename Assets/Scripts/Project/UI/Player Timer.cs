using MatrixUtils.Attributes;
using MatrixUtils.Timers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTimer : MonoBehaviour
{
    [SerializeField, RequiredField] TMP_Text m_timerText;
    [SerializeField] float m_time = 60;
    [SerializeField] UnityEvent<float> m_onNormalizedTimeUpdated = new();
    [SerializeField] UnityEvent m_onTimerComplete = new();
    CountdownTimer m_timer;
    void Start()
    {
        m_timer = new(m_time);
        m_timer.OnComplete(OnTimerComplete).OnTick(OnTimerTick);
        m_timer.Start();
    }
    void OnTimerTick()
    {
        m_onNormalizedTimeUpdated.Invoke(m_timer.Progress);
        m_timerText.text = ((m_timer.Progress) * m_time).ToString("0.0");
    }

    void OnTimerComplete()
    {
        m_timerText.text = "";
        m_timer.Dispose();
        m_onTimerComplete.Invoke();
    }
}
