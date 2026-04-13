using UnityEngine;

public class AttachTo : MonoBehaviour
{
    public Transform attachTo;
    public Vector3 offset;
    // Update is called once per frame
    void FixedUpdate()
    {
        if (attachTo) transform.position = attachTo.position + offset;
    }
}
