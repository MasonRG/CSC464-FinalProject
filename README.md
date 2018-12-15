# CSC464-FinalProject
Distributing procedural 3D mesh generation over multiple game clients.

## Resources

#### Video Demonstration
A demonstration of the project is provided in the video linked [here](https://youtu.be/z9LmcIJQMaM), which discusses the implementation and results of the project. This is likely the most convenient way to describe the project in its current state, but a brief written report is also provided below.

#### Game Executable
For completeness, a client application is provided in the DistMeshGenApp folder in the repository, in case that is of any interest. 

*Note: all files in the DistMeshGenApp directory are required to launch the application, and connections will only be possible over LAN unless some NAT port-forwarding is done.*

Mesh generation is started by issuing the chat command `/begin x` from the leader's client.
*x* is some time in milliseconds to wait between each chunk request.

#### Tools Used
- The game engine used is the [Unity Engine](https://unity3d.com/) (version 2018.2.14).
- The networking framework used is an open-source, C#/.NET solution called [Forge Networking Remastered](https://github.com/BeardedManStudios/ForgeNetworkingRemastered).
- Mesh generation is adapted from the work of [Sebastian Lague](https://www.youtube.com/user/Cercopithecan).

## Report

#### Introduction
This project aims to investigate a distributed solution for complex computational tasks as might be needed in a game environment. Game clients are used to perform computations and deliver results to other clients where the results can be converted into visible 3d meshes. Procedural 3d mesh generation was chosen as the computation task for the following reasons:

1. It is easily visualized via rendering results to the display
2. It is relatively computationally intensive and scalable
3. It is parallelizable

Each client requires a copy of the mesh on their own machine so that it can be rendered, and we require that the mesh be consistent across all clients as well as complete. 

Consistency is achieved by using a generation algorithm that allows us to easily parallelize the task into smaller subtasks ('chunking' the mesh into smaller, independent chunks), and by ensuring that all clients use the same seed value and initial parameters for the generation algorithm in their computations.

Completeness is achieved by use of timeouts to detect when a client becomes non-responsive and fails to deliver to other clients the data for some set of chunks that were assigned to it. In the event of a timeout, the leader can mark the chunk for re-generation, and then issue new requests from the remaining active clients to finish computing the chunks that the dropped client failed to deliver.

#### Task Description
Setting up the network framework to allow clients to connect to and communicate with each other was a necessary step, and that is on its own is a non-trivial endeavour. However, I was able to use code I have previously written for another project of mine using the same networking framework to handle most of the basic connectivity tasks, as well as the client chat and command system. This saved a lot of development time, but the solutions still needed to be adapted to make sense within the context of this project.

The implementation of the mesh generation algorithm was also a considerable time investment, and required some effort to adapt into a practical form for use in this demonstration. While the majority of the code and algorithm was obtained from the work of Sebastian Lague, some modifications were necessary to simplify the algorithm, to compress and decompress the mesh data, and handle chunk distribution amongst clients.

The solution implemented consists of the following procedure:

1. Clients join server
2. Leader (server) initiates generation requests
3. Requests are distributed amongst active clients
4. Clients compute data for the mesh segments requested from them, compress the results, and transmit them to other clients
5. Clients receive data from others, decompress it, and create the physical mesh
6. On dropped clients (detected by timeout), the leader initiates another round of requests (from the active clients) for the mesh segments that were not received
7. Repeat from 4 until mesh is fully generated.

#### Evaluation
Due to the nature of the project, evaluation is likely best done visually, and so the video linked above should provide a sufficient evaluation of the project, as well as some conclusions. A brief summary is provided below as well.

Evaluation of the implementation relies on visual inspection of the resulting mesh on the various participating clients. This is a convenient means to evaluate correctness and efficiency. Since all mesh segments should connect seamlessly (thus resulting in a single, larger mesh) it is easy to detect anomalies. Detecting non-responsive clients is self-evident as their segment data will not be received and can thus trigger a timeout, resulting in other clients assuming responsibility of computing the data instead.

Efficiency is very difficult to measure in this context, as there is a heavy dependency on network latency, and it is not likely that any particularly meaningful results could be gathered. However, measuring the performance of single host system compared to multiple hosts results in some notable observations.

Mainly, the primary bottleneck in terms of speed is network latency. This comes from the time it takes to transmit the data over the network to other clients (which grows at n2 as we increase the number of clients n, as each client must send its results to all other clients). We also suffer performance loss from the need to compress and decompress the data before and after transmission - this of course becomes worse as we require more data to be transmitted as the mesh complexity and size increases.

Considering the limitations of the network latency, this particular task does not seem practical for distribution over multiple clients. However, this is largely due to its complexity in relation to its data output. In the case of 3d mesh generation, we can produce large amounts of data from relatively simple computations, and this then puts most of the load on the network.

If a task that produced much less data were to be considered, it might be much more feasible to use a distributed approach, as we could easily share the data between clients. If the computation required to generate the data was sufficiently complex, we could justify the network overhead as the task might be too resource-demanding for a single host to compute independently.

#### Source Code
Code is located in the Assets/Scripts directory. 

Much of the code may not be of interest, and so I will try to briefly identify the more meaningful parts. Also, I apologize for the lack of commenting in the code. Normally, I would have provided relevant comments in order to provide more clarity but due to time limitations I neglected to do so.

Files in the MeshGeneration folder are associated with creating the meshes and may not be directly relevant. They are also largely the work of Sebastian Lague (linked above), adapted for use in this project.

Files in the GameConsole folder are associated with the client chat window, and also may not be directly relevant. This code is from another project of mine, and is reused here. Primarily it allows communication between clients, commands to be issued via the chat interface, and logging of events.

A very brief description of some of the more relevant files is provided here for convenience.

###### GameClient/ClientManager.cs
- handles joining/leaving clients
- facilitates the mesh generation (via the LeaderOperator)
- primarily utilized by the server

###### GameClient/LeaderOperator.cs
- distributes chunks between active clients for generation
- handles timeouts and re-generation requests

###### GameClient/Client.cs
- handles  RPCs for detecting new clients (so that we can properly sort active clients)
- handles RPCs for chunk generation requests
- handles RPCs for chunk data transmission

###### GameServer/MeshManager.cs
- keeps track of chunks that have been generated or are still required
- determines which chunks clients should compute (evenly distributes)
- facilitates the creation of a new mesh from received mesh data
- facilitates conversion of chunk data into byte data for network transmission

###### Engine/Utilities/NetworkHub.cs
- provides easy access to network objects
- provides functions to find active clients

###### MeshGeneration/MeshDataRequester.cs
- handles generating and delivering mesh data on separate threads
