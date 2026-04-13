using UnityEngine;
using TMPro;

public class WormTextManager : MonoBehaviour
{

    public PlayerBankManager bank;
    public TextMeshProUGUI wormTextMesh;
    public TextMeshProUGUI greenGrubTextMesh;
    public GameObject wormIcon;

    // Update is called once per frame
    void Update()
    {
        wormTextMesh.text = bank.worms.ToString();
        wormTextMesh.ForceMeshUpdate();

        greenGrubTextMesh.text = bank.greenGrubs.ToString();
        greenGrubTextMesh.ForceMeshUpdate();

        //wormIcon.transform.position = textMesh.transform.position - new Vector3(textMesh.preferredWidth, 0f, 0f);
    }
}
