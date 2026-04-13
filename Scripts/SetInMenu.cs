using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetInMenu : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    void OnEnable()
    {
        playerController.inMenu = true;
    }

    void OnDisable()
    {
        playerController.inMenu = false;
    }
}
