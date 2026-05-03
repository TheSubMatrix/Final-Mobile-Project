using MatrixUtils.DependencyInjection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
/// <summary>
/// A proxy <see cref="MonoBehaviour"/> for interacting with the <see cref="ISceneTransitionManager"/> through <see cref="UnityEvent"/>
/// </summary>
public class SceneTransitionerProxy : MonoBehaviour
{
    [Inject] ISceneTransitionManager m_sceneTransitionManager;
    /// <summary>
    /// Calls <see cref="ISceneTransitionManager.TransitionToScene"/> on the injected <see cref="ISceneTransitionManager"/>
    /// </summary>
    /// <param name="sceneName">The <see cref="Scene"/> to transition to</param>
    public void TransitionToScene(string sceneName)
    {
        m_sceneTransitionManager.TransitionToScene(sceneName);
    }

    public void QuitGame()
    {
        m_sceneTransitionManager.QuitGame();
    }
    /// <summary>
    /// Gets the value of <see cref="ISceneTransitionManager.IsTransitioning"/> from the injected <see cref="ISceneTransitionManager"/>
    /// </summary>
    public bool IsTransitioning => m_sceneTransitionManager.IsTransitioning;
}
