using System;
using System.Collections.Generic;
using CastleRescue.Core;
using UnityEngine;

namespace CastleRescue.UI
{
    /// <summary>
    /// Konuşma ekranı. Ekranın altında yarı saydam bir panel çizer:
    /// konuşulan karakterin adı, (varsa) güven çubuğu, konuşma geçmişi,
    /// "düşünüyor..." göstergesi ve yazı kutusu.
    ///
    /// Kiminle konuşulduğunu bilmez; IConversationPartner arayüzünü uygulayan herkesle
    /// (gardiyan, fare...) çalışır. Oyuncuyu durdurmak, kapı açmak gibi işleri de bilmez;
    /// onları GuardInteraction ve CompanionChatInteraction yapar. (SOLID - Single Responsibility)
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private int fontSize = 20;

        [Tooltip("Panelin ekran yüksekliğine oranı.")]
        [SerializeField, Range(0.2f, 0.8f)] private float panelHeightRatio = 0.45f;

        /// <summary>Oyuncu Esc'ye bastığında. Konuşmayı açan sınıf dinler ve kapatır.</summary>
        public event Action OnCloseRequested;

        public bool IsOpen { get; private set; }

        /// <summary>Şu an ekranda konuşulan karakter.</summary>
        public IConversationPartner CurrentPartner => partner;

        private IConversationPartner partner;
        private readonly List<string> lines = new List<string>();
        private string playerInput = "";
        private string statusLine = "";
        private Vector2 scroll;
        private bool scrollToBottom;

        // Renkli kutular çizmek için 1x1 piksellik dokular.
        private Texture2D panelTexture;
        private Texture2D barBackTexture;
        private Texture2D barFillTexture;

        private void Awake()
        {
            panelTexture = MakeTexture(new Color(0.05f, 0.04f, 0.03f, 0.85f));
            barBackTexture = MakeTexture(new Color(0.25f, 0.25f, 0.25f, 1f));
            barFillTexture = MakeTexture(Color.white);
        }

        /// <summary>Konuşma ekranını açar ve karakterin olaylarını dinlemeye başlar.</summary>
        public void Open(IConversationPartner targetPartner)
        {
            if (IsOpen || targetPartner == null) return;

            // Farklı bir karakterse ya da ekran boşsa baştan başla.
            if (partner != targetPartner || lines.Count == 0)
            {
                partner = targetPartner;
                ClearLog();
            }

            partner.OnReplied += HandleReply;
            partner.OnErrorOccurred += HandleError;

            IsOpen = true;
            scrollToBottom = true;
        }

        /// <summary>Konuşma ekranını kapatır.</summary>
        public void Close()
        {
            if (!IsOpen) return;

            partner.OnReplied -= HandleReply;
            partner.OnErrorOccurred -= HandleError;

            IsOpen = false;
            playerInput = "";
        }

        /// <summary>Konuşma geçmişini temizler ve açılış cümlesini yazar (sohbet sıfırlanınca).</summary>
        public void ClearLog()
        {
            lines.Clear();
            statusLine = "";
            if (partner != null) lines.Add(FormatPartnerLine(partner.OpeningLine));
        }

        /// <summary>
        /// Yazı kutusunun üstündeki durum satırını ayarlar.
        /// Örn. gardiyan anahtarı verince GuardInteraction burada yeşil bir mesaj gösterir.
        /// </summary>
        public void SetStatus(string richText)
        {
            statusLine = richText ?? "";
        }

        private void OnDestroy()
        {
            Close();
        }

        // ---------- Karakterin olayları ----------

        private void HandleReply(string reply, int trust)
        {
            lines.Add(FormatPartnerLine(reply));
            scrollToBottom = true;
        }

        private void HandleError(string message)
        {
            lines.Add($"<color=#FFA040><i>{message}</i></color>");
            scrollToBottom = true;
        }

        private string FormatPartnerLine(string text) => $"<color=#E8C170><b>{partner.DisplayName}:</b></color> {text}";

        // ---------- Çizim ----------

        private void OnGUI()
        {
            if (!IsOpen || partner == null) return;

            GUIStyle textStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize, wordWrap = true, richText = true };
            GUIStyle titleStyle = new GUIStyle(textStyle) { fontSize = fontSize + 4, fontStyle = FontStyle.Bold };
            GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = fontSize };
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = fontSize };
            GUIStyle hintStyle = new GUIStyle(textStyle) { fontSize = fontSize - 4, alignment = TextAnchor.MiddleRight };

            // Esc ile kapatma isteği
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                Event.current.Use();
                OnCloseRequested?.Invoke();
                return;
            }

            float margin = 20f;
            float panelHeight = Screen.height * panelHeightRatio;
            Rect panel = new Rect(margin, Screen.height - panelHeight - margin, Screen.width - margin * 2, panelHeight);
            GUI.DrawTexture(panel, panelTexture);

            float x = panel.x + 15f;
            float width = panel.width - 30f;
            float y = panel.y + 10f;

            // Başlık + (varsa) güven çubuğu
            GUI.Label(new Rect(x, y, width * 0.5f, 34f), partner.DisplayName, titleStyle);
            if (partner.HasTrust)
            {
                DrawTrustBar(new Rect(x + width * 0.55f, y + 8f, width * 0.45f, 20f), partner.Trust);
            }
            y += 40f;

            // Konuşma geçmişi
            float inputAreaHeight = 95f;
            Rect viewRect = new Rect(x, y, width, panel.yMax - y - inputAreaHeight);
            float contentWidth = width - 20f;
            float contentHeight = 0f;
            foreach (string line in lines)
            {
                contentHeight += textStyle.CalcHeight(new GUIContent(line), contentWidth) + 6f;
            }

            if (scrollToBottom && Event.current.type == EventType.Repaint)
            {
                scroll.y = Mathf.Max(0f, contentHeight - viewRect.height);
                scrollToBottom = false;
            }

            scroll = GUI.BeginScrollView(viewRect, scroll, new Rect(0, 0, contentWidth, contentHeight));
            float lineY = 0f;
            foreach (string line in lines)
            {
                float h = textStyle.CalcHeight(new GUIContent(line), contentWidth);
                GUI.Label(new Rect(0, lineY, contentWidth, h), line, textStyle);
                lineY += h + 6f;
            }
            GUI.EndScrollView();

            // Durum satırı: düşünüyor / özel durum mesajı
            float statusY = panel.yMax - inputAreaHeight + 5f;
            if (partner.IsThinking)
            {
                int dots = (int)(Time.time * 3f) % 4;
                GUI.Label(new Rect(x, statusY, width, 30f), $"<i>{partner.DisplayName} düşünüyor{new string('.', dots)}</i>", textStyle);
            }
            else if (!string.IsNullOrEmpty(statusLine))
            {
                GUI.Label(new Rect(x, statusY, width, 30f), statusLine, textStyle);
            }

            // Yazı kutusu (konuşma bittiyse gösterme)
            float inputY = panel.yMax - 52f;
            if (!partner.IsConversationOver)
            {
                DrawInputRow(new Rect(x, inputY, width, 40f), fieldStyle, buttonStyle);
            }

            GUI.Label(new Rect(x, panel.yMax - 12f - 4f, width, 16f), "Enter: gönder   •   Esc: ayrıl", hintStyle);
        }

        private void DrawInputRow(Rect area, GUIStyle fieldStyle, GUIStyle buttonStyle)
        {
            bool enterPressed = Event.current.type == EventType.KeyDown &&
                                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);

            float buttonWidth = 120f;
            GUI.SetNextControlName("DialogueInput");
            playerInput = GUI.TextField(new Rect(area.x, area.y, area.width - buttonWidth - 10f, area.height), playerInput, 300, fieldStyle);
            bool sendClicked = GUI.Button(new Rect(area.xMax - buttonWidth, area.y, buttonWidth, area.height), "Gönder", buttonStyle);

            bool canSend = !partner.IsThinking && !string.IsNullOrWhiteSpace(playerInput);
            if ((enterPressed || sendClicked) && canSend)
            {
                lines.Add($"<color=#9FD3FF><b>Sen:</b></color> {playerInput.Trim()}");
                partner.SendPlayerMessage(playerInput);
                playerInput = "";
                scrollToBottom = true;
                if (enterPressed) Event.current.Use();
            }

            GUI.FocusControl("DialogueInput");
        }

        private void DrawTrustBar(Rect area, int trust)
        {
            GUI.DrawTexture(area, barBackTexture);

            // Güven düşükse kırmızı, ortaysa sarı, yüksekse yeşil
            Color color = trust < 30 ? new Color(0.85f, 0.2f, 0.2f) :
                          trust < 60 ? new Color(0.9f, 0.75f, 0.2f) :
                                       new Color(0.3f, 0.8f, 0.3f);

            Rect fill = new Rect(area.x, area.y, area.width * trust / 100f, area.height);
            Color oldColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(fill, barFillTexture);
            GUI.color = oldColor;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 14, fontStyle = FontStyle.Bold };
            GUI.Label(area, $"Güven: {trust}", labelStyle);
        }

        private static Texture2D MakeTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
