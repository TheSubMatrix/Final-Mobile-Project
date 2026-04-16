using JetBrains.Annotations;
using UnityEngine;
using MatrixUtils.DependencyInjection;

/// <summary>
/// Base class for services that persist across scenes and integrate with dependency injection.
/// Handles duplicate instances automatically.
/// </summary>
/// <typeparam name="TInterface">The service interface this class provides (e.g., ISoundService)</typeparam>
[DefaultExecutionOrder(-999)]
public abstract class PersistentService<TInterface> : MonoBehaviour, IDependencyProvider
    where TInterface : class
{
    static TInterface s_instance;

    [Provide]
    [UsedImplicitly]
    public TInterface ProvideService()
    {
        TInterface thisAsInterface = this as TInterface;
        if (s_instance != null && s_instance != thisAsInterface)
        {
            Destroy(gameObject);
            return s_instance;
        }
        if (s_instance != null) return s_instance;
        s_instance = thisAsInterface;
        DontDestroyOnLoad(gameObject);
        transform.SetParent(null);
        InitializeService();
        return s_instance;
    }

    /// <summary>
    /// Override this for initialization logic.
    /// </summary>
    protected virtual void InitializeService() { }

    protected virtual void OnDestroy()
    {
        if (s_instance == (this as TInterface))
        {
            s_instance = null;
        }
    }
}