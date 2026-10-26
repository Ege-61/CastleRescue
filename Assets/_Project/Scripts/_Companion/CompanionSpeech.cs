using System.Collections.Generic;
using CastleRescue.UI;
using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Farenin (ve oyuncunun) kısa konuşmalarını ekranın üst kısmında altyazı gibi gösterir.
    /// Satırlar sıraya girer ve her biri okunacak kadar süre ekranda kalıp kendiliğinden geçer;
    /// oyuncu bu sırada oynamaya devam eder.
    ///
    /// Konuşma noktaları (CompanionBarkTrigger) ve gardiyan yorumcusu bu sınıfı kullanır.
    /// </summary>
    public class CompanionSpeech : MonoBehaviour
    {
        [Tooltip("Farenin adı. Konuşmalarda bu ad görünür.")]
        [SerializeField] private string companionName = "Fındık";

        [Tooltip("Her satırın ekranda kalacağı en kısa süre.")]
        [SerializeField] private float minLineDuration = 2f;

        [Tooltip("Satır uzunluğuna göre eklenen süre (harf başına saniye).")]
        [SerializeField] private float secondsPerCharacter = 0.05f;

        [SerializeField] private int fontSize = 22;

        public string CompanionName => companionName;
        public bool IsSpeaking => currentLine != null;

        private readonly Queue<DialogueLine> queue = new Queue<DialogueLine>();
        private DialogueLine currentLine;
        private float lineTimer;
        private Texture2D backgroundTexture;

        private void Awake()
        {
            backgroundTexture = new Texture2D(1, 1);
            backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.6f));
            backgroundTexture.Apply();
        }

        /// <summary>Farenin tek bir cümle söylemesi.</summary>
        public void Say(string text)
        {
            Enqueue(new DialogueLine(companionName, text));
        }

        /// <summary>Birden fazla satırı (fare ve oyuncu karışık olabilir) sırayla gösterir.</summary>
        public void Say(IEnumerable<DialogueLine> lines)
        {
            foreach (DialogueLine line in lines)
            {
                // Konuşan adı boş bırakıldıysa fare konuşuyor sayılır.
                string speaker = string.IsNullOrWhiteSpace(line.speaker) ? companionName : line.speaker;
                Enqueue(new DialogueLine(speaker, line.text));
            }
        }

        private void Enqueue(DialogueLine line)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.text)) return;
            queue.Enqueue(line);
            Debug.Log($"[Speech] {line.speaker}: {line.text}");
        }

        private void Update()
        {
            if (currentLine != null)
            {
                lineTimer -= Time.deltaTime;
                if (lineTimer > 0f) return;
                currentLine = null;
            }

            if (queue.Count > 0)
            {
                currentLine = queue.Dequeue();
                lineTimer = minLineDuration + currentLine.text.Length * secondsPerCharacter;
            }
        }

        private void OnGUI()
        {
            if (currentLine == null) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                wordWrap = true,
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };

            string text = $"<color=#9FE870><b>{currentLine.speaker}:</b></color> {currentLine.text}";
            float width = Mathf.Min(Screen.width - 40f, 900f);
            float height = style.CalcHeight(new GUIContent(text), width - 30f) + 20f;
            Rect box = new Rect((Screen.width - width) / 2f, 80f, width, height);

            GUI.DrawTexture(box, backgroundTexture);
            GUI.Label(new Rect(box.x + 15f, box.y + 10f, box.width - 30f, box.height - 20f), text, style);
        }
    }
}
