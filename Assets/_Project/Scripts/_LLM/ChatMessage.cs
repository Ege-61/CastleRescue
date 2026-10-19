using System;

namespace CastleRescue.LLM
{
    /// <summary>
    /// Sohbetteki tek bir mesaj. Ollama'nın beklediği biçimle birebir aynı:
    /// { "role": "user", "content": "Merhaba" }
    /// JsonUtility ile JSON'a çevrilebilmesi için alanlar public ve küçük harfli.
    /// </summary>
    [Serializable]
    public class ChatMessage
    {
        public string role;    // "system", "user" ya da "assistant"
        public string content;

        public ChatMessage() { } // JsonUtility için boş kurucu

        public ChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }

        // Kolay kullanım için yardımcı metotlar: ChatMessage.User("Merhaba")
        public static ChatMessage SystemPrompt(string content) => new ChatMessage("system", content);
        public static ChatMessage User(string content) => new ChatMessage("user", content);
        public static ChatMessage Assistant(string content) => new ChatMessage("assistant", content);
    }
}
