using Antymology.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
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

    private float moveInterval = 2.0f; // Seconds between moves
    private float nextMoveTime = 0.0f;
    /// <summary>
    /// 
    /// </summary>
    private float maxHealth;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize ant
        InitializeAnt();
    }

    // Update is called once per frame
    void Update()
    {
 
        if (health <= 0)
        {
            //Destroy(gameObject);
            Debug.Log("health bad");
            return;
        }

        // Movement behavior
        if (Time.time >= nextMoveTime)
        {
            MoveToTarget(); // Call your movement method here
            nextMoveTime = Time.time + moveInterval; // Set the next move time
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

    void InitializeAnt()
    {
        Debug.Log("init");
    }



    void MoveToTarget()
    {
        List<Vector3> movablePositions = FindMovableAdjacentBlocks();
        if (movablePositions.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, movablePositions.Count);
           
            transform.position = movablePositions[randomIndex];
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


    List<Vector3> FindMovableAdjacentBlocks()
    {
        List<Vector3> movableBlocks = new List<Vector3>();
        Vector3 currentPosition = transform.position;

        // Define relative positions for adjacent blocks
        Vector3[] adjacentPositions = new Vector3[]
        {
            new Vector3(1, 0, 0), // Right
            new Vector3(-1, 0, 0), // Left
            new Vector3(0, 0, 1), // Forward
            new Vector3(0, 0, -1), // Backward
            new Vector3(1, 1, 0), // Right, Up
            new Vector3(-1, 1, 0), // Left, Up
            new Vector3(0, 1, 1), // Forward. Up
            new Vector3(0, 1, -1), // Backward, Up
            new Vector3(1, -1, 0), // Right, Down
            new Vector3(-1, -1, 0), // Left, Down
            new Vector3(0, -1, 1), // Forward, Down
            new Vector3(0, -1, -1), // Backward, Down
        };

        foreach (var relativePos in adjacentPositions)
        {
            Vector3 checkPos = currentPosition + relativePos;

            // Ensure the block to move to is an AirBlock
            if (WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y, (int)checkPos.z) is AirBlock)
            {
                // Ensure the block directly below is not an AirBlock or ContainerBlock
                AbstractBlock blockBelow = WorldManager.Instance.GetBlock((int)checkPos.x, (int)checkPos.y - 1, (int)checkPos.z);
                if (!(blockBelow is AirBlock) && !(blockBelow is ContainerBlock))
                {
                    movableBlocks.Add(checkPos);
                }
            }
        }

        return movableBlocks;
    }
}