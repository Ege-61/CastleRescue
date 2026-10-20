using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// ETKİLEŞİM durumu: Fare hedefin önünde durur, ona döner, kısa bir süre bekler
    /// (kolu çekiyormuş gibi) ve ardından hedefle etkileşime girer.
    /// </summary>
    public class InteractState : IAgentState
    {
        private readonly CompanionController companion;
        private float timer;

        public string Name => "Etkileşim";

        public InteractState(CompanionController companion)
        {
            this.companion = companion;
        }

        public void Enter()
        {
            timer = 0f;
            companion.Agent.ResetPath();
            companion.Agent.isStopped = true;
        }

        public void Tick()
        {
            FaceTarget();

            timer += Time.deltaTime;
            if (timer < companion.InteractDuration) return;

            if (companion.CurrentTarget.CanInteract)
            {
                companion.CurrentTarget.Interact(companion.gameObject);
                companion.CompleteTask();
            }
            else
            {
                companion.FailTask("Hedef artık kullanılamıyor");
            }
        }

        public void Exit()
        {
            companion.Agent.isStopped = false;
        }

        private void FaceTarget()
        {
            Vector3 direction = companion.CurrentTarget.InteractionPoint.position - companion.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f) return;

            Quaternion rotation = Quaternion.LookRotation(direction);
            companion.transform.rotation = Quaternion.Slerp(companion.transform.rotation, rotation, 8f * Time.deltaTime);
        }
    }
}
