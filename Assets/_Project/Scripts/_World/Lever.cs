using System.Collections;
using CastleRescue.Core;
using UnityEngine;
using UnityEngine.Events;

namespace CastleRescue.World
{
    /// <summary>
    /// İndirilebilen şalter. İndirildiğinde Inspector'dan bağlanan olayları (onActivated) tetikler.
    /// Hangi platformların hareket edeceğini kod değil, Inspector belirler;
    /// böylece aynı şalteri farklı bölümlerde farklı işler için kullanabiliriz.
    /// </summary>
    public class Lever : MonoBehaviour, IInteractable
    {
        [SerializeField] private string displayName = "Şalter";

        [Tooltip("Dönecek kol parçası. Boş bırakılırsa objenin kendisi döner.")]
        [SerializeField] private Transform handle;

        [Tooltip("İndirilince kolun ne kadar döneceği (derece).")]
        [SerializeField] private Vector3 pulledRotation = new Vector3(0f, 0f, -70f);

        [SerializeField] private float pullDuration = 0.5f;

        [Tooltip("Etkileşim için gidilecek nokta. Boş bırakılırsa şalterin kendi konumu kullanılır.")]
        [SerializeField] private Transform interactionPoint;

        [Tooltip("Şalter indirilince çalışacak olaylar (örn. platformların Activate metodu).")]
        [SerializeField] private UnityEvent onActivated;

        public bool IsPulled { get; private set; }

        public string DisplayName => displayName;
        public bool CanInteract => !IsPulled;
        public Transform InteractionPoint => interactionPoint != null ? interactionPoint : transform;

        private void Awake()
        {
            if (handle == null) handle = transform;
        }

        public void Interact(GameObject interactor)
        {
            if (IsPulled) return;

            IsPulled = true;
            Debug.Log($"[Lever] {displayName}, {interactor.name} tarafından indirildi.");
            StartCoroutine(PullRoutine());
        }

        private IEnumerator PullRoutine()
        {
            Quaternion start = handle.localRotation;
            Quaternion end = start * Quaternion.Euler(pulledRotation);

            float time = 0f;
            while (time < pullDuration)
            {
                time += Time.deltaTime;
                handle.localRotation = Quaternion.Slerp(start, end, time / pullDuration);
                yield return null;
            }
            handle.localRotation = end;

            // Kol tamamen indikten sonra olayları tetikle.
            onActivated?.Invoke();
        }
    }
}
