using UnityEngine;

namespace CastleRescue.Player
{
    /// <summary>
    /// Oyuncu girdilerini soyutlayan arayüz.
    /// PlayerMovement ve PlayerLook klavyeyi doğrudan tanımaz, sadece bu arayüzü tanır.
    /// (SOLID - Dependency Inversion: üst seviye sınıflar somut sınıfa değil, soyutlamaya bağlıdır.)
    /// İleride gamepad desteği eklemek istersek sadece yeni bir sınıf yazmamız yeterli olur.
    /// </summary>
    public interface IPlayerInput
    {
        /// <summary>WASD / yön tuşları. x = sağ-sol, y = ileri-geri.</summary>
        Vector2 Move { get; }

        /// <summary>Fare hareketi (bu karede kaç piksel oynadı).</summary>
        Vector2 Look { get; }

        bool JumpPressed { get; }
        bool SprintHeld { get; }

        /// <summary>E tuşu: şalter, kapı, NPC ile etkileşim.</summary>
        bool InteractPressed { get; }

        /// <summary>F tuşu: fareye emir verme.</summary>
        bool CommandCompanionPressed { get; }

        /// <summary>T tuşu: fareyle sohbet etme.</summary>
        bool TalkToCompanionPressed { get; }

        /// <summary>
        /// Girdiler kapalıysa tüm değerler sıfır/false döner.
        /// Örn. NPC ile yazışırken oyuncu yürümesin diye kapatacağız.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
