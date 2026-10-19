using System;
using System.Collections.Generic;

namespace CastleRescue.LLM
{
    /// <summary>
    /// Herhangi bir dil modeli sunucusuyla konuşan istemcinin soyutlaması.
    /// Gardiyan NPC sadece bu arayüzü bilecek, Ollama'yı bilmeyecek.
    /// (SOLID - Dependency Inversion ve Open/Closed: yarın LM Studio gibi başka bir yerel
    /// sunucuya geçmek istersek sadece yeni bir sınıf yazarız, NPC koduna dokunmayız.)
    /// </summary>
    public interface ILLMClient
    {
        /// <summary>Şu an cevap bekleniyor mu?</summary>
        bool IsBusy { get; }

        /// <summary>
        /// Mesajları modele gönderir. Metot hemen döner, oyunu bekletmez.
        /// Cevap (ya da hata) geldiğinde onComplete çağrılır.
        /// </summary>
        /// <param name="messages">System prompt + sohbet geçmişi.</param>
        /// <param name="jsonSchema">Modelin uyması gereken JSON şeması. Serbest metin için null.</param>
        /// <param name="onComplete">Sonuç geldiğinde çağrılacak metot.</param>
        void SendChat(IReadOnlyList<ChatMessage> messages, string jsonSchema, Action<LLMResult> onComplete);
    }
}
