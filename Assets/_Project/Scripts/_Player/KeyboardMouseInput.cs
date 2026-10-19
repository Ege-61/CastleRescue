using UnityEngine;
using UnityEngine.InputSystem;

namespace CastleRescue.Player
{
    /// <summary>
    /// IPlayerInput arayüzünün klavye + fare ile çalışan somut hali.
    /// Unity'nin yeni Input System paketini kullanır.
    /// Tek sorumluluğu: tuşları okuyup arayüzün istediği biçime çevirmek.
    /// </summary>
    public class KeyboardMouseInput : MonoBehaviour, IPlayerInput
    {
        public bool IsEnabled { get; set; } = true;

        // Girdi kapalıysa ya da klavye takılı değilse null döner.
        // Böylece aşağıdaki her satırda ayrı ayrı kontrol yazmak zorunda kalmıyoruz.
        private Keyboard Kb => IsEnabled ? Keyboard.current : null;
        private Mouse Ms => IsEnabled ? Mouse.current : null;

        public Vector2 Move
        {
            get
            {
                if (Kb == null) return Vector2.zero;

                float x = 0f;
                float y = 0f;
                if (Kb.aKey.isPressed || Kb.leftArrowKey.isPressed) x -= 1f;
                if (Kb.dKey.isPressed || Kb.rightArrowKey.isPressed) x += 1f;
                if (Kb.sKey.isPressed || Kb.downArrowKey.isPressed) y -= 1f;
                if (Kb.wKey.isPressed || Kb.upArrowKey.isPressed) y += 1f;

                // Çapraz giderken (W+D) karakter daha hızlı gitmesin diye uzunluğu 1 ile sınırlıyoruz.
                return Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            }
        }

        public Vector2 Look => Ms != null ? Ms.delta.ReadValue() : Vector2.zero;

        public bool JumpPressed => Kb != null && Kb.spaceKey.wasPressedThisFrame;
        public bool SprintHeld => Kb != null && Kb.leftShiftKey.isPressed;
        public bool InteractPressed => Kb != null && Kb.eKey.wasPressedThisFrame;
        public bool CommandCompanionPressed => Kb != null && Kb.fKey.wasPressedThisFrame;
    }
}
