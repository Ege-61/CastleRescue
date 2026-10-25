using System.Collections.Generic;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Metin olarak çizilen bir haritadan satranç tahtası gibi karo zemin oluşturur.
    /// Her karakter bir karodur:
    ///   #  sağlam karo
    ///   K  çürük karo (basılınca düşer)
    ///   .  boşluk (karo yok)
    ///
    /// Haritanın en alt satırı girişe en yakın, en üst satırı balkona en yakın satırdır.
    /// Kullanım: Inspector'da bileşenin ⋮ menüsünden "Zemini Oluştur" seçilir.
    /// </summary>
    public class TileFloorBuilder : MonoBehaviour
    {
        [SerializeField, TextArea(8, 20)]
        private string layout =
            "#K#K#\n" +
            "K.K.K\n" +
            "#K#.#\n" +
            ".#K#K\n" +
            "K#.K#\n" +
            "#.#K.\n" +
            "K#K#K\n" +
            "#K#K#";

        [Header("Boyutlar")]
        [Tooltip("Bir karonun kenar uzunluğu.")]
        [SerializeField] private float tileSize = 2.5f;

        [Tooltip("Karolar arasındaki boşluk.")]
        [SerializeField] private float gap = 0.5f;

        [SerializeField] private float tileThickness = 0.5f;

        [Header("Görünüm (isteğe bağlı)")]
        [SerializeField] private Material solidMaterial;
        [Tooltip("Çürük karolara farklı (örn. biraz daha koyu) bir malzeme vererek oyuncuya ipucu verebilirsiniz.")]
        [SerializeField] private Material crumblingMaterial;

        [ContextMenu("Zemini Oluştur")]
        public void Build()
        {
            Clear();

            List<string> rows = ParseRows();
            if (rows.Count == 0)
            {
                Debug.LogError("TileFloorBuilder: Harita boş!");
                return;
            }

            float step = tileSize + gap;
            int solidCount = 0;
            int crumblingCount = 0;

            for (int r = 0; r < rows.Count; r++)
            {
                // En alttaki satır girişe en yakın olsun diye satır numarasını ters çeviriyoruz.
                int rowFromStart = rows.Count - 1 - r;

                for (int c = 0; c < rows[r].Length; c++)
                {
                    char cell = rows[r][c];
                    if (cell != '#' && cell != 'K') continue;

                    // Karonun üst yüzeyi y = 0'da olsun.
                    Vector3 position = new Vector3(c * step, -tileThickness / 2f, rowFromStart * step);
                    bool crumbling = cell == 'K';

                    GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tile.name = crumbling ? $"CurukKaro_{r}_{c}" : $"Karo_{r}_{c}";
                    tile.transform.SetParent(transform, false);
                    tile.transform.localPosition = position;
                    tile.transform.localScale = new Vector3(tileSize, tileThickness, tileSize);

                    Material material = crumbling ? crumblingMaterial : solidMaterial;
                    if (material != null) tile.GetComponent<Renderer>().sharedMaterial = material;

                    if (crumbling)
                    {
                        tile.AddComponent<CrumblingPlatform>();
                        crumblingCount++;
                    }
                    else
                    {
                        solidCount++;
                    }
                }
            }

            Debug.Log($"[TileFloorBuilder] {solidCount} sağlam, {crumblingCount} çürük karo oluşturuldu.");
            MarkSceneDirty();
        }

        [ContextMenu("Zemini Temizle")]
        public void Clear()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            MarkSceneDirty();
        }

        private List<string> ParseRows()
        {
            List<string> rows = new List<string>();
            foreach (string line in layout.Split('\n'))
            {
                string trimmed = line.TrimEnd('\r', ' ');
                if (trimmed.Length > 0) rows.Add(trimmed);
            }
            return rows;
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
    }
}
