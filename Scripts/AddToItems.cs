using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToItems : MonoBehaviour
{
    [SerializeField] private PlayerUpgradesManager playerUpgradesManager;

    void Start()
    {
        playerUpgradesManager = GameObject.Find("char").GetComponent<PlayerUpgradesManager>();
        playerUpgradesManager.itemObjects.Add(gameObject);
    }
}
