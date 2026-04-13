using UnityEngine;

public class ExplosionController : MonoBehaviour
{
        public int duration;
        public PlayerUpgradesManager upgradeManager;
        [SerializeField] private GameObject explosionShrapnel;
        [SerializeField] private ExplodeWithShots explodeWithShots;

        void Start()
        {
                upgradeManager = FindFirstObjectByType<PlayerUpgradesManager>();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
                duration--;

                if (upgradeManager.shrapnel > 0)
                {
                        explodeWithShots.amountToSpawn = upgradeManager.shrapnel;
                        explosionShrapnel.SetActive(true);
                }
                if (duration <= 0) Destroy(gameObject);
        }
}
