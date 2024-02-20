# Assignment 3: Antymology

This project aims to simulate the behaviors of ants through a Unity-based environment, drawing inspiration from their real-world capabilities like pathfinding, bridge formation, and nest construction. Leveraging simple rules, the simulation explores how complex emergent behaviors can evolve. The core objective is to develop a species of ant capable of maximizing nest production through an evolutionary algorithm. This involves creating an ecosystem where ants interact with their environment and each other to build the largest possible nest, showcasing the power of evolutionary principles in generating intelligent, adaptive behaviors.

![Ants](Images/AntWorld.gif)

## Features
- Ants dynamically look for food, dig through terrain, and build nests based on environmental cues.
- Distinct roles within the ant population, including protectors, workers and a queen, each with specific tasks.
- Health system where ants consume resources to survive and can die if health reaches zero.
- Ants avoid obstacles and navigate terrain with height differences.
- Implementation of pheromone trails for indirect communication between ants, influencing ant movement and nest-building.
- Specialized behaviors for the queen ant, including nest block production critical to the simulation's objective.

An evolutionary algorithm evolves the following ant behaviors over generations, optimizing nest-building efficiency:
- Ant Population 
- Starting Ant Health
- Maximum Ant Health
- Maximum Queen Health
- Pheromone Evaperation Rate 
- Hungry Threshold
- Number of Protector Ants 
- Number of Worker Ants 

## Ant Behavior Details
- Ant health decreases over time, requiring ants to consume mulch blocks for energy.
- Ants cannot consume mulch blocks already occupied by another ant.
- Movement is restricted by terrain, with ants unable to traverse heights greater than 2 units.
- Digging behavior allows remove blocks from world, with the exception of the container blocks 
- Ants standing on acidic blocks health decreases by double the normal rate.
- Health can be transferred between ants occupying the same space.
- Queen ant role is to build nest blocks.
- Protector ants role is to transfer engery to queen to build nest and ensure wueen survival.
- Worker ants role is to find food and give enegry to protector ants.
- Worker ants move according to strongest ant pheromones.
- Protector ants move according the queen scent.

### Environment
- Terrain generation creates a diverse landscape with various block types, including mulch, acidic, grass, air, stone and container blocks.
- The environment supports dynamic interactions, such as digging and mulch block consumption by ants.
- Queen ant can create new nest block in the environment.
- Queen scent and Pheromone mechanics influence ant movement and decision-making, simulating natural ant behavior.
- Evolutionary pressures within the simulation drive adaptation and optimization of ant behaviors across generations.

### Implementation
- Developed in Unity 2022.3.18f1
- The codebase is modular, with clear separations between agents, configuration, terrain generation, and UI components.
- Use of serializable Generations classes for easy configuration of ants and adaptation of simulation parameters.
- Implementation of custom algorithms for ant behavior, including foraging logic, health management, and evolutionary strategies.
- Extensible design allowing for future enhancements, such as additional ant roles or environmental challenges.

### Setup and Running Instructions
- Clone the Repository: Fork and clone the provided GitHub repository to your local machine.
- Open in Unity: Launch Unity Hub, click 'Open', and navigate to your cloned project directory. Select the project to open it in Unity Editor, ensuring you're using Unity version 2022.3.18f1.
- Load the Scene: In the Unity Editor, navigate to the 'Scenes' folder within the Project tab, and double-click the main scene to load it.
- Adjust Settings: Review and adjust the configuration settings as necessary within the ConfigurationManager script to tailor the simulation environment.
- Play Mode: Enter Play Mode in Unity to start the simulation. The environment will generate, and ants will begin their activities based on the implemented behaviors.

### UI and Interaction
- Nest Block Counter: A UI element prominently displayed on the screen, showing the current number of nest blocks within the simulation. This dynamically updates as the queen ant produces more nest blocks.
- Health Indicators: Visual indicators for each ant, reflecting their current health status, changing in real-time as ants consume resources or are affected by environmental factors.
- Simulation Controls: UI controls to pause, resume, or restart the simulation, allowing users to observe changes in ant behavior and nest construction over time.

Users can navigate the simulation using the camera controls:
- wasd : basic movement
- shift : Makes camera accelerate
- space : Moves camera on X and Z axis only.  

## Challenges and Learnings

### Future Work
- Ant Behavior Complexity: Introduce more complex behaviors such as ant specialization based on environmental challenges or nest needs.
- Environmental Variability: Implement changing environmental conditions that affect resource availability, introducing dynamic challenges for ant survival and nest building.
- Genetic Algorithms: Enhance the evolutionary algorithm to include genetic traits passed down to offspring, influencing ant behaviors and adaptability.

### Contribution and Credits

The base code for the assignment has been provided by Cooper Davies.
[GitHub - DaviesCooper/Antymology](https://free3d.com/3d-model/ant-71866.html)