using System.Collections;
using System.Collections.Generic;
using CastleRescue.Core;
using CastleRescue.Player;
using CastleRescue.UI;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Prensesin kilitli kafesi. Oyuncu yaklaşıp E'ye basınca:
    ///  - Anahtarı yoksa "kafes kilitli" uyarısı verir
    ///  - Anahtarı varsa kafes kapısını açar, son diyaloğu oynatır ve oyunu bitirir
    /// Kafes kapısı için gardiyanın kapısında da kullandığımız Door sınıfını tekrar kullanıyoruz.
    /// </summary>
    public class PrincessCage : MonoBehaviour
    {
        [SerializeField] private Door cageDoor;

        [Tooltip("Boş bırakılırsa sahnede otomatik bulunur.")]
        [SerializeField] private EndingDialogueUI endingUI;

        [SerializeField] private float interactDistance = 2.5f;

        [Tooltip("Kapı açıldıktan sonra diyalog başlamadan önce beklenecek süre.")]
        [SerializeField] private float delayBeforeDialogue = 1.5f;

        [SerializeField]
        private List<DialogueLine> endingLines = new List<DialogueLine>
        {
            new DialogueLine("Prenses", "Sonunda biri geldi! Seni Kral'ın adamları mı gönderdi?"),
            new DialogueLine("Sen", "Hayır, kendi isteğimle geldim. Seni buradan çıkaracağım."),
            new DialogueLine("Prenses", "Peki ya şu ayağının dibindeki küçük dostun? O bir fare mi?"),
            new DialogueLine("Fare", "Cik! Cik cik!"),
            new DialogueLine("Sen", "Onun yardımı olmasaydı avludaki şalteri asla indiremezdim."),
            new DialogueLine("Prenses", "O halde ikinize de borçluyum. Hadi, gün doğmadan bu kaleden çıkalım!")
        };

        private Transform player;
        private IPlayerInput playerInput;
        private bool isRescuing;
        private bool isDone;
        private string feedback = "";
        private float feedbackTimer;

        private bool PlayerInRange =>
            player != null && Vector3.Distance(player.position, transform.position) <= interactDistance;

        private void Awake()
        {
            if (endingUI == null) endingUI = FindAnyObjectByType<EndingDialogueUI>();

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerInput = playerObject.GetComponent<IPlayerInput>();
            }

            if (player == null || playerInput == null || cageDoor == null || endingUI == null)
            {
                Debug.LogError($"{name}: Player, Cage Door ya da EndingDialogueUI eksik!");
                enabled = false;
            }
        }

        private void Update()
        {
            if (feedbackTimer > 0f) feedbackTimer -= Time.deltaTime;
            if (isRescuing || isDone) return;

            if (PlayerInRange && playerInput.InteractPressed)
            {
                TryOpen();
            }
        }

        private void TryOpen()
        {
            bool hasKey = GameManager.Instance != null && GameManager.Instance.HasKey;
            if (!hasKey)
            {
                feedback = "Kafes kilitli. Anahtara ihtiyacın var.";
                feedbackTimer = 2.5f;
                return;
            }

            StartCoroutine(RescueRoutine());
        }

        private IEnumerator RescueRoutine()
        {
            isRescuing = true;
            playerInput.IsEnabled = false; // ara sahnede oyuncu yürümesin

            cageDoor.Open();
            yield return new WaitForSeconds(delayBeforeDialogue);

            endingUI.Play(endingLines, OnEndingFinished);
        }

        private void OnEndingFinished()
        {
            isRescuing = false;
            isDone = true;
            GameManager.Instance?.WinGame();
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            if (feedbackTimer > 0f)
            {
                GUI.Label(new Rect(0, Screen.height * 0.6f + 40, Screen.width, 40), feedback, style);
            }

            if (!isRescuing && !isDone && PlayerInRange)
            {
                GUI.Label(new Rect(0, Screen.height * 0.6f, Screen.width, 40), "[E] Kafesi aç", style);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, interactDistance);
        }
    }
}
