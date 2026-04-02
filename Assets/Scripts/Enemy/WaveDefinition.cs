// 定义每一波敌人的生成配置
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "WT Field/Wave Definition")]
// 存储整局战斗的波次数据
public class WaveDefinition : ScriptableObject
{
    // 波次之间的等待时间
    public float timeBetweenWaves = 3f;
    // 所有波次配置
    public Wave[] waves = Array.Empty<Wave>();

    [Serializable]
    // 表示一波敌人配置
    public class Wave
    {
        // HUD 显示的波次名称
        public string displayName = "Wave";
        // 本波包含的敌人组
        public EnemyGroup[] groups = Array.Empty<EnemyGroup>();
    }

    [Serializable]
    // 表示同类敌人的生成组
    public class EnemyGroup
    {
        // 要生成的敌人预制体
        public EnemyController enemyPrefab;
        // 本组生成数量
        public int count = 5;
        // 每个敌人的生成间隔
        public float interval = 0.6f;
    }
}
