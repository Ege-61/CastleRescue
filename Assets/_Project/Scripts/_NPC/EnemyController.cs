using CastleRescue.Companion;
using CastleRescue.Core;
using UnityEngine;
using UnityEngine.AI;

namespace CastleRescue.NPC
{
    /// <summary>
    /// Labirentteki yaratık. Farenin kullandığı StateMachine ve IAgentState yapısını tekrar kullanır.
    ///
    /// Durumlar:  Bekle → (oyuncuyu görür) → Kovala → (yakalar) → Eve Dön → Bekle
    ///                                         ↓ (gözden kaybeder)
    ///                                       Eve Dön
    ///
    /// Bu sınıf durumların ihtiyaç duyduğu bilgileri ve "oyuncuyu görüyor muyum" gibi
    /// sorguları sağlar; kararları durumlar verir.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [Header("Algılama")]
        [Tooltip("Oyuncuyu en fazla bu mesafeden görebilir.")]
        [SerializeField] private float detectionRange = 12f;

        [Tooltip("Görüş açısı (derece). 120 = önündeki 120 derecelik alanı görür.")]
        [SerializeField, Range(10f, 360f)] private float fieldOfView = 120f;

        [Tooltip("Gözlerin objenin merkezinden yüksekliği.")]
        [SerializeField] private float eyeHeight = 0.6f;

        [Tooltip("Oyuncuyu bu kadar saniye göremezse kovalamayı bırakır.")]
        [SerializeField] private float loseSightTime = 3f;

        [Header("Hareket")]
        [SerializeField] private float chaseSpeed = 3.5f;
        [SerializeField] private float returnSpeed = 2f;

        [Tooltip("Beklerken etrafına bakınma hızı (derece/saniye).")]
        [SerializeField] private float lookAroundSpeed = 40f;

        [Header("Saldırı")]
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private string attackMessage = "Karanlıktaki yaratık seni yakaladı!";

        // ---- Durumların kullandığı özellikler ----
        public NavMeshAgent Agent { get; private set; }
        public Transform Player { get; private set; }
        public Vector3 HomePosition { get; private set; }
        public float LoseSightTime => loseSightTime;
        public float ChaseSpeed => chaseSpeed;
        public float ReturnSpeed => returnSpeed;
        public float LookAroundSpeed => lookAroundSpeed;

        // ---- Durum makinesi (fareyle aynı sınıf) ----
        public StateMachine StateMachine { get; private set; }
        public EnemyIdleState IdleState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyReturnState ReturnState { get; private set; }

        private Vector3 Eye => transform.position + Vector3.up * eyeHeight;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            HomePosition = transform.position;

            StateMachine = new StateMachine();
            IdleState = new EnemyIdleState(this);
            ChaseState = new EnemyChaseState(this);
            ReturnState = new EnemyReturnState(this);
        }

        private void Start()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null)
            {
                Debug.LogError($"{name}: Oyuncu bulunamadı!");
                enabled = false;
                return;
            }

            Player = playerObject.transform;
            StateMachine.ChangeState(IdleState);
        }

        private void Update()
        {
            StateMachine.Tick();
        }

        /// <summary>
        /// Oyuncuyu görüyor mu? Üç kontrol yapar:
        ///  1) Mesafe yeterince yakın mı?
        ///  2) Görüş açısının içinde mi?
        ///  3) Arada duvar ya da kapalı kapı var mı? (Raycast)
        /// </summary>
        public bool CanSeePlayer()
        {
            if (Player == null) return false;

            Vector3 target = Player.position;
            Vector3 toPlayer = target - Eye;
            float distance = toPlayer.magnitude;

            if (distance > detectionRange) return false;

            Vector3 flatDirection = new Vector3(toPlayer.x, 0f, toPlayer.z);
            float angle = Vector3.Angle(transform.forward, flatDirection);
            if (angle > fieldOfView / 2f) return false;

            if (Physics.Raycast(Eye, toPlayer / distance, out RaycastHit hit, distance + 0.5f, ~0, QueryTriggerInteraction.Ignore))
            {
                bool sawPlayer = hit.collider.CompareTag("Player");
                Debug.DrawLine(Eye, hit.point, sawPlayer ? Color.red : Color.gray);
                return sawPlayer;
            }

            return false;
        }

        public bool IsPlayerInAttackRange()
        {
            return Player != null && Vector3.Distance(transform.position, Player.position) <= attackRange;
        }

        /// <summary>Oyuncuyu yakaladı: oyuncu ölür, yaratık odasına döner.</summary>
        public void AttackPlayer()
        {
            Debug.Log("[Enemy] Oyuncu yakalandı.");
            GameManager.Instance?.KillPlayer(attackMessage);
            StateMachine.ChangeState(ReturnState);
        }

        // Scene ekranında görüş mesafesini kırmızı çember olarak gösterir.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
