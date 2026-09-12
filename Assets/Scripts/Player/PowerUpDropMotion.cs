using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PowerUpDropMotion : MonoBehaviour
{
    [SerializeField, Min(0.1f)] float fallSpeed = 1.35f;
    [SerializeField, Min(1f)] float lifetime = 12f;
    [SerializeField, Min(0.2f)] float visualDiameter = 0.46f;

    Spline path;
    Vector3 origin;
    Transform visual;
    float elapsed;
    float travelDuration;
    bool initialized;

    public void Configure(float speed, float duration)
    {
        fallSpeed = Mathf.Max(0.1f, speed);
        lifetime = Mathf.Max(1f, duration);
        BuildPath();
    }

    public void ConfigureVisual(Sprite sprite, float diameter = 0.46f)
    {
        if (sprite == null) return;
        visualDiameter = Mathf.Max(0.2f, diameter);

        SpriteRenderer source = GetComponent<SpriteRenderer>();
        GameObject visualObject = new("PowerUp Visual", typeof(SpriteRenderer));
        visualObject.transform.SetParent(transform, false);
        SpriteRenderer renderer = visualObject.GetComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingLayerName = source != null ? source.sortingLayerName : "Characters";
        renderer.sortingOrder = source != null ? Mathf.Max(source.sortingOrder, 6) : 6;
        renderer.color = source != null ? source.color : Color.white;
        if (source != null)
        {
            renderer.sharedMaterial = source.sharedMaterial;
            source.enabled = false;
        }

        float size = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float scale = size > 0.001f ? visualDiameter / size : 1f;
        visualObject.transform.localScale = Vector3.one * scale;
        visualObject.transform.localPosition = -(Vector3)sprite.bounds.center * scale;
        visual = visualObject.transform;

        CircleCollider2D pickupCollider = GetComponent<CircleCollider2D>();
        if (pickupCollider != null)
        {
            pickupCollider.offset = Vector2.zero;
            // Mantém uma área de coleta confortável mesmo com o sprite menor.
            pickupCollider.radius = Mathf.Max(0.4f, visualDiameter * 0.55f);
        }

        StartVisualTweens();
    }

    void Start()
    {
        transform.localScale = Vector3.one;
        if (visual == null)
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer != null && renderer.sprite != null) ConfigureVisual(renderer.sprite, visualDiameter);
        }
        BuildPath();
    }

    void BuildPath()
    {
        origin = transform.position;
        elapsed = 0f;
        float distance = Mathf.Max(7f, fallSpeed * lifetime);
        travelDuration = distance / fallSpeed;
        float side = UnityEngine.Random.value < 0.5f ? -1f : 1f;
        float amplitude = UnityEngine.Random.Range(0.28f, 0.55f) * side;

        path = new Spline();
        path.Add(new BezierKnot(new float3(0f, 0f, 0f)), TangentMode.AutoSmooth);
        path.Add(new BezierKnot(new float3(amplitude, -distance * 0.28f, 0f)), TangentMode.AutoSmooth);
        path.Add(new BezierKnot(new float3(-amplitude * 0.75f, -distance * 0.62f, 0f)), TangentMode.AutoSmooth);
        path.Add(new BezierKnot(new float3(amplitude * 0.25f, -distance, 0f)), TangentMode.AutoSmooth);
        initialized = true;
    }

    void StartVisualTweens()
    {
        if (visual == null) return;
        visual.DOKill();
        Vector3 baseScale = visual.localScale;
        visual.DORotate(new Vector3(0f, 0f, 360f), 2.2f, RotateMode.FastBeyond360)
            .SetLoops(-1, LoopType.Restart).SetEase(Ease.Linear).SetLink(gameObject);
        visual.DOScale(baseScale * 1.08f, 0.48f)
            .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(gameObject);
    }

    void Update()
    {
        if (!initialized || path == null || Time.timeScale <= 0f) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / travelDuration);
        float3 point = path.EvaluatePosition(t);
        transform.position = origin + new Vector3(point.x, point.y, point.z);
        if (t >= 1f) Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (visual != null) visual.DOKill();
    }
}
