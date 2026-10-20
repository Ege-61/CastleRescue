using UnityEngine;
using UnityEngine.AI;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Yardımcı farenin ana bileşeni. Durum makinesini çalıştırır ve
    /// durumların ihtiyaç duyduğu şeyleri (NavMeshAgent, oyuncu, ayarlar) sağlar.
    /// Hareket kararlarını kendisi vermez; o iş durumların (IAgentState) sorumluluğu.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CompanionController : MonoBehaviour
    {
        [Header("Referanslar")]
        [Tooltip("Boş bırakılırsa 'Player' etiketli obje otomatik bulunur.")]
        [SerializeField] private Transform player;

        [Header("Takip Ayarları")]
        [Tooltip("Oyuncuya bu mesafeden daha yakınsa fare durur.")]
        [SerializeField] private float followDistance = 1.5f;

        [Tooltip("Hedef noktasını kaç saniyede bir yenileyeceği (her karede yenilemek gereksiz yük).")]
        [SerializeField] private float repathInterval = 0.25f;

        public NavMeshAgent Agent { get; private set; }
        public Transform Player => player;
        public float FollowDistance => followDistance;
        public float RepathInterval => repathInterval;

        public StateMachine StateMachine { get; private set; }

        // Durumlar. Sonraki adımlarda GoToTarget, Interact, Return eklenecek.
        public FollowState FollowState { get; private set; }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();

            if (player == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null) player = playerObject.transform;
            }

            if (player == null)
            {
                Debug.LogError($"{name}: Oyuncu bulunamadı! Player objesinin Tag'ini 'Player' yapın.");
                enabled = false;
                return;
            }

            StateMachine = new StateMachine();
            FollowState = new FollowState(this);
        }

        private void Start()
        {
            StateMachine.ChangeState(FollowState);
        }

        private void Update()
        {
            StateMachine.Tick();
        }

        // Ekranın sol üstünde farenin şu anki durumunu gösterir (test ve video için faydalı).
        private void OnGUI()
        {
            if (StateMachine?.CurrentState == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            GUI.Label(new Rect(20, 20, 400, 30), $"Fare durumu: {StateMachine.CurrentState.Name}", style);
        }
    }
}
