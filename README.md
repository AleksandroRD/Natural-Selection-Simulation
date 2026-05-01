# Natural Selection Simulation

A real-time simulation of natural selection built with Unity 6000.3.12f1.

## Features

- Real-time population simulation with configurable time scale
- Each creature carries a unique **Genome** composed of multiple **Genes**
- Reproduction blends parent Genomes via crossover
- Each Gene is configurable through a ScriptableObject
- Live statistics: population count, gene value distribution, and gene average history

## How to Add a New Creature

### 1. Base Class

All simulated entities derive from [`SimulationEntity`](Assets/Scripts/SimulationEntity.cs), an abstract class with a single abstract method `Simulate()`. Any object you want the simulation to drive must inherit from this class.

### 2. Creating an Animal

To create a custom animal, inherit from [`Animal`](Assets/Scripts/Animal.cs). This class manages:

- `CurrentEnergy` — the animal dies (via `protected virtual void Death()`) when this reaches zero
- `MatingUrge` and `Gender` — used to govern reproduction logic
- `CurrentBehaviour` — the active [`Behaviour`](Assets/Scripts/Behaviour.cs) that runs every `Simulate()` call

The `Animal` class declares two abstract replication methods you must override:
```csharp
public abstract void Replicate(Genome otherGenome); //for female
public abstract void Replicate(); // for male
```

It also provides a helper `protected void Eat(float amount)` to restore energy.

### 3. Implementing Behaviour

A [`Behaviour`](Assets/Scripts/Behaviour.cs) decides what the animal does each simulation step. Inside `Simulate()`, your code should determine which behaviour is appropriate and call `SetBehaviour(Behaviour newBehaviour)` — the `Perform()` method is then called automatically.

Several behaviours are already provided:

| Behaviour | Description |
|---|---|
| [`WanderingBehaviour`](Assets/Scripts/Behaviours/WanderingBehaviour.cs) | Base class for all movement-based behaviours |
| [`RoamingBehaviour`](Assets/Scripts/Behaviours/RoamingBehaviour.cs) | Aimless movement |
| [`SearchForFoodBehaviour`](Assets/Scripts/Behaviours/SearchForFoodBehaviour.cs) | Seeks out nearby food |
| [`MateBehaviour`](Assets/Scripts/Behaviours/MateBehaviour.cs) | Seeks a mate for reproduction |

### 4. Working with Genes

Genes are defined via [`GeneScriptableObject`](Assets/Scripts/GeneScriptableObject.cs) and represented at runtime by the [`Gene`](Assets/Scripts/Gene.cs) class. A collection of Genes is stored in a [`Genome`](Assets/Scripts/Genome.cs).

To construct a Gene from a ScriptableObject asset:
```csharp
Gene myGene = new Gene(myGeneScriptableObject);
```

## Inspiration

- [Primer (YouTube)](https://www.youtube.com/@PrimerBlobs/videos)
- [*The Selfish Gene* — Richard Dawkins](https://www.amazon.de/-/en/Selfish-Gene-Anniversary-Landmark-Science/dp/0198788606)
