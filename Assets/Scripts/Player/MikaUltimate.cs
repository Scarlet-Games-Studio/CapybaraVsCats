using System.Collections;
using UnityEngine;

public class MikaUltimate : MonoBehaviour
{
    [SerializeField] float maxCharge = 100f;
    [SerializeField] float chargePerEnemy = 20f;
    [SerializeField] float duration = 3.2f;
    [SerializeField] GameObject laserPrefab;
    [SerializeField] Vector3 laserLocalOffset = new Vector3(0f, -18f, 0f);
    [SerializeField] Sprite ultimateShipSprite;
    [SerializeField] Sprite splashArt;

    public float Charge01 => Mathf.Clamp01(charge / maxCharge);
    public bool IsReady => charge >= maxCharge && !active;
    float charge;
    bool active;
    SpriteRenderer shipRenderer;
    Animator shipAnimator;
    Sprite normalSprite;

    void Awake()
    {
        shipRenderer = GetComponent<SpriteRenderer>();
        shipAnimator = GetComponent<Animator>();
        if (shipRenderer != null) normalSprite = shipRenderer.sprite;
    }

    void OnEnable() => EnemyHealth.EnemyDefeated += OnEnemyDefeated;
    void OnDisable() => EnemyHealth.EnemyDefeated -= OnEnemyDefeated;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) ActivateUltimate();
    }

    void OnEnemyDefeated(EnemyHealth enemy)
    {
        if (!active) charge = Mathf.Min(maxCharge, charge + chargePerEnemy);
    }

    public void ActivateUltimate()
    {
        if (IsReady) StartCoroutine(UltimateRoutine());
    }

    IEnumerator UltimateRoutine()
    {
        active = true;
        charge = 0f;
        UltimateSplashPresenter.Show(splashArt, duration);
        if (shipAnimator != null) shipAnimator.enabled = false;
        if (shipRenderer != null && ultimateShipSprite != null) shipRenderer.sprite = ultimateShipSprite;
        GameObject laser = laserPrefab != null ? Instantiate(laserPrefab, transform) : null;
        if (laser != null)
        {
            laser.name = "Mika Ultimate Laser";
            laser.transform.localPosition = laserLocalOffset;
            laser.transform.localRotation = Quaternion.identity;
        }
        yield return new WaitForSeconds(duration);
        if (laser != null) Destroy(laser);
        if (shipRenderer != null) shipRenderer.sprite = normalSprite;
        if (shipAnimator != null) shipAnimator.enabled = true;
        active = false;
    }
}
