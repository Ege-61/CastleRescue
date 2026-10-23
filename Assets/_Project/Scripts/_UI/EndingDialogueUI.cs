using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CastleRescue.UI
{
    /// <summary>
    /// Oyun sonundaki kısa, önceden yazılmış diyaloğu satır satır gösterir.
    /// (Gardiyandan farklı olarak burada LLM yok; bu bir ara sahne.)
    /// E, Space, Enter ya da fare tıklamasıyla bir sonraki satıra geçilir.
    /// </summary>
    public class EndingDialogueUI : MonoBehaviour
    {
        [SerializeField] private int fontSize = 22;

        [Tooltip("Harflerin ekrana yazılma hızı (saniyede harf). 0 = hepsi bir anda.")]
        [SerializeField] private float charactersPerSecond = 40f;

        public bool IsPlaying { get; private set; }

        private List<DialogueLine> lines;
        private int index;
        private float revealTimer;
        private Action onFinished;
        private Texture2D panelTexture;

        private void Awake()
        {
            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, new Color(0.05f, 0.04f, 0.03f, 0.9f));
            panelTexture.Apply();
        }

        /// <summary>Diyaloğu başlatır. Bitince onFinished çağrılır.</summary>
        public void Play(List<DialogueLine> dialogueLines, Action onFinished)
        {
            if (dialogueLines == null || dialogueLines.Count == 0)
            {
                onFinished?.Invoke();
                return;
            }

            lines = dialogueLines;
            this.onFinished = onFinished;
            index = 0;
            revealTimer = 0f;
            IsPlaying = true;
        }

        private void Update()
        {
            if (!IsPlaying) return;

            revealTimer += Time.deltaTime;

            if (AdvancePressed())
            {
                // Satır henüz tamamen yazılmadıysa önce tamamını göster, sonra ilerle.
                if (!IsLineFullyRevealed())
                {
                    revealTimer = float.MaxValue;
                }
                else
                {
                    NextLine();
                }
            }
        }

        private bool AdvancePressed()
        {
            Keyboard kb = Keyboard.current;
            Mouse ms = Mouse.current;
            bool key = kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame);
            bool click = ms != null && ms.leftButton.wasPressedThisFrame;
            return key || click;
        }

        private bool IsLineFullyRevealed()
        {
            return charactersPerSecond <= 0f || revealTimer * charactersPerSecond >= lines[index].text.Length;
        }

        private void NextLine()
        {
            index++;
            revealTimer = 0f;

            if (index >= lines.Count)
            {
                IsPlaying = false;
                Action callback = onFinished;
                onFinished = null;
                callback?.Invoke();
            }
        }

        private void OnGUI()
        {
            if (!IsPlaying) return;

            DialogueLine line = lines[index];
            int visibleCount = charactersPerSecond <= 0f
                ? line.text.Length
                : Mathf.Min(line.text.Length, (int)(revealTimer * charactersPerSecond));
            string visibleText = line.text.Substring(0, visibleCount);

            float margin = 40f;
            float height = 170f;
            Rect panel = new Rect(margin, Screen.height - height - margin, Screen.width - margin * 2, height);
            GUI.DrawTexture(panel, panelTexture);

            GUIStyle speakerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize + 2,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.91f, 0.76f, 0.44f) }
            };
            GUIStyle textStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize, wordWrap = true };
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize - 6, alignment = TextAnchor.LowerRight };

            GUI.Label(new Rect(panel.x + 20, panel.y + 12, panel.width - 40, 32), line.speaker, speakerStyle);
            GUI.Label(new Rect(panel.x + 20, panel.y + 50, panel.width - 40, 90), visibleText, textStyle);
            GUI.Label(new Rect(panel.x + 20, panel.yMax - 30, panel.width - 40, 22),
                $"{index + 1}/{lines.Count}   •   E / Space: devam", hintStyle);
        }
    }
}
