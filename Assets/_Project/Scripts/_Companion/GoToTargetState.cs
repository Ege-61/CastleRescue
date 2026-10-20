using UnityEngine;
using UnityEngine.AI;

namespace CastleRescue.Companion
{
    /// <summary>
    /// HEDEFE GİT durumu: Fare, oyuncunun gösterdiği hedefe (şalter) kendi yolunu bularak gider.
    /// Yol NavMesh tarafından hesaplanır; yol yoksa, fare sıkışırsa ya da süre dolarsa görev başarısız olur.
    /// Hedefe yeterince yaklaşıp sensörüyle hedefi görünce Etkileşim durumuna geçer.
    /// </summary>
    public class GoToTargetState : IAgentState
    {
        private readonly CompanionController companion;
        private float elapsed;
        private float stuckTime;
        private string pendingFailure;

        public string Name => "Hedefe Git";

        public GoToTargetState(CompanionController companion)
        {
            this.companion = companion;
        }

        public void Enter()
        {
            elapsed = 0f;
            stuckTime = 0f;
            pendingFailure = null;

            NavMeshAgent agent = companion.Agent;
            agent.isStopped = false;

            // Hedef noktaya en yakın yürünebilir noktayı bul.
            Vector3 targetPosition = companion.CurrentTarget.InteractionPoint.position;
            if (!NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, 1.5f, NavMesh.AllAreas))
            {
                pendingFailure = "Hedefin yakınında yürünebilir alan yok";
                return;
            }

            // Yolu önceden hesapla ve tam bir yol olup olmadığına bak.
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(navHit.position, path);
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                pendingFailure = "Hedefe giden yol bulunamadı";
                return;
            }

            agent.SetPath(path);
        }

        public void Tick()
        {
            // Enter içinde durum değiştirmek yerine hatayı ilk karede bildiriyoruz.
            if (pendingFailure != null)
            {
                companion.FailTask(pendingFailure);
                return;
            }

            NavMeshAgent agent = companion.Agent;
            elapsed += Time.deltaTime;

            // 1) Zaman aşımı
            if (elapsed > companion.MaxTaskTime)
            {
                companion.FailTask("Süre doldu");
                return;
            }

            // 2) Sıkışma kontrolü: hedefe varmadan uzun süre kıpırdamadıysa
            bool notArrived = !agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 0.1f;
            bool notMoving = agent.velocity.sqrMagnitude < 0.01f && !agent.isOnOffMeshLink;
            stuckTime = (notArrived && notMoving) ? stuckTime + Time.deltaTime : 0f;

            if (stuckTime > companion.StuckTimeout)
            {
                string reason = companion.Sensor.ObstacleAhead ? "Önünde engel var, sıkıştı" : "Sıkıştı";
                companion.FailTask(reason);
                return;
            }

            // 3) Hedefe yeterince yakın ve hedefi görüyor mu?
            float distance = Vector3.Distance(companion.transform.position, companion.CurrentTarget.InteractionPoint.position);
            if (distance <= companion.InteractReach && companion.Sensor.HasLineOfSight(companion.CurrentTarget))
            {
                companion.StateMachine.ChangeState(companion.InteractState);
            }
        }

        public void Exit()
        {
            companion.Agent.ResetPath();
        }
    }
}
