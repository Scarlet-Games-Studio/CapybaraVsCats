using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Spin : MonoBehaviour
{
    [Header("Spin Circle")]
    [SerializeField] private float speed;
    [SerializeField] private RawImage spinCircle;
    [SerializeField] private RawImage background2;


    [Header("Growth Circle")]
    [SerializeField] private RawImage transitionCircle;
    [SerializeField] private GameObject transitionObject;
    [SerializeField] private float timer = 0f;
    [SerializeField] private float growTime = 6f;
    [SerializeField] private float maxSize = 2f;
    [SerializeField] private float minSize = 2f;
    [SerializeField] private bool expanding = false;
    [SerializeField] private bool shrinking = false;
    private bool count = false;

    //Sprites das splash arts na seleção
    [Header("Splash Art")]
    [SerializeField] private GameObject hiro;
    [SerializeField] private GameObject mika;
    [SerializeField] private GameObject jack;

    //Sprites das naves na seleção
    [Header("AirShips Sprite")]
    [SerializeField] private GameObject hiroS;
    [SerializeField] private GameObject mikaS;
    [SerializeField] private GameObject jackS;

    [Header("Audios")]
    [SerializeField] private AudioClip mikaA1;
    [SerializeField] private AudioClip mikaA2;
    [SerializeField] private AudioSource source;
    private float voiceRng;

    public string inGameSceneName = "InGame"; // Nome da cena do jogo
    public string inGameSceneName2 = "MainMenu";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hiro.SetActive(false);
        mika.SetActive(false);
        jack.SetActive(false);
        hiroS.SetActive(false);
        jackS.SetActive(false);
        mikaS.SetActive(false);
        expanding = false;
    }

    // Update is called once per frame
    void Update()
    {
        //Rotaciona a engrenagem no canto inferior esquerdo
        transform.Rotate(0, 0, speed);

        if (count)
        {
            timer += Time.deltaTime;
        }
        else if(!count)
        {
            timer = 0f;
        }
    }

    IEnumerator timerDelay()
    {
        count = false;
        yield return null;
        count = false;
    }

    //Apenas adicionei isso para não precisar inutilizar todo o script do MainMenuController
    public void StartGame()
    {
        SceneManager.LoadScene(inGameSceneName);
    }
    public void ExitMenu()
    {
        SceneManager.LoadScene(inGameSceneName2);
    }

    private void FixedUpdate()
    {
        if (expanding)
        {
            count = true;
            RectTransform rt = transitionCircle.GetComponent<RectTransform>();
            Vector2 startScale = transitionCircle.rectTransform.localScale;
            Vector2 maxScale = new Vector2(maxSize, maxSize);
            rt.sizeDelta = Vector2.Lerp(startScale, maxScale, timer / growTime);
        }
        if (shrinking)
        {
            count = true;
            RectTransform rt = transitionCircle.GetComponent<RectTransform>();
            Vector2 startScale = transitionCircle.rectTransform.localScale;
            Vector2 minScale = new Vector2(minSize, minSize);
            rt.sizeDelta = Vector2.Lerp(startScale, minScale, timer / growTime);
        }
    }

    public void Shrink()
    {
        count = false;
        source.Stop();
        background2.color = new Color32(255, 255, 255, 255);
        spinCircle.color = new Color32(255, 255, 255, 255);
        expanding = false;
        shrinking = true;
        hiro.SetActive(false);
        hiroS.SetActive(false);
        jack.SetActive(false);
        jackS.SetActive(false);
        mika.SetActive(false);
        mikaS.SetActive(false);
    }

    public void ChangeColorGreen()
    {
        StartCoroutine(timerDelay());
        background2.color = new Color32(127, 255, 130, 255);
        spinCircle.color = new Color32(127, 255, 130, 255);
        transitionCircle.color = new Color32(127, 255, 130, 255);
        expanding = true;
        shrinking = false;
        transitionObject.active = true;
        hiro.SetActive(true);
        hiroS.SetActive(true);
    }
    public void ChangeColorPurple()
    {
        StartCoroutine(timerDelay());
        background2.color = new Color32(212, 127, 255, 255);
        spinCircle.color = new Color32(212, 127, 255, 255);
        transitionCircle.color = new Color32(212, 127, 255, 255);
        expanding = true;
        shrinking = false;
        transitionObject.active = true;
        jack.SetActive(true);
        jackS.SetActive(true);
    }
    public void ChangeColorPink()
    {
        StartCoroutine(timerDelay());
        background2.color = new Color32(255, 127, 217, 255);
        spinCircle.color = new Color32(255, 127, 217, 255);
        transitionCircle.color = new Color32(255, 127, 217, 255);
        voiceRng = Random.Range(1, 100);
        if(voiceRng < 50)
        {
            source.PlayOneShot(mikaA1);
        }
        else
        {
            source.PlayOneShot(mikaA2);
        }
        expanding = true;
        shrinking = false;
        transitionObject.active = true;
        mika.SetActive(true);
        mikaS.SetActive(true);
    }
}
