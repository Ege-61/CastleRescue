using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Bir alanı sivri dikenlerle (koni şeklinde) doldurur. Sadece görünüş içindir;
    /// oyuncuyu öldüren şey aynı yerdeki KillZone'dur.
    ///
    /// Koni şekli Unity'nin hazır şekilleri arasında olmadığı için kodla bir kez üretilir
    /// (köşe noktaları ve üçgenler tanımlanarak) ve bütün dikenler aynı şekli paylaşır.
    ///
    /// Kullanım: Inspector'da bileşenin ⋮ menüsünden "Dikenleri Oluştur" seçilir.
    /// </summary>
    public class SpikeFieldBuilder : MonoBehaviour
    {
        [Tooltip("Dikenlerin kaplayacağı alan (genişlik X, derinlik Z).")]
        [SerializeField] private Vector2 areaSize = new Vector2(9f, 9f);

        [Tooltip("Dikenler arası mesafe. Küçüldükçe daha sık, daha çok diken.")]
        [SerializeField] private float spacing = 0.6f;

        [SerializeField] private float spikeHeight = 0.7f;
        [SerializeField] private float spikeRadius = 0.12f;

        [Tooltip("Dikenlerin boy, konum ve eğiminde ne kadar rastgelelik olsun (0 = hepsi aynı).")]
        [SerializeField, Range(0f, 1f)] private float randomness = 0.4f;

        [SerializeField] private Material spikeMaterial;

        [Tooltip("Aynı sayı = aynı yerleşim. Değiştirirseniz dikenler farklı dizilir.")]
        [SerializeField] private int randomSeed = 7;

        [ContextMenu("Dikenleri Oluştur")]
        public void Build()
        {
            Clear();

            Mesh cone = CreateConeMesh(6);
            Random.InitState(randomSeed);

            int countX = Mathf.Max(1, Mathf.FloorToInt(areaSize.x / spacing));
            int countZ = Mathf.Max(1, Mathf.FloorToInt(areaSize.y / spacing));

            for (int x = 0; x < countX; x++)
            {
                for (int z = 0; z < countZ; z++)
                {
                    // Izgara üzerindeki yer + biraz rastgele kaydırma
                    float jitter = spacing * 0.4f * randomness;
                    Vector3 position = new Vector3(
                        (x + 0.5f) * spacing - areaSize.x / 2f + Random.Range(-jitter, jitter),
                        0f,
                        (z + 0.5f) * spacing - areaSize.y / 2f + Random.Range(-jitter, jitter));

                    float height = spikeHeight * (1f + Random.Range(-0.5f, 0.5f) * randomness);
                    float tilt = 15f * randomness;

                    GameObject spike = new GameObject("Diken");
                    spike.transform.SetParent(transform, false);
                    spike.transform.localPosition = position;
                    spike.transform.localRotation = Quaternion.Euler(
                        Random.Range(-tilt, tilt), Random.Range(0f, 360f), Random.Range(-tilt, tilt));
                    spike.transform.localScale = new Vector3(spikeRadius * 2f, height, spikeRadius * 2f);

                    spike.AddComponent<MeshFilter>().sharedMesh = cone;
                    MeshRenderer meshRenderer = spike.AddComponent<MeshRenderer>();
                    if (spikeMaterial != null) meshRenderer.sharedMaterial = spikeMaterial;
                }
            }

            Debug.Log($"[SpikeFieldBuilder] {countX * countZ} diken oluşturuldu.");
            MarkSceneDirty();
        }

        [ContextMenu("Dikenleri Temizle")]
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            MarkSceneDirty();
        }

        /// <summary>
        /// Tabanı 1 birim genişliğinde, boyu 1 birim olan bir koni (piramit) şekli üretir.
        /// Her yan yüz ayrı köşe noktaları kullanır ki ışık yüzlerde keskin görünsün.
        /// </summary>
        private static Mesh CreateConeMesh(int sides)
        {
            Vector3 apex = new Vector3(0f, 1f, 0f);
            Vector3[] vertices = new Vector3[sides * 3];
            int[] triangles = new int[sides * 3];

            for (int i = 0; i < sides; i++)
            {
                float angleA = i * Mathf.PI * 2f / sides;
                float angleB = (i + 1) * Mathf.PI * 2f / sides;
                Vector3 a = new Vector3(Mathf.Cos(angleA) * 0.5f, 0f, Mathf.Sin(angleA) * 0.5f);
                Vector3 b = new Vector3(Mathf.Cos(angleB) * 0.5f, 0f, Mathf.Sin(angleB) * 0.5f);

                // Üçgenin köşe sırası, yüzün dışa bakması için saat yönünde olmalı.
                vertices[i * 3] = a;
                vertices[i * 3 + 1] = apex;
                vertices[i * 3 + 2] = b;
                triangles[i * 3] = i * 3;
                triangles[i * 3 + 1] = i * 3 + 1;
                triangles[i * 3 + 2] = i * 3 + 2;
            }

            Mesh mesh = new Mesh { name = "DikenKonisi", vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void MarkSceneDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }

        // Scene ekranında dikenlerin kaplayacağı alanı kırmızı çerçeveyle gösterir.
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0f, spikeHeight / 2f, 0f), new Vector3(areaSize.x, spikeHeight, areaSize.y));
        }
    }
}
