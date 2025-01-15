using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType { Health, FireRate, Speed }
    public PowerUpType powerUpType;
    public int value = 20; // Valor do power-up (pode ser saúde ou incremento de outra característica)

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyPowerUp(player);
            }
            Destroy(gameObject);
        }
    }

    void ApplyPowerUp(PlayerController player)
    {
        switch (powerUpType)
        {
            case PowerUpType.Health:
                player.GetComponent<Health>().Heal(value);
                break;
            case PowerUpType.FireRate:
                player.IncreaseFireRate(0.1f * value); // Ajuste conforme necessário
                break;
            case PowerUpType.Speed:
                player.IncreaseSpeed(0.1f * value); // Ajuste conforme necessário
                break;
        }
    }
}