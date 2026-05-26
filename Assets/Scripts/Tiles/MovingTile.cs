using UnityEngine;

public class MovingTile : MonoBehaviour
{
    [Tooltip("Waypoints transforms the tile will move between (in order)")]
    public Transform[] waypoints;
    [Tooltip("Movement speed in world units per second")]
    public float speed = 2f;
    [Tooltip("If true the path loops; otherwise it ping-pongs")]
    public bool loop = true;

    private int currentIndex = 0;
    private int direction = 1;

    private void Start()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        transform.position = waypoints[0].position;
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.01f)
        {
            currentIndex += direction;

            if (currentIndex >= waypoints.Length)
            {
                if (loop)
                {
                    currentIndex = 0;
                }
                else
                {
                    currentIndex = waypoints.Length - 2;
                    direction = -1;
                }
            }

            if (currentIndex < 0)
            {
                if (loop)
                {
                    currentIndex = waypoints.Length - 1;
                }
                else
                {
                    currentIndex = 1;
                    direction = 1;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.05f);
            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
