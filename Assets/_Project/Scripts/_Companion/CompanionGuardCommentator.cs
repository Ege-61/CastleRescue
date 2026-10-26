using System.Collections.Generic;
using CastleRescue.NPC;
using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Oyuncu gardiyanla konuşurken farenin araya girip fısıldamasını sağlar.
    /// Gardiyanın her cevabında güvenin nasıl değiştiğine bakar:
    ///  - arttıysa cesaretlendirir, azaldıysa uyarır, değişmediyse bazen ipucu verir.
    /// Her cevapta konuşmaz (rastgele ve bekleme süreli), yoksa can sıkıcı olurdu.
    /// </summary>
    [RequireComponent(typeof(CompanionSpeech))]
    public class CompanionGuardCommentator : MonoBehaviour
    {
        [Tooltip("Gardiyanın bir cevabından sonra farenin yorum yapma ihtimali (0-1).")]
        [SerializeField, Range(0f, 1f)] private float commentChance = 0.6f;

        [Tooltip("İki yorum arasında en az kaç saniye geçsin.")]
        [SerializeField] private float cooldown = 6f;

        [SerializeField] private List<string> trustUpLines = new List<string>
        {
            "(fısıldar) Cik! Bence ikna oluyor, devam et!",
            "(fısıldar) Yüzü yumuşadı, doğru yoldasın!",
            "(fısıldar) Aferin! Biraz daha üstüne git."
        };

        [SerializeField] private List<string> trustDownLines = new List<string>
        {
            "(fısıldar) Dikkat et, sinirleniyor!",
            "(fısıldar) Cik cik... Bu hiç iyi gitmedi.",
            "(fısıldar) Sakin ol, kızdırırsan bizi kovar!"
        };

        [SerializeField] private List<string> neutralLines = new List<string>
        {
            "(fısıldar) Yaşlı adam altına bayılır derler...",
            "(fısıldar) Şarap kokusu alıyorum, belki şaraba zaafı vardır?",
            "(fısıldar) Prensesin masum olduğunu söylesene!"
        };

        [SerializeField] private string keyGivenLine = "Cik cik! Başardık! Hadi prensese!";
        [SerializeField] private string attackedLine = "Kaaaç! Kızdırdın onu!";

        private CompanionSpeech speech;
        private GuardNPC[] guards;
        private readonly Dictionary<GuardNPC, int> lastTrust = new Dictionary<GuardNPC, int>();
        private float cooldownTimer;

        private void Awake()
        {
            speech = GetComponent<CompanionSpeech>();
        }

        private void Start()
        {
            // Sahnedeki tüm gardiyanları bul ve olaylarını dinle. Gardiyan olmayan bölümde hiçbir şey yapmaz.
            guards = FindObjectsByType<GuardNPC>();
            foreach (GuardNPC guard in guards)
            {
                lastTrust[guard] = guard.Trust;

                // Lambda (=>) içinde hangi gardiyanın cevap verdiğini de iletiyoruz.
                GuardNPC current = guard;
                guard.OnReplied += (reply, trust) => HandleReply(current, trust);
                guard.OnKeyGiven += () => speech.Say(keyGivenLine);
                guard.OnAttacked += () => speech.Say(attackedLine);
            }
        }

        private void Update()
        {
            if (cooldownTimer > 0f) cooldownTimer -= Time.deltaTime;
        }

        private void HandleReply(GuardNPC guard, int newTrust)
        {
            int previous = lastTrust.TryGetValue(guard, out int value) ? value : newTrust;
            lastTrust[guard] = newTrust;

            if (guard.IsConversationOver) return; // anahtar/saldırı yorumları ayrı
            if (cooldownTimer > 0f || Random.value > commentChance) return;

            int change = newTrust - previous;
            List<string> pool = change > 0 ? trustUpLines : change < 0 ? trustDownLines : neutralLines;
            if (pool.Count == 0) return;

            speech.Say(pool[Random.Range(0, pool.Count)]);
            cooldownTimer = cooldown;
        }
    }
}
