using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Behaviour for the authored Coming Soon scene. The visual hierarchy stays in the scene
/// so designers can tune copy, sprites, layout and button styling in the Inspector.
/// </summary>
public sealed class ComingSoonController : MonoBehaviour
{
    [Header("Scene navigation")]
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] Button backButton;

    [Header("Optional ambient motion")]
    [SerializeField] RectTransform marsArtwork;
    [SerializeField] RectTransform titleArtwork;

    void OnEnable()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(ReturnToMainMenu);
            backButton.onClick.AddListener(ReturnToMainMenu);
        }

        if (marsArtwork != null)
        {
            marsArtwork.DOKill();
            marsArtwork.DOAnchorPosY(marsArtwork.anchoredPosition.y + 12f, 2.8f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .SetLink(marsArtwork.gameObject);
        }

        if (titleArtwork != null)
        {
            titleArtwork.DOKill();
            titleArtwork.DOScale(1.025f, 1.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .SetUpdate(true)
                .SetLink(titleArtwork.gameObject);
        }
    }

    void OnDisable()
    {
        if (backButton != null) backButton.onClick.RemoveListener(ReturnToMainMenu);
        if (marsArtwork != null) marsArtwork.DOKill();
        if (titleArtwork != null) titleArtwork.DOKill();
    }

    public void ReturnToMainMenu()
    {
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
        else
            Debug.LogError($"A cena '{mainMenuSceneName}' não está habilitada no Build Settings.", this);
    }
}
