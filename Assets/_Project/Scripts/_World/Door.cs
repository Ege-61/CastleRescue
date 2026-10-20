using System.Collections;
using UnityEngine;

namespace CastleRescue.World
{
    /// <summary>
    /// Kale kapısı (demir parmaklık). Open() çağrılınca yukarı doğru kayarak açılır.
    /// Gardiyan anahtarı verdiğinde GuardInteraction'ın onKeyGiven olayına bağlanır.
    /// </summary>
    public class Door : MonoBehaviour
    {
        [Tooltip("Kapı açılırken ne kadar kayacak. Varsayılan: 3 birim yukarı.")]
        [SerializeField] private Vector3 openOffset = new Vector3(0f, 3f, 0f);

        [SerializeField] private float openDuration = 2f;

        public bool IsOpen { get; private set; }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            Debug.Log($"[Door] {name} açılıyor.");
            StartCoroutine(OpenRoutine());
        }

        private IEnumerator OpenRoutine()
        {
            Vector3 start = transform.position;
            Vector3 end = start + openOffset;

            float time = 0f;
            while (time < openDuration)
            {
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, time / openDuration));
                yield return null;
            }
            transform.position = end;
        }
    }
}
