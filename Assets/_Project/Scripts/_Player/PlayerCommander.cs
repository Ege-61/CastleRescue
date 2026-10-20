using CastleRescue.Companion;
using CastleRescue.Core;
using UnityEngine;

namespace CastleRescue.Player
{
    /// <summary>
    /// Oyuncunun fareye emir vermesini sağlar.
    /// F tuşuna basınca:
    ///  1) Önce oyuncunun baktığı yere ışın atar; bir şaltere bakıyorsa fareyi oraya yollar.
    ///  2) Hiçbir şeye bakmıyorsa yakındaki kullanılabilir en yakın hedefi seçer.
    /// </summary>
    public class PlayerCommander : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa sahnedeki fare otomatik bulunur.")]
        [SerializeField] private CompanionController companion;

        [Tooltip("Boş bırakılırsa ana kamera kullanılır.")]
        [SerializeField] private Transform cameraTransform;

        [Tooltip("Bakılan hedefin en fazla ne kadar uzakta olabileceği.")]
        [SerializeField] private float aimRange = 40f;

        [Tooltip("Bakılan bir hedef yoksa, bu yarıçap içindeki en yakın hedef seçilir.")]
        [SerializeField] private float searchRadius = 20f;

        private IPlayerInput input;
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

            if (!input.CommandCompanionPressed) return;

            if (companion == null)
            {
                ShowFeedback("Sahnede fare yok!");
                return;
            }

            IInteractable target = FindAimedTarget() ?? FindNearestTarget();
            if (target == null)
            {
                ShowFeedback("Farenin yapabileceği bir görev yok.");
                return;
            }

            if (companion.CommandInteract(target))
            {
                ShowFeedback($"Fare, {target.DisplayName} için yola çıktı!");
            }
            else
            {
                ShowFeedback("Fare şu an meşgul.");
            }
        }

        /// <summary>Kameranın baktığı yöne ışın atar.</summary>
        private IInteractable FindAimedTarget()
        {
            if (cameraTransform == null) return null;

            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, aimRange,
                    ~0, QueryTriggerInteraction.Ignore))
            {
                IInteractable target = hit.collider.GetComponentInParent<IInteractable>();
                if (target != null && target.CanInteract) return target;
            }
            return null;
        }

        /// <summary>Çevredeki kullanılabilir en yakın hedefi bulur.</summary>
        private IInteractable FindNearestTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, searchRadius, ~0, QueryTriggerInteraction.Collide);

            IInteractable nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (Collider hit in hits)
            {
                IInteractable candidate = hit.GetComponentInParent<IInteractable>();
                if (candidate == null || !candidate.CanInteract) continue;

                float distance = Vector3.Distance(transform.position, candidate.InteractionPoint.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = candidate;
                }
            }
            return nearest;
        }

        private void ShowFeedback(string message)
        {
            feedback = message;
            feedbackTimer = 3f;
            Debug.Log($"[Commander] {message}");
        }

        private void OnGUI()
        {
            if (feedbackTimer <= 0f) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            GUI.Label(new Rect(0, Screen.height - 80, Screen.width, 40), feedback, style);
        }
    }
}
