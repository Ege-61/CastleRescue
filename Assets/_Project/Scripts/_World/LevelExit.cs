using UnityEngine;
using UnityEngine.SceneManagement;

namespace CastleRescue.World
{
    /// <summary>
    /// Oyuncu içine girince bir sonraki bölümü yükleyen bölge.
    /// Collider'ı "Is Trigger" olmalı. Yüklenecek sahne Build Profiles'daki sahne listesinde olmalı.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class LevelExit : MonoBehaviour
    {
        [Tooltip("Yüklenecek sahnenin adı (birebir aynı yazılmalı).")]
        [SerializeField] private string nextSceneName = "Level3_Avlu";

        private bool isLoading;
        private string errorMessage = "";

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isLoading || !other.CompareTag("Player")) return;

            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                errorMessage = $"'{nextSceneName}' sahnesi bulunamadı. Build Profiles'a eklendi mi?";
                Debug.LogError(errorMessage);
                return;
            }

            isLoading = true;
            Debug.Log($"[LevelExit] {nextSceneName} yükleniyor.");
            SceneManager.LoadScene(nextSceneName);
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(errorMessage)) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 20, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, 60, Screen.width, 30), errorMessage, style);
        }
    }
}
