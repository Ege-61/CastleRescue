using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// OMUZDA durumu: Fare yürümeyi bırakır ve oyuncunun omzunda taşınır.
    /// Level 2'deki karolar fare için çok küçük olduğu için orada bu durumda başlar.
    /// NavMeshAgent ve collider kapatılır; fare her karede oyuncunun omzuna yerleştirilir.
    /// </summary>
    public class ShoulderState : IAgentState
    {
        private readonly CompanionController companion;
        private Collider ownCollider;

        public string Name => "Omuzda";

        public ShoulderState(CompanionController companion)
        {
            this.companion = companion;
        }

        public void Enter()
        {
            // Yürümeyi kapat, yoksa NavMeshAgent fareyi tekrar yere çekmeye çalışır.
            companion.Agent.enabled = false;

            // Collider'ı kapat, yoksa oyuncunun CharacterController'ı fareye çarpıp takılır.
            ownCollider = companion.GetComponent<Collider>();
            if (ownCollider != null) ownCollider.enabled = false;
        }

        public void Tick()
        {
            // Omuz noktası oyuncuya göre tanımlı; oyuncu dönünce fare de onunla döner.
            companion.transform.position = companion.Player.TransformPoint(companion.ShoulderOffset);
            companion.transform.rotation = companion.Player.rotation;
        }

        public void Exit()
        {
            if (ownCollider != null) ownCollider.enabled = true;

            // Yere inerken en yakın yürünebilir noktaya yerleştir.
            companion.Agent.enabled = true;
            companion.Agent.Warp(companion.Player.position);
        }
    }
}
