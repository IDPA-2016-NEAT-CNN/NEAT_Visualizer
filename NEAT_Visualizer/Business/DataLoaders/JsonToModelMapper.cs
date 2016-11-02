﻿using System.Collections.Generic;
using System.Linq;
using NEAT_Visualizer.Model;

namespace NEAT_Visualizer.Business.DataLoaders
{
  public static class JsonToModelMapper
  {
    public static Generation ToModel(this JsonRepresentation.Rootobject jsonRoot)
    {
      var generation = new Generation
      {
        GenerationsPassed = jsonRoot.generationsPassed,
        //PopulationSize = jsonRoot.populationSize, //TODO verify this is correct
        PopulationSize = jsonRoot.species.Sum(z => z.population.Length)
      };

      foreach (var species in jsonRoot.species)
      {
        generation.Species.Add(species.ToModel());
      }

      generation.FitnessHighscore = generation.Species.Select(n => n.FitnessHighscore).Max();

      return generation;
    }

    private static Species ToModel(this JsonRepresentation.Species speciesRepresentation)
    {
      var species = new Species();

      foreach (var population in speciesRepresentation.population)
      {
        species.Networks.Add(CreateNetworkFromData(population));
      }

      return species;
    }

    private static NeuralNetwork CreateNetworkFromData(this JsonRepresentation.Population organism)
    {
      var network = new NeuralNetwork
      {
        Fitness = organism.fitness,
        FitnessModifier = organism.fitnessModifier
      };

      var neurons = organism.network.neurons.Select(n => new Neuron() { Layer = n.layer }).ToList();

      var geneEnumerator = organism.network.genome.genes.ToList().GetEnumerator();
      var straightToOutputConnections = new List<JsonRepresentation.Gene1>();
      while (geneEnumerator.MoveNext() && geneEnumerator.Current.from == 0)
      {
        straightToOutputConnections.Add(geneEnumerator.Current);
      }  
      
      // creates the connections from the genomes
      foreach (var genome in organism.network.genome.genes)
      {
        if (genome.isEnabled)
        {
          neurons[genome.to].IncomingConnections.Add(
            new Connection(neurons[genome.from], genome.weight, genome.historicalMarking));
        }
      }

      int maxLayer = neurons.Max(n => n.Layer);
      foreach (var straightToOutputConnection in straightToOutputConnections)
      {
        neurons[straightToOutputConnection.to].Layer = maxLayer;
      }

      network.Neurons = neurons;
      return network;
    }
  }
}