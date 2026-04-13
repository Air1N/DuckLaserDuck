using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateInventoryTitlesOnOpen : MonoBehaviour
{
    [SerializeField] private PlayerInventoryManager inventoryManager;
    void OnEnable()
    {
        inventoryManager.UpdateTitleInformation();
    }
}
