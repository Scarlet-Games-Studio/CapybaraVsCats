using UnityEngine;
using UnityEngine.UI;

public class XPManager : MonoBehaviour
{
    public static XPManager instance;

    public int currentXP = 0;
    public int currentLevel = 1;
    public int xpToNextLevel = 100;
    public Text xpText;
    public Text levelText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    void LevelUp()
    {
        currentXP -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.5f); // Aumenta o XP necessário para o próximo nível
        ApplyLevelUpRewards();
    }

    void ApplyLevelUpRewards()
    {
        // Recompensas por subir de nível, como aumentar a saúde ou a taxa de disparo
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.IncreaseFireRate(0.1f); // Exemplo: aumenta a taxa de disparo
            player.GetComponent<Health>().Heal(20); // Exemplo: cura o jogador
        }
    }

    void UpdateUI()
    {
        if (xpText != null)
        {
            xpText.text = "XP: " + currentXP + "/" + xpToNextLevel;
        }
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
        }
    }
}