using System;
using MatrixUtils.Attributes;
using MatrixUtils.Timers;
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteAlways]
public class TimeOfDay : MonoBehaviour
{
    [SerializeField] TimeOfDaySettings[] m_timeOfDaySettings;
    CountdownTimer m_timer;
    [SerializeField, Range(0,1)]float m_time;
    public void UpdateVolumeWeights(float t)
    {
        foreach (TimeOfDaySettings settings in m_timeOfDaySettings)
                settings.Volume.weight = 0f;
        foreach (TimeOfDaySettings settings in m_timeOfDaySettings)
                settings.Volume.weight += settings.ComputeWeight(t);
    }

    void OnValidate()
    {
        if(Application.isPlaying) return;
        UpdateVolumeWeights(m_time);
    }

    [Serializable]
    struct TimeOfDaySettings
    {
        [SerializeField, Range(0, 1)] float m_startTime;
        [SerializeField, Range(0, 1)] float m_endTime;
        [SerializeField] float m_fadeInTime;
        [SerializeField] float m_fadeOutTime;
        [SerializeField, RequiredField] Volume m_volume;

        public Volume Volume => m_volume;

        public float ComputeWeight(float t)
        {
            if (t < m_startTime || t > m_endTime) return 0f;
            float localT = t - m_startTime;
            float span = m_endTime - m_startTime;
            if (localT < m_fadeInTime) return Mathf.Clamp01(localT / m_fadeInTime);
            return localT > span - m_fadeOutTime ? Mathf.Clamp01((span - localT) / m_fadeOutTime) : 1f;
        }
    }
}