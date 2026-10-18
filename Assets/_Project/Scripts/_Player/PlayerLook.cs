using UnityEngine;

namespace CastleRescue.Player
{
    /// <summary>
    /// Birinci şahıs kamera kontrolü.
    /// Fare sağa-sola: bütün gövde döner. Fare yukarı-aşağı: sadece kamera döner.
    /// </summary>
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float sensitivity = 0.1f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        private IPlayerInput input;
        private float pitch; // kameranın yukarı-aşağı açısı

        private void Awake()
        {
            input = GetComponent<IPlayerInput>();
            if (input == null)
            {
                Debug.LogError($"{name}: IPlayerInput bileşeni bulunamadı!");
                enabled = false;
                return;
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void Update()
        {
            Vector2 look = input.Look * sensitivity;

            transform.Rotate(Vector3.up * look.x);

            pitch -= look.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // ters takla atmasın
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>
        /// İmleci kilitler/serbest bırakır.
        /// NPC ile yazışırken imlecin görünmesi için ileride bunu çağıracağız.
        /// </summary>
        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
