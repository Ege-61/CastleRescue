using CastleRescue.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CastleRescue.UI
{
    /// <summary>
    /// Oyun kazanılınca ekranı karartıp "SON" yazısını, oyun süresini ve seçenekleri gösterir.
    /// GameManager'ın OnGameWon olayını dinler.
    /// </summary>
    public class EndScreenUI : MonoBehaviour
    {
        [SerializeField] private string title = "SON";
        [SerializeField] private string subtitle = "Prensesi kurtardın!";

        private bool isVisible;
        private float completionTime;
        private Texture2D overlayTexture;

        private void Awake()
        {
            overlayTexture = new Texture2D(1, 1);
            overlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.85f));
            overlayTexture.Apply();
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameWon += Show;
            }
            else
            {
                Debug.LogWarning("EndScreenUI: Sahnede GameManager bulunamadı.");
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null) GameManager.Instance.OnGameWon -= Show;
        }

        private void Show()
        {
            isVisible = true;
            completionTime = Time.timeSinceLevelLoad;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (!isVisible || Keyboard.current == null) return;

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                GameManager.Instance?.RestartLevel();
            }
            else if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                Debug.Log("Oyundan çıkılıyor.");
                Application.Quit(); // Editörde çalışmaz, sadece build'de kapanır.
            }
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), overlayTexture);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 72,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.91f, 0.76f, 0.44f) }
            };
            GUIStyle textStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, alignment = TextAnchor.MiddleCenter };
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter };

            float centerY = Screen.height * 0.4f;
            GUI.Label(new Rect(0, centerY - 60, Screen.width, 90), title, titleStyle);
            GUI.Label(new Rect(0, centerY + 30, Screen.width, 40), subtitle, textStyle);

            int minutes = (int)(completionTime / 60f);
            int seconds = (int)(completionTime % 60f);
            GUI.Label(new Rect(0, centerY + 75, Screen.width, 30), $"Süre: {minutes:00}:{seconds:00}", hintStyle);
            GUI.Label(new Rect(0, centerY + 120, Screen.width, 30), "R: Yeniden başla   •   Q: Çıkış", hintStyle);
        }
    }
}
