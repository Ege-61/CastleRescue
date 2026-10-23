using System;
using CastleRescue.NPC;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CastleRescue.Core
{
    /// <summary>
    /// Oyunun genel durumunu tutar: oyuncunun anahtarı var mı, oyun bitti mi.
    /// Sahnede tek bir tane olur; diğer sınıflar GameManager.Instance ile ulaşır.
    ///
    /// Gardiyanları kendisi bulup "anahtar verildi" olaylarını dinler;
    /// böylece gardiyanın GameManager'ı bilmesine gerek kalmaz.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool HasKey { get; private set; }
        public bool IsGameWon { get; private set; }

        public event Action OnKeyCollected;
        public event Action OnGameWon;

        private GuardNPC[] guards;

        private void Awake()
        {
            // Sahnede yanlışlıkla iki GameManager varsa fazlasını sil.
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Sahnede birden fazla GameManager var, fazlası silindi.");
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            guards = FindObjectsByType<GuardNPC>();
            foreach (GuardNPC guard in guards)
            {
                guard.OnKeyGiven += GiveKey;
            }
        }

        private void OnDestroy()
        {
            if (guards != null)
            {
                foreach (GuardNPC guard in guards)
                {
                    if (guard != null) guard.OnKeyGiven -= GiveKey;
                }
            }

            if (Instance == this) Instance = null;
        }

        public void GiveKey()
        {
            if (HasKey) return;
            HasKey = true;
            Debug.Log("[GameManager] Oyuncu anahtarı aldı.");
            OnKeyCollected?.Invoke();
        }

        public void WinGame()
        {
            if (IsGameWon) return;
            IsGameWon = true;
            Debug.Log("[GameManager] Oyun kazanıldı!");
            OnGameWon?.Invoke();
        }

        /// <summary>Bölümü baştan yükler. Sahne Build Settings'e eklenmiş olmalı.</summary>
        public void RestartLevel()
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            if (index < 0)
            {
                Debug.LogWarning("Sahne Build Settings'e eklenmemiş, yeniden başlatılamıyor.");
                return;
            }
            SceneManager.LoadScene(index);
        }

        // Sağ üstte anahtar göstergesi
        private void OnGUI()
        {
            if (!HasKey || IsGameWon) return;
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperRight,
                normal = { textColor = new Color(1f, 0.85f, 0.3f) }
            };
            GUI.Label(new Rect(Screen.width - 220, 15, 200, 30), "[ Anahtar ]", style);
        }
    }
}
