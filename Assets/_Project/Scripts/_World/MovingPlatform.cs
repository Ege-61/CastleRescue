using System.Collections;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Activate() çağrılınca belirtilen yöne yumuşakça hareket eden platform.
    /// Level 3'te yerin altından yükselip kapıya giden parkuru oluşturur.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [Tooltip("Platform başlangıç konumundan ne kadar kayacak. Örn. (0, 3, 0) = 3 birim yukarı.")]
        [SerializeField] private Vector3 moveOffset = new Vector3(0f, 3f, 0f);

        [SerializeField] private float moveDuration = 1.5f;

        [Tooltip("Hareketten önce beklenecek süre. Platformlar sırayla yükselsin diye farklı değerler verin.")]
        [SerializeField] private float startDelay = 0f;

        public bool IsActivated { get; private set; }

        private Vector3 startPosition;

        private void Awake()
        {
            startPosition = transform.position;
        }

        /// <summary>Platformu harekete geçirir. Şalterin onActivated olayına bağlanır.</summary>
        public void Activate()
        {
            if (IsActivated) return;
            IsActivated = true;
            StartCoroutine(MoveRoutine());
        }

        private IEnumerator MoveRoutine()
        {
            if (startDelay > 0f) yield return new WaitForSeconds(startDelay);

            Vector3 endPosition = startPosition + moveOffset;
            float time = 0f;
            while (time < moveDuration)
            {
                time += Time.deltaTime;
                // SmoothStep: yavaş başlar, hızlanır, yavaş durur. Daha doğal görünür.
                float t = Mathf.SmoothStep(0f, 1f, time / moveDuration);
                transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }
            transform.position = endPosition;
        }

        // Scene ekranında platformun gideceği yeri sarı kutu olarak gösterir (yerleştirmeyi kolaylaştırır).
        private void OnDrawGizmosSelected()
        {
            Vector3 basePosition = Application.isPlaying ? startPosition : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(basePosition + moveOffset, transform.lossyScale);
        }
    }
}
