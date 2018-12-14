# CSC464-FinalProject
Game-client distributed system simulation via procedural 3d mesh generation with Raft.

#### Current State
The implementation currently just handles allowing clients to join the server, know about each other, and send/receive messages (via a chat box). Much of the code thusfar is actually from an ongoing project of mine that utilizes this same networking framework, and has been simplified and retooled to be appropriate for this project.

#### Next Steps
With this in place, the next components to be implemented are: 
1. The algorithm that clients will use to generate data to be assembled (likely 3d-mesh generation, time permitting)
2. Dynamic client status (dead, delayed, healthy).
3. Raft for leadership elections and log replication.
4. Probably some improved visuals to better demonstrate the process.

Getting Raft implemented is the main priority, and so I may forgo the mesh generation for a much simpler task just for demonstration to ensure that time is spent on the topic of most relevance to the course.

#### Tools
- The game engine used is the [Unity Engine](https://unity3d.com/) (version 2018.2.14).
- The networking framework used is an open-source, C#/.NET solution called [Forge Networking Remastered](https://github.com/BeardedManStudios/ForgeNetworkingRemastered).

### Update: Dec. 13
3D mesh generation complete. Generation is run on multiple threads (locally) and parallelized to be distibuted amongst multiple hosts.

Currently does not handle non-responsive hosts or leaders.

Video demonstrating current implementation [here](https://youtu.be/y7oAPoLOB94)
