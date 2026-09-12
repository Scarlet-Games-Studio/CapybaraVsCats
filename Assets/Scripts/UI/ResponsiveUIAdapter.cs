using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Padroniza a escala da UI, cria um fundo full-bleed e mantém controles
/// interativos dentro da área segura em qualquer proporção de celular.
/// </summary>
[DefaultExecutionOrder(-2500)]
public sealed class ResponsiveUIAdapter : MonoBehaviour
{
    const string FullBleedName = "Responsive Full Bleed Background";
    static ResponsiveUIAdapter instance;
    Coroutine refreshRoutine;
    int lastScreenWidth;
    int lastScreenHeight;
    Rect lastSafeArea;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() => instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (instance != null) return;
        var adapterObject = new GameObject("Responsive UI Adapter");
        instance = adapterObject.AddComponent<ResponsiveUIAdapter>();
        DontDestroyOnLoad(adapterObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        QueueRefresh();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => QueueRefresh();

    void Update()
    {
        if (refreshRoutine != null) return;
        if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight && Screen.safeArea == lastSafeArea) return;
        QueueRefresh();
    }

    void QueueRefresh()
    {
        if (refreshRoutine != null) StopCoroutine(refreshRoutine);
        refreshRoutine = StartCoroutine(RefreshAfterLayout());
    }

    IEnumerator RefreshAfterLayout()
    {
        // Aguarda Awake/Start e mais uma reconstrução do layout da UI.
        yield return null;
        yield return new WaitForEndOfFrame();
        ApplyToLoadedScene();
        yield return null;
        ClampControlsToSafeArea();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastSafeArea = Screen.safeArea;
        refreshRoutine = null;
    }

    static void ApplyToLoadedScene()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null || canvas.renderMode == RenderMode.WorldSpace) continue;
            ConfigureScaler(canvas);
            EnsureFullBleedBackground(canvas);
        }

        FitWorldBackgrounds();
    }

    static void ConfigureScaler(Canvas canvas)
    {
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.referencePixelsPerUnit = 100f;
    }

    static void EnsureFullBleedBackground(Canvas canvas)
    {
        if (canvas.transform.Find(FullBleedName) != null) return;
        Graphic source = FindBestBackground(canvas);
        if (source == null) return;

        GameObject underlay;
        Graphic copy;
        float aspect;

        if (source is RawImage raw && raw.texture != null)
        {
            underlay = new GameObject(FullBleedName, typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            RawImage target = underlay.GetComponent<RawImage>();
            target.texture = raw.texture;
            target.uvRect = raw.uvRect;
            copy = target;
            aspect = (float)raw.texture.width / Mathf.Max(1, raw.texture.height);
        }
        else if (source is Image image && image.sprite != null)
        {
            underlay = new GameObject(FullBleedName, typeof(RectTransform), typeof(Image), typeof(AspectRatioFitter));
            Image target = underlay.GetComponent<Image>();
            target.sprite = image.sprite;
            target.type = Image.Type.Simple;
            target.preserveAspect = false;
            copy = target;
            Rect spriteRect = image.sprite.rect;
            aspect = spriteRect.width / Mathf.Max(1f, spriteRect.height);
        }
        else return;

        underlay.transform.SetParent(canvas.transform, false);
        underlay.transform.SetAsFirstSibling();
        RectTransform rect = (RectTransform)underlay.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = ((RectTransform)canvas.transform).rect.size;
        rect.localScale = Vector3.one;

        copy.color = source.color;
        copy.material = source.material;
        copy.raycastTarget = false;

        AspectRatioFitter fitter = underlay.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fitter.aspectRatio = Mathf.Max(.1f, aspect);
    }

    static Graphic FindBestBackground(Canvas canvas)
    {
        Graphic best = null;
        float bestScore = float.MinValue;
        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null || !graphic.gameObject.activeInHierarchy) continue;
            if (graphic.canvas != canvas || graphic.GetComponent<Selectable>() != null) continue;
            if (graphic is Image image && image.sprite == null) continue;
            if (graphic is RawImage raw && raw.texture == null) continue;

            string id = graphic.name.ToLowerInvariant();
            string hierarchy = BuildHierarchyId(graphic.transform, canvas.transform);
            if (ContainsAny(hierarchy, "settings", "credits", "game over", "stage complete", "analogic", "music", "button", "slider")) continue;
            if (id.Contains("scrolling")) continue;

            float score = 0f;
            if (id.Contains("fundo")) score += 240f;
            if (id.Contains("background") || id.Contains("backdrop") || id.Contains("backgroundgrid")) score += 190f;
            if (id == "image (1)") score += 260f;
            if (id.Contains("sky") || id.Contains("space") || id.Contains("ceu")) score += 180f;
            if (score <= 0f) continue;

            RectTransform rect = graphic.rectTransform;
            Vector2 displayedSize = Vector2.Scale(rect.rect.size, new Vector2(Mathf.Abs(rect.lossyScale.x), Mathf.Abs(rect.lossyScale.y)));
            score += Mathf.Sqrt(Mathf.Max(0f, displayedSize.x * displayedSize.y)) * .02f;
            score -= GetDepthBelow(rect, canvas.transform) * 4f;

            if (score > bestScore)
            {
                bestScore = score;
                best = graphic;
            }
        }

        return best;
    }

    static string BuildHierarchyId(Transform item, Transform stop)
    {
        string value = string.Empty;
        while (item != null && item != stop)
        {
            value += "/" + item.name.ToLowerInvariant();
            item = item.parent;
        }
        return value;
    }

    static int GetDepthBelow(Transform item, Transform stop)
    {
        int depth = 0;
        while (item != null && item != stop) { depth++; item = item.parent; }
        return depth;
    }

    static bool ContainsAny(string value, params string[] terms)
    {
        foreach (string term in terms)
            if (value.Contains(term)) return true;
        return false;
    }

    static void FitWorldBackgrounds()
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic) return;

        float targetHeight = camera.orthographicSize * 2f;
        float targetWidth = targetHeight * camera.aspect;
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Exclude);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer.sprite == null) continue;
            string id = renderer.name.ToLowerInvariant();
            if (!ContainsAny(id, "background", "fundo", "backdrop", "sky", "space", "ceu")) continue;
            if (ContainsAny(id, "music", "analogic", "projectile", "bullet")) continue;

            Vector2 spriteSize = renderer.sprite.bounds.size;
            if (spriteSize.x <= .001f || spriteSize.y <= .001f) continue;
            float worldWidth = spriteSize.x * Mathf.Abs(renderer.transform.lossyScale.x);
            float worldHeight = spriteSize.y * Mathf.Abs(renderer.transform.lossyScale.y);
            float coverMultiplier = Mathf.Max(targetWidth / Mathf.Max(.001f, worldWidth), targetHeight / Mathf.Max(.001f, worldHeight));
            Vector3 scale = renderer.transform.localScale;
            renderer.transform.localScale = new Vector3(scale.x * coverMultiplier, scale.y * coverMultiplier, scale.z == 0f ? 1f : scale.z);
        }
    }

    static void ClampControlsToSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        if (safeArea.width <= 0f || safeArea.height <= 0f) return;
        safeArea.xMin += 12f;
        safeArea.xMax -= 12f;
        safeArea.yMin += 12f;
        safeArea.yMax -= 12f;

        Selectable[] controls = FindObjectsByType<Selectable>(FindObjectsInactive.Include);
        var processed = new HashSet<RectTransform>();
        foreach (Selectable control in controls)
        {
            if (control == null || !control.gameObject.activeInHierarchy) continue;
            RectTransform rect = control.transform as RectTransform;
            Canvas canvas = control.GetComponentInParent<Canvas>();
            if (rect == null || canvas == null || canvas.renderMode == RenderMode.WorldSpace || !processed.Add(rect)) continue;

            Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            Vector2 min = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[0]);
            Vector2 max = RectTransformUtility.WorldToScreenPoint(uiCamera, corners[2]);
            Vector2 correction = Vector2.zero;
            if (min.x < safeArea.xMin) correction.x += safeArea.xMin - min.x;
            if (max.x > safeArea.xMax) correction.x -= max.x - safeArea.xMax;
            if (min.y < safeArea.yMin) correction.y += safeArea.yMin - min.y;
            if (max.y > safeArea.yMax) correction.y -= max.y - safeArea.yMax;

            if (correction.sqrMagnitude > .01f && rect.parent is RectTransform parent)
            {
                Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, rect.position);
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition, uiCamera, out Vector2 currentLocal) &&
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPosition + correction, uiCamera, out Vector2 correctedLocal))
                    rect.anchoredPosition += correctedLocal - currentLocal;
            }
        }
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
