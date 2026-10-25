using CastleRescue.Companion;

namespace CastleRescue.NPC
{
    /// <summary>
    /// EVE DÖN durumu: Yaratık başladığı noktaya yürüyerek döner.
    /// Yolda oyuncuyu görürse tekrar kovalar; varınca Bekle durumuna geçer.
    /// </summary>
    public class EnemyReturnState : IAgentState
    {
        private readonly EnemyController enemy;

        public string Name => "Eve Dön";

        public EnemyReturnState(EnemyController enemy)
        {
            this.enemy = enemy;
        }

        public void Enter()
        {
            enemy.Agent.speed = enemy.ReturnSpeed;
            enemy.Agent.isStopped = false;
            enemy.Agent.SetDestination(enemy.HomePosition);
        }

        public void Tick()
        {
            if (enemy.CanSeePlayer())
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
                return;
            }

            bool arrived = !enemy.Agent.pathPending && enemy.Agent.remainingDistance <= 0.5f;
            if (arrived)
            {
                enemy.StateMachine.ChangeState(enemy.IdleState);
            }
        }

        public void Exit() { }
    }
}
