using System;
using System.Collections.Generic;

public static class RunMapGenerator
{
    public const int CurrentMapVersion = 2;
    private const int LayerCount = 5;
    private const int MinimumNodesPerLayer = 2;
    private const int MaximumNodesPerLayer = 3;
    private const int TutorialNodeId = 0;
    private const int FirstGeneratedNodeId = 1000;

    public static RunMapNode[] Generate(int seed)
    {
        Random random = new(seed);
        LevelDefinition[] levels = LevelCatalog.GetWorldLevels(LevelCatalog.ForestWorldId);
        List<RunMapNode> nodes = new()
        {
            new RunMapNode
            {
                Id = TutorialNodeId,
                Layer = 0,
                Position = 0,
                LevelId = "Tutorial",
                DisplayName = "Tutorial",
                DifficultyMultiplier = 1.0f,
                IsTutorial = true
            }
        };

        List<RunMapNode> previousLayer = new() { nodes[0] };
        for (int layer = 1; layer <= LayerCount; layer++)
        {
            int nodeCount = layer == LayerCount
                ? 1
                : random.Next(MinimumNodesPerLayer, MaximumNodesPerLayer + 1);
            List<RunMapNode> currentLayer = new(nodeCount);

            for (int position = 0; position < nodeCount; position++)
            {
                LevelDefinition level = levels[random.Next(levels.Length)];
                float progressionMultiplier = 1.0f + ((layer - 1) * 0.2f);
                float variation = (float)(random.NextDouble() * 0.1 - 0.05);

                RunMapNode node = new()
                {
                    Id = FirstGeneratedNodeId + (layer * 10) + position,
                    Layer = layer,
                    Position = position,
                    LevelId = level.Id,
                    DisplayName = level.DisplayName,
                    DifficultyMultiplier = MathF.Max(1.0f, level.BaseDifficulty + progressionMultiplier - 1.0f + variation),
                    IsFinal = layer == LayerCount
                };

                currentLayer.Add(node);
                nodes.Add(node);
            }

            ConnectLayers(previousLayer, currentLayer, random);
            previousLayer = currentLayer;
        }

        return nodes.ToArray();
    }

    public static bool IsValid(RunMapNode[] nodes)
    {
        if (nodes == null || nodes.Length == 0)
            return false;

        bool hasTutorial = false;
        bool hasFinal = false;
        HashSet<int> nodeIds = new();
        Dictionary<int, RunMapNode> nodesById = new();

        foreach (RunMapNode node in nodes)
        {
            if (node == null || !nodeIds.Add(node.Id))
                return false;

            nodesById[node.Id] = node;
            hasTutorial |= node.IsTutorial && node.Id == TutorialNodeId && node.Layer == 0;
            hasFinal |= node.IsFinal && node.Layer == LayerCount;
        }

        if (!hasTutorial || !hasFinal)
            return false;

        foreach (RunMapNode node in nodes)
        {
            foreach (int connectedId in node.ConnectedNodeIds)
            {
                if (!nodesById.TryGetValue(connectedId, out RunMapNode connectedNode))
                    return false;

                if (connectedNode.Layer != node.Layer + 1)
                    return false;
            }
        }

        return true;
    }

    private static void ConnectLayers(List<RunMapNode> previousLayer, List<RunMapNode> currentLayer, Random random)
    {
        for (int currentIndex = 0; currentIndex < currentLayer.Count; currentIndex++)
        {
            RunMapNode currentNode = currentLayer[currentIndex];
            RunMapNode prerequisite = previousLayer[Math.Min(currentIndex, previousLayer.Count - 1)];
            AddConnection(prerequisite, currentNode.Id);
        }

        foreach (RunMapNode previousNode in previousLayer)
        {
            bool hasConnection = false;
            foreach (int connectedId in previousNode.ConnectedNodeIds)
            {
                if (ContainsNode(currentLayer, connectedId))
                {
                    hasConnection = true;
                    break;
                }
            }

            if (!hasConnection)
            {
                int targetIndex = random.Next(currentLayer.Count);
                AddConnection(previousNode, currentLayer[targetIndex].Id);
            }
        }
    }

    private static void AddConnection(RunMapNode node, int connectedNodeId)
    {
        foreach (int existingId in node.ConnectedNodeIds)
        {
            if (existingId == connectedNodeId)
                return;
        }

        int[] connections = new int[node.ConnectedNodeIds.Length + 1];
        node.ConnectedNodeIds.CopyTo(connections, 0);
        connections[^1] = connectedNodeId;
        node.ConnectedNodeIds = connections;
    }

    private static bool ContainsNode(List<RunMapNode> nodes, int nodeId)
    {
        foreach (RunMapNode node in nodes)
        {
            if (node.Id == nodeId)
                return true;
        }

        return false;
    }
}
