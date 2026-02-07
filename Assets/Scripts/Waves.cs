using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waves : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount = 6;
        public float spawnInterval = 0.25f;
    }

    [Header("Waves")]
    public Wave[] waves;

    [Header("Spawning")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public float betweenWavesDelay = 1.5f;
    public float startDelay = 1.0f;

    [Header("State (read-only)")]
    public int currentWaveIndex = -1;
    public int aliveEnemies = 0;
    public bool runFinished = false;

    public event System.Action AllWavesCompleted;

    [Header("Upgrades (optional)")]
    public UpgradeUI upgradeUI;
    public UpgradeDatabase upgradeDb;
    public Player player;

    private Coroutine _runCo;

    private void Start()
    {
        
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogError("Waves: enemyPrefabs are not assigned.");
            enabled = false;
            return;
        }
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i] == null)
            {
                Debug.LogError($"Waves: enemyPrefabs[{i}] is NULL.");
                enabled = false;
                return;
            }
        }

        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Waves: spawnPoints are not assigned.");
            enabled = false;
            return;
        }
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (spawnPoints[i] == null)
            {
                Debug.LogError($"Waves: spawnPoints[{i}] is NULL.");
                enabled = false;
                return;
            }
        }

        
        if (waves == null || waves.Length == 0)
        {
            waves = new Wave[]
            {
                new Wave { enemyCount = 6,  spawnInterval = 0.25f },
                new Wave { enemyCount = 8,  spawnInterval = 0.22f },
                new Wave { enemyCount = 10, spawnInterval = 0.20f },
                new Wave { enemyCount = 12, spawnInterval = 0.18f },
            };
        }

        
        if (player == null) player = FindFirstObjectByType<Player>();
        if (upgradeUI == null) upgradeUI = FindFirstObjectByType<UpgradeUI>();
        if (upgradeDb == null) upgradeDb = FindFirstObjectByType<UpgradeDatabase>();

        _runCo = StartCoroutine(RunRoutine());
    }

    private IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < waves.Length; i++)
        {
            currentWaveIndex = i;

            if (i > 0)
                yield return new WaitForSeconds(betweenWavesDelay);

            
            yield return StartCoroutine(SpawnWaveRoutine(waves[i]));

            
            while (aliveEnemies > 0)
                yield return null;

            bool lastWave = (i == waves.Length - 1);

            
            if (!lastWave)
            {
                
                if (upgradeUI != null && upgradeDb != null && player != null)
                {
                    var pick = upgradeDb.GetRandom3();
                    if (pick != null && pick.Count >= 3)
                    {
                        var labels = new List<string> { pick[0].title, pick[1].title, pick[2].title };
                        var applies = new List<System.Action>
                        {
                            () => upgradeDb.Apply(player, pick[0]),
                            () => upgradeDb.Apply(player, pick[1]),
                            () => upgradeDb.Apply(player, pick[2]),
                        };

                        // Freeze game while choosing
                        Time.timeScale = 0f;
                        upgradeUI.Show(labels, applies);

                        while (!upgradeUI.Chosen)
                            yield return null;

                        Time.timeScale = 1f;
                    }
                }
            }
        }

        runFinished = true;
        Debug.Log("RUN FINISHED: all waves cleared.");
        AllWavesCompleted?.Invoke();
    }

    private IEnumerator SpawnWaveRoutine(Wave wave)
    {
        int count = Mathf.Max(0, wave.enemyCount);
        float interval = Mathf.Max(0f, wave.spawnInterval);

        for (int n = 0; n < count; n++)
        {
            SpawnOne();
            if (interval > 0f)
                yield return new WaitForSeconds(interval);
            else
                yield return null;
        }
    }

    private void SpawnOne()
    {
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
        GameObject go = Instantiate(prefab, sp.position, Quaternion.identity);

        aliveEnemies++;

       
        var tracker = go.AddComponent<EnemyDeathTracker>();
        tracker.manager = this;
    }

    public void NotifyEnemyDied()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
    }
}

public class EnemyDeathTracker : MonoBehaviour
{
    public Waves manager;

    private void OnDestroy()
    {
        if (manager != null && manager.gameObject != null)
            manager.NotifyEnemyDied();
    }
}
