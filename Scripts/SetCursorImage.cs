using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCursorImage : MonoBehaviour
{
    [SerializeField] private Texture2D customCursorTexture;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.SetCursor(customCursorTexture, new Vector2(20f, 20f), CursorMode.Auto);
    }
}
