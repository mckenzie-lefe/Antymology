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
            Ant_Population = rng.Next(ConfigurationManager.Instance.Min_Starting_Ants, ConfigurationManager.Instance.Max_Starting_Ants);
            Starting_Ant_Health = rng.Next(10, ConfigurationManager.Instance.Max_Ant_Health);
            Max_Ant_Health = Max_Queen_Health = rng.Next(30, ConfigurationManager.Instance.Max_Ant_Health);
            Max_Queen_Health = rng.Next(30, ConfigurationManager.Instance.Max_Ant_Health);
            Step_Health_Reduction = 5;
            Pheromone_Evaperation_Rate = rng.Next(1, 100) / 100.0;
        }

        public Generation(Generation g1, Generation g2)
        {

        }

        public void PrintConfiguration()
        {
            Debug.Log("Ant_Population=" + Ant_Population);
            Debug.Log("Starting_Ant_Health=" + Starting_Ant_Health);
            Debug.Log("Max_Ant_Health=" + Max_Ant_Health);
            Debug.Log("Max_Queen_Health=" + Max_Queen_Health);
            Debug.Log("Step_Health_Reduction=" + Step_Health_Reduction);
            Debug.Log("Pheromone_Evaperation_Rate=" + Pheromone_Evaperation_Rate);
        }
    }
}
