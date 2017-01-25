using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleLibrary
{
    [System.Serializable]
    public class PoolInfo
    {
        public int SelectedPoolIndex = 0;

        public void NameIndexChanged(int from, int to)
        {
            if (from == SelectedPoolIndex)
                SelectedPoolIndex = to;
        }

        public PooledBehaviour Spawn()
        {
            return SimplePoolManager.Spawn(this, Vector3.zero, Quaternion.Euler(Vector3.zero), Vector3.one);
        }

        public PooledBehaviour Spawn(Vector3 localPosition)
        {
            return SimplePoolManager.Spawn(this, localPosition, Quaternion.Euler(Vector3.zero), Vector3.one);
        }

        public PooledBehaviour Spawn(Vector3 localPosition, Quaternion localRotation)
        {
            return SimplePoolManager.Spawn(this, localPosition, localRotation, Vector3.one);
        }

        public PooledBehaviour Spawn(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            return SimplePoolManager.Spawn(this, localPosition, localRotation, localScale);
        }
    }

    public class SimplePool : MonoBehaviour
    {
        public struct DestroyInfo
        {
            public SimplePool pool;
        }

        public struct InstantiateInfo
        {
            public SimplePool pool;
        }

        [SerializeField]
        public string PoolName = "Unknown";

        public bool EnablePooling = true;
        public GameObject ThisPrefab = null;
        public int InstantiateAmountOnStart = 0;

        public bool DoNotDestroyOnLoad = false;

        public bool UseThisAsParent = true;
        public Transform Parent = null;
        public bool ReparentOnDespawn = true;

        public bool UseOnSpawnMessage = true;
        public bool UseOnDespawnMessage = true;

        public bool ActivateOnSpawn = true;
        public bool DeactivateOnDespawn = true;

        public bool MoveOnDespawn = false;
        public bool UseTransformPosition = false;
        public Vector3 DeactivatePosition = Vector3.zero;
        public Transform TargetPositionTransform = null;

        public bool DestroyUnusedObjects = false;
        public bool Intelligence = false;

        public int WantedFreeObjects = 10;

        public Queue<PooledBehaviour> availableObjects = new Queue<PooledBehaviour>();
        public List<PooledBehaviour> spawnedObjects = new List<PooledBehaviour>();

        //Editor
        public int ObjectCount = 0;

        public int AvailableObjects = 0;
        public int SpawnedObjects = 0;
        public int TabState = 0;
        public int RunningDestroyWorker = 0;
        public int RunningInstantiateWorker = 0;

        protected int UpdateFrame = 0;

        public bool EnableProfiler = false;
        public int UsedTimesCount = 1000;
        protected Queue<float> LastUsedTimes = new Queue<float>();
        protected int ProfilingFrame = 0;

        public float StartTime = 0f;
        public float CurrentUsedTime = 0f;
        public float MinUsedTime = 0f;
        public float MaxUsedTime = 0f;
        public float AverageUsedTime = 0f;

        private void Update()
        {
            UpdateFrame = UpdateFrame == 1 ? 0 : 1;
        }

        protected void ProfilingStart()
        {
            if (!EnableProfiler)
                return;
            StartTime = Time.realtimeSinceStartup;
        }

        protected void ProfilingEnd()
        {
            if (!EnableProfiler)
                return;
            if (ProfilingFrame != UpdateFrame)
            {
                ProfilingFrame = UpdateFrame;

                if (LastUsedTimes.Count == UsedTimesCount)
                    LastUsedTimes.Dequeue();
                LastUsedTimes.Enqueue(CurrentUsedTime);

                MinUsedTime = LastUsedTimes.Min();
                MaxUsedTime = LastUsedTimes.Max();
                AverageUsedTime = LastUsedTimes.Average();

                CurrentUsedTime = 0;
            }
            CurrentUsedTime += TimeDiff(StartTime);
        }

        public virtual bool HasSpawned(PooledBehaviour script)
        {
            if (!spawnedObjects.Contains(script))
            {
                for (int i = 0; i < spawnedObjects.Count; i++)
                {
                    if (spawnedObjects[i] == null)
                        continue;
                    if (spawnedObjects[i].gameObject == script.gameObject)
                        return true;
                }
                return false;
            }
            return true;
        }

        public virtual bool Uses(GameObject prefab)
        {
            return prefab == ThisPrefab;
        }

        public virtual PooledBehaviour Spawn()
        {
            ProfilingStart();
            if (availableObjects.Count == 0)
                InstantiateNewObject();
            if (availableObjects.Count == 0)
            {
                Debug.LogWarning("No new objects", gameObject);
                ProfilingEnd();
                return null;
            }

            PooledBehaviour script = availableObjects.Dequeue();
            AvailableObjects = availableObjects.Count;

            spawnedObjects.Add(script);
            SpawnedObjects = spawnedObjects.Count;

            RegisterAction(Action.Spawn);
            ProfilingEnd();
            return script;
        }

        public virtual PooledBehaviour Spawn(Vector3 position)
        {
            var script = Spawn();
            if (script)
                script.transform.localPosition = position;
            return script;
        }

        public virtual PooledBehaviour Spawn(Vector3 position, Quaternion rotation)
        {
            var script = Spawn();
            if (script)
            {
                script.transform.localPosition = position;
                script.transform.localRotation = rotation;
            }
            return script;
        }

        public virtual PooledBehaviour Spawn(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var script = Spawn();
            if (script)
            {
                script.transform.localPosition = position;
                script.transform.localRotation = rotation;
                script.transform.localScale = scale;
            }
            return script;
        }

        public virtual void AfterSpawn(PooledBehaviour go)
        {
            if (!go)
                return;
            if (UseOnSpawnMessage)
                go.OnSpawn();
        }

        protected virtual void DespawnObject(PooledBehaviour script, bool countAction = true)
        {
            if (UseOnDespawnMessage)
                script.OnDespawn();

            if (MoveOnDespawn)
            {
                if (UseTransformPosition && TargetPositionTransform != null)
                    script.transform.position = TargetPositionTransform.position;
                else
                    script.transform.position = DeactivatePosition;
            }
            if (ReparentOnDespawn)
                script.transform.SetParent(Parent);

            availableObjects.Enqueue(script);
            AvailableObjects = availableObjects.Count;

            if (countAction)
            {
                RegisterAction(Action.Despawn);
            }
        }

        public virtual bool TryDespawnObject(PooledBehaviour script, bool countAction = true)
        {
            if (!HasSpawned(script))
                return false;
            ProfilingStart();

            spawnedObjects.Remove(script);
            SpawnedObjects = spawnedObjects.Count;

            if (!EnablePooling)
            {
                DestroyObject(script);
                return true;
            }
            DespawnObject(script);
            ProfilingEnd();
            return true;
        }

        public virtual void InstantiateNewObject()
        {
            if (ThisPrefab == null)
            {
                Debug.LogWarning(string.Format("Prefab of pool is null"), gameObject);
                return;
            }
            GameObject go = GameObject.Instantiate(ThisPrefab) as GameObject;
            go.transform.SetParent(Parent);

            var script = go.GetComponent<PooledBehaviour>();
            if (script == null)
                script = go.AddComponent<PooledBehaviour>();
            script.Prefab = ThisPrefab;

            DespawnObject(script, false);
            ObjectCount++;

            //decrease worker count, but do not go under 0 (should never happen, but safety)
            RunningInstantiateWorker = Mathf.Max(RunningInstantiateWorker - 1, 0);
        }

        public virtual void DestroyOneObject()
        {
            //decrease worker count, but do not go under 0 (should never happen, but safety)
            RunningDestroyWorker = Mathf.Max(RunningDestroyWorker - 1, 0);

            //We have no free objects, we can not destroy anything
            if (availableObjects.Count == 0)
            {
                return;
            }
            DestroyObject(availableObjects.Dequeue());
            AvailableObjects = availableObjects.Count;
        }

        protected virtual void Awake()
        {
            Init();
        }

        protected virtual void Init()
        {
            ObjectCount = 0;
            AvailableObjects = 0;
            SpawnedObjects = 0;
            RunningDestroyWorker = 0;
            RunningInstantiateWorker = 0;
            intValue = 0;

            availableObjects.Clear();
            spawnedObjects.Clear();
            lastActions.Clear();
            LastUsedTimes.Clear();

            if (DoNotDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
            if (UseThisAsParent)
                Parent = transform;

            if (Parent == null)
                Parent = SimplePoolManager.Instance.transform;

            if (ThisPrefab != null)
            {
                //Prefill
                StartInstantiateWorker(InstantiateAmountOnStart);
            }
        }

        private float TimeDiff(float last)
        {
            return Time.realtimeSinceStartup - last;
        }

        protected void StartDestroyWorker()
        {
            //We can't destroy anything, when everything is spawned
            if (availableObjects.Count == 0)
                return;
            SimplePoolManager.Instance.AddDestroyInfo(new DestroyInfo() { pool = this });
            RunningDestroyWorker++;
        }

        protected void StartDestroyWorker(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                StartDestroyWorker();
            }
        }

        protected void StartInstantiateWorker()
        {
            SimplePoolManager.Instance.AddInstantiateInfo(new InstantiateInfo() { pool = this });
            RunningInstantiateWorker++;
        }

        protected void StartInstantiateWorker(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                StartInstantiateWorker();
            }
        }

        #region Intelligence

        public enum Action
        {
            Spawn = 1,
            Despawn = -1
        }

        protected float IntelligenceValue
        {
            get
            {
                return (intValue + ActionErrorCorrection) / (float)actionCount;
            }
        }

        public int ExpectedFreeObjectsCount
        {
            get
            {
                return availableObjects.Count + RunningInstantiateWorker - RunningDestroyWorker;
            }
        }

        public int ExpectedFreeObjectDifference
        {
            get
            {
                return (int)(IntelligenceValue * WantedIntelligenceObjectDifference);
            }
        }

        public int WantedIntelligenceObjectDifference = 10;

        public int ActionErrorCorrection = 2;

        public int intValue = 0;
        public int actionCount = 50;

        public Queue<int> lastActions = new Queue<int>();

        public void RegisterAction(Action action)
        {
            if (!EnablePooling)
                return;

            //Do not use Intelligence, ok, just try to stay between min and max free object count
            if (!Intelligence)
            {
                if (ExpectedFreeObjectsCount < WantedFreeObjects)
                {
                    StartInstantiateWorker(WantedFreeObjects - ExpectedFreeObjectsCount);
                }
                else if (ExpectedFreeObjectsCount > WantedFreeObjects)
                {
                    if (DestroyUnusedObjects) StartDestroyWorker(ExpectedFreeObjectsCount - WantedFreeObjects);
                }
                return;
            }

            if (lastActions.Count == actionCount)
                intValue -= lastActions.Dequeue();
            intValue += (int)action;
            lastActions.Enqueue((int)action);

            int objectDifference = 0;
            if (ExpectedFreeObjectsCount < WantedFreeObjects - ActionErrorCorrection)
                objectDifference -= WantedFreeObjects - ExpectedFreeObjectsCount;
            if (ExpectedFreeObjectsCount > WantedFreeObjects + ActionErrorCorrection)
                objectDifference += ExpectedFreeObjectsCount - WantedFreeObjects;

            objectDifference -= ExpectedFreeObjectDifference;

            if (objectDifference < 0)
            {
                StartInstantiateWorker(-objectDifference);
            }
            else if (objectDifference > 0)
            {
                if (DestroyUnusedObjects) StartDestroyWorker(objectDifference);
            }
        }

        #endregion Intelligence
    }
}