using UnityEngine;

public class MoveThroughPoints : MonoBehaviour
{
    [SerializeField]
    private Vector3[] points;

    [SerializeField]
    private float speed;

    private int idx = 0;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, startPos + points[idx], speed);
        if (Vector3.Distance(transform.position, startPos + points[idx]) < 0.1f)
        {
            idx++;
            if (idx >= points.Length) idx = 0;
        }
    }

    void OnDrawGizmos()
    {
        Vector3 gizStartPos = transform.position;
        if (startPos != Vector3.zero) gizStartPos = startPos;

        Gizmos.color = Color.red;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(gizStartPos + points[i], gizStartPos + points[i + 1]);
        }
    }
}
