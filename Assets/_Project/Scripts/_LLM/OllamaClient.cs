using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CastleRescue.LLM
{
    /// <summary>
    /// ILLMClient arayüzünün Ollama ile çalışan hali.
    /// İstekleri Coroutine ile gönderir: istek beklenirken oyun akmaya devam eder (frame rate düşmez).
    /// Hiçbir durumda exception fırlatmaz; her hata LLMResult.Failure olarak döner.
    /// </summary>
    public class OllamaClient : MonoBehaviour, ILLMClient
    {
        [Header("Sunucu Ayarları (koda dokunmadan buradan değiştirilebilir)")]
        [SerializeField] private string serverUrl = "http://localhost:11434";
        [SerializeField] private string modelName = "llama3.2:3b";

        [Tooltip("Bu kadar saniye içinde cevap gelmezse istek iptal edilir.")]
        [SerializeField, Range(1, 120)] private int timeoutSeconds = 30;

        [SerializeField, Range(0f, 1.5f)] private float temperature = 0.7f;

        public bool IsBusy { get; private set; }

        public void SendChat(IReadOnlyList<ChatMessage> messages, string jsonSchema, Action<LLMResult> onComplete)
        {
            if (onComplete == null)
            {
                Debug.LogError("OllamaClient: onComplete boş olamaz.");
                return;
            }

            if (IsBusy)
            {
                onComplete(LLMResult.Failure(LLMErrorType.Busy, "Önceki cevap henüz gelmedi, biraz bekleyin.", 0f));
                return;
            }

            if (messages == null || messages.Count == 0)
            {
                onComplete(LLMResult.Failure(LLMErrorType.InvalidResponse, "Gönderilecek mesaj yok.", 0f));
                return;
            }

            StartCoroutine(SendChatRoutine(messages, jsonSchema, onComplete));
        }

        private IEnumerator SendChatRoutine(IReadOnlyList<ChatMessage> messages, string jsonSchema, Action<LLMResult> onComplete)
        {
            IsBusy = true;
            float startTime = Time.realtimeSinceStartup;

            string url = serverUrl.TrimEnd('/') + "/api/chat";
            string body = BuildRequestBody(messages, jsonSchema);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;

                // Burada coroutine durur ve cevap gelene kadar bekler,
                // ama Unity diğer her şeyi (hareket, animasyon, UI) çalıştırmaya devam eder.
                yield return request.SendWebRequest();

                float duration = Time.realtimeSinceStartup - startTime;
                LLMResult result = HandleResponse(request, duration);

                IsBusy = false;
                onComplete(result);
            }
        }

        private string BuildRequestBody(IReadOnlyList<ChatMessage> messages, string jsonSchema)
        {
            OllamaChatRequest requestData = new OllamaChatRequest
            {
                model = modelName,
                messages = new List<ChatMessage>(messages),
                stream = false,
                options = new OllamaOptions { temperature = temperature }
            };

            string json = JsonUtility.ToJson(requestData);

            // JsonUtility serbest yapılı bir şemayı (sözlük gibi) yazamıyor.
            // Bu yüzden şema verildiyse "format" alanını JSON'un sonuna elle ekliyoruz:
            // {...} → {..., "format": { şema }}
            if (!string.IsNullOrWhiteSpace(jsonSchema))
            {
                json = json.Substring(0, json.Length - 1) + ",\"format\":" + jsonSchema + "}";
            }

            return json;
        }

        private LLMResult HandleResponse(UnityWebRequest request, float duration)
        {
            // 1) Sunucuya hiç ulaşılamadı ya da süre doldu
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning($"OllamaClient bağlantı hatası: {request.error}");

                bool isTimeout = request.error != null &&
                                 request.error.IndexOf("timeout", StringComparison.OrdinalIgnoreCase) >= 0;
                if (isTimeout)
                {
                    return LLMResult.Failure(LLMErrorType.Timeout,
                        $"Model {timeoutSeconds} saniye içinde cevap vermedi.", duration);
                }

                return LLMResult.Failure(LLMErrorType.ServerUnreachable,
                    "Ollama sunucusuna ulaşılamıyor. Ollama'nın açık olduğundan emin olun.", duration);
            }

            // 2) Sunucuya ulaşıldı ama hata kodu döndü (404, 500 vb.)
            if (request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning($"OllamaClient HTTP {request.responseCode}: {request.downloadHandler.text}");

                if (request.responseCode == 404)
                {
                    return LLMResult.Failure(LLMErrorType.ModelNotFound,
                        $"'{modelName}' modeli bulunamadı. Terminalde 'ollama pull {modelName}' çalıştırın.", duration);
                }

                return LLMResult.Failure(LLMErrorType.HttpError,
                    $"Sunucu hata döndürdü (kod {request.responseCode}).", duration);
            }

            // 3) Başka bir aktarım hatası
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"OllamaClient beklenmeyen hata: {request.error}");
                return LLMResult.Failure(LLMErrorType.InvalidResponse, "Sunucudan gelen veri okunamadı.", duration);
            }

            // 4) Başarılı cevap geldi, içini okuyalım
            OllamaChatResponse response = null;
            try
            {
                response = JsonUtility.FromJson<OllamaChatResponse>(request.downloadHandler.text);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"OllamaClient cevap ayrıştırılamadı: {e.Message}");
            }

            if (response == null || response.message == null || string.IsNullOrWhiteSpace(response.message.content))
            {
                return LLMResult.Failure(LLMErrorType.InvalidResponse,
                    "Model boş ya da okunamayan bir cevap döndürdü.", duration);
            }

            return LLMResult.Success(response.message.content, duration);
        }

        // Obje kapatılırsa coroutine yarıda kalır; IsBusy takılı kalmasın.
        private void OnDisable()
        {
            IsBusy = false;
        }
    }
}
