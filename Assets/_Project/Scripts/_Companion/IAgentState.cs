namespace CastleRescue.Companion
{
    /// <summary>
    /// Farenin durum makinesindeki her durumun (Takip, Hedefe Git, Etkileşim, Geri Dön) ortak arayüzü.
    /// Her durum kendi sınıfında yazılır; böylece yeni durum eklemek eski kodu bozmaz.
    /// (SOLID - Open/Closed: genişlemeye açık, değişikliğe kapalı.)
    /// </summary>
    public interface IAgentState
    {
        /// <summary>Raporlarda ve ekranda görünecek durum adı.</summary>
        string Name { get; }

        /// <summary>Duruma girildiğinde bir kere çalışır.</summary>
        void Enter();

        /// <summary>Durum aktifken her karede çalışır.</summary>
        void Tick();

        /// <summary>Durumdan çıkılırken bir kere çalışır.</summary>
        void Exit();
    }
}
