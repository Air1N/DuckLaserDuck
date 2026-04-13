using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthManager : MonoBehaviour
{
        public HitManager hitManager;
        float lastHealth = 0;

        public AudioSource audioSource;
        [SerializeField] private Image healthBar;
        [SerializeField] private TextMeshProUGUI healthText;

        void FixedUpdate()
        {
                if (hitManager.health != lastHealth)
                {
                        if (hitManager.health < lastHealth)
                        {
                                PlayHurtSound();
                                StartCoroutine(SlowTimeBriefly());
                        }

                        healthBar.fillAmount = hitManager.health / hitManager.maxHealth;

                        if (healthText != null)
                                healthText.text = $"{Mathf.CeilToInt(hitManager.health)}";
                }

                lastHealth = hitManager.health;
        }

        private IEnumerator SlowTimeBriefly()
        {
                Time.timeScale = 0.01f;
                yield return new WaitForSecondsRealtime(0.25f);
                Time.timeScale = 1f;
        }

        private void PlayHurtSound()
        {
                audioSource.Play();
        }
}