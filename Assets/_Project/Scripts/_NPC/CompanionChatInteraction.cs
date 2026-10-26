using CastleRescue.Player;
using CastleRescue.UI;
using UnityEngine;

namespace CastleRescue.NPC
{
    /// <summary>
    /// T tuşuna basınca fareyle sohbet ekranını açar, Esc ile kapatır.
    /// GuardInteraction ile aynı mantık: oyuncuyu durdurur, imleci açar, konuşma ekranını açar.
    /// </summary>
    [RequireComponent(typeof(CompanionChat))]
    public class CompanionChatInteraction : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa sahnede otomatik bulunur.")]
        [SerializeField] private DialogueUI dialogueUI;

        private CompanionChat chat;
        private IPlayerInput playerInput;
        private PlayerLook playerLook;
        private bool isTalking;

        private void Awake()
        {
            chat = GetComponent<CompanionChat>();
            if (dialogueUI == null) dialogueUI = FindAnyObjectByType<DialogueUI>();
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerInput = playerObject.GetComponent<IPlayerInput>();
                playerLook = playerObject.GetComponent<PlayerLook>();
            }

            if (dialogueUI == null || playerInput == null)
            {
                Debug.LogError($"{name}: DialogueUI ya da Player bulunamadı!");
                enabled = false;
                return;
            }

            dialogueUI.OnCloseRequested += HandleCloseRequested;
        }

        private void OnDestroy()
        {
            if (dialogueUI != null) dialogueUI.OnCloseRequested -= HandleCloseRequested;
        }

        private void Update()
        {
            // Başka bir konuşma (örn. gardiyan) açıksa oyuncu girdisi zaten kapalıdır, T çalışmaz.
            if (isTalking || dialogueUI.IsOpen) return;

            if (playerInput.TalkToCompanionPressed)
            {
                StartTalking();
            }
        }

        private void StartTalking()
        {
            isTalking = true;
            playerInput.IsEnabled = false;
            playerLook?.SetCursorLocked(false);
            dialogueUI.Open(chat);
        }

        private void HandleCloseRequested()
        {
            // Konuşma ekranını gardiyan da kullanıyor; sadece fareyle konuşuluyorsa kapat.
            if (!isTalking) return;

            isTalking = false;
            dialogueUI.Close();
            playerInput.IsEnabled = true;
            playerLook?.SetCursorLocked(true);
        }

        private void OnGUI()
        {
            if (isTalking || dialogueUI == null || dialogueUI.IsOpen) return;

            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.LowerLeft };
            GUI.Label(new Rect(20, Screen.height - 40, 400, 30), $"[T] {chat.DisplayName} ile konuş", style);
        }
    }
}
