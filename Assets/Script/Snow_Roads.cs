using System.Collections.Generic;
using UnityEngine;

public class SnowRoad : MonoBehaviour
{

    public Material snowMaterial;
    private Player player; // Player script referansý ekle

    void Start()
    {
        // "Road" tag'lý objelerin çocuklarýnda "Road Lane" içerenleri bul
        List<GameObject> roadLanes = new List<GameObject>();
        GameObject[] roadParents = GameObject.FindGameObjectsWithTag("Road");

        foreach (GameObject parent in roadParents)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                Transform child = parent.transform.GetChild(i);
                if (child.name.Contains("Road Lane"))
                {
                    roadLanes.Add(child.gameObject);
                }
            }
        }

        if (roadLanes.Count == 0)
        {
            //Debug.LogError("Hiçbir 'Road Lane' nesnesi bulunamadý!");
            return;
        }

        int snowRoadCount = roadLanes.Count / 3;
        List<GameObject> selectedRoads = new List<GameObject>();

        for (int i = 0; i < snowRoadCount && i < roadLanes.Count; i++)
        {

            //// Player script referansýný bul
            //player = FindObjectOfType<Player>();

            //// Raycast iþlemi için temel ayar
            //RaycastHit hit;
            //Ray ray = new Ray(transform.position, transform.forward);

            //// Raycast ile çarpma tespiti
            //if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            //{
            //    if (hit.collider.CompareTag("OilyRoad")) // OilyRoad tag'li bir nesneyle çarpýldýðýnda
            //    {
            //        // Yaðlý yol ile temas edildiðinde hýzýný azalt
            //        player.isSlowingDown = true;
            //        // Debug.Log("Kaygan yol tespit edildi!");
            //    }
            //}

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, roadLanes.Count);
            } while (selectedRoads.Contains(roadLanes[randomIndex]));

            selectedRoads.Add(roadLanes[randomIndex]);

            GameObject road = roadLanes[randomIndex];
            Renderer renderer = road.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material newMaterial = new Material(snowMaterial); // Yeni materyal instance'ý bu dosyayý kullanýyor musun silice
                renderer.material = newMaterial;

            }
        }
    }
}
