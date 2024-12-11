using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using TMPro;
using Unity.VisualScripting;
using System.Collections;



public class Player : MonoBehaviour
{
    public TextMeshProUGUI speedText;

    public float currentSpeed;
    private float moveSpeed = 5;
    public float maxSpeed;
    public float deceleration = 0.3f; // Yavaþlama hýzý
    public float acceleration = 0.5f;  //hizlanma hizi 
    public bool isSlowingDown = false;
    public bool isAccelerating = true;
    public bool isAstar;
    public bool canSkid;
    public bool carSlowDown;

    public float followDistance;

    private Vector3 targetPosition; // Oyuncunun gitmesi gereken hedef pozisyon
    private int currentNodeIndex = 0; // Þu anda üzerinde bulunulan yol düðümünün indeksi
    private List<Node> path; // Oyuncunun takip edeceði yol

    public float baseTurnSpeed = 50f; // Temel dönüþ hýzý (derece/saniye)
    private float turnSpeed; // Dönüþ hýzýný sabit
    private Quaternion targetRotation; // Hedef rotayý saklayan deðiþken
    private bool isTurning = false; // Dönüþ yapýlýp yapýlmadýðý kontrolü

    private AudioSource gazAudioSource;

    private void Start()
    {
        gazAudioSource = GetComponent<AudioSource>();
        SpeedTextUpdate();  // baþlangýçta hýz metnini güncelle
    }

    // Yolun bitip bitmediðini kontrol eden metot
    public bool IsPathFinished()
    {
        return path == null || currentNodeIndex >= path.Count;
    }

    // Oyuncunun takip edeceði yolu ayarlayan metot
    public void SetPath(List<Node> newPath)
    {
        path = newPath; // Yeni yolu belirle
        currentNodeIndex = 0; // Ýlk düðümden baþla
        if (path.Count > 0)
        {
            // Définir la position cible en fixant Y à 1
            targetPosition = new Vector3(path[currentNodeIndex].WorldPosition.x, 1, path[currentNodeIndex].WorldPosition.z);
        }
    }

    public void GidilcekYer(Vector3 hedefNoktasi)
    {
        ++hedefNoktasi.y;

        // Önündeki araç varsa yavaşlama kontrolü
        if (carSlowDown)
        {
            // Öndeki araç varsa yavaşla
            currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime);
            Debug.Log("Öndeki araç yakın, yavaşlıyorum.");
        }

        else if (isSlowingDown)
        {
            currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime);
            Debug.Log("Kırmızı ışık | Yaya");
        }
        else if (canSkid)
        {
            if (isAccelerating)
            {
                currentSpeed = Mathf.Max(maxSpeed / 2, currentSpeed - deceleration * Time.deltaTime);
                Debug.Log("Karlı yol yavaşla");
            }
            else if (isSlowingDown)
            {
                currentSpeed = Mathf.Min(maxSpeed / 2, currentSpeed + acceleration * Time.deltaTime);
                Debug.Log("Karlı yol hızlan");
            }
        }
        else if (isAccelerating)
        {
            currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
            Debug.Log("Sıkıntı yok devam et");
        }

        // Aracı hedef noktaya hareket ettir
        transform.position = Vector3.MoveTowards(transform.position, hedefNoktasi, currentSpeed * Time.deltaTime);

        LookToTarget(hedefNoktasi); // Hedefe doğru yavaşça dönecek
    }


    // Oyuncuyu hedefe doðru hareket ettiren metot
    public void MoveToTarget(Vector3 target)
    {
        LookToTarget(target); // Hedefe dön
        transform.position = Vector3.MoveTowards(transform.position, target, currentSpeed * Time.deltaTime); // Hedefe doðru hareket et
    }

    public void LookToTarget(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        // Eðer hedefe çok yakýn deðilse
        if (direction.magnitude > 0.1f)
        {
            direction.y = 0;  // y ekseninde dönmeyi engelliyoruz

            targetRotation = Quaternion.LookRotation(direction);

            // Mevcut rotasyonu al ve yalnýzca Y eksenini ayarla
            Vector3 currentRotation = transform.rotation.eulerAngles;
            targetRotation = Quaternion.Euler(currentRotation.x, targetRotation.eulerAngles.y, currentRotation.z);

            // Arabayý sadece Y ekseninde hedefe bakacak þekilde yumuþakça döndür
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.0f);

            // Dönüþ hýzýný sabit tutalým, dönüþ yaparken hýzla orantýlý yapalým
            turnSpeed = Mathf.Lerp(baseTurnSpeed, baseTurnSpeed * 1.5f, currentSpeed / maxSpeed);  // Hýzla orantýlý dönüþ

            // Eðer dönüþ hala yapýlýyorsa, rotayý yavaþça hedefe doðru döndür
            if (!isTurning)
            {
                StartCoroutine(SmoothTurn()); // Yumuþak dönüþ baþlat
            }
        }
    }

    private IEnumerator SmoothTurn()
    {
        isTurning = true;

        // Dönüþ baþladýðýnda yavaþça dön
        float timeElapsed = 30f;
        float turnDuration = 3f; // Dönüþ süresi (saniye)

        // Hedef rotaya kademeli geçiþ saðlamak için
        Quaternion initialRotation = transform.rotation;

        while (timeElapsed < turnDuration)
        {
            timeElapsed += Time.deltaTime;

            // Yavaþça hedef rotaya dönüyoruz
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, timeElapsed / turnDuration);
            yield return null;
        }

        // Dönüþ tamamlandýktan sonra
        isTurning = false;
    }

    private void GameKontrol()
    {
        Vector3 newPosition = transform.position;

        // Yukarý ok tuþu z ekseninde artýrma yapar
        if (Input.GetKey(KeyCode.UpArrow))
        {
            newPosition.z += moveSpeed * Time.deltaTime;
        }

        // Aþaðý ok tuþu z ekseninde azaltma yapar
        if (Input.GetKey(KeyCode.DownArrow))
        {
            newPosition.z -= moveSpeed * Time.deltaTime;
        }

        // Sað ok tuþu x ekseninde artýrma yapar
        if (Input.GetKey(KeyCode.RightArrow))
        {
            newPosition.x += moveSpeed * Time.deltaTime;
        }

        // Sol ok tuþu x ekseninde azaltma yapar
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition.x -= moveSpeed * Time.deltaTime;
        }

        // Yeni pozisyonu güncelle
        transform.position = newPosition;

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
            ++Time.timeScale;
        if (Input.GetKeyDown(KeyCode.KeypadMinus))
            --Time.timeScale;



        SpeedTextUpdate();

        //if (maxSpeed == 0)
        //{
        //    isSlowingDown = false;
        //}
        if (maxSpeed == currentSpeed)
        {
            isAccelerating = false;
        }
        if (maxSpeed != currentSpeed)
        {

            if (isSlowingDown)
            {
            }
            else
            {
                isAccelerating = true;
            }
        }

        if (currentSpeed == maxSpeed)
        {
            gazAudioSource.pitch = Mathf.Lerp(gazAudioSource.pitch, Random.Range(1.5f, 1.7f), currentSpeed / maxSpeed);
        }
        else if (currentSpeed != 0)
        {
            gazAudioSource.pitch = Mathf.Lerp(0.42f, 1.7f, currentSpeed / maxSpeed);

            if (!gazAudioSource.isPlaying)
            {
                gazAudioSource.Play();
            }
        }

        else
        {
            gazAudioSource.pitch = Mathf.Lerp(0.42f, 1.7f, currentSpeed / maxSpeed);
            gazAudioSource.volume = 0.2f;

        }




    }

    private void Update()
    {
        if (isAstar)
        {


            // Oyuncunun takip edebileceði bir yol varsa hedef pozisyona doðru hareket et
            if (path != null && path.Count > 0)
            {
                // Hýzlanma veya yavaþlama durumuna göre hýz güncellemesi
                if (isAccelerating)
                {
                    currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime); // Maksimum hýzý aþma
                }
                else if (isSlowingDown)
                {
                    currentSpeed = Mathf.Max(0, currentSpeed - deceleration * Time.deltaTime); // Hýzý sýfýrýn altýna düþürme
                }

                MoveToTarget(targetPosition); // Hedefe doðru hareket et

                // Hedef düðüme ulaþýlýp ulaþýlmadýðýný kontrol et
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f) // Eþik deðeri (mesafe kontrolü)
                {
                    currentNodeIndex++; // Sonraki düðüme geç
                    if (currentNodeIndex < path.Count)
                    {
                        // Définir la position cible en fixant Y à 1
                        targetPosition = new Vector3(path[currentNodeIndex].WorldPosition.x, 1, path[currentNodeIndex].WorldPosition.z);

                    }
                    else
                    {
                        Debug.Log("Yolun sonuna ulaþýldý!"); // Yolun bittiðini bildir
                        path = null; // Yol bilgisini sýfýrla
                    }
                }
            }
        }
        GameKontrol();
    }

    private void SpeedTextUpdate()
    {

        speedText.text = (currentSpeed * 10).ToString("F1") + " Km/H";

    }
}