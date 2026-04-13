using System;
using UnityEngine;

/// <summary>
/// Attach to a watched GameObject. Fires <see cref="Destroyed"/> 
/// just before the object is destroyed so external listeners can react.
/// Added automatically by ConditionMonitor when using OnDestroyAnother.
/// </summary>
public class DestroyNotifier : MonoBehaviour
{
    public event Action<GameObject> Destroyed;

    void OnDestroy()
    {
        Destroyed?.Invoke(gameObject);
    }

    /// <summary>
    /// Returns existing notifier on target or adds one.
    /// </summary>
    public static DestroyNotifier Require(GameObject target)
    {
        if (target == null) return null;
        var notifier = target.GetComponent<DestroyNotifier>();
        if (notifier == null)
            notifier = target.AddComponent<DestroyNotifier>();
        return notifier;
    }
}