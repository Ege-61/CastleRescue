using System.Collections.Generic;
using CastleRescue.UI;
using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Konuşma noktası: Oyuncu bu görünmez bölgeye girince fare (ve oyuncu) kısa bir konuşma yapar.
    /// Örn. yaratığın kapısında "Burası hiç hoşuma gitmedi...", avlunun girişinde
    /// "Şurada bir şalter var, işe yarayabilir!" gibi.
    /// Collider'ı "Is Trigger" olmalı.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CompanionBarkTrigger : MonoBehaviour
    {
        [Tooltip("Konuşan adı boş bırakılırsa fare konuşuyor sayılır. Oyuncu için 'Sen' yazın.")]
        [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>
        {
            new DialogueLine("", "Cik! Burada bir şey var gibi...")
        };

        [Tooltip("Sadece ilk girişte mi konuşsun?")]
        [SerializeField] private bool playOnce = true;

        private bool hasPlayed;
        private CompanionSpeech speech;

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void Start()
        {
            speech = FindAnyObjectByType<CompanionSpeech>();
            if (speech == null)
            {
                Debug.LogWarning($"{name}: Sahnede CompanionSpeech (fare) bulunamadı, konuşma gösterilemeyecek.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            Play();
        }

        /// <summary>Konuşmayı başlatır. İsterseniz UnityEvent'lerden de çağırabilirsiniz.</summary>
        public void Play()
        {
            if (speech == null) return;
            if (playOnce && hasPlayed) return;

            hasPlayed = true;
            speech.Say(lines);
        }

        // Scene ekranında bölgeyi yeşil kutu olarak gösterir.
        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;

            Gizmos.color = new Color(0.4f, 1f, 0.4f, 0.35f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}
