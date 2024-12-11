using UnityEngine;

public class RaycastDistance : MonoBehaviour
{
    public float maxDistance;
    public Transform rayOrigin;
    RaycastHit hit;
    Pathfinding pathfinding;
    Player player;

    private void Start()
    {
        pathfinding = FindObjectOfType<Pathfinding>();
        player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        StartRaycast();
    }

    private void StartRaycast()
    {
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward); // Ray'i başlat
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Collide);

        bool shouldSlowDown = false;
        bool botSlowDown = false;
        bool karliyol = false;

        foreach (RaycastHit hit in hits)
        {
            float distance = hit.distance;
            GameObject hitObject = hit.collider.gameObject;

            // 1. İleri Raycast: Yaya ve trafik ışığı kontrolü
            CrosswalkController crosswalk = hitObject.GetComponent<CrosswalkController>();
            if (crosswalk != null)
            {
                if (crosswalk.PedestrianCount > 0 && distance < 8.25f) // Eğer crosswalk'ta yaya varsa
                {
                    Debug.Log("Yaya var, yavaşlıyorum");
                    shouldSlowDown = true;
                    break;
                }

                else
                {
                    Debug.Log("Crosswalk boş");
                }
            }

            // Trafik ışığı kontrolü
            LightSystemSC lightSystem = hitObject.GetComponent<LightSystemSC>();
            if (lightSystem != null && lightSystem.red)
            {
                if (distance < 8.25f)
                {
                    Debug.Log("Kırmızı ışık, yavaşlıyorum");
                    shouldSlowDown = true;
                    break;
                }
            }
            // Önündeki araç kontrolü
            if (hitObject.CompareTag("araba"))
            {
                if (distance < player.followDistance) // Araç çok yakınsa
                {
                    Debug.Log("Öndeki araç çok yakın, yavaşlıyorum");
                    botSlowDown = true;
                    break;
                }
            }
        }

        // 2. Aşağı Raycast: Yol materyali kontrolü
        Ray downRay = new Ray(rayOrigin.position, Vector3.down);
        if (Physics.Raycast(downRay, out RaycastHit downHit, maxDistance))
        {
            GameObject hitObject = downHit.collider.gameObject;

            Renderer renderer = hitObject.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                string materialName = renderer.sharedMaterial.name.ToLower();
                if (materialName.Contains("snow_road"))
                {
                    karliyol = true;
                    Debug.Log("Snow road, yavaşlıyorum");

                }
            }
        }

        // Hız kontrolü
        if (shouldSlowDown)
        {
            player.isSlowingDown = true;
            player.isAccelerating = false;
        }
        else
        {
            player.isSlowingDown = false;
            player.isAccelerating = true;
        }

        if (karliyol)
        {
            player.canSkid = true;
        }
        else
        {
            player.canSkid = false;
        }

        if(botSlowDown)
        {
            player.carSlowDown = true;
        }

        else
        {
            player.carSlowDown= false;
        }


        Debug.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance, Color.green);
        Debug.DrawRay(rayOrigin.position, Vector3.down * maxDistance, Color.blue);
    }
}
