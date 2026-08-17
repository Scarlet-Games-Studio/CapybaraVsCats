using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteSequencePlayer : MonoBehaviour
{
    public int CurrentFrame => currentFrame;
    public int FrameCount => frames != null ? frames.Length : 0;

    [SerializeField] Sprite[] frames;
    [SerializeField] float framesPerSecond = 15f;
    [SerializeField] bool loop = true;
    [Header("Timing accents")]
    [SerializeField, Range(0f, 1f)] float slowSectionStart = 0.35f;
    [SerializeField, Range(0f, 1f)] float slowSectionEnd = 0.65f;
    [SerializeField, Range(0.1f, 1f)] float slowSectionSpeed = 0.58f;
    [SerializeField] int holdFrame = 35;
    [SerializeField, Min(0f)] float holdDuration = 1.5f;
    SpriteRenderer rendererComponent;
    float frameTimer;
    int currentFrame;
    bool holdUsed;

    void Awake()
    {
        rendererComponent = GetComponent<SpriteRenderer>();
        ShowCurrentFrame();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        if (!loop && currentFrame >= frames.Length - 1) return;

        frameTimer += Time.deltaTime;
        float frameDuration = GetFrameDuration(currentFrame);
        if (frameTimer < frameDuration) return;

        frameTimer -= frameDuration;
        if (currentFrame == holdFrame) holdUsed = true;
        currentFrame++;
        if (currentFrame >= frames.Length)
        {
            if (!loop)
            {
                currentFrame = frames.Length - 1;
                ShowCurrentFrame();
                return;
            }
            currentFrame = 0;
            holdUsed = false;
        }
        ShowCurrentFrame();
    }

    float GetFrameDuration(int frame)
    {
        float duration = 1f / Mathf.Max(1f, framesPerSecond);
        float normalized = frames.Length > 1 ? frame / (float)(frames.Length - 1) : 0f;
        if (normalized >= slowSectionStart && normalized <= slowSectionEnd)
            duration /= slowSectionSpeed;
        if (frame == holdFrame && !holdUsed)
            duration += holdDuration;
        return duration;
    }

    void ShowCurrentFrame()
    {
        if (rendererComponent != null && frames != null && frames.Length > 0)
            rendererComponent.sprite = frames[Mathf.Clamp(currentFrame, 0, frames.Length - 1)];
    }
}
