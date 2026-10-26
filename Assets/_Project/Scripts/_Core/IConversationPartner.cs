using System;

namespace CastleRescue.Core
{
    /// <summary>
    /// Oyuncunun konuşma ekranında yazışabildiği her karakterin ortak arayüzü.
    /// Gardiyan ve fare ikisi de bunu uygular; böylece aynı konuşma ekranı (DialogueUI)
    /// ikisi için de kullanılır. Ekran kiminle konuştuğunu bilmez, sadece bu arayüzü bilir.
    /// (OOP - Polimorfizm, SOLID - Dependency Inversion)
    /// </summary>
    public interface IConversationPartner
    {
        /// <summary>Konuşma ekranında görünecek ad.</summary>
        string DisplayName { get; }

        /// <summary>Konuşma açılınca ilk görünen cümle.</summary>
        string OpeningLine { get; }

        /// <summary>Güven çubuğu gösterilsin mi? (Gardiyanda var, farede yok.)</summary>
        bool HasTrust { get; }

        int Trust { get; }

        /// <summary>Cevap bekleniyor mu? ("Düşünüyor..." göstergesi için)</summary>
        bool IsThinking { get; }

        /// <summary>Konuşma bitti mi? Bittiyse yazı kutusu gizlenir.</summary>
        bool IsConversationOver { get; }

        /// <summary>Cevap geldiğinde (metin, güncel güven).</summary>
        event Action<string, int> OnReplied;

        /// <summary>Hata olduğunda oyuncuya gösterilecek mesaj.</summary>
        event Action<string> OnErrorOccurred;

        void SendPlayerMessage(string playerText);
    }
}
