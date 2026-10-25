using CastleRescue.Companion;
using UnityEngine;

namespace CastleRescue.NPC
{
    /// <summary>
    /// KOVALA durumu: Yaratık NavMesh ile oyuncunun peşine düşer.
    /// Yakalarsa saldırır; belli bir süre göremezse vazgeçip odasına döner.
    /// </summary>
    public class EnemyChaseState : IAgentState
    {
        private const float RepathInterval = 0.2f;

        private readonly EnemyController enemy;
        private float lostSightTimer;
        private float repathTimer;

        public string Name => "Kovala";

        public EnemyChaseState(EnemyController enemy)
        {
            this.enemy = enemy;
        }

        public void Enter()
        {
            Debug.Log("[Enemy] Oyuncuyu gördü, kovalıyor!");
            enemy.Agent.speed = enemy.ChaseSpeed;
            enemy.Agent.isStopped = false;
            lostSightTimer = 0f;
            repathTimer = 0f;
        }

        public void Tick()
        {
            if (enemy.IsPlayerInAttackRange())
            {
                enemy.AttackPlayer();
                return;
            }

            // Oyuncuyu görmüyorsa sayaç işler; görürse sıfırlanır.
            lostSightTimer = enemy.CanSeePlayer() ? 0f : lostSightTimer + Time.deltaTime;
            if (lostSightTimer > enemy.LoseSightTime)
            {
                Debug.Log("[Enemy] Oyuncuyu kaybetti, odasına dönüyor.");
                enemy.StateMachine.ChangeState(enemy.ReturnState);
                return;
            }

            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                enemy.Agent.SetDestination(enemy.Player.position);
                repathTimer = RepathInterval;
            }
        }

        public void Exit()
        {
            enemy.Agent.ResetPath();
        }
    }
}
