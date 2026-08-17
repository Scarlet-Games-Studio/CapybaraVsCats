using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UltimateButton : MonoBehaviour
{
    [SerializeField] Image chargeFill;
    [SerializeField] TMP_Text label;
    Button button;
    MikaUltimate ultimate;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(Activate);
    }

    void Update()
    {
        if (ultimate == null) ultimate = FindAnyObjectByType<MikaUltimate>();
        bool hasMika = ultimate != null;
        gameObject.SetActive(hasMika);
        if (!hasMika) return;
        chargeFill.fillAmount = ultimate.Charge01;
        button.interactable = ultimate.IsReady;
        label.text = ultimate.IsReady ? "ULTIMATE!" : Mathf.RoundToInt(ultimate.Charge01 * 100f) + "%";
    }

    void Activate() => ultimate?.ActivateUltimate();
}
