using UnityEngine;

public class Waves : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public int enemyCount = 6;
        public float spawnInterval = 0.25f; 
    }

    [Header("Waves (3–5 is enough)")]
    public Wave[] waves;

    [Header("Spawning")]
    public GameObject enemyPrefab;
    public Transform[] spawnPoints; 

    [Tooltip("Pause between waves (seconds).")]
    public float betweenWavesDelay = 1.5f;

    [Tooltip("Optional delay before first wave starts.")]
    public float startDelay = 1.0f;

    [Header("State (read-only)")]
    public int currentWaveIndex = -1;
    public int aliveEnemies = 0;
    public bool runFinished = false;

    private void Start()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("WaveManager: enemyPrefab is not assigned.");
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("WaveManager: spawnPoints are not assigned.");
            enabled = false;
            return;
        }

        if (waves == null || waves.Length == 0)
        {
            
            waves = new Wave[]
            {
                new Wave { enemyCount = 6, spawnInterval = 0.25f },
                new Wave { enemyCount = 8, spawnInterval = 0.22f },
                new Wave { enemyCount = 10, spawnInterval = 0.20f },
                new Wave { enemyCount = 12, spawnInterval = 0.18f },
            };
        }

        StartCoroutine(RunRoutine());
    }

    private System.Collections.IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(startDelay);

        for (int i = 0; i < waves.Length; i++)
        {
            currentWaveIndex = i;

            
            if (i > 0) yield return new WaitForSeconds(betweenWavesDelay);

            
            yield return StartCoroutine(SpawnWaveRoutine(waves[i]));

            
            while (aliveEnemies > 0)
                yield return null;
        }

        runFinished = true;
        Debug.Log("RUN FINISHED: all waves cleared.");
    }

    private System.Collections.IEnumerator SpawnWaveRoutine(Wave wave)
    {
        for (int n = 0; n < wave.enemyCount; n++)
        {
            SpawnOne();
            yield return new WaitForSeconds(wave.spawnInterval);
        }
    }

    private void SpawnOne()
    {
        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject go = Instantiate(enemyPrefab, sp.position, Quaternion.identity);

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
