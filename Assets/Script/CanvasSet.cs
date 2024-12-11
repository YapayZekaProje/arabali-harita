using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class CanvasSet : MonoBehaviour
{
    public TextMeshProUGUI speedText;
    public ParticleSystem rainParticleSystem;

    [SerializeField] private GameObject target;

    [SerializeField] Camera camera1; // İlk kamera
    [SerializeField] Camera camera2; // İkinci kamera

    [SerializeField] TextMeshProUGUI mesafeText;


    public Grid grid;

    private void Update()
    {
        MesafeUpdate();
    }
    private void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;
        if (rainParticleSystem.isPlaying) // Eğer partikül sistemi çalışıyorsa
        {
            rainParticleSystem.Stop(); // Yağmuru durdur
        }
    }

    public void YagmurKontrol()
    {
        if (rainParticleSystem.isPlaying) // Eğer partikül sistemi çalışıyorsa
        {
            rainParticleSystem.Stop(); // Yağmuru durdur
        }
        else if (camera1.enabled)// Eğer partikül sistemi çalışmıyorsa
        {
            rainParticleSystem.Play(); // Yağmuru başlat
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

    public void NewTarget()
    {
        target.transform.position = grid.GetRandomWalkablePosition();
    }


    public void SwitchCamera()
    {
        if (camera1.enabled)
        {
            camera1.enabled = false;
            camera2.enabled = true;
            if (rainParticleSystem.isPlaying) 
            {
                rainParticleSystem.Stop(); 
            }
        }
        else
        {
            camera1.enabled = true;
            camera2.enabled = false;
            grid.yoluCiz = false;


        }
    }

    public void MesafeUpdate()
    {
        mesafeText.text = (grid.path1.Count).ToString("F1") + " M";
    }

    public void YoluCiz()
    {
        if (grid.yoluCiz == false && camera2.enabled == true)
        {
            grid.yoluCiz = true;
            Debug.Log("patlar");
        }
        else
        {
            grid.yoluCiz = false;
        }

    }
    public void Hizlandir()
    {
        ++Time.timeScale;
    }
    public void Yavaslat()
    {
        Time.timeScale = 0;

    }
}
