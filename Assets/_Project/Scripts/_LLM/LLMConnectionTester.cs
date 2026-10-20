using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CastleRescue.LLM
{
    /// <summary>
    /// SADECE TEST İÇİN. Ollama bağlantısını ve hata yönetimini denemek için kullanılır.
    /// T tuşu: test mesajı gönderir. Cevap/hata ekranın sol üstünde görünür.
    /// Bekleme sırasında küp dönmeye devam ediyorsa oyun donmuyor demektir.
    /// </summary>
    public class LLMConnectionTester : MonoBehaviour
    {
        [SerializeField, TextArea(2, 4)]
        private string testMessage = "Merhaba! Kendini iki cümleyle tanıt.";

        [Tooltip("Donma testi için sürekli dönecek bir obje (örn. bir küp).")]
        [SerializeField] private Transform spinningObject;

        private ILLMClient llmClient;
        private string statusText = "T tuşuna basarak test mesajı gönderin.";

        private void Awake()
        {
            // Somut sınıfı (OllamaClient) değil, arayüzü istiyoruz.
            llmClient = GetComponent<ILLMClient>();
            if (llmClient == null)
            {
                Debug.LogError("LLMConnectionTester: Aynı objede ILLMClient (OllamaClient) bulunamadı!");
                enabled = false;
            }
        }

        private void Update()
        {
            if (spinningObject != null)
            {
                spinningObject.Rotate(Vector3.up, 90f * Time.deltaTime);
            }

            if (Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
            {
                SendTestMessage();
            }
        }

        private void SendTestMessage()
        {
            if (llmClient.IsBusy) return;

            List<ChatMessage> messages = new List<ChatMessage>
            {
                ChatMessage.User(testMessage)
            };

            llmClient.SendChat(messages, null, OnResult);
        }

        private void OnResult(LLMResult result)
        {
            if (result.IsSuccess)
            {
                statusText = $"Cevap ({result.DurationSeconds:F2} sn):\n{result.Content}";
                Debug.Log($"[LLM TEST] {result.DurationSeconds:F2} sn\n{result.Content}");
            }
            else
            {
                statusText = $"HATA [{result.ErrorType}]:\n{result.ErrorMessage}";
                Debug.LogWarning($"[LLM TEST] {result.ErrorType}: {result.ErrorMessage}");
            }
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true };

            string text = statusText;
            if (llmClient != null && llmClient.IsBusy)
            {
                int dotCount = (int)(Time.time * 3f) % 4; // 0-3 arası nokta: "Düşünüyor..." animasyonu
                text = "Düşünüyor" + new string('.', dotCount);
            }

            // Genişliği ekrana göre ayarlıyoruz ki küçük Game penceresinde yazı kesilmesin.
            GUI.Label(new Rect(20, 20, Screen.width - 40, Screen.height - 40), text, style);
        }
    }
}
