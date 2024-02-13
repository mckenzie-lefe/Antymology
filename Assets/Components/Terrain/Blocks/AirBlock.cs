using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Antymology.Terrain
{
    /// <summary>
    /// The air type of block. Contains the internal data representing phermones in the air.
    /// </summary>
    public class AirBlock : AbstractBlock
    {

        #region Fields

        /// <summary>
        /// Statically held is visible.
        /// </summary>
        private static bool _isVisible = false;

        private int queenScent = 0;

        /// <summary>
        /// A dictionary representing the pheromone deposits in the air. Each type of phermone gets it's own byte key, and each phermone type has a concentration.
        /// THIS CURRENTLY ONLY EXISTS AS A WAY OF SHOWING YOU HOW YOU CAN MANIPULATE THE BLOCKS.
        /// </summary>
        public Dictionary<byte, double> pheromoneDeposits = new Dictionary<byte, double> {
            { 1, 0.0 }
        };

        #endregion

        #region Methods

        /// <summary>
        /// Air blocks are going to be invisible.
        /// </summary>
        public override bool isVisible()
        {
            return _isVisible;
        }

        /// <summary>
        /// Air blocks are invisible so asking for their tile map coordinate doesn't make sense.
        /// </summary>
        public override Vector2 tileMapCoordinate()
        {
            throw new Exception("An invisible tile cannot have a tile map coordinate.");
        }

        public void SetQueenScent(int val)
        {
            queenScent = val;
        }

        /// <summary>
        /// Each phermone type diffuses 50% of it value to its neighboring AirBlocks.
        /// </summary>
        /// <param name="neighbours">Airblocks in radius</param>
        public void DiffuseFoodPheromone(AirBlock[] neighbours)
        {
            pheromoneDeposits[1] = pheromoneDeposits[1] / 2.0;
            var diffuseAmount = pheromoneDeposits[1] / neighbours.Length;
            foreach (AirBlock block in neighbours)
            {
                block.Deposit(1, diffuseAmount);
            }
        }

        public void Deposit(byte pheromoneType, double amount)
        {
            pheromoneDeposits[pheromoneType] += amount;
            Debug.Log("ab=" + pheromoneDeposits[1]);
        }

        public void Evaporate()
        {
            List<byte> keys = new List<byte>(pheromoneDeposits.Keys);
            foreach (byte key in keys)
            {
                pheromoneDeposits[key] *= (1 - ConfigurationManager.Instance.Pheromone_Evaperation_Rate); 
            }
        }

        #endregion

    }
}
