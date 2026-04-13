using UnityEngine;

public class ScaleWithRaycast : MonoBehaviour
{
    private void LateUpdate()
    {
        float facingDirSign = transform.lossyScale.x / Mathf.Abs(transform.lossyScale.x);
        Vector3 origin = transform.position + 1f * facingDirSign * -transform.right;
        Vector3 direction = 1f * facingDirSign * -transform.right;
        float rayDistance = 100f;

        RaycastHit2D hitInfo = Physics2D.Raycast(origin, direction, rayDistance, LayerMask.GetMask("ObstacleLayer"));

        float actualDistance = rayDistance;
        if (hitInfo.collider != null)
        {
            actualDistance = hitInfo.distance + 1f;
        }

        transform.localScale = new Vector3(actualDistance * 0.04882812f, transform.localScale.y, transform.localScale.z);
    }
}
