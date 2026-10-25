using System;
using CastleRescue.NPC;
using CastleRescue.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CastleRescue.Core
{
    /// <summary>
    /// Oyunun genel durumunu tutar: oyuncunun anahtarı var mı, oyun bitti mi, oyuncu öldü mü.
    /// Sahnede tek bir tane olur; diğer sınıflar GameManager.Instance ile ulaşır.
    ///
    /// Gardiyanları kendisi bulup "anahtar verildi" olaylarını dinler;
    /// böylece gardiyanın GameManager'ı bilmesine gerek kalmaz.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Tooltip("Oyuncu öldüğünde ekranda mesajın kaç saniye kalacağı.")]
        [SerializeField] private float deathMessageDuration = 2.5f;

        public bool HasKey { get; private set; }
        public bool IsGameWon { get; private set; }
        public int DeathCount { get; private set; }

        public event Action OnKeyCollected;
        public event Action OnGameWon;
        /// <summary>Oyuncu öldüğünde (ölüm sebebi).</summary>
        public event Action<string> OnPlayerDied;

        private GuardNPC[] guards;
        private PlayerMovement playerMovement;
        private Vector3 playerStartPosition;
        private string deathMessage = "";
        private float deathMessageTimer;
        private Texture2D flashTexture;

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

            flashTexture = new Texture2D(1, 1);
            flashTexture.SetPixel(0, 0, Color.white);
            flashTexture.Apply();
        }

        private void Start()
        {
            guards = FindObjectsByType<GuardNPC>();
            foreach (GuardNPC guard in guards)
            {
                guard.OnKeyGiven += GiveKey;
            }

            // Oyuncunun başlangıç noktasını hatırla; öldüğünde buraya döner.
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerMovement = playerObject.GetComponent<PlayerMovement>();
                playerStartPosition = playerObject.transform.position;
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

        private void Update()
        {
            if (deathMessageTimer > 0f) deathMessageTimer -= Time.deltaTime;
        }

        public void GiveKey()
        {
            if (HasKey) return;
            HasKey = true;
            Debug.Log("[GameManager] Oyuncu anahtarı aldı.");
            OnKeyCollected?.Invoke();
        }

        /// <summary>
        /// Oyuncuyu öldürür: bölümün başına geri gönderir ve ekranda sebebi gösterir.
        /// Dikenli çukur, yaratık, suya düşme gibi her şey bu metodu çağırır.
        /// </summary>
        public void KillPlayer(string reason)
        {
            if (IsGameWon || playerMovement == null) return;

            DeathCount++;
            Debug.Log($"[GameManager] Oyuncu öldü ({DeathCount}. kez): {reason}");

            playerMovement.Teleport(playerStartPosition);
            deathMessage = reason;
            deathMessageTimer = deathMessageDuration;
            OnPlayerDied?.Invoke(reason);
        }

        public void WinGame()
        {
            if (IsGameWon) return;
            IsGameWon = true;
            Debug.Log("[GameManager] Oyun kazanıldı!");
            OnGameWon?.Invoke();
        }

        /// <summary>Bölümü baştan yükler. Sahne Build Profiles'daki listeye eklenmiş olmalı.</summary>
        public void RestartLevel()
        {
            int index = SceneManager.GetActiveScene().buildIndex;
            if (index < 0)
            {
                Debug.LogWarning("Sahne Build Profiles'a eklenmemiş, yeniden başlatılamıyor.");
                return;
            }
            SceneManager.LoadScene(index);
        }

        private void OnGUI()
        {
            // Ölüm: kırmızı ekran ve mesaj
            if (deathMessageTimer > 0f)
            {
                float alpha = Mathf.Clamp01(deathMessageTimer / deathMessageDuration) * 0.5f;
                Color oldColor = GUI.color;
                GUI.color = new Color(0.6f, 0f, 0f, alpha);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), flashTexture);
                GUI.color = oldColor;

                GUIStyle deathStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 32,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 50), deathMessage, deathStyle);
            }

            // Sağ üstte anahtar göstergesi
            if (HasKey && !IsGameWon)
            {
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
}
