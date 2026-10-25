using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Metin olarak çizilen bir haritadan labirent oluşturur.
    /// Her karakter bir hücredir:
    ///   #  duvar
    ///   .  boş zemin
    ///   S  oyuncunun başlangıç noktası (işaret konur)
    ///   1, 2, 3  kapı (oraya açılabilir bir kapı konur)
    ///
    /// Kullanım: Inspector'da bileşenin ⋮ menüsünden "Labirenti Oluştur" seçilir.
    /// Labirent oyun başlamadan, editörde oluşturulur; böylece NavMesh de bake edilebilir.
    /// </summary>
    public class MazeBuilder : MonoBehaviour
    {
        [Tooltip("Haritanın en üst satırı kuzeydir (+Z yönü).")]
        [SerializeField, TextArea(10, 25)]
        private string layout =
            "###############\n" +
            "#....#...#....#\n" +
            "#.##.#...#.##.#\n" +
            "#..#.#...#..#.#\n" +
            "##.#.##3###.#.#\n" +
            "#...........#.#\n" +
            "#####.###.###.#\n" +
            "#...#...#.....#\n" +
            "#...1.#.#######\n" +
            "#...#.#...#...#\n" +
            "#####.###.2...#\n" +
            "#.......#.#...#\n" +
            "#.#####.#.#####\n" +
            "#S....#.......#\n" +
            "###############";

        [Header("Boyutlar")]
        [SerializeField] private float cellSize = 3f;
        [SerializeField] private float wallHeight = 3f;

        [Header("Görünüm (isteğe bağlı)")]
        [SerializeField] private Material wallMaterial;
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material doorMaterial;

        [Tooltip("Tavan olsun mu? Labirentin karanlık olması için açık bırakın.")]
        [SerializeField] private bool buildCeiling = true;

        [ContextMenu("Labirenti Oluştur")]
        public void Build()
        {
            Clear();

            string[] rows = ParseRows();
            if (rows.Length == 0)
            {
                Debug.LogError("MazeBuilder: Harita boş!");
                return;
            }

            int rowCount = rows.Length;
            int columnCount = 0;
            foreach (string row in rows) columnCount = Mathf.Max(columnCount, row.Length);

            Transform walls = CreateGroup("Duvarlar");
            Transform doors = CreateGroup("Kapilar");

            int wallCount = 0;
            for (int r = 0; r < rowCount; r++)
            {
                for (int c = 0; c < rows[r].Length; c++)
                {
                    char cell = rows[r][c];
                    Vector3 position = CellToLocal(r, c);

                    if (cell == '#')
                    {
                        CreateCube($"Duvar_{r}_{c}", walls,
                            position + Vector3.up * (wallHeight / 2f),
                            new Vector3(cellSize, wallHeight, cellSize), wallMaterial);
                        wallCount++;
                    }
                    else if (cell == 'S')
                    {
                        Transform start = new GameObject("Baslangic").transform;
                        start.SetParent(transform, false);
                        start.localPosition = position + Vector3.up;
                    }
                    else if (char.IsDigit(cell))
                    {
                        // Kapının solunda ve sağında duvar varsa geçit kuzey-güney yönündedir.
                        // Yoksa geçit doğu-batı yönündedir ve kapıyı 90 derece çevirmemiz gerekir.
                        bool wallsOnSides = IsWall(rows, r, c - 1) && IsWall(rows, r, c + 1);
                        CreateDoor(cell, doors, position, rotate: !wallsOnSides);
                    }
                }
            }

            // Zemin ve tavan: tüm labirenti kaplayan iki büyük küp
            float width = columnCount * cellSize;
            float depth = rowCount * cellSize;
            Vector3 center = new Vector3(width / 2f - cellSize / 2f, 0f, -(depth / 2f - cellSize / 2f));

            CreateCube("Zemin", transform, center + Vector3.down * 0.25f, new Vector3(width, 0.5f, depth), floorMaterial);
            if (buildCeiling)
            {
                CreateCube("Tavan", transform, center + Vector3.up * (wallHeight + 0.25f), new Vector3(width, 0.5f, depth), wallMaterial);
            }

            Debug.Log($"[MazeBuilder] {columnCount}x{rowCount} labirent oluşturuldu, {wallCount} duvar.");
            MarkSceneDirty();
        }

        [ContextMenu("Labirenti Temizle")]
        public void Clear()
        {
            // Çocukları sondan başa doğru siliyoruz (silerken liste kaymasın diye).
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            MarkSceneDirty();
        }

        private string[] ParseRows()
        {
            List<string> rows = new List<string>();
            foreach (string line in layout.Split('\n'))
            {
                string trimmed = line.TrimEnd('\r', ' ');
                if (trimmed.Length > 0) rows.Add(trimmed);
            }
            return rows.ToArray();
        }

        /// <summary>Satır/sütun numarasını labirentin içindeki konuma çevirir.</summary>
        private Vector3 CellToLocal(int row, int column)
        {
            return new Vector3(column * cellSize, 0f, -row * cellSize);
        }

        private Transform CreateGroup(string groupName)
        {
            Transform group = new GameObject(groupName).transform;
            group.SetParent(transform, false);
            return group;
        }

        private GameObject CreateCube(string objectName, Transform parent, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = scale;
            if (material != null) cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        /// <summary>Verilen hücre duvar mı? Haritanın dışı da duvar sayılır.</summary>
        private bool IsWall(string[] rows, int row, int column)
        {
            if (row < 0 || row >= rows.Length) return true;
            if (column < 0 || column >= rows[row].Length) return true;
            return rows[row][column] == '#';
        }

        private void CreateDoor(char number, Transform parent, Vector3 localPosition, bool rotate)
        {
            GameObject door = CreateCube($"Kapi{number}", parent,
                localPosition + Vector3.up * (wallHeight / 2f),
                new Vector3(cellSize, wallHeight, 0.3f), doorMaterial);

            if (rotate) door.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);

            // NavMesh hesaplanırken kapıları yok say. Kapı kapalıyken bake edilse yaratık
            // kapı açıldıktan sonra da oradan geçemeyeceğini sanırdı.
            NavMeshModifier modifier = door.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;

            door.AddComponent<Door>();
            door.AddComponent<InteractableDoor>();
        }

        private void MarkSceneDirty()
        {
#if UNITY_EDITOR
            // Editörde yapılan değişikliğin kaydedilmesi gerektiğini Unity'ye bildiriyoruz.
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
#endif
        }
    }
}
