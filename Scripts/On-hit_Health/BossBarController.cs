using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossBarController : MonoBehaviour
{
    [SerializeField]
    private HitManager hitManager;

    [SerializeField]
    private Slider slider;

    private void FixedUpdate()
    {
        if (hitManager.health / hitManager.maxHealth <= 0f)
        {
            Destroy(gameObject, 1f);
        }
        else
        {
            slider.value = hitManager.health / hitManager.maxHealth;
        }
    }
}
