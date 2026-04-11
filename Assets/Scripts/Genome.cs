using System.Collections.Generic;

public class Genome
{
    public readonly Dictionary<string, Gene> genes = new Dictionary<string, Gene>();
    
    public Genome(IEnumerable<GeneScriptableObject> genesInitialData)
    {
        if(genesInitialData == null) { return; }
        foreach(var geneData in genesInitialData)
        {
            genes.Add(geneData.Name,new Gene(geneData));
        }
    }

    private Genome(Dictionary<string, Gene> genes)
    {
        this.genes = genes;
    }

    public float GetGeneValue(string geneName)
    {
        return genes[geneName].Value;
    }

    public Gene GetGene(string geneName)
    {
        return genes[geneName];
    }

    public Genome Recombine(Genome other)
    {
        Dictionary<string, Gene> newGenome = new Dictionary<string, Gene>();
        System.Random rnd = new System.Random();
        
        foreach(var gene in genes)
        {
            int result = rnd.Next(0,2);
            if(result == 0)
            {
                newGenome.Add(gene.Key,gene.Value.Replicate());
            }
            else
            {
                newGenome.Add(gene.Key,other.genes[gene.Key].Replicate());
            }
        }

        return new Genome(newGenome);
    }
}