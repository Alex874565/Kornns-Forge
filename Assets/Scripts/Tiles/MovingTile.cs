using Unity.Netcode;
using UnityEngine;

public class MovingTile : NetworkBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public bool loop = true;

    public float HorizontalVelocity { get; private set; }

    private int currentIndex = 0;
    private int direction = 1;
    private Vector3 lastPosition;

    private void Start()
    {
        if (!IsServer) return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        transform.position = waypoints[0].position;
        lastPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (waypoints == null || waypoints.Length == 0)
            return;

        Transform target = waypoints[currentIndex];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.fixedDeltaTime
        );

        HorizontalVelocity =
            (transform.position.x - lastPosition.x) / Time.fixedDeltaTime;

        lastPosition = transform.position;

        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            currentIndex += direction;

            if (currentIndex >= waypoints.Length)
            {
                if (loop) currentIndex = 0;
                else
                {
                    currentIndex = waypoints.Length - 2;
                    direction = -1;
                }
            }

            if (currentIndex < 0)
            {
                if (loop) currentIndex = waypoints.Length - 1;
                else
                {
                    currentIndex = 1;
                    direction = 1;
                }
            }
        }
    }
}