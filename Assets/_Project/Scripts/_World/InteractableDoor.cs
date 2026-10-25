using CastleRescue.Player;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Oyuncu yaklaşıp E'ye basınca açılan kapı.
    /// Açılma hareketini Door sınıfı yapar; bu sınıf sadece "ne zaman açılacağına" karar verir.
    /// (Gardiyanın kapısı ve kafes kapısı da aynı Door sınıfını kullanıyor.)
    /// </summary>
    [RequireComponent(typeof(Door))]
    public class InteractableDoor : MonoBehaviour
    {
        [SerializeField] private float interactDistance = 2.5f;

        private Door door;
        private Transform player;
        private IPlayerInput playerInput;

        private bool PlayerInRange =>
            player != null && Vector3.Distance(player.position, transform.position) <= interactDistance;

        private void Awake()
        {
            door = GetComponent<Door>();
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerInput = playerObject.GetComponent<IPlayerInput>();
            }
        }

        private void Update()
        {
            if (door.IsOpen || playerInput == null) return;

            if (PlayerInRange && playerInput.InteractPressed)
            {
                door.Open();
            }
        }

        private void OnGUI()
        {
            if (door.IsOpen || !PlayerInRange) return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(0, Screen.height * 0.6f, Screen.width, 40), "[E] Kapıyı aç", style);
        }
    }
}
