using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabOnDestroy : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    void OnDestroy()
    {
        Instantiate(prefab, transform.position, Quaternion.identity);
    }
}
