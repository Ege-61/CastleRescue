using UnityEngine;
using UnityEngine.AI;

namespace CastleRescue.NPC
{
    /// <summary>
    /// NavMeshAgent'ın hızını Animator'a iletir. Animator Controller bu sayıya bakarak
    /// bekleme, yürüme ve koşma animasyonları arasında kendisi geçiş yapar.
    ///
    /// Yaratığın yapay zekası (EnemyController) animasyonları bilmez; bu sınıf sadece
    /// "şu an ne kadar hızlı gidiyor" bilgisini Animator'a aktarır.
    /// </summary>
    public class AgentAnimatorDriver : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa bu objede ya da üst objelerde aranır.")]
        [SerializeField] private NavMeshAgent agent;

        [Tooltip("Boş bırakılırsa bu objede ya da içindeki objelerde aranır.")]
        [SerializeField] private Animator animator;

        [Header("Animator parametreleri (Animator penceresinin Parameters sekmesindeki adlar)")]
        [Tooltip("Hızın yazılacağı float parametre.")]
        [SerializeField] private string speedParameter = "MoveSpeed";

        [Tooltip("Varsa her zaman true yapılacak bool parametre (yere basıyor mu). Yoksa boş bırakın.")]
        [SerializeField] private string groundedParameter = "Grounded";

        [Tooltip("Ajanın hızı bu sayıyla çarpılıp Animator'a yazılır. Animasyon çok yavaş ya da hızlı geçiş yapıyorsa ayarlayın.")]
        [SerializeField] private float speedMultiplier = 1f;

        private int speedHash;
        private int groundedHash;
        private bool hasSpeed;
        private bool speedIsInt; // bazı paketler hızı tam sayı (Int) olarak tutuyor
        private bool hasGrounded;

        private void Awake()
        {
            if (agent == null) agent = GetComponentInParent<NavMeshAgent>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (agent == null || animator == null)
            {
                Debug.LogError($"{name}: NavMeshAgent ya da Animator bulunamadı!");
                enabled = false;
                return;
            }

            speedHash = Animator.StringToHash(speedParameter);
            groundedHash = Animator.StringToHash(groundedParameter);
            // Hız parametresi Float da olabilir Int de; ikisini de kabul ediyoruz.
            hasSpeed = HasParameter(speedParameter, AnimatorControllerParameterType.Float);
            if (!hasSpeed && HasParameter(speedParameter, AnimatorControllerParameterType.Int))
            {
                hasSpeed = true;
                speedIsInt = true;
            }
            hasGrounded = HasParameter(groundedParameter, AnimatorControllerParameterType.Bool);

            if (!hasSpeed)
            {
                Debug.LogWarning($"{name}: Animator'da '{speedParameter}' adında bir hız parametresi yok. " +
                                 $"Mevcut parametreler: {ListParameters()}");
            }
        }

        private void Update()
        {
            if (hasSpeed)
            {
                float speed = agent.velocity.magnitude * speedMultiplier;
                if (speedIsInt) animator.SetInteger(speedHash, Mathf.RoundToInt(speed));
                else animator.SetFloat(speedHash, speed);
            }

            if (hasGrounded)
            {
                animator.SetBool(groundedHash, true);
            }
        }

        /// <summary>Animator'daki bütün parametrelerin adlarını ve türlerini tek satırda verir (uyarı mesajı için).</summary>
        private string ListParameters()
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                text.Append($"'{parameter.name}' ({parameter.type})  ");
            }
            return text.Length > 0 ? text.ToString() : "(hiç yok, Animator'a controller atanmamış olabilir)";
        }

        private bool HasParameter(string parameterName, AnimatorControllerParameterType type)
        {
            if (string.IsNullOrEmpty(parameterName) || animator.runtimeAnimatorController == null) return false;

            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName && parameter.type == type) return true;
            }
            return false;
        }
    }
}
