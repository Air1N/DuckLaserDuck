using UnityEngine;

public class LineAttackSpawner : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private int projectileCount = 5;
    [SerializeField] private float spacingBetweenProjectiles = 0.5f;
    [SerializeField] private int ticksBetweenSpawns = 3;

    [Header("Timing")]
    [SerializeField] private float projectileLifetime = 1f;

    [Header("References")]
    [SerializeField] private Transform spawnOrigin;

    private int spawnIndex = 0;
    private int spawnTick = 0;
    private bool spawning = false;
    private Vector3 spawnDirection;
    private Vector3 lineDirection;

    public void StartLineAttack(Vector3 facingDirection)
    {
        spawnDirection = facingDirection.normalized;
        lineDirection = Vector3.Cross(spawnDirection, Vector3.forward).normalized;
        spawnIndex = 0;
        spawnTick = 0;
        spawning = true;
    }

    private void FixedUpdate()
    {
        if (!spawning) return;

        spawnTick++;

        if (spawnTick >= ticksBetweenSpawns)
        {
            spawnTick = 0;
            SpawnProjectile();
            spawnIndex++;

            if (spawnIndex >= projectileCount)
                spawning = false;
        }
    }

    private void SpawnProjectile()
    {
        float offset = spawnIndex * spacingBetweenProjectiles;
        Vector3 spawnPos = spawnOrigin.position + spawnDirection * offset;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        if (proj.TryGetComponent(out Rigidbody2D projRb))
            projRb.velocity = Vector2.zero;

        Destroy(proj, projectileLifetime);
    }
}