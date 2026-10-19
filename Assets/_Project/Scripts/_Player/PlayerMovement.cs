using UnityEngine;

namespace CastleRescue.Player
{
    /// <summary>
    /// Oyuncunun yürüme, koşma, zıplama ve yerçekimi hareketini yönetir.
    /// Girdiyi doğrudan klavyeden değil, IPlayerInput arayüzünden alır.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Hız")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintSpeed = 7f;

        [Header("Zıplama ve Yerçekimi")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;

        private CharacterController controller;
        private IPlayerInput input;
        private float verticalVelocity;

        public bool IsGrounded => controller.isGrounded;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            // Arayüz üzerinden buluyoruz: aynı objede IPlayerInput'u uygulayan hangi bileşen varsa onu alır.
            input = GetComponent<IPlayerInput>();
            if (input == null)
            {
                Debug.LogError($"{name}: IPlayerInput bileşeni bulunamadı! KeyboardMouseInput ekleyin.");
                enabled = false;
            }
        }

        private void Update()
        {
            bool grounded = controller.isGrounded;

            // Yerdeyken küçük bir aşağı hız veriyoruz ki karakter rampalarda yere yapışık kalsın.
            if (grounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            // Girdiyi karakterin baktığı yöne göre dünya yönüne çevir.
            Vector2 moveInput = input.Move;
            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            float speed = input.SprintHeld ? sprintSpeed : walkSpeed;

            // Zıplama: v = kök(h * -2 * g) fizik formülü ile istenen yüksekliğe çıkacak hızı hesaplıyoruz.
            if (grounded && input.JumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = moveDirection * speed + Vector3.up * verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// Oyuncuyu bir noktaya ışınlar. (Dikenli çukura düşünce yeniden doğma vb. için.)
        /// CharacterController açıkken transform.position'ı değiştirmek çalışmaz, bu yüzden önce kapatıyoruz.
        /// </summary>
        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            transform.position = position;
            controller.enabled = true;
            verticalVelocity = 0f;
        }
    }
}
