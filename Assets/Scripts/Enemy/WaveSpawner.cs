// 按波次生成敌人并汇报战斗进度
using System;
using System.Collections;
using UnityEngine;

// 控制整局敌人波次流程
public class WaveSpawner : MonoBehaviour
{
    // 敌人生成点数组
    [SerializeField] private Transform[] spawnPoints;
    // 波次配置数据
    [SerializeField] private WaveDefinition waveDefinition;
    // 是否进入场景后自动开始
    [SerializeField] private bool startAutomatically;
    // 开始前延迟时间
    [SerializeField] private float initialDelay = 2f;

    // 当前波次协程
    private Coroutine waveRoutine;
    // 当前存活敌人数量
    private int aliveEnemies;

    // 波次切换时通知 HUD
    public event Action<int, int, string> WaveChanged;
    // 存活敌人数量变化时通知 HUD
    public event Action<int> AliveCountChanged;
    // 所有波次完成时通知 GameManager
    public event Action AllWavesCompleted;

    // 根据配置决定是否自动开波
    private void Start()
    {
        if (startAutomatically)
            Begin();
    }

    // 由构建器配置生成点和波次数据
    public void Configure(Transform[] points, WaveDefinition definition, float delay = 2f)
    {
        spawnPoints = points;
        waveDefinition = definition;
        initialDelay = Mathf.Max(0f, delay);
    }

    // 开始执行波次流程
    public void Begin()
    {
        if (waveRoutine != null || waveDefinition == null || waveDefinition.waves == null)
            return;

        waveRoutine = StartCoroutine(RunWaves());
    }

    // 按顺序运行所有波次
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

    // 生成一整波的所有敌人组
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

    // 从随机生成点生成敌人
    private void SpawnEnemy(EnemyController prefab)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
            return;

        Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        SpawnEnemy(prefab, point.position);
    }

    // 在指定位置生成敌人并统计存活数
    private void SpawnEnemy(EnemyController prefab, Vector3 position)
    {
        EnemyController enemy = Instantiate(prefab, position, Quaternion.identity);
        enemy.Died += HandleEnemyDied;
        aliveEnemies++;
        AliveCountChanged?.Invoke(aliveEnemies);
    }

    // 敌人死亡后更新存活数量
    private void HandleEnemyDied(EnemyController enemy)
    {
        if (enemy != null)
            enemy.Died -= HandleEnemyDied;

        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        AliveCountChanged?.Invoke(aliveEnemies);
    }
}
