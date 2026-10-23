using CastleRescue.Companion;
using CastleRescue.Core;
using UnityEngine;

namespace CastleRescue.Player
{
    /// <summary>
    /// Oyuncunun fareye emir vermesini sağlar.
    /// Her karede oyuncunun baktığı yöne bir ışın atar. Işın, farenin kullanabileceği bir hedefe
    /// (şalter) çarpıyorsa ekranda "[F] Fareyi gönder" yazar. F'ye basılınca fare o hedefe gider.
    /// </summary>
    public class PlayerCommander : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa sahnedeki fare otomatik bulunur.")]
        [SerializeField] private CompanionController companion;

        [Tooltip("Boş bırakılırsa ana kamera kullanılır.")]
        [SerializeField] private Transform cameraTransform;

        [Tooltip("Bakılan hedefin en fazla ne kadar uzakta olabileceği.")]
        [SerializeField] private float aimRange = 40f;

        [Tooltip("Nişan alma kalınlığı. Küçük hedefleri tutturmayı kolaylaştırır.")]
        [SerializeField] private float aimRadius = 0.3f;

        private IPlayerInput input;
        private IInteractable aimedTarget;
        private string feedback = "";
        private float feedbackTimer;

        private void Awake()
        {
            input = GetComponent<IPlayerInput>();
            if (input == null)
            {
                Debug.LogError($"{name}: IPlayerInput bulunamadı!");
                enabled = false;
                return;
            }

            if (companion == null) companion = FindAnyObjectByType<CompanionController>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
        }

        private void Update()
        {
            if (feedbackTimer > 0f) feedbackTimer -= Time.deltaTime;

            // Girdi kapalıysa (örn. gardiyanla konuşurken) hedef gösterme.
            aimedTarget = input.IsEnabled ? FindAimedTarget() : null;

            if (!input.CommandCompanionPressed || aimedTarget == null) return;

            if (companion == null)
            {
                ShowFeedback("Sahnede fare yok!");
                return;
            }

            if (companion.CommandInteract(aimedTarget))
            {
                ShowFeedback($"Fare, {aimedTarget.DisplayName} için yola çıktı!");
            }
            else
            {
                ShowFeedback("Fare şu an meşgul.");
            }
        }

        /// <summary>
        /// Kameranın baktığı yöne kalın bir ışın (SphereCast) atar.
        /// Çarptığı obje kullanılabilir bir hedefse onu döndürür, değilse null.
        /// </summary>
        private IInteractable FindAimedTarget()
        {
            if (cameraTransform == null) return null;

            if (Physics.SphereCast(cameraTransform.position, aimRadius, cameraTransform.forward, out RaycastHit hit,
                    aimRange, ~0, QueryTriggerInteraction.Ignore))
            {
                IInteractable target = hit.collider.GetComponentInParent<IInteractable>();
                if (target != null && target.CanInteract) return target;
            }
            return null;
        }

        private void ShowFeedback(string message)
        {
            feedback = message;
            feedbackTimer = 3f;
            Debug.Log($"[Commander] {message}");
        }

        private void OnGUI()
        {
            if (!input.IsEnabled) return;

            float cx = Screen.width / 2f;
            float cy = Screen.height / 2f;

            // Nişangah: hedefe bakılıyorsa sarı, değilse beyaz küçük nokta
            GUIStyle crosshairStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = aimedTarget != null ? Color.yellow : Color.white }
            };
            GUI.Label(new Rect(cx - 15, cy - 15, 30, 30), "•", crosshairStyle);

            GUIStyle promptStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            // Hedefe bakılıyorsa ve fare boştaysa ipucu göster
            bool companionFree = companion != null && !companion.IsBusy;
            if (aimedTarget != null && companionFree && feedbackTimer <= 0f)
            {
                GUI.Label(new Rect(0, cy + 30, Screen.width, 40), $"[F] Fareyi gönder: {aimedTarget.DisplayName}", promptStyle);
            }

            if (feedbackTimer > 0f)
            {
                GUIStyle feedbackStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
                GUI.Label(new Rect(0, Screen.height - 80, Screen.width, 40), feedback, feedbackStyle);
            }
        }
    }
}
