using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Manages a queue of <see cref="Coroutine"/> that are executed in sequence.
/// </summary>
public class RoutineQueue
{
    readonly Queue<IEnumerator> m_routineQueue = new();
    Coroutine m_activeRoutine;
    MonoBehaviour m_monoBehaviour;
    /// <summary>
    /// Initializes the <see cref="RoutineQueue"/>
    /// </summary>
    /// <param name="monoBehaviour">The <see cref="MonoBehaviour"/> that the <see cref="Coroutine"/> will execute on</param>
    public void Initialize(MonoBehaviour monoBehaviour)
    {
        m_monoBehaviour = monoBehaviour;
    }
    /// <summary>
    /// Queues a <see cref="Coroutine"/> to be executed and starts the queue if it's not already running.
    /// </summary>
    /// <param name="routineToQueue">The new <see cref="Coroutine"/> to queue</param>
    public void QueueRoutine(IEnumerator routineToQueue)
    {
        m_routineQueue.Enqueue(routineToQueue);
        m_activeRoutine ??= m_monoBehaviour.StartCoroutine(HandleQueue());
    }
    IEnumerator HandleQueue()
    {
        while (m_routineQueue.Count > 0)
        {
            yield return m_routineQueue.Dequeue();
        }
        m_activeRoutine = null;
    }
}