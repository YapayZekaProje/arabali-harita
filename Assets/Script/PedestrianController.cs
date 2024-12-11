using System.Collections;
using UnityEngine;

public class PedestrianController : MonoBehaviour
{
    public Transform[] waypoints;       // Hareket edilecek waypoint'ler
    public float speed = 2f;            // Hareket hızı
    public float reachDistance = 0.5f;  // Waypoint'e ulaşma mesafesi
    public float defaultWaitDuration = 2f; // Trafik ışığı yoksa bekleme süresi
    public LightSystemSC trafficLight; // Trafik ışığı kontrolü için referans

    private int currentWaypointIndex = 0; // Şu anki hedef waypoint
    private bool isMoving = false;        // Hareket durumu
    private bool isWaiting = false;       // Bekleme durumu

    void Update()
    {
        if (isMoving && !isWaiting)
        {
            MoveTowardsWaypoint(); 
        }
    }

    public void SetMovement(bool shouldMove)
    {
        isMoving = shouldMove;
    }

    private void MoveTowardsWaypoint()
    {
        if (waypoints.Length < 2) return; // En az iki waypoint olmalı

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 direction = targetWaypoint.position - transform.position;

        if (direction.magnitude < reachDistance) // Waypoint'e ulaşıldı
        {
            if (currentWaypointIndex == 1) // Sadece ikinci waypoint'te bekle
            {
                if (trafficLight != null && !trafficLight.red) // Trafik ışığı kırmızı değilse bekle
                {
                    StartCoroutine(WaitAtTrafficLight());
                }
                else if (trafficLight == null) // Trafik ışığı yoksa varsayılan süre bekle
                {
                    StartCoroutine(WaitWithoutTrafficLight());
                }
                else
                {
                    GoToNextWaypoint();
                }
            }
            else
            {
                GoToNextWaypoint();
            }
        }
        else
        {
            // Hedef waypoint'e doğru hareket et
            transform.position += direction.normalized * speed * Time.deltaTime;

            // Dönme hareketi
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }
    }

    private IEnumerator WaitAtTrafficLight()
    {
        isWaiting = true; // Bekleme durumu aktif
        while (!trafficLight.red) // Trafik ışığı kırmızı olmadığı sürece bekle
        {
            yield return null; // Beklemeye devam et
        }
        GoToNextWaypoint();
        isWaiting = false; // Bekleme durumu sona erdi
    }

    private IEnumerator WaitWithoutTrafficLight()
    {
        isWaiting = true; // Bekleme durumu aktif
        yield return new WaitForSeconds(defaultWaitDuration); // Varsayılan süre boyunca bekle
        GoToNextWaypoint();
        isWaiting = false; // Bekleme durumu sona erdi
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length; // Sonraki waypoint'e geç
    }
}
