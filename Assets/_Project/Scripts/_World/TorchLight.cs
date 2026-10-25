using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Oyuncunun elindeki meşale. Kameranın biraz sağ altında durur ve
    /// ışığı ateş gibi titrer. Bu objeye bir Point Light eklenmelidir.
    ///
    /// Oyuncu prefab'ına dokunmamak için meşale ayrı bir obje olarak kamerayı takip eder;
    /// böylece sadece Level 1'de meşale olur, diğer bölümler etkilenmez.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class TorchLight : MonoBehaviour
    {
        [Tooltip("Boş bırakılırsa ana kamera takip edilir.")]
        [SerializeField] private Transform followTarget;

        [Tooltip("Kameraya göre konum: sağ, yukarı, ileri.")]
        [SerializeField] private Vector3 offset = new Vector3(0.35f, -0.25f, 0.5f);

        [Header("Titreme")]
        [SerializeField] private float baseIntensity = 3f;
        [SerializeField] private float flickerAmount = 0.6f;
        [SerializeField] private float flickerSpeed = 8f;

        private Light torchLight;
        private float noiseSeed;

        private void Awake()
        {
            torchLight = GetComponent<Light>();
            noiseSeed = Random.Range(0f, 100f);
        }

        private void Start()
        {
            if (followTarget == null && Camera.main != null) followTarget = Camera.main.transform;
        }

        // LateUpdate: kamera bu karedeki hareketini bitirdikten sonra çalışır, meşale titremez.
        private void LateUpdate()
        {
            if (followTarget != null)
            {
                transform.position = followTarget.TransformPoint(offset);
                transform.rotation = followTarget.rotation;
            }

            // Perlin gürültüsü: rastgele ama yumuşak değişen bir sayı (0-1 arası). Ateş titremesine benzer.
            float noise = Mathf.PerlinNoise(noiseSeed, Time.time * flickerSpeed);
            torchLight.intensity = baseIntensity + (noise - 0.5f) * 2f * flickerAmount;
        }
    }
}
