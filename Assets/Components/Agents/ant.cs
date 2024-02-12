using Antymology.Terrain;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ant : MonoBehaviour
{
    /// <summary>
    /// Ant health measurement
    /// </summary>
    public float health = 100;

    /// <summary>
    /// 
    /// </summary>
    public float moveSpeed = 5;

    /// <summary>
    /// 
    /// </summary>
    private Vector3 targetPosition;

    /// <summary>
    /// Boolean indicating Queen ant status
    /// </summary>
    public bool isQueen = false;

    /// <summary>
    /// Seconds between ant steps 
    /// </summary>
    private float stepInterval = 1.0f;

    /// <summary>
    /// Time of next ant step
    /// </summary>
    private float nextStepTime = 0.0f;

    /// <summary>
    /// 
    /// </summary>
    private float maxHealth;

    private Material queenMaterial;

    // Start is called before the first frame update
    void Start()
    {
        queenMaterial = (Material)Resources.Load("Materials/QueenAntMaterial", typeof(Material));

        if (!ConfigurationManager.Instance.Show_Health)
        {
            ToggleHealthBarVisibility();
        }

        testStart();

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

        if (isQueen)
        {
            testMove();

        }

        // Movement behavior
        if (Time.time >= nextStepTime)
        {
            MoveToTarget(); 
            nextStepTime = Time.time + stepInterval; 
        }

        // Check for resource consumption
        ConsumeResourcesIfNeeded();

        // Health management
        ManageHealth();

        // Check for digging ability
        DigIfNeeded();

        // Check for queen's nest building
        if (isQueen)
        {
            BuildNest();
        }


    }

    // Call this method to toggle the visibility of the health bar
    public void ToggleHealthBarVisibility()
    {
        // Assuming 'HealthBar' is the name of the child GameObject with the health bar
        GameObject healthBar = transform.Find("HealthBar").gameObject;

        if (healthBar != null)
        {
            // Toggle the active state of the health bar
            healthBar.SetActive(!healthBar.activeSelf);
        }
        else
        {
            Debug.LogError("HealthBar object not found");
        }
    }

    public void InitializeQueen()
    {
        MeshRenderer meshRenderer = transform.Find("ant_blk").GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Set the material of the MeshRenderer to the new material
            meshRenderer.material = queenMaterial;
        }
        else
        {
            Debug.LogError("MeshRenderer component not found on 'ant_blk'");
        }
    }

    void testStart()
    {
        if (isQueen)
        {
            

            transform.position = new Vector3(50, 21.4f, 59.1f);
            Dictionary<string, Vector3> movablePositions = FindMovableAdjacentBlocks();
            foreach (var p in movablePositions)
            {
                Debug.Log(p.Key + ": " + p.Value.x + ", " + p.Value.x + ", " + p.Value.z);

            }
        }
    }

    public void testMove()
    {
        Dictionary<string, Vector3> movablePositions = FindMovableAdjacentBlocks();
        if (Input.GetKeyDown(KeyCode.F))
        {
            transform.position = movablePositions["F"];
            Debug.Log("Moved to Forward");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            transform.position = movablePositions["B"];
            Debug.Log("Moved to BAckward");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = movablePositions["RD"];
            Debug.Log("Moved to Right down");
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            transform.position = movablePositions["LU"];
            Debug.Log("Moved to Left up");
        }
    }

    void MoveToTarget()
    {
        Dictionary<string, Vector3> movablePositions = FindMovableAdjacentBlocks();
        if (movablePositions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, movablePositions.Count);
           
            transform.position = movablePositions.Values.ElementAt(randomIndex);
        }
    }
    

    void ConsumeResourcesIfNeeded()
    {
        // if near resource
        // consume resource
        // replenish health or carry resource
    }

    void ManageHealth()
    {
        // decrease health over time
        // if health below threshold, seek food
    }

    void DigIfNeeded()
    {
        // Implement logic to dig up the world here
        // Check for the block type beneath and remove it if it's diggable
    }

    void BuildNest()
    {
        // Implement logic for the queen ant to build a nest here
        // Reduce health by a third and create a nest block
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

    AbstractBlock GetBlockBelow()
    {
        return WorldManager.Instance.GetBlock((int)transform.position.x, (int)transform.position.y - 1, (int)transform.position.z);
    }


    Dictionary<string, Vector3> FindMovableAdjacentBlocks()
    {
        Dictionary<string, Vector3> movableBlocks = new Dictionary<string, Vector3>();
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
                AbstractBlock blockAbove = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z);
                if (WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y + 1, (int)checkPos.z) is AirBlock)
                {
                    movableBlocks.Add(relativePos.Item1, checkPos);
                }
            }
        }

        return movableBlocks;
    }
}