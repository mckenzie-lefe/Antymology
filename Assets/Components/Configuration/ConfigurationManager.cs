using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationManager : Singleton<ConfigurationManager>
{

    /// <summary>
    /// The seed for world generation.
    /// </summary>
    public int Seed = 1337;

    /// <summary>
    /// The number of chunks in the x and z dimension of the world.
    /// </summary>
    public int World_Diameter = 16;

    /// <summary>
    /// The number of chunks in the y dimension of the world.
    /// </summary>
    public int World_Height = 4;

    /// <summary>
    /// The number of blocks in any dimension of a chunk.
    /// </summary>
    public int Chunk_Diameter = 8;

    /// <summary>
    /// How much of the tile map does each tile take up.
    /// </summary>
    public float Tile_Map_Unit_Ratio = 0.25f;

    /// <summary>
    /// The number of acidic regions on the map.
    /// </summary>
    public int Number_Of_Acidic_Regions = 10;

    /// <summary>
    /// The radius of each acidic region
    /// </summary>
    public int Acidic_Region_Radius = 5;

    /// <summary>
    /// The number of acidic regions on the map.
    /// </summary>
    public int Number_Of_Conatiner_Spheres = 5;

    /// <summary>
    /// The radius of each acidic region
    /// </summary>
    public int Conatiner_Sphere_Radius = 20;

    /// <summary>
    /// The number of randomly created inital Generations to create before using crossover
    /// to create new generation.
    /// </summary>
    public int Number_Of_Starting_Generations = 5;

    /// <summary>
    /// Lowest possible starting ant population. 
    /// Used as lower bound for ant population random number generator
    /// </summary>
    public int Min_Starting_Ants = 2;

    /// <summary>
    /// Highest possible starting ant population. 
    /// Used as upper bound for ant population random number generator
    /// </summary>
    public int Max_Starting_Ants = 600;

    /// <summary>
    /// Highest possible ant health. 
    /// Used as upper bound for ant health random number generator
    /// </summary>
    public int Max_Ant_Health = 300;

    /// <summary>
    /// The maximum percentage health that the hungry threshold can be set to for ants
    /// Ants start looking for food when their health is less than the hungry threshold
    /// </summary>
    public int Max_Hungry_Threshold_Percent = 70;

    /// <summary>
    /// Show ants health bars
    /// </summary>
    public bool Show_Health = true;


    /// <summary>
    /// Load ant generation data from pervious program runs
    /// </summary>
    public bool Use_Generation_Data = true;
}
