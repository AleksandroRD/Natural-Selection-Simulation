class MateBehaviour : Behaviour
{
    private readonly SensoryNervousSystem sensorySystem;
    private readonly Muscles Muscles;

    public MateBehaviour(SensoryNervousSystem sensorySystem, Muscles muscles)
    {
        this.sensorySystem = sensorySystem;
        this.Muscles = muscles;
    }
    public override void Perform()
    {
        if (Muscles.HasArrived())
        {
            //mating behaviour
        }
        
        Rabbit nearestMate = sensorySystem.LookFor<Rabbit>();

        if(nearestMate == null)
        {
            //keep looking
        }

        Muscles.MoveTo(nearestMate.transform);
    }

}