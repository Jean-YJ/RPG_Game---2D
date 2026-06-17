using UnityEngine;

public class StateMachine
{
    private EntityState currentState;
    public EntityState CurrentState
    {
        get { return this.currentState; }
        private set { this.currentState = value; }
    }
    public bool canEnterNewState;

    public void Initialize(EntityState originState)
    {
        this.canEnterNewState = true;
        this.currentState = originState;
        this.currentState.Enter();
    }
    public void ChangeState(EntityState neWState)
    {
        if (!this.canEnterNewState)
            return;

        this.currentState.Exit();
        this.currentState = neWState;
        this.currentState.Enter();
    }
    public void UpdateActiveState()
    {
        this.currentState.Update();
    }
    public void SwitchOffStateMachine() => this.canEnterNewState = false;
}
