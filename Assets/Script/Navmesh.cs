using UnityEngine;
using UnityEngine.AI;

public class RandomPatrol : MonoBehaviour
{
    public float walkRadius = 100f;  // Rastgele hedefin seçileceği yarıçap
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        SetNewRandomDestination();
    }

    void Update()
    {
        // Eğer ajan hedefe ulaştıysa yeni bir hedef belirle
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SetNewRandomDestination();
        }
    }

    void SetNewRandomDestination()
    {
        // Rastgele bir hedef noktası bul
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, NavMesh.AllAreas))
        {
            // Elde edilen rastgele pozisyonu NavMesh içinde geçerli bir hedef olarak ayarla
            agent.SetDestination(hit.position);
        }
    }
}