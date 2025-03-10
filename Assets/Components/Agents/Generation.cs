﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Components.Agents
{
    [System.Serializable]
    public class Generation 
    {
        public int ID;

        /// <summary>
        /// The starting population of ants
        /// </summary>
        public int Ant_Population;

        /// <summary>
        /// Ants starting health measure
        /// </summary>
        public int Starting_Ant_Health;

        /// <summary>
        /// Ants max health 
        /// </summary>
        public int Max_Ant_Health;

        /// <summary>
        /// Queen ant max health
        /// </summary>
        public int Max_Queen_Health;

        /// <summary>
        /// Ants starting health measure
        /// </summary>
        public int Step_Health_Reduction;

        /// <summary>
        /// Rate that pheromones evaperated from airblocks each step
        /// </summary>
        public double Pheromone_Evaperation_Rate;

        /// <summary>
        /// How much remaining ant health for ant to be considered hungry and start looking for food.  
        /// </summary>
        public float Hungry_Threshold;

        /// <summary>
        /// Percentage of ants whose job is to find food and transfer to energy to Nest building ants (jobType 1) 
        /// </summary>
        public int Percent_Worker_Ants;

        /// <summary>
        /// Percentage of ants whose job is to transfer enegry to queen so that she can build NestBlocks (jobType 2) 
        /// </summary>
        public int Percent_Protector_Ants;

        /// <summary>
        /// Nest blocks created by generation
        /// </summary>
        public int Nest_Blocks;

        public Generation() 
        {
            Nest_Blocks = 0;
        }

        public void RandomInitialization()
        {
            System.Random rng = new System.Random();
            Nest_Blocks = 0;
            Step_Health_Reduction = 5;
            Ant_Population = rng.Next(ConfigurationManager.Instance.Min_Starting_Ants, ConfigurationManager.Instance.Max_Starting_Ants);
            Max_Ant_Health = rng.Next(30, ConfigurationManager.Instance.Max_Ant_Health);
            Starting_Ant_Health = rng.Next(Max_Ant_Health / 3, Max_Ant_Health);
            Max_Queen_Health = rng.Next(Max_Ant_Health, ConfigurationManager.Instance.Max_Ant_Health);  
            Pheromone_Evaperation_Rate = rng.Next(1, 100) / 100.0;
            Hungry_Threshold = (float) Math.Round(rng.Next(1, ConfigurationManager.Instance.Max_Hungry_Threshold_Percent) / 100.0, 2);
            Percent_Protector_Ants = (int) Math.Round(((rng.Next(1, Ant_Population)) / (double)Ant_Population) * 100);
            Percent_Worker_Ants = 100 - Percent_Protector_Ants;
        }

        public Generation(Generation g1, Generation g2)
        {
            Nest_Blocks = 0;
            Step_Health_Reduction = 5;
            Ant_Population = g1.Ant_Population;
            Starting_Ant_Health = g2.Ant_Population;
            Max_Ant_Health = g2.Max_Ant_Health;
            Max_Queen_Health = g2.Max_Queen_Health;
            Pheromone_Evaperation_Rate = g1.Pheromone_Evaperation_Rate;
            Hungry_Threshold= g1.Hungry_Threshold;
            Percent_Protector_Ants = g2.Percent_Protector_Ants;
            Percent_Worker_Ants = g2.Percent_Worker_Ants;
        }

        public void PrintConfiguration()
        {
            Debug.Log("Ant_Population=" + Ant_Population + "\n" +
                "Starting_Ant_Health=" + Starting_Ant_Health + "\n" +
                "Max_Ant_Health=" + Max_Ant_Health + "\n" +
                "Max_Queen_Health=" + Max_Queen_Health + "\n" +
                //"Step_Health_Reduction=" + Step_Health_Reduction + "\n" + 
                "Pheromone_Evaperation_Rate=" + Pheromone_Evaperation_Rate + "\n" +
                "Hungry_Threshold=" + Hungry_Threshold + "\n" +
                //"Percent_Nest_Builder_Ants=" + Percent_Nest_Builder_Ants + "\n" +
                "Percent_Worker_Antse=" + Percent_Worker_Ants);
        }
    }
}
