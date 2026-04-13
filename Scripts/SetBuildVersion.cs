using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetBuildVersion : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textField;

    void Start()
    {
        textField.text += Application.version;
    }
}
