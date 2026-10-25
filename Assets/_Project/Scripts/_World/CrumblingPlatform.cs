using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Üzerine basılınca bir süre sallanıp düşen çürük kiriş.
    /// Düştükten bir süre sonra eski yerine geri gelir, böylece oyuncu tekrar deneyebilir.
    ///
    /// Durumlar:  Sağlam → (oyuncu basar) → Sallanıyor → (süre dolar) → Düştü → (süre dolar) → Sağlam
    ///
    /// Oyuncunun basıp basmadığını anlamak için kirişin hemen üstüne görünmez bir
    /// algılayıcı (trigger) ekler.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CrumblingPlatform : MonoBehaviour
    {
        private enum PlatformState
        {
            Stable,   // sağlam
            Shaking,  // sallanıyor, birazdan düşecek
            Fallen    // düştü, geri gelmeyi bekliyor
        }

        [Tooltip("Basıldıktan kaç saniye sonra düşeceği.")]
        [SerializeField] private float crumbleDelay = 1.2f;

        [Tooltip("Sallanma şiddeti.")]
        [SerializeField] private float shakeAmount = 0.05f;

        [Tooltip("Düştükten kaç saniye sonra yerine geri geleceği.")]
        [SerializeField] private float respawnDelay = 4f;

        private PlatformState state = PlatformState.Stable;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Rigidbody body;
        private float timer;

        private void Awake()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;

            // Düşebilmesi için Rigidbody gerekiyor. Başta kinematic: yerçekiminden etkilenmez, yerinde durur.
            body = GetComponent<Rigidbody>();
            if (body == null) body = gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = true;

            CreateStepSensor();
        }

        /// <summary>
        /// Kirişin üst yüzeyinin hemen üstüne görünmez bir trigger kutusu ekler.
        /// Oyuncu kirişe bastığında bu kutunun içine girer ve OnTriggerEnter çalışır.
        /// </summary>
        private void CreateStepSensor()
        {
            BoxCollider sensor = gameObject.AddComponent<BoxCollider>();
            sensor.isTrigger = true;
            sensor.center = new Vector3(0f, 1f, 0f);       // kirişin bir kalınlık kadar üstü
            sensor.size = new Vector3(0.95f, 1f, 0.95f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (state != PlatformState.Stable || !other.CompareTag("Player")) return;

            state = PlatformState.Shaking;
            timer = crumbleDelay;
        }

        private void Update()
        {
            switch (state)
            {
                case PlatformState.Shaking:
                    // Rastgele küçük titremeler
                    transform.position = startPosition + Random.insideUnitSphere * shakeAmount;

                    timer -= Time.deltaTime;
                    if (timer <= 0f) Fall();
                    break;

                case PlatformState.Fallen:
                    timer -= Time.deltaTime;
                    if (timer <= 0f) ResetPlatform();
                    break;
            }
        }

        private void Fall()
        {
            state = PlatformState.Fallen;
            transform.position = startPosition;
            body.isKinematic = false; // artık yerçekimi çalışır, kiriş aşağı düşer
            timer = respawnDelay;
        }

        private void ResetPlatform()
        {
            // Hızları sıfırla, sonra tekrar kinematic yap ve eski yerine koy.
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;

            transform.position = startPosition;
            transform.rotation = startRotation;
            state = PlatformState.Stable;
        }
    }
}
