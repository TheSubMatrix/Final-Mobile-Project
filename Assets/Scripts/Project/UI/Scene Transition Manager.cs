using System.Collections;
using JetBrains.Annotations;
using MatrixUtils.DependencyInjection;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Handles transitioning from one <see cref="Scene"/> to another asynchronously with a nice fade animation using a <see cref="CanvasGroup"/>
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SceneTransitionManager : PersistentService<ISceneTransitionManager>, ISceneTransitionManager
{
    [Provide, UsedImplicitly] ISceneTransitionManager GetSceneTransitionManager() => this;
    /// <inheritdoc/>
    /// 
    
    public bool IsTransitioning { get; private set;}
    CanvasGroup m_canvasGroup;

    void Awake()
    {
        m_canvasGroup = GetComponent<CanvasGroup>();
        m_canvasGroup.alpha = 0;
    }
    /// <inheritdoc/>
    public void TransitionToScene(string sceneName)
    {
        if(IsTransitioning) return;
        StartCoroutine(TransitionToSceneAsync(sceneName));
    }

    public void QuitGame()
    {
        if(IsTransitioning) return;
        StartCoroutine(FadeAndQuit());
    }
    
    IEnumerator TransitionToSceneAsync(string sceneName)
    {
        IsTransitioning = true;
        m_canvasGroup.blocksRaycasts = true;
        m_canvasGroup.interactable = true;
        yield return m_canvasGroup.FadeToOpacity(1, 0.5f);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return m_canvasGroup.FadeToOpacity(0, 0.5f);
        m_canvasGroup.blocksRaycasts = false;
        m_canvasGroup.interactable = false;
        IsTransitioning = false;
    }

    IEnumerator FadeAndQuit()
    {
        IsTransitioning = true;
        m_canvasGroup.blocksRaycasts = true;
        m_canvasGroup.interactable = true;
        yield return m_canvasGroup.FadeToOpacity(1, 0.5f);
        Application.Quit();
    }
}