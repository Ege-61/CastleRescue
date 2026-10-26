using System.Collections;
using CastleRescue.Player;
using CastleRescue.UI;
using UnityEngine;
using UnityEngine.Events;

namespace CastleRescue.NPC
{
    /// <summary>
    /// Gardiyan ile oyun dünyası arasındaki köprü:
    ///  - Oyuncu yaklaşınca "E - Konuş" yazısını gösterir
    ///  - E'ye basınca oyuncuyu durdurur, imleci açar ve konuşma ekranını açar
    ///  - Anahtar verilirse onKeyGiven olayını tetikler (Inspector'dan kapıya bağlanır)
    ///  - Saldırı olursa oyuncuyu geri atar ve sohbeti sıfırlar, oyuncu tekrar deneyebilir
    /// </summary>
    [RequireComponent(typeof(GuardNPC))]
    public class GuardInteraction : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa sahnede otomatik bulunur.")]
        [SerializeField] private DialogueUI dialogueUI;

        [Tooltip("Oyuncu bu mesafeden yakınsa konuşabilir.")]
        [SerializeField] private float talkDistance = 3f;

        [Tooltip("Gardiyan saldırınca oyuncunun ışınlanacağı nokta.")]
        [SerializeField] private Transform respawnPoint;

        [Tooltip("Konuşma bittikten (anahtar ya da saldırı) sonra ekranın kapanmadan önce bekleyeceği süre.")]
        [SerializeField] private float endDelay = 3f;

        [Header("Olaylar")]
        [Tooltip("Anahtar verilince çalışır. Buraya kapının Open metodunu bağlayın.")]
        [SerializeField] private UnityEvent onKeyGiven;

        [Tooltip("Gardiyan saldırınca çalışır (ses, efekt vb. için).")]
        [SerializeField] private UnityEvent onAttacked;

        private GuardNPC guard;
        private Transform player;
        private IPlayerInput playerInput;
        private PlayerLook playerLook;
        private PlayerMovement playerMovement;
        private bool isTalking;
        private bool isEnding;

        private bool PlayerInRange =>
            player != null && Vector3.Distance(player.position, transform.position) <= talkDistance;

        private void Awake()
        {
            guard = GetComponent<GuardNPC>();
            if (dialogueUI == null) dialogueUI = FindAnyObjectByType<DialogueUI>();

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerInput = playerObject.GetComponent<IPlayerInput>();
                playerLook = playerObject.GetComponent<PlayerLook>();
                playerMovement = playerObject.GetComponent<PlayerMovement>();
            }

            if (dialogueUI == null || player == null || playerInput == null)
            {
                Debug.LogError($"{name}: DialogueUI ya da Player bulunamadı! Sahnede UI objesi ve 'Player' etiketli oyuncu olmalı.");
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (!enabled) return;
            guard.OnKeyGiven += HandleKeyGiven;
            guard.OnAttacked += HandleAttacked;
            dialogueUI.OnCloseRequested += HandleCloseRequested;
        }

        private void OnDisable()
        {
            if (guard != null)
            {
                guard.OnKeyGiven -= HandleKeyGiven;
                guard.OnAttacked -= HandleAttacked;
            }
            if (dialogueUI != null) dialogueUI.OnCloseRequested -= HandleCloseRequested;
        }

        private void Update()
        {
            if (isTalking || guard.IsConversationOver) return;

            if (PlayerInRange && playerInput.InteractPressed)
            {
                StartTalking();
            }
        }

        private void StartTalking()
        {
            isTalking = true;
            playerInput.IsEnabled = false;            // oyuncu yazarken yürümesin
            playerLook?.SetCursorLocked(false);       // imleç görünsün
            dialogueUI.Open(guard);
        }

        private void StopTalking()
        {
            isTalking = false;
            dialogueUI.Close();
            playerInput.IsEnabled = true;
            playerLook?.SetCursorLocked(true);
        }

        private void HandleCloseRequested()
        {
            // Konuşma ekranını fare de kullanıyor; sadece gardiyanla konuşuluyorsa kapat.
            if (!isTalking) return;

            // Konuşmanın sonu bekleniyorsa (anahtar/saldırı) Esc ile erken kapatmaya izin verme.
            if (isEnding) return;
            StopTalking();
        }

        private void HandleKeyGiven()
        {
            Debug.Log("[GuardInteraction] Anahtar verildi, kapı açılıyor.");
            dialogueUI.SetStatus($"<color=#7CFC00><b>{guard.GuardName} anahtarı sana uzattı. Kapı açılıyor!</b></color>");
            onKeyGiven?.Invoke();
            StartCoroutine(EndConversationRoutine(false));
        }

        private void HandleAttacked()
        {
            Debug.Log("[GuardInteraction] Gardiyan saldırdı, oyuncu geri atılıyor.");
            dialogueUI.SetStatus($"<color=#FF4040><b>{guard.GuardName} öfkeyle sana saldırdı!</b></color>");
            onAttacked?.Invoke();
            StartCoroutine(EndConversationRoutine(true));
        }

        private IEnumerator EndConversationRoutine(bool wasAttacked)
        {
            isEnding = true;
            yield return new WaitForSeconds(endDelay); // oyuncu son cümleyi okuyabilsin
            StopTalking();

            if (wasAttacked)
            {
                if (respawnPoint != null && playerMovement != null)
                {
                    playerMovement.Teleport(respawnPoint.position);
                }

                // Oyuncu tekrar deneyebilsin diye sohbeti sıfırla.
                guard.ResetConversation();
                dialogueUI.ClearLog();
            }

            isEnding = false;
        }

        private void OnGUI()
        {
            if (isTalking || guard.IsConversationOver || !PlayerInRange) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            GUI.Label(new Rect(0, Screen.height * 0.6f, Screen.width, 40), $"[E] {guard.GuardName} ile konuş", style);
        }

        // Scene ekranında konuşma mesafesini sarı çember olarak gösterir.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, talkDistance);
        }
    }
}
