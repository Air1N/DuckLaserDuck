using UnityEngine;
using TMPro;

public class LoadingTextController : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    int tick = 0;
    string intendedText = "Loading...";
    string textString = "";
    public float schmoovement;

    public Animator anim;

    // Update is called once per frame
    void Update()
    {
        if (textString.Length == intendedText.Length)
        {
            tick = 0;
        }

        textString = "";

        tick++;
        for (int i = 0; i < (int)(tick * schmoovement); i++)
        {
            textString += intendedText[i];
        }

        textMesh.text = textString;

        Destroy(transform.parent.gameObject);
        anim.SetBool("loaded", true);
    }
}
