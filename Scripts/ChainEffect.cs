using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainEffect : MonoBehaviour
{
    [SerializeField] public int maxChainTimes;
    [SerializeField] public float tooShortToChainDist = 0.1f;

    private PlayerUpgradesManager upgradeManager;
    private InstancedPrefabManager instancedPrefabManager;

    private void Start()
    {
        upgradeManager = GameObject.Find("char").GetComponent<PlayerUpgradesManager>();
        instancedPrefabManager = GameObject.Find("InstancedPrefabManager").GetComponent<InstancedPrefabManager>();
    }

    public void StartChainEffect()
    {
        if (maxChainTimes <= 0) return;

        if (upgradeManager.thorHammer || Random.Range(0f, 1f) < upgradeManager.chainLightningOdds)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyBody");
            GameObject nearestEnemy = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (GameObject enemy in enemies)
            {
                float enemyDist = Vector3.Distance(enemy.transform.position, transform.position);
                if (enemyDist < nearestDistance)
                {
                    if (enemyDist < tooShortToChainDist) continue;

                    nearestDistance = enemyDist;
                    nearestEnemy = enemy;
                }
            }

            GameObject chainSpawn = Instantiate(instancedPrefabManager.lightningPrefab, transform.position, transform.rotation, null);
            ChainEffect spawnScript = chainSpawn.GetComponent<ChainEffect>();
            ProjectileController chainProjController = chainSpawn.GetComponent<ProjectileController>();

            if (upgradeManager.highVoltage > 0)
            {
                chainProjController.canPierce = true;
                chainProjController.destroyAfterHits = upgradeManager.highVoltage;
            }
            if (nearestEnemy) chainSpawn.transform.right = nearestEnemy.transform.position - transform.position;

            chainSpawn.GetComponent<Damage>().damage += upgradeManager.chainLightningDamage;
            spawnScript.maxChainTimes = maxChainTimes - 1;
        }
    }
}
