using UnityEngine;

class MateBehaviour : SteeringBehaviour
{
    public enum State{
        Searching,
        Found,
        MovingToPartner,
        Mating
    }

    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles muscles;
    private readonly Animal animal;
    private readonly Timer timer;

    public State state { get; protected set; } = State.Searching;
    private const float matingTime = 3f;

    Animal potentialMate;
    Animal partner;
    public MateBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles, Animal animal) : base(muscles)
    {
        this.sensorySystem = sensorySystem;
        this.muscles = muscles;
        this.animal = animal;

        timer = new Timer(matingTime);
        timer.OnTimerFinished += () => { 
            Mate(partner); 
            partner = null;
            state = State.Searching;
        };
    }

    public override void Perform()
    {
        switch (state)
        {
            case State.Searching:
                potentialMate = sensorySystem.LookFor<Rabbit>();
                //Wander();
                if (IsCompatibleMate(potentialMate))
                {
                    state = State.Found;
                }
                break;
            case State.Found:
                if((potentialMate.getCurrentBehaviour() as MateBehaviour).RecieveProposal(this.animal))
                {
                    Commit(potentialMate);
                    state = State.MovingToPartner;
                }
                else
                {
                    state = State.Searching;
                }
                break;
            case State.MovingToPartner:
                //GoTo(partner.transform.position);
            #if UNITY_EDITOR
                Debug.DrawLine(muscles.transform.position, partner.transform.position, Color.red);
            #endif
                //if (HasArrived())
                {
                    timer.Start();
                    state = State.Mating;
                }
                break;
            case State.Mating:
                timer.Tick();
                break;
        }
        base.Perform();        
    }

    public bool RecieveProposal(Animal suitor)
    {
        //for now accept any proposal
        Commit(suitor);
        return true;
    }

    void Commit(Animal partner)
    {
        state = State.MovingToPartner;
        this.partner = partner;
    }

    private bool IsCompatibleMate(Animal candidate)
    {
        return candidate != null && candidate.Gender != animal.Gender && candidate.isSearchingMate();
    }

    private void Mate(Animal mate)
    {
        if(animal.Gender == Gender.Male)
        {
            animal.ReplicateMale();
        }
        else
        {
            animal.ReplicateFemale(mate.Genome);  
        }
    }
}