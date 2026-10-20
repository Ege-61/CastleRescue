using System;
using UnityEngine;

namespace CastleRescue.Companion
{
    /// <summary>
    /// Basit sonlu durum makinesi (Finite State Machine).
    /// Aynı anda sadece bir durum aktiftir. MonoBehaviour değildir, düz bir C# sınıfıdır;
    /// bu sayede başka ajanlar (örn. Level 1'deki düşman) da aynı sınıfı kullanabilir.
    /// </summary>
    public class StateMachine
    {
        public IAgentState CurrentState { get; private set; }

        /// <summary>Durum değiştiğinde (eski, yeni) bildirir. Loglama ve UI için.</summary>
        public event Action<IAgentState, IAgentState> OnStateChanged;

        public void ChangeState(IAgentState newState)
        {
            if (newState == null || newState == CurrentState) return;

            IAgentState oldState = CurrentState;
            oldState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();

            Debug.Log($"[StateMachine] {oldState?.Name ?? "-"} → {newState.Name}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        public void Tick()
        {
            CurrentState?.Tick();
        }
    }
}
