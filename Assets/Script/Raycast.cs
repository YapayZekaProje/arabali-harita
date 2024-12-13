using UnityEngine;

public class RaycastDistance : MonoBehaviour
{
    public float maxDistance;
    public Transform rayOrigin;
    RaycastHit hit;
    Pathfinding pathfinding;
    Player player;

    private string lastLightStatus = "Yeşil";  // Son trafik ışığı durumu
    private bool isPedestrianLogged = false;  // Yaya geçişi yalnızca bir kez loglanacak
    private bool isMovingLogged = false;  // Araç hareket etmeye başladığında yalnızca bir kez loglanacak

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
                    if (!isPedestrianLogged)
                    {
                        Debug.Log("Yaya var, yavaşlıyorum");
                        isPedestrianLogged = true;  // Yaya geçişi kaydedildi
                        LogPedestrianDetected(distance);  // Yayayı tespit et
                    }
                    shouldSlowDown = true;
                    break;
                }

                else if (crosswalk.PedestrianCount == 0)
                {
                    // Yaya geçidi boşsa, araç hareket edebilir
                    if (isPedestrianLogged)
                    {
                        Debug.Log("Yaya geçti, hareket ediyorum");
                        isPedestrianLogged = false;  // Yaya geçtiği için bir daha yazdırmayacak
                        LogPedestrianCleared();  // Yaya geçişi verisini kaydet
                    }
                }
            }

            // Trafik ışığı kontrolü
            LightSystemSC lightSystem = hitObject.GetComponent<LightSystemSC>();
            if (lightSystem != null && lightSystem.red)
            {
                if (lightSystem.red && distance < 8.25f)  // Kırmızı ışık ve uygun mesafe
                {
                    if (lastLightStatus != "Kırmızı")
                    {
                        LogTrafficLight("Kırmızı", distance);
                        lastLightStatus = "Kırmızı";
                    }
                    shouldSlowDown = true;
                    break;
                }
                else if (!lightSystem.red && lastLightStatus != "Yeşil")
                {
                    LogTrafficLight("Yeşil", distance);
                    lastLightStatus = "Yeşil";
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
            // Araç hızlanabilir
            if (player.currentSpeed <= 0.1f)  // Eğer hız çok düşükse (neredeyse durduysa)
            {
                // Durduktan sonra hızlanmaya başla
                player.isSlowingDown = false;
                player.isAccelerating = true;

                // Sadece bir defa "Hareket Ediyor" yazdıracağız
                if (!isMovingLogged)
                {
                    Debug.Log("Araç hareket ediyor");
                    isMovingLogged = true;  // Hareket etme kaydedildi
                }
            }
            else
            {
                player.isSlowingDown = false;  // Eğer hız azalmıyorsa, yavaşlatma durumu sıfırlanır
                player.isAccelerating = true;  // Hızlanma devam eder
            }
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

    private void LogPedestrianDetected(float distance)
    {
        CsvLogger.Log("Yaya", "Algılandı", distance, player.currentSpeed * 10, "Duruyor");
    }

    private void LogPedestrianCleared()
    {
        CsvLogger.Log("Yaya", "Geçiş Tamamlandı", 0, player.currentSpeed * 10, "Hareket Ediyor");
    }

    private void LogTrafficLight(string lightStatus, float distance)
    {
        string eventType = lightStatus == "Kırmızı" ? "Trafik Işığı Kırmızı" : "Trafik Işığı Yeşil";
        string eventStatus = lightStatus == "Kırmızı" ? "Algılandı" : "Geçiş Tamamlandı";
        string vehicleStatus = lightStatus == "Kırmızı" ? "Duruyor" : "Hareket Ediyor";

        CsvLogger.Log(eventType, eventStatus, distance, player.currentSpeed * 10, vehicleStatus);
    }
}
