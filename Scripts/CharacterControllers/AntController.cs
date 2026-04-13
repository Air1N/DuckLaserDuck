using UnityEngine;
using Pathfinding;

public class AntController : MonoBehaviour
{

        public float moveSpeed;
        public AIPath aiPath;

        void Awake()
        {
                aiPath.maxSpeed = moveSpeed;
        }
}
