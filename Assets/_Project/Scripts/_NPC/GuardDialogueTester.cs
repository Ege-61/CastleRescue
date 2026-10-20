using System.Collections.Generic;
using UnityEngine;

namespace CastleRescue.NPC
{
    /// <summary>
    /// SADECE TEST İÇİN basit sohbet ekranı. Asıl, güzel diyalog arayüzünü sonraki adımda yapacağız.
    /// Yazı kutusuna yazıp Enter'a (ya da Gönder'e) basarak gardiyanla konuşabilirsiniz.
    /// </summary>
    [RequireComponent(typeof(GuardNPC))]
    public class GuardDialogueTester : MonoBehaviour
    {
        private GuardNPC guard;
        private readonly List<string> log = new List<string>();
        private string input = "";
        private string result = "";
        private Vector2 scroll;

        private void Awake()
        {
            guard = GetComponent<GuardNPC>();
        }

        // Event'lere abone ol / aboneliği bırak. (Bırakmazsak obje silinince hata verebilir.)
        private void OnEnable()
        {
            guard.OnReplied += HandleReply;
            guard.OnErrorOccurred += HandleError;
            guard.OnKeyGiven += HandleKeyGiven;
            guard.OnAttacked += HandleAttacked;
        }

        private void OnDisable()
        {
            guard.OnReplied -= HandleReply;
            guard.OnErrorOccurred -= HandleError;
            guard.OnKeyGiven -= HandleKeyGiven;
            guard.OnAttacked -= HandleAttacked;
        }

        private void Start()
        {
            log.Add($"<b>{guard.GuardName}:</b> {guard.OpeningLine}");
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void HandleReply(string reply, int trust) => log.Add($"<b>{guard.GuardName}:</b> {reply}");
        private void HandleError(string message) => log.Add($"<color=orange>{message}</color>");
        private void HandleKeyGiven() => result = "<color=lime>ANAHTAR ALINDI! Kapı açılıyor.</color>";
        private void HandleAttacked() => result = "<color=red>GARDİYAN SALDIRDI!</color>";

        private void OnGUI()
        {
            GUIStyle text = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true, richText = true };
            GUIStyle field = new GUIStyle(GUI.skin.textField) { fontSize = 18 };
            GUIStyle button = new GUIStyle(GUI.skin.button) { fontSize = 18 };

            float w = Screen.width - 40;
            float h = Screen.height;

            // Üst bilgi: güven ve durum
            GUI.Label(new Rect(20, 10, w, 30), $"Güven: {guard.Trust}/100   {result}", text);

            // Sohbet geçmişi (kaydırılabilir)
            Rect viewRect = new Rect(20, 45, w, h - 140);
            Rect contentRect = new Rect(0, 0, w - 20, Mathf.Max(viewRect.height, log.Count * 60));
            scroll = GUI.BeginScrollView(viewRect, scroll, contentRect);
            float y = 0;
            foreach (string line in log)
            {
                float lineHeight = text.CalcHeight(new GUIContent(line), contentRect.width);
                GUI.Label(new Rect(0, y, contentRect.width, lineHeight), line, text);
                y += lineHeight + 8;
            }
            GUI.EndScrollView();

            // "Düşünüyor..." göstergesi
            if (guard.IsThinking)
            {
                int dots = (int)(Time.time * 3f) % 4;
                GUI.Label(new Rect(20, h - 90, w, 30), $"<i>{guard.GuardName} düşünüyor{new string('.', dots)}</i>", text);
            }

            if (guard.IsConversationOver) return;

            // Yazı kutusu + Enter ile gönderme
            bool enterPressed = Event.current.type == EventType.KeyDown &&
                                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            GUI.SetNextControlName("PlayerInput");
            input = GUI.TextField(new Rect(20, h - 55, w - 130, 40), input, 300, field);
            bool sendClicked = GUI.Button(new Rect(w - 100, h - 55, 120, 40), "Gönder", button);

            if ((enterPressed || sendClicked) && !guard.IsThinking && !string.IsNullOrWhiteSpace(input))
            {
                log.Add($"<b>Sen:</b> {input}");
                guard.SendPlayerMessage(input);
                input = "";
                scroll.y = float.MaxValue; // en alta kaydır
                Event.current.Use();
            }

            GUI.FocusControl("PlayerInput");
        }
    }
}
