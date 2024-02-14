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
    private string target = "Queen";

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


    // Start is called before the first frame update
    void Start()
    {
        queenMaterial = (Material)Resources.Load("Materials/QueenAntMaterial", typeof(Material));
        weakHealthMaterial = (Material)Resources.Load("Materials/WeakHealthMaterial", typeof(Material));
        strongHealthMaterial = (Material)Resources.Load("Materials/StrongHealthMaterial", typeof(Material));

        if (isQueen)
        {
            InitializeQueen();
        }

        if (!ConfigurationManager.Instance.Show_Health)
        {
            ToggleHealthBarVisibility();
        }

        maxHealth = ConfigurationManager.Instance.Max_Ant_Health;
        health = ConfigurationManager.Instance.Starting_Ant_Health;
        healthReduction = ConfigurationManager.Instance.Step_Health_Reduction;
        healthThreshold = maxHealth / 3;

    }

    // Update is called once per frame
    void Update()
    {
 
        if (health <= 0)
        {
            Debug.Log("health bad");
            Destroy(gameObject);
            return;
        }

        // Check for digging ability
        //DigIfNeeded();

        // Check for queen's nest building
        if (isQueen)
        {
            BuildNest();
        }

    }

    public void UpdateAnt()
    {
        MoveToTarget();

        // Check for resource consumption
        ConsumeResourcesIfNeeded();
        ManageHealth();
        DepositePheramone();
    }

    void DepositePheramone()
    {
        // Get AirBlock directly above current block
        AbstractBlock curr = WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y + 1, (int)transform.position.z);
        if (curr is not AirBlock)
        {
            Debug.LogError("Not AirBlock ????");
        }
        ((AirBlock)curr).DepositPheromone(10);
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
            int randomIndex = Random.Range(0, movablePositions.Count);
            var random = new System.Random();

            movablePositions.OrderBy(i => i.Key.queenScent);

            if (target == "Food")
            {
                // select from positions farthest from nest with highest pheromones
                movablePositions.OrderBy(i => i.Key.queenScent).Take(movablePositions.Count/2).OrderBy(i => i.Key.pheromone).Take(2);
            }
            else if (target == "Queen")
            {
                // select from 3 closes positions to queen
                movablePositions.OrderByDescending(i => i.Key.queenScent).Take(3);
            }
           
            transform.position = movablePositions.Values.ElementAt(random.Next(movablePositions.Count));
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

    void BuildNest()
    {
        if (health > (maxHealth / 3) * 2)
        {
            WorldManager.Instance.SetBlock((int)transform.position.x, (int)transform.position.y, (int)transform.position.z, new NestBlock());
            transform.position = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);

            health -= maxHealth / 3;
        }
        
        // Implement logic for the queen ant to build a nest here
        // Reduce health by a third and create a nest block
    }

    #region Methods

    public void InitializeQueen()
    {
        MeshRenderer meshRenderer = transform.Find("ant_blk").GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.material = queenMaterial;
            Debug.Log("QUEEN");
        }
        else
        {
            Debug.LogError("MeshRenderer component not found on 'ant_blk'");
        }
    }

    // Method for ant to give health to another ant
    public void GiveHealth(Ant recipient, float amount)
    {
        if (this.health > amount)
        {
            this.health -= amount;
            recipient.ReceiveHealth(amount);
        }
    }

    // Method for ant to receive health from another ant
    public void ReceiveHealth(float amount)
    {
        this.health += amount;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Toggle the visibility of the ants health bar
    /// </summary>
    private void ToggleHealthBarVisibility()
    {
        GameObject healthBar = transform.Find("HealthBar").gameObject;

        if (healthBar != null)
        {
            healthBar.SetActive(!healthBar.activeSelf);
        }
        else
        {
            Debug.LogError("HealthBar object not found");
        }
    }

    private AbstractBlock GetBlockBelow()
    {
        return WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z);
    }

    private void RemoveBlockBelow()
    {
        WorldManager.Instance.SetBlock((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z, new AirBlock());
        transform.position = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
    }

    private Dictionary<AirBlock, Vector3> FindMovableAdjacentBlocks()
    {
        Dictionary<AirBlock, Vector3> movableBlocks = new Dictionary<AirBlock, Vector3>();
        Vector3 currentPosition = transform.position;

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
            Vector3 checkPos = currentPosition + relativePos.Item2;
            AbstractBlock block = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y, (int)checkPos.z);
            // Ensure the block to move to is not an AirBlock or ContainerBlock
            if (!(block is AirBlock) && !(block is ContainerBlock))
            {
                // Ensure the block directly above is an AirBlock 
                AirBlock blockAbove = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z) as AirBlock;
                if (blockAbove != null)
                {
                    movableBlocks.Add(blockAbove, checkPos);
                }
            }
        }

        return movableBlocks;
    }

    private void ManageHealth()
    {
        if (GetBlockBelow() is AcidicBlock)
        {
            health -= (healthReduction * 2);
        }
        else
        {
            health -= healthReduction;
        }

        if (health < healthThreshold)
        {
            target = "Food";
        }
        UpdateHealthBar();
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