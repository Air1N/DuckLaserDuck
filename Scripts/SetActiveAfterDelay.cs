using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveAfterDelay : MonoBehaviour
{
    [SerializeField] private float delaySeconds;
    [SerializeField] private GameObject toSetActive;
    [SerializeField] private bool active;

    private void Start()
    {
        StartCoroutine(SetActiveDelayed());
    }

    private void OnEnable()
    {
        StartCoroutine(SetActiveDelayed());
    }

    private IEnumerator SetActiveDelayed()
    {
        yield return new WaitForSecondsRealtime(delaySeconds);
        toSetActive.SetActive(active);
    }
}
