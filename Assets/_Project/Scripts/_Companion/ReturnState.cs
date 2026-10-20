using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// GERİ DÖN durumu: Görev bittikten (ya da başarısız olduktan) sonra fare oyuncunun yanına döner.
    /// Oyuncuya ulaşınca tekrar Takip durumuna geçer.
    /// </summary>
    public class ReturnState : IAgentState
    {
        private readonly CompanionController companion;
        private float repathTimer;

        public string Name => "Geri Dön";

        public ReturnState(CompanionController companion)
        {
            this.companion = companion;
        }

        public void Enter()
        {
            companion.Agent.isStopped = false;
            repathTimer = 0f;
        }

        public void Tick()
        {
            float distance = Vector3.Distance(companion.transform.position, companion.Player.position);
            if (distance <= companion.FollowDistance)
            {
                companion.StateMachine.ChangeState(companion.FollowState);
                return;
            }

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
    }
}
