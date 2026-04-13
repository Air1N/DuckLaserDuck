using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCurrency : MonoBehaviour
{
    [SerializeField] private WormTextEnabler wormTextEnabler;
    [SerializeField] private bool constantly = false;
    // Start is called before the first frame update
    void Start()
    {
        wormTextEnabler = FindFirstObjectByType<WormTextEnabler>();
        wormTextEnabler.ShowWorms();
    }

    // Update is called once per frame
    void OnEnable()
    {
        wormTextEnabler.ShowWorms();
    }

    void Update()
    {
        if (!constantly) return;

        if (wormTextEnabler.tick > 1) wormTextEnabler.ShowWorms();
        wormTextEnabler.tick = 0;
    }
}
