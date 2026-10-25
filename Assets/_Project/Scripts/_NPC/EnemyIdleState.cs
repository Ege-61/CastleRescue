using CastleRescue.Companion;
using UnityEngine;

namespace CastleRescue.NPC
{
    /// <summary>
    /// BEKLE durumu: Yaratık olduğu yerde durur ve yavaşça etrafına bakınır.
    /// Oyuncuyu görürse Kovala durumuna geçer.
    /// </summary>
    public class EnemyIdleState : IAgentState
    {
        private readonly EnemyController enemy;

        public string Name => "Bekle";

        public EnemyIdleState(EnemyController enemy)
        {
            this.enemy = enemy;
        }

        public void Enter()
        {
            enemy.Agent.ResetPath();
        }

        public void Tick()
        {
            // Etrafına bakınma: kendi ekseninde yavaşça döner, böylece görüş açısı odayı tarar.
            enemy.transform.Rotate(Vector3.up, enemy.LookAroundSpeed * Time.deltaTime);

            if (enemy.CanSeePlayer())
            {
                enemy.StateMachine.ChangeState(enemy.ChaseState);
            }
        }

        public void Exit() { }
    }
}
