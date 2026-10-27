using System.Collections.Generic;
using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)] public float speed = 0.72f;
    public Transform parallaxController;
    [Tooltip("Painel usado como referencia para tamanho e encaixe da continuacao.")]
    public Transform parallaxInfinito;
    [SerializeField] float parallaxHeight;

    [Header("Generated continuation")]
    [SerializeField] bool buildGeneratedContinuation = true;
    [SerializeField] string[] generatedSpriteResources =
    {
        "ParallaxContinuation/ceu_5_atmosfera",
        "ParallaxContinuation/ceu_6_espaco",
        "ParallaxContinuation/ceu_7_pre_boss"
    };
    [SerializeField, Range(0f, 0.15f)] float seamOverlap = 0.025f;
    [SerializeField, Tooltip("Mantido desligado no stage 1 para o skylauch11 nao se repetir na arena do boss.")] bool loopFinalBossArena;
    [SerializeField] bool addExistingArtDecorations = true;
    readonly List<Transform> generatedPanels = new List<Transform>();
    readonly List<Transform> finalArenaPanels = new List<Transform>();

    Camera gameCamera;
    Transform finalArenaParent;
    float finalArenaSpacing;
    bool hasFiniteSequence;
    bool hasAuthoredSkySequence;
    bool stoppedForBossArena;

    public void StopForBossArena()
    {
        stoppedForBossArena = true;
    }

    void Start()
    {
        if (parallaxController == null) parallaxController = transform;
        Rigidbody2D body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        gameCamera = Camera.main;
        hasAuthoredSkySequence = HasAuthoredSkySequence();

        // Preserve the skylauch panels already positioned in the scene.
        if (!hasAuthoredSkySequence && buildGeneratedContinuation) BuildContinuation();
        if (loopFinalBossArena) ConfigureFinalArenaLoop();
        if (!hasAuthoredSkySequence && addExistingArtDecorations)
        {
            DecorateLaunchAscent();
            DecorateSpaceWithGalaxies();
        }
    }

    void Update()
    {
        if (parallaxController == null) return;
        if (stoppedForBossArena) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        Vector3 position = parallaxController.position;
        position.y -= speed * Time.deltaTime;
        parallaxController.position = position;
        UpdateFinalArenaLoop();
    }

    void FixedUpdate()
    {
        // Mantem compatibilidade com cenas antigas caso as novas imagens nao existam.
        if (hasFiniteSequence || hasAuthoredSkySequence || parallaxInfinito == null) return;

        parallaxHeight = parallaxInfinito.position.y;
        if (parallaxHeight >= -12f) return;

        Vector3 position = parallaxInfinito.position;
        position.y = 12f;
        parallaxInfinito.position = position;
    }

    void BuildContinuation()
    {
        if (parallaxInfinito == null || generatedSpriteResources == null || generatedSpriteResources.Length == 0)
            return;

        SpriteRenderer templateRenderer = parallaxInfinito.GetComponent<SpriteRenderer>();
        if (templateRenderer == null || templateRenderer.sprite == null) return;

        float panelWidth = templateRenderer.sprite.bounds.size.x * Mathf.Abs(parallaxInfinito.localScale.x);
        float panelHeight = templateRenderer.sprite.bounds.size.y * Mathf.Abs(parallaxInfinito.localScale.y);
        if (panelWidth <= 0.01f || panelHeight <= 0.01f) return;

        float nextBottom = parallaxInfinito.localPosition.y + panelHeight - seamOverlap;
        for (int i = 0; i < generatedSpriteResources.Length; i++)
        {
            string panelName = $"ceu_{i + 5}_generated";
            Transform existingPanel = FindDirectChild(panelName);
            if (existingPanel != null)
            {
                generatedPanels.Add(existingPanel);
                nextBottom = existingPanel.localPosition.y + panelHeight - seamOverlap;
                continue;
            }

            Sprite sprite = Resources.Load<Sprite>(generatedSpriteResources[i]);
            if (sprite == null)
            {
                Debug.LogWarning($"Parallax: sprite nao encontrado em Resources/{generatedSpriteResources[i]}.", this);
                continue;
            }

            Transform panel = CreatePanel(sprite, templateRenderer, nextBottom, i + 5, panelWidth, panelHeight);
            generatedPanels.Add(panel);
            nextBottom += panelHeight - seamOverlap;
        }

        hasFiniteSequence = generatedPanels.Count > 0;
    }

    Transform CreatePanel(Sprite sprite, SpriteRenderer templateRenderer, float localBottom, int skyNumber,
        float targetWidth, float targetHeight)
    {
        GameObject panelObject = new GameObject($"ceu_{skyNumber}_generated");
        panelObject.layer = parallaxInfinito.gameObject.layer;
        Transform panel = panelObject.transform;
        panel.SetParent(parallaxController, false);
        panel.localPosition = new Vector3(parallaxInfinito.localPosition.x, localBottom, parallaxInfinito.localPosition.z);
        panel.localRotation = parallaxInfinito.localRotation;

        float scaleX = targetWidth / Mathf.Max(0.01f, sprite.bounds.size.x);
        float scaleY = targetHeight / Mathf.Max(0.01f, sprite.bounds.size.y);
        panel.localScale = new Vector3(
            Mathf.Sign(parallaxInfinito.localScale.x) * scaleX,
            Mathf.Sign(parallaxInfinito.localScale.y) * scaleY,
            parallaxInfinito.localScale.z);

        SpriteRenderer renderer = panelObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = Color.white;
        renderer.sharedMaterial = templateRenderer.sharedMaterial;
        renderer.sortingLayerID = templateRenderer.sortingLayerID;
        renderer.sortingOrder = templateRenderer.sortingOrder;
        renderer.maskInteraction = templateRenderer.maskInteraction;
        renderer.spriteSortPoint = templateRenderer.spriteSortPoint;
        return panel;
    }

    void ConfigureFinalArenaLoop()
    {
        finalArenaPanels.Clear();
        Transform finalPanel = generatedPanels.Count > 0
            ? generatedPanels[generatedPanels.Count - 1]
            : FindLastAuthoredSkyPanel();
        if (finalPanel == null || finalPanel.parent == null) return;

        finalArenaParent = finalPanel.parent;
        finalArenaPanels.Add(finalPanel);

        Transform previousPanel = null;
        float highestY = float.NegativeInfinity;
        for (int i = 0; i < finalArenaParent.childCount; i++)
        {
            Transform child = finalArenaParent.GetChild(i);
            if (child == finalPanel || child.GetComponent<SpriteRenderer>() == null) continue;
            float y = child.localPosition.y;
            if (y > highestY)
            {
                highestY = y;
                previousPanel = child;
            }
        }

        finalArenaSpacing = previousPanel != null
            ? finalPanel.localPosition.y - previousPanel.localPosition.y
            : 0f;
        if (finalArenaSpacing <= 0.01f)
        {
            SpriteRenderer finalRenderer = finalPanel.GetComponent<SpriteRenderer>();
            float parentScale = Mathf.Max(0.01f, Mathf.Abs(finalArenaParent.lossyScale.y));
            finalArenaSpacing = finalRenderer != null
                ? finalRenderer.bounds.size.y / parentScale
                : 10f;
        }

        for (int i = 1; i <= 2; i++)
        {
            Transform copy = Instantiate(finalPanel.gameObject, finalArenaParent, false).transform;
            copy.name = $"{finalPanel.name} Arena Loop {i}";
            copy.localPosition = finalPanel.localPosition + Vector3.up * (finalArenaSpacing * i);
            finalArenaPanels.Add(copy);
        }
    }

    Transform FindLastAuthoredSkyPanel()
    {
        Transform authoredTransition = FindDirectChild("ceu_3_0 (1)");
        if (authoredTransition == null) return null;

        Transform lastPanel = null;
        for (int i = 0; i < authoredTransition.childCount; i++)
        {
            Transform child = authoredTransition.GetChild(i);
            if (child.GetComponent<SpriteRenderer>() == null) continue;
            if (lastPanel == null || child.localPosition.y > lastPanel.localPosition.y)
                lastPanel = child;
        }
        return lastPanel;
    }

    void UpdateFinalArenaLoop()
    {
        if (gameCamera == null) gameCamera = Camera.main;
        if (gameCamera == null || finalArenaPanels.Count < 2 || finalArenaParent == null) return;

        for (int i = 0; i < finalArenaPanels.Count; i++)
        {
            Transform panel = finalArenaPanels[i];
            if (panel == null) continue;
            SpriteRenderer renderer = panel.GetComponent<SpriteRenderer>();
            if (renderer == null || gameCamera.WorldToViewportPoint(renderer.bounds.max).y >= -0.15f) continue;

            float highestY = float.NegativeInfinity;
            for (int j = 0; j < finalArenaPanels.Count; j++)
            {
                Transform other = finalArenaPanels[j];
                if (other != null && other != panel)
                    highestY = Mathf.Max(highestY, other.localPosition.y);
            }

            Vector3 position = panel.localPosition;
            position.y = highestY + finalArenaSpacing;
            panel.localPosition = position;
        }
    }

    void DecorateLaunchAscent()
    {
        Transform cloudTemplate = FindDirectChild("nuvens");
        Transform skyOne = FindDirectChild("ceu_1_0");
        Transform skyTwo = FindDirectChild("ceu_2_0");
        if (cloudTemplate == null || skyOne == null || skyTwo == null) return;

        if (FindDirectChild("Launch Clouds Generated") != null) return;

        GameObject groupObject = new GameObject("Launch Clouds Generated");
        groupObject.transform.SetParent(parallaxController, false);

        AddCloud(cloudTemplate, groupObject.transform, skyOne, 0.18f, 0.2f, 0.72f, 0.72f, false, 8f);
        AddCloud(cloudTemplate, groupObject.transform, skyOne, 0.63f, 0.47f, 0.54f, 0.58f, true, -5f);
        AddCloud(cloudTemplate, groupObject.transform, skyOne, 0.32f, 0.77f, 0.42f, 0.46f, true, 3f);
        AddCloud(cloudTemplate, groupObject.transform, skyTwo, 0.76f, 0.28f, 0.5f, 0.5f, false, -7f);
        AddCloud(cloudTemplate, groupObject.transform, skyTwo, 0.08f, 0.68f, 0.38f, 0.42f, false, 4f);
    }

    void AddCloud(Transform template, Transform group, Transform sky, float horizontal, float vertical,
        float scaleMultiplier, float alpha, bool flipX, float rotation)
    {
        SpriteRenderer skyRenderer = sky.GetComponent<SpriteRenderer>();
        SpriteRenderer templateRenderer = template.GetComponent<SpriteRenderer>();
        if (skyRenderer == null || skyRenderer.sprite == null || templateRenderer == null) return;

        float skyWidth = skyRenderer.sprite.bounds.size.x * Mathf.Abs(sky.localScale.x);
        float skyHeight = skyRenderer.sprite.bounds.size.y * Mathf.Abs(sky.localScale.y);
        GameObject cloud = Instantiate(template.gameObject, group);
        cloud.name = $"Cloud Accent {group.childCount}";
        cloud.transform.localPosition = new Vector3(
            sky.localPosition.x + skyWidth * horizontal,
            sky.localPosition.y + skyHeight * vertical,
            template.localPosition.z);
        cloud.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        cloud.transform.localScale = template.localScale * scaleMultiplier;

        SpriteRenderer renderer = cloud.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.flipX = flipX;
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, skyRenderer.sortingOrder + 1);
        }
    }

    void DecorateSpaceWithGalaxies()
    {
        if (generatedPanels.Count == 0) return;
        Transform galaxyTemplate = FindDirectChild("galaxias");
        if (galaxyTemplate == null || FindDirectChild("Space Galaxies Generated") != null) return;

        GameObject groupObject = new GameObject("Space Galaxies Generated");
        groupObject.transform.SetParent(parallaxController, false);

        AddGalaxy(galaxyTemplate, groupObject.transform, generatedPanels[0], 0.1f, 0.38f, 0.52f, false, -14f);
        if (generatedPanels.Count > 1)
            AddGalaxy(galaxyTemplate, groupObject.transform, generatedPanels[1], 0.82f, 0.68f, 0.44f, true, 12f);
        if (generatedPanels.Count > 2)
            AddGalaxy(galaxyTemplate, groupObject.transform, generatedPanels[2], 0.12f, 0.76f, 0.34f, false, -8f);
    }

    void AddGalaxy(Transform template, Transform group, Transform panel, float horizontal, float vertical,
        float scaleMultiplier, bool mirror, float rotation)
    {
        SpriteRenderer panelRenderer = panel.GetComponent<SpriteRenderer>();
        if (panelRenderer == null || panelRenderer.sprite == null) return;

        float width = panelRenderer.sprite.bounds.size.x * Mathf.Abs(panel.localScale.x);
        float height = panelRenderer.sprite.bounds.size.y * Mathf.Abs(panel.localScale.y);
        GameObject galaxy = Instantiate(template.gameObject, group);
        galaxy.name = $"Animated Galaxy {group.childCount}";
        galaxy.transform.localPosition = new Vector3(
            panel.localPosition.x + width * horizontal,
            panel.localPosition.y + height * vertical,
            template.localPosition.z);
        galaxy.transform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        Vector3 scale = template.localScale * scaleMultiplier;
        if (mirror) scale.x *= -1f;
        galaxy.transform.localScale = scale;

        SpriteRenderer renderer = galaxy.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = Mathf.Max(color.a, 0.52f);
            renderer.color = color;
            renderer.sortingOrder = Mathf.Max(renderer.sortingOrder, panelRenderer.sortingOrder + 2);
        }
    }

    Transform FindDirectChild(string objectName)
    {
        if (parallaxController == null) return null;
        for (int i = 0; i < parallaxController.childCount; i++)
        {
            Transform child = parallaxController.GetChild(i);
            if (child.name == objectName) return child;
        }
        return null;
    }

    bool HasAuthoredSkySequence()
    {
        Transform authoredTransition = FindDirectChild("ceu_3_0 (1)");
        if (authoredTransition == null || authoredTransition.childCount < 8) return false;

        // The eight skylauch4-11 panels are already arranged under this object.
        for (int i = 0; i < authoredTransition.childCount; i++)
        {
            if (authoredTransition.GetChild(i).GetComponent<SpriteRenderer>() == null) return false;
        }

        return true;
    }
}
