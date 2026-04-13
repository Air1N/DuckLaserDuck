using System.Collections.Generic;
using UnityEngine;

public class SpriteLoadingManager : MonoBehaviour
{

public List<GameObject> objects = new List<GameObject>();
public float disableDistance;
public float enableDistance;
// Update is called once per frame
void LateUpdate()
{
        foreach (GameObject obj in objects) {
                if (Vector3.Distance(transform.position, obj.transform.position) > disableDistance) {
                        obj.SetActive(false);
                }

                if (Vector3.Distance(transform.position, obj.transform.position) < enableDistance) {
                        obj.SetActive(true);
                }
        }
}

}
