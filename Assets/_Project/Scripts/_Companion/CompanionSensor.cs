using CastleRescue.Core;
using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Farenin "gözleri". Raycast (ışın) atarak çevresini algılar:
    ///  - Önündeki engelleri 3 ışınla tarar (ileri, sol çapraz, sağ çapraz)
    ///  - Hedefi görüp göremediğini (arada duvar var mı) kontrol eder
    /// Işınlar Scene ekranında renkli çizgiler olarak görünür (yeşil = boş, kırmızı = engel).
    /// </summary>
    public class CompanionSensor : MonoBehaviour
    {
        [Tooltip("Işınların farenin merkezinden ne kadar yukarıdan atılacağı.")]
        [SerializeField] private float eyeHeight = 0.15f;

        [Tooltip("Engel algılama ışınlarının uzunluğu.")]
        [SerializeField] private float obstacleRayLength = 1f;

        [Tooltip("Sol ve sağ ışınların ileri yönle yaptığı açı.")]
        [SerializeField, Range(0f, 90f)] private float sideRayAngle = 30f;

        [Tooltip("Hedefi en fazla bu mesafeden görebilir.")]
        [SerializeField] private float sightRange = 25f;

        [Tooltip("Hangi katmanlar engel sayılsın. Varsayılan: hepsi.")]
        [SerializeField] private LayerMask obstacleMask = ~0;

        /// <summary>Önünde engel var mı?</summary>
        public bool ObstacleAhead { get; private set; }

        /// <summary>En yakın engele uzaklık. Engel yoksa ışın uzunluğu.</summary>
        public float NearestObstacleDistance { get; private set; }

        private Vector3 Eye => transform.position + Vector3.up * eyeHeight;

        private void Update()
        {
            ScanForObstacles();
        }

        private void ScanForObstacles()
        {
            Vector3 forward = transform.forward;
            Vector3 left = Quaternion.Euler(0f, -sideRayAngle, 0f) * forward;
            Vector3 right = Quaternion.Euler(0f, sideRayAngle, 0f) * forward;

            float nearest = obstacleRayLength;
            nearest = Mathf.Min(nearest, CastRay(forward));
            nearest = Mathf.Min(nearest, CastRay(left));
            nearest = Mathf.Min(nearest, CastRay(right));

            NearestObstacleDistance = nearest;
            ObstacleAhead = nearest < obstacleRayLength;
        }

        /// <summary>Tek bir ışın atar, çarptığı uzaklığı döndürür.</summary>
        private float CastRay(Vector3 direction)
        {
            if (Physics.Raycast(Eye, direction, out RaycastHit hit, obstacleRayLength, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawLine(Eye, hit.point, Color.red);
                return hit.distance;
            }

            Debug.DrawRay(Eye, direction * obstacleRayLength, Color.green);
            return obstacleRayLength;
        }

        /// <summary>
        /// Fare ile hedef arasında engel var mı? Hedefe doğru bir ışın atar.
        /// Işın önce hedefin kendisine çarparsa ya da hiçbir şeye çarpmazsa hedef görünüyordur.
        /// </summary>
        public bool HasLineOfSight(IInteractable target)
        {
            if (target == null) return false;

            Vector3 targetPosition = target.InteractionPoint.position;
            Vector3 toTarget = targetPosition - Eye;
            float distance = toTarget.magnitude;

            if (distance > sightRange) return false;
            if (distance < 0.01f) return true;

            if (Physics.Raycast(Eye, toTarget / distance, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                bool hitTarget = hit.collider.GetComponentInParent<IInteractable>() == target;
                Debug.DrawLine(Eye, hit.point, hitTarget ? Color.cyan : Color.magenta);
                return hitTarget;
            }

            Debug.DrawLine(Eye, targetPosition, Color.cyan);
            return true;
        }
    }
}
