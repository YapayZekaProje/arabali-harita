using UnityEngine;
using System.Collections.Generic;

public class MoveBetweenPoints : MonoBehaviour
{
    public GameObject hedeflerParent; // Hedeflerin bulunduğu ana nesneyi buraya sürükle
    public float speed = 2f;          // Hareket hızı
    public float rotationSpeed = 5f;  // Dönme hızı

    private List<Transform> points = new List<Transform>(); // Hedef noktaların listesi
    private int currentPointIndex = 0;                      // Başlangıç noktası
    private Transform targetPoint;

    void Start()
    {

        foreach (Transform child in hedeflerParent.transform)
        {
            points.Add(child);
        }

        if (points.Count > 0)
        {
            targetPoint = points[currentPointIndex];
        }
        else
        {
            Debug.LogWarning("Lütfen hedef noktalar belirleyin.");
        }
    }

    void Update()
    {
        if (points.Count > 0)
        {
            MoveToNextPoint();
            RotateTowardsTarget();
        }
    }

    void MoveToNextPoint()
    {
        // Hedef noktaya doğru hareket et
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, speed * Time.deltaTime);

        // Eğer hedef noktaya ulaştıysa bir sonraki noktaya geç
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // Sıradaki noktaya geç ve tekrar başa dön (döngüsel)
            currentPointIndex = (currentPointIndex + 1) % points.Count;
            targetPoint = points[currentPointIndex];
        }
    }

    void RotateTowardsTarget()
    {
        // Hedef pozisyona bakacak şekilde yumuşakça dön
        Vector3 direction = (targetPoint.position - transform.position).normalized; // Hedef yön
        Quaternion lookRotation = Quaternion.LookRotation(direction);                // Bakılması gereken yön

        // Yumuşak bir geçiş ile nesneyi o yöne doğru döndür
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
    }
}
