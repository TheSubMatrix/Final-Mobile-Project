using UnityEngine.SceneManagement;
/// <summary>
/// An interface representing an object that handles transitioning between <see cref="Scene"/>
/// </summary>
public interface ISceneTransitionManager
{
    /// <summary>
    /// Whether we are currently transitioning to another <see cref="Scene"/>
    /// </summary>
    public bool IsTransitioning { get; }
    /// <summary>
    /// Transitions to a <see cref="Scene"/> with a given name
    /// </summary>
    /// <param name="sceneName">The name of the <see cref="Scene"/> to transition to</param>
    public void TransitionToScene(string sceneName);
    /// <summary>
    /// Quits the game after fading out the current <see cref="Scene"/>
    /// </summary>
    public void QuitGame();
}
