using System;
using CastleRescue.Core;
using UnityEngine;
using UnityEngine.AI;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Yardımcı farenin ana bileşeni. Durum makinesini çalıştırır ve
    /// durumların ihtiyaç duyduğu şeyleri (NavMeshAgent, sensör, oyuncu, ayarlar) sağlar.
    /// Hareket kararlarını kendisi vermez; o iş durumların (IAgentState) sorumluluğu.
    ///
    /// Durumlar:  Takip → (oyuncu F'ye basar) → Hedefe Git → Etkileşim → Geri Dön → Takip
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CompanionSensor))]
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

        [Header("Görev Ayarları")]
        [Tooltip("Hedefe bu mesafeye kadar yaklaşınca etkileşime geçer.")]
        [SerializeField] private float interactReach = 1f;

        [Tooltip("Etkileşim (kolu çekme) kaç saniye sürsün.")]
        [SerializeField] private float interactDuration = 1f;

        [Tooltip("Görev bu süreden uzun sürerse başarısız sayılır.")]
        [SerializeField] private float maxTaskTime = 30f;

        [Tooltip("Fare bu kadar saniye kıpırdamazsa sıkıştı sayılır.")]
        [SerializeField] private float stuckTimeout = 2f;

        // ---- Durumların kullandığı özellikler ----
        public NavMeshAgent Agent { get; private set; }
        public CompanionSensor Sensor { get; private set; }
        public Transform Player => player;
        public float FollowDistance => followDistance;
        public float RepathInterval => repathInterval;
        public float InteractReach => interactReach;
        public float InteractDuration => interactDuration;
        public float MaxTaskTime => maxTaskTime;
        public float StuckTimeout => stuckTimeout;

        // ---- Durum makinesi ve durumlar ----
        public StateMachine StateMachine { get; private set; }
        public FollowState FollowState { get; private set; }
        public GoToTargetState GoToTargetState { get; private set; }
        public InteractState InteractState { get; private set; }
        public ReturnState ReturnState { get; private set; }

        // ---- Görev bilgisi ----
        public IInteractable CurrentTarget { get; private set; }
        public string LastMessage { get; private set; } = "";

        /// <summary>Fare şu an bir görevle meşgul mü?</summary>
        public bool IsBusy => StateMachine.CurrentState == GoToTargetState || StateMachine.CurrentState == InteractState;

        /// <summary>Görev bittiğinde (başarılı mı, kaç saniye sürdü, açıklama). Test istatistikleri için.</summary>
        public event Action<bool, float, string> OnTaskFinished;

        private float taskStartTime;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Sensor = GetComponent<CompanionSensor>();
            // Fare objesi bu bileşen eklenmeden önce oluşturulduysa sensör eksik olabilir; kendimiz ekleyelim.
            if (Sensor == null) Sensor = gameObject.AddComponent<CompanionSensor>();

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
            GoToTargetState = new GoToTargetState(this);
            InteractState = new InteractState(this);
            ReturnState = new ReturnState(this);
        }

        private void Start()
        {
            StateMachine.ChangeState(FollowState);
        }

        private void Update()
        {
            StateMachine.Tick();
        }

        /// <summary>
        /// Oyuncunun emri: "Şu hedefe git ve etkileşime gir."
        /// Fare meşgulse ya da hedef kullanılamıyorsa false döner.
        /// </summary>
        public bool CommandInteract(IInteractable target)
        {
            if (target == null || !target.CanInteract) return false;
            if (IsBusy) return false;

            CurrentTarget = target;
            taskStartTime = Time.time;
            LastMessage = $"{target.DisplayName} hedefine gidiyor";
            StateMachine.ChangeState(GoToTargetState);
            return true;
        }

        /// <summary>Durumlar görevi başarıyla bitirdiğinde çağırır.</summary>
        public void CompleteTask()
        {
            float duration = Time.time - taskStartTime;
            LastMessage = $"Görev tamamlandı ({duration:F1} sn)";
            Debug.Log($"[Companion] BAŞARILI: {CurrentTarget?.DisplayName} | {duration:F2} sn");

            OnTaskFinished?.Invoke(true, duration, "Başarılı");
            CurrentTarget = null;
            StateMachine.ChangeState(ReturnState);
        }

        /// <summary>Durumlar görev başarısız olduğunda çağırır.</summary>
        public void FailTask(string reason)
        {
            float duration = Time.time - taskStartTime;
            LastMessage = $"Görev başarısız: {reason}";
            Debug.LogWarning($"[Companion] BAŞARISIZ: {reason} | {duration:F2} sn");

            OnTaskFinished?.Invoke(false, duration, reason);
            CurrentTarget = null;
            StateMachine.ChangeState(ReturnState);
        }

        // Ekranın sol üstünde farenin durumunu gösterir (test ve video için faydalı).
        private void OnGUI()
        {
            if (StateMachine?.CurrentState == null) return;
            GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = 18 };
            GUI.Label(new Rect(20, 20, 600, 30), $"Fare durumu: {StateMachine.CurrentState.Name}", style);
            if (!string.IsNullOrEmpty(LastMessage))
            {
                GUI.Label(new Rect(20, 45, 600, 30), LastMessage, style);
            }
        }
    }
}
