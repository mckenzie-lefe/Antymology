using Antymology.Terrain;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant : MonoBehaviour
{
    /// <summary>
    /// Tracks the ant's health status
    /// </summary>
    public float health;

    /// <summary>
    ///  Indicates whether the ant has found food.
    /// </summary>
    public bool foundFood = false;

    /// <summary>
    /// Represents if the ant is hungry.
    /// </summary>
    private bool hungry;

    /// <summary>
    /// Defines the ant's role within the colony (1 for nest builder, 2 for worker ant).
    /// nest builder antsrole is to transfer engery to queen to build nest
    /// worker ants role is to find food and give to nest building ants
    /// </summary>
    public int role;

    /// <summary>
    /// Specifies if the ant is the queen.
    /// </summary>
    public bool isQueen = false;

    /// <summary>
    /// The current air block the ant is interacting with.
    /// </summary>
    public AirBlock currentAirBlock;

    /// <summary>
    /// The maximum health an ant can have.
    /// </summary>
    private float maxHealth;

    /// <summary>
    /// The amount of health reduced in each step.
    /// </summary>
    private float healthReduction;

    /// <summary>
    /// The threshold below which an ant is considered hungry.
    /// </summary>
    private float healthThreshold;

    /// <summary>
    /// Material used to visually indicate the queen ant.
    /// </summary>
    private Material queenMaterial;

    /// <summary>
    /// Material used to visually indicate the ant's health status.
    /// </summary>
    private Material weakHealthMaterial;

    /// <summary>
    /// Material used to visually indicate the ant's health status.
    /// </summary>
    private Material strongHealthMaterial;

    /// <summary>
    /// A random number generator for various operations.
    /// </summary>
    private System.Random RNG;


    // Start is called before the first frame update
    void Start()
    {
        RNG = new System.Random();
        queenMaterial = (Material)Resources.Load("Materials/QueenAntMaterial", typeof(Material));
        weakHealthMaterial = (Material)Resources.Load("Materials/WeakHealthMaterial", typeof(Material));
        strongHealthMaterial = (Material)Resources.Load("Materials/StrongHealthMaterial", typeof(Material));
        currentAirBlock = WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y + 1, (int)transform.position.z) as AirBlock;
  
        maxHealth = WorldManager.Instance.Current_Generation.Max_Ant_Health;
        health = WorldManager.Instance.Current_Generation.Starting_Ant_Health;
        healthReduction = WorldManager.Instance.Current_Generation.Step_Health_Reduction;
        healthThreshold = (float) System.Math.Round((WorldManager.Instance.Current_Generation.Hungry_Threshold * WorldManager.Instance.Current_Generation.Max_Ant_Health) / 1);

        if (health > healthThreshold)
            hungry = false;
        else
            hungry = true;

        if (isQueen)
            InitializeQueen();

        SetHealthBarVisibility();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Main logic for ant behavior per frame, including movement and health management.
    /// </summary>
    public void UpdateAnt()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
            return;
        }

        if (isQueen)
            MoveQueen();
        else
        {
            // cannot consume mulch if another ant is also on the same mulch block
            if (hungry && GetBlockBelow() is MulchBlock && !AntAtLocation(transform.position))
                ConsumeMulchBlock();
            else
                MoveToTarget();

            if (!hungry)
                GiveHealth();

            DepositePheramone();
        }
        
        try
        {
            currentAirBlock = WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y + 1, (int)transform.position.z) as AirBlock;
        } catch (Exception ex)
        {
            Debug.Log("Current airblock set: " + ex.ToString());
        }
       
        ManageHealth();
    }

    #region Methods

    /// <summary>
    /// Sets up special properties for the queen ant.
    /// </summary>
    public void InitializeQueen()
    {
        MeshRenderer meshRenderer = transform.Find("ant_blk").GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = queenMaterial;
        }
        else
        {
            Debug.LogError("MeshRenderer component not found on 'ant_blk'");
        }
        maxHealth = WorldManager.Instance.Current_Generation.Max_Queen_Health;
    }

    /// <summary>
    /// Allows an ant to receive health from another ant.
    /// </summary>
    public void ReceiveHealth(float amount)
    {// Method for ant to receive health from another ant
        health += amount;

        if (isQueen)
            Debug.Log(this.name + " GOT HEALTH");

        if (!isQueen && hungry && health >= healthThreshold) 
            hungry = false;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Consumes a mulch block to restore health and set the foundFood flag.
    /// </summary>
    private void ConsumeMulchBlock()
    {   // assume safe to consume mulch block
        if (GetBlockBelow() is MulchBlock)
        {
            foundFood = true;
            RemoveBlockBelow();
            health = maxHealth;
            hungry = false;
        }
    }

    /// <summary>
    /// Contains the logic for queen movement and action, including consuming mulch blocks or building the nest.
    /// </summary>
    private void MoveQueen()
    {

        if (GetBlockBelow() is MulchBlock && health < (maxHealth / 3) * 2 && !AntAtLocation(transform.position))
        {
            ConsumeMulchBlock();
        }
        else if (health > (maxHealth / 3) * 2)
        {
            BuildNest();
        }
        else
        {
            Dictionary<AirBlock, Vector3> movablePositions = FindMovableAdjacentBlocks();
            movablePositions.OrderBy(i => i.Key.pheromone);
            transform.position = movablePositions.Values.ElementAt(RNG.Next(movablePositions.Count));
        }
    }

    /// <summary>
    /// Moves the ant towards another ant within a certain range.
    /// </summary>
    private void MoveToAnt(Dictionary<AirBlock, Vector3> movablePositions)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.3f);
        foreach (var hitCollider in hitColliders)
        {
            Ant receivingAnt = hitCollider.gameObject.GetComponent<Ant>();
            if (receivingAnt != null)
            {
                if (receivingAnt == this) continue;
                //Debug.Log(this.name + " found " + hitCollider.name + " near by");
                transform.position = receivingAnt.transform.position;
                return;
            }
        }
        transform.position = movablePositions.Values.ElementAt(RNG.Next(System.Math.Min(movablePositions.Count, 3)));
    }

    /// <summary>
    /// Enables an ant to transfer health to another ant if conditions are met.
    /// </summary>
    public void GiveHealth()
    {// Method for ant to give health to another ant
     // assume that health is above threshold
     // find ants occupying same space
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.3f);
        foreach (var hitCollider in hitColliders)
        {
            // ants can only give health in access of healthThreshold + (healthThreshold / 5)
            float amountToGive = (float)System.Math.Round(health - (healthThreshold + (healthThreshold / 5)), 2);
            if (amountToGive <= 0) return;

            Ant receivingAnt = hitCollider.GetComponent<Ant>();
            if (receivingAnt != null)
            {
                if (receivingAnt == this) continue;

                // if nest builder only give enegry to ants other than the queen if ant is hungry)
                if (!receivingAnt.isQueen && !receivingAnt.hungry && role == 2) continue;

                var amountNeeded = receivingAnt.maxHealth - receivingAnt.health;

                // don't gove health to ants who don't need it
                if (amountNeeded <= 0) continue;

                if (amountNeeded > amountToGive)
                {
                    //Debug.Log(this.name + this.role.ToString() + " giving " + hitCollider.name + this.role.ToString() + " " + amountToGive);
                    health -= amountToGive;
                    receivingAnt.ReceiveHealth(amountToGive);
                }
                else
                {
                    //Debug.Log(this.name + this.role.ToString() + " giving " + hitCollider.name + this.role.ToString() + " " + amountNeeded);
                    health -= amountNeeded;
                    receivingAnt.ReceiveHealth(amountNeeded);
                }
            }
        }
    }

    /// <summary>
    /// Specific movement logic for moving towards the queen.
    /// </summary>
    private void MoveToQueen(Dictionary<AirBlock, Vector3> movablePositions)
    {

        Collider[] hitColliders = Physics.OverlapSphere(movablePositions.Values.ElementAt(0), 0.3f);
        foreach (var hitCollider in hitColliders)
        {
            Ant receivingAnt = hitCollider.gameObject.GetComponent<Ant>();
            if (receivingAnt != null && receivingAnt.isQueen)
            {
                Debug.Log(this.name + " moving to " + hitCollider.name);
                Debug.Log("queenScent=" + movablePositions.Keys.ElementAt(0).queenScent);
                transform.position = receivingAnt.transform.position;
                return;
            }
        }

        transform.position = movablePositions.Values.ElementAt(RNG.Next(System.Math.Min(movablePositions.Count, 2)));
    }

    /// <summary>
    /// General movement logic for ants based on their role and state (hungry or not).
    /// </summary>
    private void MoveToTarget()
    {
        Dictionary<AirBlock, Vector3> movablePositions = FindMovableAdjacentBlocks();
        if (movablePositions.Count == 0)
        {
            //Dig();
        }

        if (movablePositions.Count > 0)
        {
            if (hungry)
            {
                // select from positions farthest from nest with highest pheromones
                movablePositions.OrderBy(i => i.Key.queenScent).Take(movablePositions.Count / 2).OrderBy(i => i.Key.pheromone).Take(2);
                transform.position = movablePositions.Values.ElementAt(RNG.Next(movablePositions.Count));
                return;
            }

            movablePositions.OrderByDescending(i => i.Key.queenScent);
            if (role == 1)
                MoveToQueen(movablePositions);
            else if (role == 2)
                MoveToAnt(movablePositions);
        }
    }

    /// <summary>
    /// Manages the ant's health, including reduction based on environment and hunger status.
    /// </summary>
    private void ManageHealth()
    {
        if (GetBlockBelow() is AcidicBlock)
            health -= (healthReduction * 2);
        else
            health -= healthReduction;

        if (health <= healthThreshold)
        {
            hungry = true;
            foundFood = false;
        }


        UpdateHealthBar();
    }

    /// <summary>
    /// Deposits pheromones in the current air block based on whether food was found.
    /// </summary>
    private void DepositePheramone()
    {
        if (foundFood && currentAirBlock != null)
            currentAirBlock.DepositPheromone(50);
        else
            currentAirBlock.DepositPheromone(10);
    }

    /// <summary>
    /// Logic for removing a block below the ant, potentially for nest expansion.
    /// </summary>
    private void Dig()
    {
        // don't want to dig if container block or another ant is on the block
        if (GetBlockBelow() is not ContainerBlock)
        {
            RemoveBlockBelow();
        }
    }

    /// <summary>
    /// Logic for building a nest block in an appropriate location.
    /// </summary>
    private void BuildNest()
    {
        if (health > (maxHealth / 3) * 2)
        {
            try
            {
                Dictionary<AirBlock, Vector3> buildLocations = FindBuildLocations();
                if (buildLocations.Count <= 0)
                {
                    WorldManager.Instance.SetBlock((int)transform.position.x, (int)transform.position.y + 1, (int)transform.position.z, new NestBlock());
                    transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
                }
                else
                {
                    WorldManager.Instance.SetBlock((int)buildLocations.Values.ElementAt(0).x, (int)buildLocations.Values.ElementAt(0).y, (int)buildLocations.Values.ElementAt(0).z, new NestBlock());
                }
                WorldManager.Instance.Current_Generation.Nest_Blocks += 1;
                health -= maxHealth / 3;
            }
            catch (Exception e)
            {
                Debug.LogWarning("BuildNest: " + e.ToString());
            }

        }
    }

    /// <summary>
    /// Retrieves the block directly below the ant.
    /// </summary>
    private AbstractBlock GetBlockBelow()
    {
        return WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }

    /// <summary>
    /// 
    /// </summary>
    private void RemoveBlockBelow()
    {
        WorldManager.Instance.SetBlock((int)transform.position.x, (int)transform.position.y, (int)transform.position.z, new AirBlock());
        transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
    }

    /// <summary>
    /// Removes the block directly below the ant and adjusts its position.
    /// </summary>
    /// <returns>true if anthoer ant occupying the space at 'pos', otherwise false</returns>
    private bool AntAtLocation(Vector3 pos)
    {
        try
        {
            Collider[] hitColliders = Physics.OverlapSphere(pos, 0.3f);
            foreach (var hitCollider in hitColliders)
            {
                Ant ant = hitCollider.gameObject.GetComponent<Ant>();
                if (ant != null && this != ant)
                    return true;
            }
        } catch(Exception e)
        {
            Debug.Log("Ant location: " + e.ToString());
        }
        
        return false;
    }

    /// <summary>
    /// Finds potential locations for building nest blocks.
    /// </summary>
    private Dictionary<AirBlock, Vector3> FindBuildLocations()
    {
        Dictionary<AirBlock, Vector3> buildLocations = new Dictionary<AirBlock, Vector3>();
        Vector3 position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // Define relative positions for adjacent blocks
        var adjacentPositions = new List<(string, Vector3)>
        {
            ("R", new Vector3(1, 0, 0)), // Right
            ("L", new Vector3(-1, 0, 0)), // Left
            ("F", new Vector3(0, 0, 1)), // Forward
            ("B", new Vector3(0, 0, -1)), // Backward
            ("RD", new Vector3(1, -1, 0)), // Right, Down
            ("LD", new Vector3(-1, -1, 0)), // Left, Down
            ("FD", new Vector3(0, -1, 1)), // Forward, Down
            ("BD", new Vector3(0, -1, -1)) // Backward, Down
        };

        foreach (var relativePos in adjacentPositions)
        {
            Vector3 checkPos = position + relativePos.Item2;
            AbstractBlock blockBelow = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y, (int)checkPos.z);
            AirBlock buildSite = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z) as AirBlock;

            // Ensure block at build site is an AirBlock and block below is not an AirBlock 
            if (buildSite != null && blockBelow is not AirBlock)
            {
                // don't build on top of another ant
                //if (AntAtLocation(new Vector3((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z))) continue;
                
                buildLocations.Add(buildSite, new Vector3((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z));
            }
        }

        return buildLocations;
    }

    /// <summary>
    ///  Identifies adjacent blocks that the ant can move to.
    /// </summary>
    private Dictionary<AirBlock, Vector3> FindMovableAdjacentBlocks()
    {
        Dictionary<AirBlock, Vector3> movableBlocks = new Dictionary<AirBlock, Vector3>();
        Vector3 position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // Define relative positions for adjacent blocks
        var adjacentPositions = new List<(string, Vector3)>
        {
            ("R", new Vector3(1, 0, 0)), // Right
            ("L", new Vector3(-1, 0, 0)), // Left
            ("F", new Vector3(0, 0, 1)), // Forward
            ("B", new Vector3(0, 0, -1)), // Backward
            ("RU", new Vector3(1, 1, 0)), // Right, Up
            ("LU", new Vector3(-1, 1, 0)), // Left, Up
            ("FU", new Vector3(0, 1, 1)), // Forward. Up
            ("BU", new Vector3(0, 1, -1)), // Backward, Up
            ("RD", new Vector3(1, -1, 0)), // Right, Down
            ("LD", new Vector3(-1, -1, 0)), // Left, Down
            ("FD", new Vector3(0, -1, 1)), // Forward, Down
            ("BD", new Vector3(0, -1, -1)) // Backward, Down
        };

        foreach (var relativePos in adjacentPositions)
        {
            Vector3 checkPos = position + relativePos.Item2;
            AbstractBlock block = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y, (int)checkPos.z);

            // Ensure the block to move to is not an AirBlock or ContainerBlock
            if (block is not AirBlock && block is not ContainerBlock)
            {
                // Ensure the block directly above is an AirBlock 
                AirBlock blockAbove = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z) as AirBlock;
                
                if (blockAbove != null) 
                    movableBlocks.Add(blockAbove, checkPos);
            }
        }

        return movableBlocks;
    }

    /// <summary>
    /// Controls the visibility of the ant's health bar based on game settings.
    /// </summary>
    private void SetHealthBarVisibility()
    {
        if (!ConfigurationManager.Instance.Show_Health)
        {
            GameObject healthBar = transform.Find("HealthBar").gameObject;

            if (healthBar != null)
                healthBar.SetActive(false);
            else
                Debug.LogError("HealthBar object not found");
        }
    }

    /// <summary>
    /// Updates the visual representation of the ant's health bar.
    /// </summary>
    private void UpdateHealthBar()
    {
        if (ConfigurationManager.Instance.Show_Health)
        {
            int bars = (int)System.Math.Round(health / (maxHealth / 7));

            foreach (int bar in Enumerable.Range(1, 7))
            {
                GameObject healthBar = transform.Find("HealthBar").gameObject.transform.Find("Bar" + bar.ToString()).gameObject;
                if (healthBar != null)
                {
                    MeshRenderer meshRenderer = healthBar.GetComponent<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        if (health <= healthThreshold && meshRenderer.material != weakHealthMaterial)
                            meshRenderer.material = weakHealthMaterial;

                        else if (health > healthThreshold && meshRenderer.material != strongHealthMaterial)
                            meshRenderer.material = strongHealthMaterial;
                    }

                    if (bar <= bars)
                        healthBar.SetActive(true);
                    else
                        healthBar.SetActive(false);

                }
                else
                {
                    Debug.Log("nO BAR object");
                }
            }
        }
    }
    #endregion

}