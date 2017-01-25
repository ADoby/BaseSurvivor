using SimpleLibrary;
using UnityEngine;

public static class GameObject_Extension
{
    public static PooledBehaviour Spawn(this GameObject prefab, bool autoCreatePool = true)
    {
        return SimpleLibrary.SimplePoolManager.Spawn(prefab, Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one, autoCreatePool);
    }

    public static PooledBehaviour Spawn(this GameObject prefab, Vector3 localPosition, bool autoCreatePool = true)
    {
        return SimpleLibrary.SimplePoolManager.Spawn(prefab, localPosition, Quaternion.Euler(Vector3.zero), prefab.transform.localScale, autoCreatePool);
    }

    public static PooledBehaviour Spawn(this GameObject prefab, Vector3 localPosition, Quaternion localRotation, bool autoCreatePool = true)
    {
        return SimpleLibrary.SimplePoolManager.Spawn(prefab, localPosition, localRotation, Vector3.one, autoCreatePool);
    }

    public static PooledBehaviour Spawn(this GameObject prefab, Vector3 localPosition, Quaternion localRotation, Vector3 localScale, bool autoCreatePool = true)
    {
        return SimpleLibrary.SimplePoolManager.Spawn(prefab, localPosition, localRotation, localScale, autoCreatePool);
    }

    public static void Despawn(this PooledBehaviour go)
    {
        SimpleLibrary.SimplePoolManager.Despawn(go);
    }
}