using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledBehaviour : SimpleView
{
    [ReadOnly]
    public GameObject Prefab;

    public bool ToggleActiveOnSpawn = true;
    public bool ToggleActiveOnDeSpawn = true;

    private GameObject _gameObject;

    public new GameObject gameObject
    {
        get
        {
            if (_gameObject == null)
                _gameObject = base.gameObject;
            return _gameObject;
        }
    }

    public virtual void Despawn()
    {
        if (Application.isPlaying)
        {
            SimpleLibrary.SimplePoolManager.Despawn(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public virtual void OnSpawn()
    {
        if (ToggleActiveOnSpawn)
            gameObject.SetActive(true);
    }

    public virtual void OnDespawn()
    {
        if (ToggleActiveOnDeSpawn)
            gameObject.SetActive(false);
    }
}