using Antymology.Helpers;
using Assets.Components.Agents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Antymology.Terrain
{
    public class WorldManager : Singleton<WorldManager>
    {

        #region Fields

        /// <summary>
        /// The prefab containing the ant.
        /// </summary>
        public GameObject antPrefab;

        /// <summary>
        /// The material used for eech block.
        /// </summary>
        public Material blockMaterial;

        /// <summary>
        /// The raw data of the underlying world structure.
        /// </summary>
        private AbstractBlock[,,] Blocks;

        /// <summary>
        /// Reference to the geometry data of the chunks.
        /// </summary>
        private Chunk[,,] Chunks;

        /// <summary>
        /// Random number generator.
        /// </summary>
        private System.Random RNG;

        /// <summary>
        /// Random number generator.
        /// </summary>
        private SimplexNoise SimplexNoise;

        private List<Generation> Generations;

        public Generation Current_Generation;

        /// <summary>
        /// References to ants in world
        /// </summary>
        public List<Ant> Ants;

        public Ant Queen;

        #endregion

        #region Initialization

        /// <summary>
        /// Awake is called before any start method is called.
        /// </summary>
        void Awake()
        {
            Debug.Log(Application.persistentDataPath);
            // Generate new random number generator
            RNG = new System.Random(ConfigurationManager.Instance.Seed);

            // Generate new simplex noise generator
            SimplexNoise = new SimplexNoise(ConfigurationManager.Instance.Seed);

            // Initialize a new 3D array of blocks with size of the number of chunks times the size of each chunk
            Blocks = new AbstractBlock[
                ConfigurationManager.Instance.World_Diameter * ConfigurationManager.Instance.Chunk_Diameter,
                ConfigurationManager.Instance.World_Height * ConfigurationManager.Instance.Chunk_Diameter,
                ConfigurationManager.Instance.World_Diameter * ConfigurationManager.Instance.Chunk_Diameter];

            // Initialize a new 3D array of chunks with size of the number of chunks
            Chunks = new Chunk[
                ConfigurationManager.Instance.World_Diameter,
                ConfigurationManager.Instance.World_Height,
                ConfigurationManager.Instance.World_Diameter];

            Ants = new List<Ant>();

            Generations = new List<Generation>();
            foreach (var file in System.IO.Directory.GetFiles(Application.persistentDataPath, "*.json"))
            {
                string json = System.IO.File.ReadAllText(file);
                Generation data = JsonUtility.FromJson<Generation>(json);
                Generations.Add(data);
            }
        }

        /// <summary>
        /// Called after every awake has been called.
        /// </summary>
        private void Start()
        {
            GenerateData();
            GenerateChunks();

            Camera.main.transform.position = new Vector3(0 / 2, Blocks.GetLength(1), 0);
            Camera.main.transform.LookAt(new Vector3(Blocks.GetLength(0), 0, Blocks.GetLength(2)));

            CreateGenerationConfiguration();
            GenerateAnts();
            StartCoroutine(TimeStepUpdate());
        }


        private IEnumerator TimeStepUpdate()
        {
            while (true)
            {
                // Wait for one second
                yield return new WaitForSeconds(1); 
                UpdateAnts();
                if (Queen == null)
                {
                    Debug.Log("DONE " + Current_Generation.ID);
                    string json = JsonUtility.ToJson(Current_Generation);
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/Generation" + Current_Generation.ID + "Data.json", json);
                    Start();
                    break;
                }
                UpdatePhermones();
            }      
        }

        private void UpdatePhermones()
        {
            // Iterate through all blocks in the world
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                for (int y = 0; y < Blocks.GetLength(1); y++)
                {
                    for (int z = 0; z < Blocks.GetLength(2); z++)
                    {
                        // Check if the block is an AirBlock
                        AirBlock airBlock = Blocks[x, y, z] as AirBlock;
                        if (airBlock != null)
                        {
                            AirBlock[] neighbours = GetNeighbouringAirBlocks(x, y, z);

                            airBlock.DiffuseFoodPheromone(neighbours);
                            airBlock.EvaporatePheromone();

                            float distance = Vector3.Distance(Queen.transform.position, new Vector3(x, y, z));
                            airBlock.queenScent = 100.0 / (1 + distance);
                        }
                        
                    }
                }
            }
        }

        private void UpdateAnts()
        {
            Ants.RemoveAll(item => item == null);
            foreach (var ant in Ants)
            { 
                ant.UpdateAnt();
            }
                   
        }

        public AirBlock[] GetNeighbouringAirBlocks(int x, int y, int z)
        {
            List<AirBlock> neighbours = new List<AirBlock>();

            // Iterate through all neighboring positions including diagonals in 3D
            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                for (int offsetY = -1; offsetY <= 1; offsetY++)
                {
                    for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
                    {
                        // Skip the current block (no offset)
                        if (offsetX == 0 && offsetY == 0 && offsetZ == 0) continue;

                        int neighbourX = x + offsetX;
                        int neighbourY = y + offsetY;
                        int neighbourZ = z + offsetZ;

                        // Check bounds to ensure indices are within the world array
                        if (neighbourX >= 0 && neighbourX < Blocks.GetLength(0) &&
                            neighbourY >= 0 && neighbourY < Blocks.GetLength(1) &&
                            neighbourZ >= 0 && neighbourZ < Blocks.GetLength(2))
                        {
                            AirBlock neighbour = Blocks[neighbourX, neighbourY, neighbourZ] as AirBlock;
                            if (neighbour != null)
                            {
                                neighbours.Add(neighbour);
                            }
                        }
                    }
                }
            }

            return neighbours.ToArray();
        }

        /// <summary>
        /// TO BE IMPLEMENTED BY YOU
        /// </summary>
        private void GenerateAnts()
        {
            GameObject antsParent = new GameObject("Ants");
            List<Vector3> spawnLocations = GetSpawnLocations();

            // Create Queen
            SpawnAnt(spawnLocations, antsParent, true);

            // Loop through the desired number of ants to generate 
            for (int i = 0; i < Current_Generation.Ant_Population - 1; i++)
            {
                if (spawnLocations.Count == 0)
                {
                    Debug.LogError("No location found for spawning ants.");
                    continue;
                }

                SpawnAnt(spawnLocations, antsParent);

                // TO DO setup for the ant can go here (e.g., assigning roles, initial resources, etc.)
            }
        }

        private void SpawnAnt(List<Vector3> locations, GameObject antsParent, bool isQueen = false)
        {
            // Select a random index from the list of grass top positions
            int randomIndex = RNG.Next(locations.Count);

            // Instantiate the ant prefab at the determined location
            GameObject antObject = Instantiate(antPrefab, locations[randomIndex], Quaternion.identity) as GameObject;
            if (isQueen)
            {
                Queen = antObject.GetComponent<Ant>();
                Queen.isQueen = true;
                antObject.name = "Queen";

            } else
            {
                antObject.transform.SetParent(antsParent.transform, false);
            }
            locations.RemoveAt(randomIndex);
            Ants.Add(antObject.GetComponent<Ant>());
        }


        private void CreateGenerationConfiguration() 
        {
            Generation gen;
            if (Generations.Count < ConfigurationManager.Instance.Number_Of_Starting_Generations)
            {
                gen = new Generation();
                gen.RandomInitialization();
            }
            else
            {
                Generations.OrderByDescending(i => i.Nest_Blocks);
                gen = new Generation(Generations[1], Generations[2]);
            }

            Generations.Add(gen);
            gen.ID = Generations.Count;
            gen.PrintConfiguration();
            Current_Generation = gen;
        }

        #endregion

        #region Methods

        private void ResetSimulation()
        {
            
        }

        /// <summary>
        /// Finds all topmost blocks that are not container or airblock blocks. i.e. possible spawn locations
        /// </summary>
        /// <returns>A list of Vector3 positions for the topmost non-container blocks.</returns>
        public List<Vector3> GetSpawnLocations()
        {
            List<Vector3> topmostBlocks = new List<Vector3>();

            // Iterate over each column in the world.
            for (int x = 0; x < Blocks.GetLength(0); x++)
            {
                for (int z = 0; z < Blocks.GetLength(2); z++)
                {
                    // Start from the top of the column and search downwards.
                    for (int y = Blocks.GetLength(1) - 1; y >= 0; y--)
                    {
                        AbstractBlock currentBlock = Blocks[x, y, z];

                        // Check if the current block is not a ContainerBlock OR AirBlock.
                        if (!(currentBlock is ContainerBlock) && currentBlock.isVisible())
                        {
                            // If the block above is an AirBlock, it means we have found the topmost block.
                            if (y == Blocks.GetLength(1) - 1 || Blocks[x, y + 1, z] is AirBlock)
                            {
                                topmostBlocks.Add(new Vector3(x, (float)(y + 0.4), (float)(z + 0.1)));
                            }

                            // Break the loop after finding the topmost block for this column.
                            break;
                        }
                    }
                }
            }

            return topmostBlocks;
        }

        /// <summary>
        /// Retrieves an abstract block type at the desired world coordinates.
        /// </summary>
        public AbstractBlock GetBlock(int WorldXCoordinate, int WorldYCoordinate, int WorldZCoordinate)
        {
            if
            (
                WorldXCoordinate < 0 ||
                WorldYCoordinate < 0 ||
                WorldZCoordinate < 0 ||
                WorldXCoordinate >= Blocks.GetLength(0) ||
                WorldYCoordinate >= Blocks.GetLength(1) ||
                WorldZCoordinate >= Blocks.GetLength(2)
            )
                return new AirBlock();

            return Blocks[WorldXCoordinate, WorldYCoordinate, WorldZCoordinate];
        }

        /// <summary>
        /// Retrieves an abstract block type at the desired local coordinates within a chunk.
        /// </summary>
        public AbstractBlock GetBlock(
            int ChunkXCoordinate, int ChunkYCoordinate, int ChunkZCoordinate,
            int LocalXCoordinate, int LocalYCoordinate, int LocalZCoordinate)
        {
            if
            (
                LocalXCoordinate < 0 ||
                LocalYCoordinate < 0 ||
                LocalZCoordinate < 0 ||
                LocalXCoordinate >= Blocks.GetLength(0) ||
                LocalYCoordinate >= Blocks.GetLength(1) ||
                LocalZCoordinate >= Blocks.GetLength(2) ||
                ChunkXCoordinate < 0 ||
                ChunkYCoordinate < 0 ||
                ChunkZCoordinate < 0 ||
                ChunkXCoordinate >= Blocks.GetLength(0) ||
                ChunkYCoordinate >= Blocks.GetLength(1) ||
                ChunkZCoordinate >= Blocks.GetLength(2) 
            )
                return new AirBlock();

            return Blocks
            [
                ChunkXCoordinate * LocalXCoordinate,
                ChunkYCoordinate * LocalYCoordinate,
                ChunkZCoordinate * LocalZCoordinate
            ];
        }

        /// <summary>
        /// sets an abstract block type at the desired world coordinates.
        /// </summary>
        public void SetBlock(int WorldXCoordinate, int WorldYCoordinate, int WorldZCoordinate, AbstractBlock toSet)
        {
            if
            (
                WorldXCoordinate < 0 ||
                WorldYCoordinate < 0 ||
                WorldZCoordinate < 0 ||
                WorldXCoordinate > Blocks.GetLength(0) ||
                WorldYCoordinate > Blocks.GetLength(1) ||
                WorldZCoordinate > Blocks.GetLength(2)
            )
            {
                Debug.Log("Attempted to set a block which didn't exist");
                return;
            }

            Blocks[WorldXCoordinate, WorldYCoordinate, WorldZCoordinate] = toSet;

            SetChunkContainingBlockToUpdate
            (
                WorldXCoordinate,
                WorldYCoordinate,
                WorldZCoordinate
            );
        }

        /// <summary>
        /// sets an abstract block type at the desired local coordinates within a chunk.
        /// </summary>
        public void SetBlock(
            int ChunkXCoordinate, int ChunkYCoordinate, int ChunkZCoordinate,
            int LocalXCoordinate, int LocalYCoordinate, int LocalZCoordinate,
            AbstractBlock toSet)
        {
            if
            (
                LocalXCoordinate < 0 ||
                LocalYCoordinate < 0 ||
                LocalZCoordinate < 0 ||
                LocalXCoordinate > Blocks.GetLength(0) ||
                LocalYCoordinate > Blocks.GetLength(1) ||
                LocalZCoordinate > Blocks.GetLength(2) ||
                ChunkXCoordinate < 0 ||
                ChunkYCoordinate < 0 ||
                ChunkZCoordinate < 0 ||
                ChunkXCoordinate > Blocks.GetLength(0) ||
                ChunkYCoordinate > Blocks.GetLength(1) ||
                ChunkZCoordinate > Blocks.GetLength(2)
            )
            {
                Debug.Log("Attempted to set a block which didn't exist");
                return;
            }
            Blocks
            [
                ChunkXCoordinate * LocalXCoordinate,
                ChunkYCoordinate * LocalYCoordinate,
                ChunkZCoordinate * LocalZCoordinate
            ] = toSet;

            SetChunkContainingBlockToUpdate
            (
                ChunkXCoordinate * LocalXCoordinate,
                ChunkYCoordinate * LocalYCoordinate,
                ChunkZCoordinate * LocalZCoordinate
            );
        }

        #endregion

        #region Helpers

        #region Ants

        
        #endregion

        #region Blocks

        /// <summary>
        /// Is responsible for generating the base, acid, and spheres.
        /// </summary>
        private void GenerateData()
        {
            GeneratePreliminaryWorld();
            GenerateAcidicRegions();
            GenerateSphericalContainers();
        }

        /// <summary>
        /// Generates the preliminary world data based on perlin noise.
        /// </summary>
        private void GeneratePreliminaryWorld()
        {
            for (int x = 0; x < Blocks.GetLength(0); x++)
                for (int z = 0; z < Blocks.GetLength(2); z++)
                {
                    /**
                     * These numbers have been fine-tuned and tweaked through trial and error.
                     * Altering these numbers may produce weird looking worlds.
                     **/
                    int stoneCeiling = SimplexNoise.GetPerlinNoise(x, 0, z, 10, 3, 1.2) +
                                       SimplexNoise.GetPerlinNoise(x, 300, z, 20, 4, 0) +
                                       10;
                    int grassHeight = SimplexNoise.GetPerlinNoise(x, 100, z, 30, 10, 0);
                    int foodHeight = SimplexNoise.GetPerlinNoise(x, 200, z, 20, 5, 1.5);

                    for (int y = 0; y < Blocks.GetLength(1); y++)
                    {
                        if (y <= stoneCeiling)
                        {
                            Blocks[x, y, z] = new StoneBlock();
                        }
                        else if (y <= stoneCeiling + grassHeight)
                        {
                            Blocks[x, y, z] = new GrassBlock();
                        }
                        else if (y <= stoneCeiling + grassHeight + foodHeight)
                        {
                            Blocks[x, y, z] = new MulchBlock();
                        }
                        else
                        {
                            Blocks[x, y, z] = new AirBlock();
                        }
                        if
                        (
                            x == 0 ||
                            x >= Blocks.GetLength(0) - 1 ||
                            z == 0 ||
                            z >= Blocks.GetLength(2) - 1 ||
                            y == 0
                        )
                            Blocks[x, y, z] = new ContainerBlock();
                    }
                }
        }

        /// <summary>
        /// Alters a pre-generated map so that acid blocks exist.
        /// </summary>


        private void GenerateAcidicRegions()
        {
            for (int i = 0; i < ConfigurationManager.Instance.Number_Of_Acidic_Regions; i++)
            {
                int xCoord = RNG.Next(0, Blocks.GetLength(0));
                int zCoord = RNG.Next(0, Blocks.GetLength(2));
                int yCoord = -1;
                for (int j = Blocks.GetLength(1) - 1; j >= 0; j--)
                {
                    if (Blocks[xCoord, j, zCoord] as AirBlock == null)
                    {
                        yCoord = j;
                        break;
                    }
                }

                //Generate a sphere around this point overriding non-air blocks
                for (int HX = xCoord - ConfigurationManager.Instance.Acidic_Region_Radius; HX < xCoord + ConfigurationManager.Instance.Acidic_Region_Radius; HX++)
                {
                    for (int HZ = zCoord - ConfigurationManager.Instance.Acidic_Region_Radius; HZ < zCoord + ConfigurationManager.Instance.Acidic_Region_Radius; HZ++)
                    {
                        for (int HY = yCoord - ConfigurationManager.Instance.Acidic_Region_Radius; HY < yCoord + ConfigurationManager.Instance.Acidic_Region_Radius; HY++)
                        {
                            float xSquare = (xCoord - HX) * (xCoord - HX);
                            float ySquare = (yCoord - HY) * (yCoord - HY);
                            float zSquare = (zCoord - HZ) * (zCoord - HZ);
                            float Dist = Mathf.Sqrt(xSquare + ySquare + zSquare);
                            if (Dist <= ConfigurationManager.Instance.Acidic_Region_Radius)
                            {
                                int CX, CY, CZ;
                                CX = Mathf.Clamp(HX, 1, Blocks.GetLength(0) - 2);
                                CZ = Mathf.Clamp(HZ, 1, Blocks.GetLength(2) - 2);
                                CY = Mathf.Clamp(HY, 1, Blocks.GetLength(1) - 2);
                                if (Blocks[CX, CY, CZ] as AirBlock != null)
                                    Blocks[CX, CY, CZ] = new AcidicBlock();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alters a pre-generated map so that obstructions exist within the map.
        /// </summary>
        private void GenerateSphericalContainers()
        {

            //Generate hazards
            for (int i = 0; i < ConfigurationManager.Instance.Number_Of_Conatiner_Spheres; i++)
            {
                int xCoord = RNG.Next(0, Blocks.GetLength(0));
                int zCoord = RNG.Next(0, Blocks.GetLength(2));
                int yCoord = RNG.Next(0, Blocks.GetLength(1));


                //Generate a sphere around this point overriding non-air blocks
                for (int HX = xCoord - ConfigurationManager.Instance.Conatiner_Sphere_Radius; HX < xCoord + ConfigurationManager.Instance.Conatiner_Sphere_Radius; HX++)
                {
                    for (int HZ = zCoord - ConfigurationManager.Instance.Conatiner_Sphere_Radius; HZ < zCoord + ConfigurationManager.Instance.Conatiner_Sphere_Radius; HZ++)
                    {
                        for (int HY = yCoord - ConfigurationManager.Instance.Conatiner_Sphere_Radius; HY < yCoord + ConfigurationManager.Instance.Conatiner_Sphere_Radius; HY++)
                        {
                            float xSquare = (xCoord - HX) * (xCoord - HX);
                            float ySquare = (yCoord - HY) * (yCoord - HY);
                            float zSquare = (zCoord - HZ) * (zCoord - HZ);
                            float Dist = Mathf.Sqrt(xSquare + ySquare + zSquare);
                            if (Dist <= ConfigurationManager.Instance.Conatiner_Sphere_Radius)
                            {
                                int CX, CY, CZ;
                                CX = Mathf.Clamp(HX, 1, Blocks.GetLength(0) - 2);
                                CZ = Mathf.Clamp(HZ, 1, Blocks.GetLength(2) - 2);
                                CY = Mathf.Clamp(HY, 1, Blocks.GetLength(1) - 2);
                                Blocks[CX, CY, CZ] = new ContainerBlock();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Given a world coordinate, tells the chunk holding that coordinate to update.
        /// Also tells all 4 neighbours to update (as an altered block might exist on the
        /// edge of a chunk).
        /// </summary>
        /// <param name="worldXCoordinate"></param>
        /// <param name="worldYCoordinate"></param>
        /// <param name="worldZCoordinate"></param>
        private void SetChunkContainingBlockToUpdate(int worldXCoordinate, int worldYCoordinate, int worldZCoordinate)
        {
            //Updates the chunk containing this block
            int updateX = Mathf.FloorToInt(worldXCoordinate / ConfigurationManager.Instance.Chunk_Diameter);
            int updateY = Mathf.FloorToInt(worldYCoordinate / ConfigurationManager.Instance.Chunk_Diameter);
            int updateZ = Mathf.FloorToInt(worldZCoordinate / ConfigurationManager.Instance.Chunk_Diameter);
            Chunks[updateX, updateY, updateZ].updateNeeded = true;
            
            // Also flag all 6 neighbours for update as well
            if(updateX - 1 >= 0)
                Chunks[updateX - 1, updateY, updateZ].updateNeeded = true;
            if (updateX + 1 < Chunks.GetLength(0))
                Chunks[updateX + 1, updateY, updateZ].updateNeeded = true;

            if (updateY - 1 >= 0)
                Chunks[updateX, updateY - 1, updateZ].updateNeeded = true;
            if (updateY + 1 < Chunks.GetLength(1))
                Chunks[updateX, updateY + 1, updateZ].updateNeeded = true;

            if (updateZ - 1 >= 0)
                Chunks[updateX, updateY, updateZ - 1].updateNeeded = true;
            if (updateZ + 1 < Chunks.GetLength(2))
                Chunks[updateX, updateY, updateZ + 1].updateNeeded = true;
        }

        #endregion

        #region Chunks

        /// <summary>
        /// Takes the world data and generates the associated chunk objects.
        /// </summary>
        private void GenerateChunks()
        {
            GameObject chunkObg = new GameObject("Chunks");

            for (int x = 0; x < Chunks.GetLength(0); x++)
                for (int z = 0; z < Chunks.GetLength(2); z++)
                    for (int y = 0; y < Chunks.GetLength(1); y++)
                    {
                        GameObject temp = new GameObject();
                        temp.transform.parent = chunkObg.transform;
                        temp.transform.position = new Vector3
                        (
                            x * ConfigurationManager.Instance.Chunk_Diameter - 0.5f,
                            y * ConfigurationManager.Instance.Chunk_Diameter + 0.5f,
                            z * ConfigurationManager.Instance.Chunk_Diameter - 0.5f
                        );
                        Chunk chunkScript = temp.AddComponent<Chunk>();
                        chunkScript.x = x * ConfigurationManager.Instance.Chunk_Diameter;
                        chunkScript.y = y * ConfigurationManager.Instance.Chunk_Diameter;
                        chunkScript.z = z * ConfigurationManager.Instance.Chunk_Diameter;
                        chunkScript.Init(blockMaterial);
                        chunkScript.GenerateMesh();
                        Chunks[x, y, z] = chunkScript;
                    }
        }

        #endregion

        #endregion
    }
}
