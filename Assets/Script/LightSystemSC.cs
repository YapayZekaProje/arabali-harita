using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSystemSC : MonoBehaviour
{
  public  bool red;

    GameObject parentObject;
    GameObject redLight, yellowLight, greenLight;

    Renderer redRenderer, greenRenderer, yellowRenderer;

    private Collider objCollider;

    private void Start()
    {
        parentObject = this.gameObject;

        redLight = parentObject.transform.Find("Red").gameObject;
        yellowLight = parentObject.transform.Find("Yellow").gameObject;
        greenLight = parentObject.transform.Find("Green").gameObject;

        redRenderer = redLight.GetComponent<Renderer>();
        greenRenderer = greenLight.GetComponent<Renderer>();
        yellowRenderer = yellowLight.GetComponent<Renderer>();

        objCollider = GetComponent<BoxCollider>();

        float random = Random.Range(0f, 6f);
        StartCoroutine(StartTrafficLightsAfterDelay(random));

    }

    IEnumerator StartTrafficLightsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(TrafficLightDongusu());
    }

    IEnumerator TrafficLightDongusu()
    {
        while (true)
        {
            GreenLight();
            yield return new WaitForSeconds(7f);
          
            YellowLight();
            yield return new WaitForSeconds(2f);

            RedLight();
            yield return new WaitForSeconds(7f);
        }
    }
    public void RedLight()
    {
        ResetLights();
        redRenderer.material.EnableKeyword("_EMISSION");
        objCollider.enabled = true;
        red = true;
    }

    public void YellowLight()
    {

        ResetLights();
        yellowRenderer.material.EnableKeyword("_EMISSION");
    }

    public void GreenLight()
    {
        red = false;
  //      objCollider.enabled = false;
        ResetLights();
        greenRenderer.material.EnableKeyword("_EMISSION");
    
    }

    private void ResetLights()
    {
        redRenderer.material.DisableKeyword("_EMISSION");
        yellowRenderer.material.DisableKeyword("_EMISSION");
        greenRenderer.material.DisableKeyword("_EMISSION");
    }

}
