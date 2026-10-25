using CastleRescue.Core;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Oyuncu içine girince ölmesine sebep olan bölge (dikenli çukur, su, kazıklar...).
    /// Collider'ı "Is Trigger" olmalı. Ölüm işini GameManager yapar.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        [SerializeField] private string deathMessage = "Dikenli çukura düştün!";

        private void Reset()
        {
            // Bileşen ilk eklendiğinde collider'ı otomatik trigger yap.
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.KillPlayer(deathMessage);
            }
            else
            {
                Debug.LogWarning("KillZone: Sahnede GameManager yok!");
            }
        }
    }
}
