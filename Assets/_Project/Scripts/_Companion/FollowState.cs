using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// TAKİP durumu: Fare oyuncunun peşinden gelir.
    /// Yolu kendisi hesaplamaz, NavMeshAgent'a "oraya git" der; NavMesh en kısa yolu bulur
    /// ve engellerin etrafından dolaşır.
    /// </summary>
    public class FollowState : IAgentState
    {
        private readonly CompanionController companion;
        private float repathTimer;

        public string Name => "Takip";

        public FollowState(CompanionController companion)
        {
            this.companion = companion;
        }

        public void Enter()
        {
            companion.Agent.isStopped = false;
            repathTimer = 0f; // ilk karede hemen yol hesaplasın
        }

        public void Tick()
        {
            float distance = Vector3.Distance(companion.transform.position, companion.Player.position);

            // Oyuncuya yeterince yakınsak dur ve ona dön.
            if (distance <= companion.FollowDistance)
            {
                if (companion.Agent.hasPath) companion.Agent.ResetPath();
                LookAtPlayer();
                return;
            }

            // Uzaksak belli aralıklarla hedefi güncelle.
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                companion.Agent.SetDestination(companion.Player.position);
                repathTimer = companion.RepathInterval;
            }
        }

        public void Exit()
        {
            companion.Agent.ResetPath();
        }

        private void LookAtPlayer()
        {
            Vector3 direction = companion.Player.position - companion.transform.position;
            direction.y = 0f; // sadece yatayda dönsün, yukarı-aşağı eğilmesin
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion target = Quaternion.LookRotation(direction);
            companion.transform.rotation = Quaternion.Slerp(companion.transform.rotation, target, 5f * Time.deltaTime);
        }
    }
}
