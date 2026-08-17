using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Spin : MonoBehaviour
{
    [Header("Spin Circle")]
    [SerializeField] float speed;
    [SerializeField] RawImage spinCircle;
    [SerializeField] RawImage background2;

    [Header("Transition Circle")]
    [SerializeField] RawImage transitionCircle;
    [SerializeField] GameObject transitionObject;
    [SerializeField] float timer;
    [SerializeField] float growTime = 0.6f;
    [SerializeField] float maxSize = 3500f;
    [SerializeField] float minSize = 1f;
    [SerializeField] bool expanding;
    [SerializeField] bool shrinking;

    [Header("Splash Art")]
    [SerializeField] GameObject hiro;
    [SerializeField] GameObject mika;
    [SerializeField] GameObject jack;

    [Header("AirShips Sprite")]
    [SerializeField] GameObject hiroS;
    [SerializeField] GameObject mikaS;
    [SerializeField] GameObject jackS;

    [Header("Audios")]
    [SerializeField] AudioClip mikaA1;
    [SerializeField] AudioClip mikaA2;
    [SerializeField] AudioSource source;

    public string inGameSceneName = "ingame";
    public string inGameSceneName2 = "MainMenu";

    bool HasSelectionReferences => hiro != null && mika != null && jack != null &&
                                   hiroS != null && mikaS != null && jackS != null;

    void Start()
    {
        // A cena também usa UI_Spin somente para rotacionar elementos decorativos.
        // Esses componentes não possuem as referências do seletor e não devem controlá-lo.
        if (!HasSelectionReferences) return;
        HideAllCharacters();
        expanding = false;
        shrinking = false;
        timer = 0f;
    }

    void Update()
    {
        if (!Mathf.Approximately(speed, 0f))
            transform.Rotate(0f, 0f, speed);

        if (expanding || shrinking)
            timer += Time.deltaTime;

        AnimateTransition();
    }

    void AnimateTransition()
    {
        if (transitionCircle == null || (!expanding && !shrinking)) return;
        float duration = Mathf.Max(0.01f, growTime);
        float targetSize = expanding ? maxSize : minSize;
        float progress = Mathf.Clamp01(timer / duration);
        RectTransform rt = transitionCircle.rectTransform;
        rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, Vector2.one * targetSize, progress);
    }

    public void StartGame()
    {
        string sceneName = string.Equals(inGameSceneName, "InGame", System.StringComparison.OrdinalIgnoreCase)
            ? "ingame"
            : inGameSceneName;
        SceneManager.LoadScene(sceneName);
    }

    public void ExitMenu() => SceneManager.LoadScene(inGameSceneName2);

    public void Shrink()
    {
        source?.Stop();
        SetInterfaceColor(Color.white);
        expanding = false;
        shrinking = true;
        timer = 0f;
        HideAllCharacters();
    }

    public void ChangeColorGreen()
    {
        CharacterSelection.Selected = CharacterSelection.Character.Hiro;
        SelectCharacter(hiro, hiroS, new Color32(127, 255, 130, 255));
    }

    public void ChangeColorPurple()
    {
        CharacterSelection.Selected = CharacterSelection.Character.Edge;
        SelectCharacter(jack, jackS, new Color32(212, 127, 255, 255));
    }

    public void ChangeColorPink()
    {
        CharacterSelection.Selected = CharacterSelection.Character.Mika;
        SelectCharacter(mika, mikaS, new Color32(255, 127, 217, 255));
        if (source != null)
        {
            AudioClip clip = Random.value < 0.5f ? mikaA1 : mikaA2;
            if (clip != null) source.PlayOneShot(clip);
        }
    }

    void SelectCharacter(GameObject splash, GameObject ship, Color color)
    {
        if (!HasSelectionReferences) return;
        HideAllCharacters();
        SetInterfaceColor(color);
        expanding = true;
        shrinking = false;
        timer = 0f;
        if (transitionObject != null) transitionObject.SetActive(true);
        splash.SetActive(true);
        ship.SetActive(true);
    }

    void SetInterfaceColor(Color color)
    {
        if (background2 != null) background2.color = color;
        if (spinCircle != null) spinCircle.color = color;
        if (transitionCircle != null) transitionCircle.color = color;
    }

    void HideAllCharacters()
    {
        if (hiro != null) hiro.SetActive(false);
        if (mika != null) mika.SetActive(false);
        if (jack != null) jack.SetActive(false);
        if (hiroS != null) hiroS.SetActive(false);
        if (mikaS != null) mikaS.SetActive(false);
        if (jackS != null) jackS.SetActive(false);
    }
}
