using Antymology.Terrain;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant : MonoBehaviour
{
    /// <summary>
    /// Ant health measurement
    /// </summary>
    public float health;

    /// <summary>
    /// 
    /// </summary>
    private string target;

    private bool hungry;

    /// <summary>
    /// 1 == worker ant == role is to find food and give to nest building ants
    /// 2 == nest builder == role is to transfer engery to queen to build nest
    /// </summary>
    public int role;

    /// <summary>
    /// Boolean indicating Queen ant status
    /// </summary>
    public bool isQueen = false;

    /// <summary>
    /// 
    /// </summary>
    private float maxHealth;

    private float healthReduction;

    private float healthThreshold;

    private Material queenMaterial;

    private Material weakHealthMaterial;

    private Material strongHealthMaterial;

    private System.Random RNG;


    // Start is called before the first frame update
    void Start()
    {
        RNG = new System.Random();
        queenMaterial = (Material)Resources.Load("Materials/QueenAntMaterial", typeof(Material));
        weakHealthMaterial = (Material)Resources.Load("Materials/WeakHealthMaterial", typeof(Material));
        strongHealthMaterial = (Material)Resources.Load("Materials/StrongHealthMaterial", typeof(Material));
        if (WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y + 1, (int)transform.position.z) is not AirBlock)
        {
            Debug.Log("curr is airblock");
        }

        maxHealth = WorldManager.Instance.Current_Generation.Max_Ant_Health;
        health = WorldManager.Instance.Current_Generation.Starting_Ant_Health;
        healthReduction = WorldManager.Instance.Current_Generation.Step_Health_Reduction;
        healthThreshold = (WorldManager.Instance.Current_Generation.Hungry_Threshold * WorldManager.Instance.Current_Generation.Max_Ant_Health) / 1;

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
        if (health <= 0)
        {
            Destroy(gameObject);
            return;
        }

        // Check for digging ability
        //DigIfNeeded();
    }

    public void UpdateAnt()
    {
        if (isQueen)
        {
            MoveQueen();
        } 
        else
        {
            if (!hungry)  
                GiveHealth();

            MoveToTarget();
            ConsumeResourcesIfNeeded();
            DepositePheramone();    
        }
        
        ManageHealth();
    }

    void DepositePheramone()
    {
        // only deposit pheromone if returning from food
        if (target == "queen")
        {
            // Get AirBlock directly above current block
            AbstractBlock curr = WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y + 1, (int)transform.position.z);
            if (curr is not AirBlock)
            {
                Debug.LogError("Not AirBlock ????");
            }
            ((AirBlock)curr).DepositPheromone(10);
        }
    }

    void ConsumeResourcesIfNeeded()
    {
        if(GetBlockBelow() is MulchBlock && health <= healthThreshold)
        {
            RemoveBlockBelow();
            health = maxHealth;
            target = "queen";
        }
    }

    void Dig()
    {
        if(GetBlockBelow() is not ContainerBlock)
        {
            RemoveBlockBelow();
        }
        // Implement logic to dig up the world here
        // Check for the block type beneath and remove it if it's diggable
    }

    void BuildNest(int x, int y, int z)
    {
        if (health > (maxHealth / 3) * 2)
        {
            WorldManager.Instance.SetBlock(x, y, z, new NestBlock());
            WorldManager.Instance.Current_Generation.Nest_Blocks += 1;
            health -= maxHealth / 3;
        }
    }

    void MoveQueen()
    {
        // Method that moves queen ant to best location to build next nest block
        // When the queen is ready to build a nest block it will in the AirBlock directly infront of her
        // Airblocks which are touching other nest blocks are better build locations then airblocks that are not
        // All ants including the queen are not allowed to move to a block that is greater than 2 units in height difference
        // Do not want to build nest block such that other ants can not reach the queen ant or the queen is stuck and can not move.
        if (GetBlockBelow() is not ContainerBlock or NestBlock)
        {
            BuildNest((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z);
            return;
        }

        Dictionary<AirBlock, Vector3> movablePositions = FindMovableAdjacentBlocks();
        if (WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z + 1) is AirBlock && movablePositions.Count > 1)
        {
            BuildNest((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z + 1);
            return;

        } 
        else if (WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z - 1) is AirBlock && movablePositions.Count > 1)
        {
            BuildNest((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z - 1);
            return;
        }

        movablePositions.OrderBy(i => i.Key.pheromone);
        transform.position = movablePositions.Values.ElementAt(RNG.Next(movablePositions.Count));
    }

    #region Methods

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
    /// assume not queen
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
                Debug.Log("queenScent="+movablePositions.Keys.ElementAt(0).queenScent);
                transform.position = receivingAnt.transform.position;
                return;
            }
        }

        transform.position = movablePositions.Values.ElementAt(RNG.Next(System.Math.Min(movablePositions.Count, 2)));
    }

    private void MoveToTarget()
    {
        Dictionary<AirBlock, Vector3> movablePositions = FindMovableAdjacentBlocks();
        if (movablePositions.Count == 0)
        {
            Dig();
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
    
    private void ManageHealth()
    {
        if (GetBlockBelow() is AcidicBlock) 
            health -= (healthReduction * 2);
        else 
            health -= healthReduction;

        if (health <= healthThreshold && !isQueen) 
            hungry = true;

        UpdateHealthBar();
    }

    // Method for ant to give health to another ant
    // assume that health is above threshold
    public void GiveHealth()
    {
        // find ants occupying same space
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.3f);
        foreach (var hitCollider in hitColliders)
        {
            // ants can only give health in access of healthThreshold + (healthThreshold / 5)
            var amountToGive = health - (healthThreshold + (healthThreshold / 5));
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
                    Debug.Log(this.name + this.role.ToString() + " giving " + hitCollider.name + this.role.ToString() + " " + amountToGive);
                    health -= amountToGive;
                    receivingAnt.ReceiveHealth(amountToGive);
                } 
                else
                {
                    Debug.Log(this.name + this.role.ToString() + " giving " + hitCollider.name + this.role.ToString() + " " + amountNeeded);
                    health -= amountNeeded;
                    receivingAnt.ReceiveHealth(amountNeeded);
                }
            }
        }  
    }

    // Method for ant to receive health from another ant
    public void ReceiveHealth(float amount)
    {
        health += amount;

        //Debug.Log(this.name + " GOT HEALTH");

        if (!isQueen && hungry && health >= healthThreshold) 
            hungry = false;
    }

    #endregion

    #region Helpers

    private AbstractBlock GetBlockBelow()
    {
        return WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
    }

    private void RemoveBlockBelow()
    {
        WorldManager.Instance.SetBlock((int)transform.position.x, (int)transform.position.y, (int)transform.position.z, new AirBlock());
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
    }

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
    /// Toggle the visibility of the ants health bar
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