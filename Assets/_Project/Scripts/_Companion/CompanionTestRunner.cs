using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CastleRescue.Companion;
using CastleRescue.World;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace CastleRescue.Tests
{
    /// <summary>
    /// Farenin şalter görevini otomatik olarak tekrar tekrar test eder.
    /// Her denemede fareyi rastgele bir noktaya ışınlar, şaltere gönderir ve
    /// başarılı olup olmadığını, ne kadar sürdüğünü kaydeder.
    /// Sonuçları proje klasöründeki TestResults klasörüne CSV olarak yazar.
    ///
    /// Kullanım: Play'e basın, F9'a basın, test bitene kadar bekleyin.
    /// </summary>
    public class CompanionTestRunner : MonoBehaviour
    {
        [Header("Test Edilecekler")]
        [Tooltip("Boş bırakılırsa sahnede otomatik bulunur.")]
        [SerializeField] private CompanionController companion;

        [Tooltip("Boş bırakılırsa sahnede otomatik bulunur.")]
        [SerializeField] private Lever lever;

        [Header("Test Ayarları")]
        [SerializeField, Range(1, 100)] private int runCount = 20;

        [Tooltip("Farenin başlatılacağı rastgele noktaların merkezi. Boş bırakılırsa bu objenin konumu.")]
        [SerializeField] private Transform spawnCenter;

        [Tooltip("Merkezden en fazla bu uzaklıkta rastgele nokta seçilir.")]
        [SerializeField] private float spawnRadius = 15f;

        [Tooltip("Her denemeden sonra beklenecek süre.")]
        [SerializeField] private float delayBetweenRuns = 1f;

        [Tooltip("Rastgele sayıların başlangıç değeri. Aynı değer = aynı noktalar (tekrarlanabilir test).")]
        [SerializeField] private int randomSeed = 42;

        private class RunResult
        {
            public int run;
            public Vector3 start;
            public float straightDistance;
            public float pathLength;
            public bool success;
            public float duration;
            public string reason;
        }

        private readonly List<RunResult> results = new List<RunResult>();
        private bool isRunning;
        private int currentRun;
        private string resultFilePath = "";

        // Görev sonucunu beklemek için
        private bool taskFinished;
        private bool taskSuccess;
        private float taskDuration;
        private string taskReason;

        private void Start()
        {
            if (companion == null) companion = FindAnyObjectByType<CompanionController>();
            if (lever == null) lever = FindAnyObjectByType<Lever>();
            if (spawnCenter == null) spawnCenter = transform;

            if (companion == null || lever == null)
            {
                Debug.LogError("CompanionTestRunner: Sahnede fare ya da şalter bulunamadı!");
                enabled = false;
                return;
            }

            companion.OnTaskFinished += HandleTaskFinished;
        }

        private void OnDestroy()
        {
            if (companion != null) companion.OnTaskFinished -= HandleTaskFinished;
        }

        private void Update()
        {
            if (!isRunning && Keyboard.current != null && Keyboard.current.f9Key.wasPressedThisFrame)
            {
                StartCoroutine(RunTests());
            }
        }

        private void HandleTaskFinished(bool success, float duration, string reason)
        {
            taskFinished = true;
            taskSuccess = success;
            taskDuration = duration;
            taskReason = reason;
        }

        private IEnumerator RunTests()
        {
            isRunning = true;
            results.Clear();
            Random.InitState(randomSeed);
            Debug.Log($"[CompanionTest] {runCount} denemelik test başladı.");

            for (currentRun = 1; currentRun <= runCount; currentRun++)
            {
                // 1) Şalteri sıfırla, fareyi rastgele bir yürünebilir noktaya ışınla
                lever.ResetLever();
                companion.StateMachine.ChangeState(companion.FollowState);

                Vector3 start = PickRandomStartPoint();
                companion.Agent.Warp(start);
                yield return null; // ışınlanma bir kare otursun

                RunResult result = new RunResult
                {
                    run = currentRun,
                    start = start,
                    straightDistance = Vector3.Distance(start, lever.InteractionPoint.position),
                    pathLength = CalculatePathLength(start, lever.InteractionPoint.position)
                };

                // 2) Emri ver ve sonucu bekle
                taskFinished = false;
                if (!companion.CommandInteract(lever))
                {
                    result.success = false;
                    result.reason = "Emir kabul edilmedi";
                }
                else
                {
                    float timeout = companion.MaxTaskTime + 5f;
                    float waited = 0f;
                    while (!taskFinished && waited < timeout)
                    {
                        waited += Time.deltaTime;
                        yield return null;
                    }

                    result.success = taskFinished && taskSuccess;
                    result.duration = taskFinished ? taskDuration : waited;
                    result.reason = taskFinished ? taskReason : "Test zaman aşımı";
                }

                results.Add(result);
                Debug.Log($"[CompanionTest] {currentRun}/{runCount}: {(result.success ? "BAŞARILI" : "BAŞARISIZ")} " +
                          $"{result.duration:F2} sn ({result.reason})");

                yield return new WaitForSeconds(delayBetweenRuns);
            }

            WriteResults();
            companion.StateMachine.ChangeState(companion.FollowState);
            isRunning = false;
        }

        private Vector3 PickRandomStartPoint()
        {
            // Yürünebilir VE şaltere yolu olan bir nokta arıyoruz. (Örn. basamakların üstü ayrı bir ada
            // olduğu için oradan şaltere yol yoktur; oradan başlatmak testi haksız yere başarısız yapar.)
            // En fazla 50 kez dener, bulamazsa merkezi kullanır.
            for (int i = 0; i < 50; i++)
            {
                Vector2 circle = Random.insideUnitCircle * spawnRadius;
                Vector3 candidate = spawnCenter.position + new Vector3(circle.x, 0f, circle.y);
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas)) continue;
                if (CalculatePathLength(hit.position, lever.InteractionPoint.position) < 0f) continue;
                return hit.position;
            }
            return spawnCenter.position;
        }

        /// <summary>
        /// NavMesh yolunun uzunluğu (köşe noktaları arasındaki mesafelerin toplamı).
        /// Tam bir yol yoksa -1 döner.
        /// </summary>
        private float CalculatePathLength(Vector3 from, Vector3 to)
        {
            if (!NavMesh.SamplePosition(to, out NavMeshHit targetHit, 1.5f, NavMesh.AllAreas)) return -1f;

            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(from, targetHit.position, NavMesh.AllAreas, path);
            if (path.status != NavMeshPathStatus.PathComplete) return -1f;

            float length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return length;
        }

        private void WriteResults()
        {
            CultureInfo inv = CultureInfo.InvariantCulture; // ondalık ayırıcı her bilgisayarda nokta olsun
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("deneme,baslangic_x,baslangic_z,kus_ucusu_mesafe,yol_uzunlugu,basarili,sure_sn,aciklama");

            int successCount = 0;
            float totalTime = 0f;
            float minTime = float.MaxValue;
            float maxTime = 0f;

            foreach (RunResult r in results)
            {
                csv.AppendLine(string.Join(",",
                    r.run.ToString(inv),
                    r.start.x.ToString("F2", inv),
                    r.start.z.ToString("F2", inv),
                    r.straightDistance.ToString("F2", inv),
                    r.pathLength.ToString("F2", inv),
                    r.success ? "1" : "0",
                    r.duration.ToString("F2", inv),
                    "\"" + r.reason + "\""));

                if (r.success)
                {
                    successCount++;
                    totalTime += r.duration;
                    minTime = Mathf.Min(minTime, r.duration);
                    maxTime = Mathf.Max(maxTime, r.duration);
                }
            }

            float successRate = results.Count > 0 ? 100f * successCount / results.Count : 0f;
            float averageTime = successCount > 0 ? totalTime / successCount : 0f;
            if (successCount == 0) minTime = 0f;

            csv.AppendLine();
            csv.AppendLine($"# Basari orani: {successCount}/{results.Count} (%{successRate.ToString("F1", inv)})");
            csv.AppendLine($"# Ortalama sure (basarili): {averageTime.ToString("F2", inv)} sn, " +
                           $"en kisa {minTime.ToString("F2", inv)} sn, en uzun {maxTime.ToString("F2", inv)} sn");

            string folder = Path.Combine(Application.dataPath, "..", "TestResults");
            Directory.CreateDirectory(folder);
            resultFilePath = Path.GetFullPath(Path.Combine(folder, $"companion_test_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv"));
            File.WriteAllText(resultFilePath, csv.ToString(), Encoding.UTF8);

            Debug.Log($"[CompanionTest] BİTTİ. Başarı: {successCount}/{results.Count} (%{successRate:F1}), " +
                      $"ortalama {averageTime:F2} sn.\nSonuçlar: {resultFilePath}");
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.UpperLeft, wordWrap = true };
            Rect area = new Rect(Screen.width - 420, 80, 400, 150);

            if (isRunning)
            {
                GUI.Box(area, $"Fare testi çalışıyor...\nDeneme {Mathf.Min(currentRun, runCount)}/{runCount}", style);
            }
            else if (results.Count > 0)
            {
                int ok = results.FindAll(r => r.success).Count;
                GUI.Box(area, $"Fare testi bitti\nBaşarı: {ok}/{results.Count}\nDosya: TestResults klasörü\n(F9: tekrar)", style);
            }
            else
            {
                GUI.Box(new Rect(area.x, area.y, area.width, 40), "F9: Fare testini başlat", style);
            }
        }
    }
}
