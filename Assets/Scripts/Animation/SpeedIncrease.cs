using UnityEngine;

/// <summary>
/// Briefly accelerates the background when the authored cloud and Earth scenery
/// pass the camera trigger line. Player collisions do not affect parallax speed.
/// </summary>
public class SpeedIncrease : MonoBehaviour
{
    [SerializeField] ParallaxScroller parallax;
    [SerializeField] Transform cloudMarker;
    [SerializeField] Transform earthMarker;
    [SerializeField, Range(0.1f, 0.9f)] float triggerViewportY = 0.45f;
    [SerializeField, Range(0f, 0.3f)] float boostAmount = 0.12f;
    [SerializeField, Min(0.2f)] float boostDuration = 3f;

    Camera gameCamera;
    float baseSpeed;
    float boostElapsed;
    bool cloudTriggered;
    bool earthTriggered;
    bool boostActive;

    void Start()
    {
        if (parallax == null) parallax = FindAnyObjectByType<ParallaxScroller>();
        gameCamera = Camera.main;
        if (parallax != null) baseSpeed = parallax.speed;

        // Resolve by the authored scene names if a scene reference was not assigned.
        if (cloudMarker == null)
        {
            GameObject cloud = GameObject.Find("Nuvem-Cenario1 1_0");
            if (cloud != null) cloudMarker = cloud.transform;
        }
        if (earthMarker == null)
        {
            GameObject earth = GameObject.Find("terra_0");
            if (earth != null) earthMarker = earth.transform;
        }
    }

    void Update()
    {
        if (parallax == null || gameCamera == null) return;
        if (GameManager.instance != null && !GameManager.instance.IsPlaying) return;

        if (!cloudTriggered && HasReachedScenery(cloudMarker))
        {
            cloudTriggered = true;
            BeginBoost();
        }
        if (!earthTriggered && HasReachedScenery(earthMarker))
        {
            earthTriggered = true;
            BeginBoost();
        }

        if (!boostActive) return;

        boostElapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(boostElapsed / Mathf.Max(0.2f, boostDuration));
        float envelope = Mathf.Sin(progress * Mathf.PI);
        parallax.speed = baseSpeed * (1f + boostAmount * envelope);

        if (progress >= 1f)
        {
            parallax.speed = baseSpeed;
            boostActive = false;
        }
    }

    bool HasReachedScenery(Transform marker)
    {
        if (marker == null) return false;
        float viewportY = gameCamera.WorldToViewportPoint(marker.position).y;
        return viewportY <= triggerViewportY && viewportY >= -0.2f;
    }

    void BeginBoost()
    {
        if (parallax == null) return;
        boostElapsed = 0f;
        boostActive = true;
    }
}
