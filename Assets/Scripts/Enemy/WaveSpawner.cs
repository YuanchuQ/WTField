using System;
using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private WaveDefinition waveDefinition;
    [SerializeField] private bool startAutomatically;
    [SerializeField] private float initialDelay = 2f;

    private Coroutine waveRoutine;
    private int aliveEnemies;

    public event Action<int, int, string> WaveChanged;
    public event Action<int> AliveCountChanged;
    public event Action AllWavesCompleted;

    private void Start()
    {
        if (startAutomatically)
            Begin();
    }

    public void Begin()
    {
        if (waveRoutine != null || waveDefinition == null || waveDefinition.waves == null)
            return;

        waveRoutine = StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        aliveEnemies = 0;
        AliveCountChanged?.Invoke(aliveEnemies);

        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        for (int i = 0; i < waveDefinition.waves.Length; i++)
        {
            WaveDefinition.Wave wave = waveDefinition.waves[i];
            WaveChanged?.Invoke(i + 1, waveDefinition.waves.Length, wave.displayName);

            yield return SpawnWave(wave);
            yield return new WaitUntil(() => aliveEnemies <= 0);

            if (i < waveDefinition.waves.Length - 1)
                yield return new WaitForSeconds(waveDefinition.timeBetweenWaves);
        }

        AllWavesCompleted?.Invoke();
        waveRoutine = null;
    }

    private IEnumerator SpawnWave(WaveDefinition.Wave wave)
    {
        if (wave.groups == null)
            yield break;

        foreach (WaveDefinition.EnemyGroup group in wave.groups)
        {
            if (group.enemyPrefab == null)
                continue;

            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyPrefab);
                yield return new WaitForSeconds(group.interval);
            }
        }
    }

    private void SpawnEnemy(EnemyController prefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        SpawnEnemy(prefab, point.position);
    }

    private void SpawnEnemy(EnemyController prefab, Vector3 position)
    {
        EnemyController enemy = Instantiate(prefab, position, Quaternion.identity);
        enemy.Died += HandleEnemyDied;
        aliveEnemies++;
        AliveCountChanged?.Invoke(aliveEnemies);
    }

    private void HandleEnemyDied(EnemyController enemy)
    {
        if (enemy != null)
            enemy.Died -= HandleEnemyDied;

        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        AliveCountChanged?.Invoke(aliveEnemies);
    }
}
