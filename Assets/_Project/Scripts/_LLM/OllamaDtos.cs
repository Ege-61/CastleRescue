using System;
using System.Collections.Generic;

namespace CastleRescue.LLM
{
    // Bu dosyadaki sınıflar sadece Ollama API'sine gönderilen / ondan gelen JSON'un kalıbıdır.
    // (DTO = Data Transfer Object). İçlerinde hiç mantık yok.
    // Alan adları Ollama'nın JSON'undakiyle birebir aynı olmak zorunda.

    /// <summary>POST /api/chat isteğinin gövdesi.</summary>
    [Serializable]
    internal class OllamaChatRequest
    {
        public string model;
        public List<ChatMessage> messages;
        public bool stream;          // false: cevabın tamamını tek seferde al
        public OllamaOptions options;
    }

    [Serializable]
    internal class OllamaOptions
    {
        public float temperature;    // 0 = hep aynı cevap, yükseldikçe daha yaratıcı
    }

    /// <summary>/api/chat cevabı. Hata durumunda sadece "error" alanı dolu gelir.</summary>
    [Serializable]
    internal class OllamaChatResponse
    {
        public ChatMessage message;
        public bool done;
        public string error;
    }
}
