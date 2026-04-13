using UnityEngine;

public class ProjectileDestroyable : MonoBehaviour
{

        public int health;
        public bool enemyDestroyable;
        public bool playerDestroyable;

        void OnTriggerEnter2D(Collider2D col)
        {
                if (enemyDestroyable && (col.gameObject.CompareTag("EnemyProjectile") || col.gameObject.CompareTag("EnemyProjectileAndObstacle")))
                {
                        health--;

                        if (health <= 0) Destroy(gameObject);
                }

                if (playerDestroyable && col.gameObject.CompareTag("AllyProjectile"))
                {
                        health--;

                        if (health <= 0) Destroy(gameObject);
                }
        }
}
