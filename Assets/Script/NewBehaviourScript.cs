using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public ParticleSystem rainParticleSystem;


    public void YagmurKontrol()
    {
        if (speedText.text == "Yagmur Kapali")
        {
            rainParticleSystem.Play();
            speedText.text = "Yagmur Acik";
        }
        else
        {
            speedText.text = "Yagmur Kapali";
            rainParticleSystem.Stop();
        }
        

    }

        public void YagmurHiziArtir()
        {
        var emission = rainParticleSystem.emission;

        emission.rateOverTime = emission.rateOverTime.constant + 200;
    }

    public void YagmurHiziAzalt()
    {
        var emission = rainParticleSystem.emission;

        emission.rateOverTime = emission.rateOverTime.constant - 200;
    }

}
