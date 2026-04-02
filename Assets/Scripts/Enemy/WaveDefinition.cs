using System;
using UnityEngine;

[CreateAssetMenu(menuName = "WT Field/Wave Definition")]
public class WaveDefinition : ScriptableObject
{
    public float timeBetweenWaves = 3f;
    public Wave[] waves = Array.Empty<Wave>();

    [Serializable]
    public class Wave
    {
        public string displayName = "Wave";
        public EnemyGroup[] groups = Array.Empty<EnemyGroup>();
    }

    [Serializable]
    public class EnemyGroup
    {
        public EnemyController enemyPrefab;
        public int count = 5;
        public float interval = 0.6f;
    }
}
