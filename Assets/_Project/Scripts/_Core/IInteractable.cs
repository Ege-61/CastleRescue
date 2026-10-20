using UnityEngine;

namespace CastleRescue.Core
{
    /// <summary>
    /// Etkileşime girilebilen her şeyin (şalter, kol, düğme...) ortak arayüzü.
    /// Fare bir şaltere mi yoksa başka bir şeye mi gittiğini bilmez; sadece bu arayüzü tanır.
    /// Böylece yeni bir etkileşim nesnesi eklemek için farenin koduna dokunmamız gerekmez.
    /// (SOLID - Open/Closed ve Dependency Inversion)
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Ekranda görünecek ad. Örn. "Şalter".</summary>
        string DisplayName { get; }

        /// <summary>Şu an etkileşime girilebilir mi? (Örn. indirilmiş şalter tekrar indirilemez.)</summary>
        bool CanInteract { get; }

        /// <summary>Etkileşim için gidilmesi gereken nokta.</summary>
        Transform InteractionPoint { get; }

        /// <summary>Etkileşimi gerçekleştirir.</summary>
        /// <param name="interactor">Etkileşimi yapan obje (fare, oyuncu...).</param>
        void Interact(GameObject interactor);
    }
}
