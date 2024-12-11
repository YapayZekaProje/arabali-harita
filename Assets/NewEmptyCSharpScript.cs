using UnityEngine;

public class PedestrianMover : MonoBehaviour
{
    public Transform[] waypoints;    // Array of waypoints (A and B)
    public float speed = 2f;         // Speed of the pedestrian
    public float turnSpeed = 5f;     // Speed at which the pedestrian turns
    public float reachDistance = 0.5f;  // Distance to consider a waypoint "reached"
    public float waitTime = 5f;      // Time to wait at each waypoint

    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Update()
    {
        if (waypoints.Length > 0)
        {
            MoveTowardsWaypoint();
        }
    }

    void MoveTowardsWaypoint()
    {
        if (isWaiting)
        {
            // Wait at the waypoint
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                // After waiting, reset the timer and switch the target waypoint
                isWaiting = false;
                waitTimer = 0f;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        // Get the current waypoint position
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move the pedestrian towards the waypoint
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0; // Keep the pedestrian on the ground, ignoring the Y-axis

        if (direction.magnitude < reachDistance)
        {
            // Start waiting at both waypoints (A and B)
            isWaiting = true;
        }
        else
        {
            // Move and rotate the pedestrian toward the target
            Vector3 moveDir = direction.normalized;
            transform.position += moveDir * speed * Time.deltaTime;
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
        }
    }
}
