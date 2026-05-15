using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool{
        public string tag;
        public GameObject prefab;
        public int size;
    }
    public static ObjectPooler Instance;
    private void Awake()=>Instance = this;

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    // This will hold all the pool parent objects
    private Transform poolContainer;

    void Start()
    {
        // Create a main container for all pools to keep the hierarchy clean
        poolContainer = new GameObject("ObjectPool_Container").transform;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            // Create a parent for this specific pool
            Transform poolParent = new GameObject(pool.tag + "_Pool").transform;
            poolParent.SetParent(poolContainer);

            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                // Set the parent right after creating it
                obj.transform.SetParent(poolParent);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, float fallDelay)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning($"Pool with tag '{tag}' is empty. Expanding the pool.");
            Pool pool = pools.Find(p => p.tag == tag);
            if (pool != null)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.transform.SetParent(poolContainer.Find(tag + "_Pool"));
                obj.SetActive(false);
                poolDictionary[tag].Enqueue(obj);
            }
            else
            {
                return null;
            }
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;

        Obstacle obstacle = objectToSpawn.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            obstacle.ActivateFall(fallDelay);
        }

        return objectToSpawn;
    }

    // Call this method to return an object to its pool
    public void ReturnToPool(string tag, GameObject objectToReturn)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist. Destroying object.");
            Destroy(objectToReturn);
            return;
        }

        objectToReturn.SetActive(false);
        poolDictionary[tag].Enqueue(objectToReturn);
    }
}
