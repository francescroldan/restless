using UnityEngine;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Builds the abstract graph for a run.
    /// No prefabs, no geometry — only nodes and edges.
    ///
    /// Guaranteed structure every run:
    ///   Entrance (Safe) → main path → Exit (Collapse/Landmark)
    ///   At least 1 Memory node (for fragments)
    ///   At least 1 fork with a dead end
    /// </summary>
    public static class RunGraphGenerator
    {
        public static RunGraph Generate(int seed, int targetRooms)
        {
            var rng   = new System.Random(seed);
            var graph = new RunGraph();
            int idx   = 0;

            targetRooms = Mathf.Clamp(targetRooms, 6, 12);

            // ── Entrance ──────────────────────────────────────────────────
            var entrance = new GraphNode(idx++, RoomType.Safe, RoomSize.Medium)
            {
                IsEntrance  = true,
                DangerHint  = 0f
            };
            graph.Nodes.Add(entrance);

            // ── Main path ─────────────────────────────────────────────────
            // Build a spine from entrance to exit, inserting variety
            var prev         = entrance;
            int spineLength  = Rng(rng, 3, 5);
            bool memoryPlaced = false;

            for (int i = 0; i < spineLength; i++)
            {
                float progress = (float)(i + 1) / spineLength;
                var   node     = BuildSpineNode(idx++, progress, rng, ref memoryPlaced);
                graph.Nodes.Add(node);
                graph.AddEdge(prev, node);
                prev = node;
            }

            // Guarantee memory node on spine if not placed yet
            if (!memoryPlaced)
            {
                int memIdx = Rng(rng, 1, graph.Nodes.Count - 1);
                graph.Nodes[memIdx].Type             = RoomType.Memory;
                graph.Nodes[memIdx].MustHaveFragment = true;
            }

            // ── Forks and dead ends ───────────────────────────────────────
            int forksAdded = 0;
            int maxForks   = Mathf.Max(1, targetRooms - graph.Nodes.Count - 1);

            foreach (var node in graph.Nodes.ToArray())
            {
                if (forksAdded >= maxForks) break;
                if (graph.Nodes.Count >= targetRooms - 1) break;
                if (node.IsEntrance || node.Type == RoomType.DeadEnd) continue;
                if (node.Neighbours.Count >= 3) continue;
                if (rng.NextDouble() > 0.45f) continue;

                var branch = new GraphNode(idx++, RoomType.DeadEnd, RoomSize.Small)
                {
                    DangerHint = node.DangerHint * 1.2f
                };
                graph.Nodes.Add(branch);
                graph.AddEdge(node, branch);
                forksAdded++;
            }

            // ── Exit / Landmark ───────────────────────────────────────────
            var exit = new GraphNode(idx++, RoomType.Landmark, RoomSize.Large)
            {
                IsExit     = true,
                DangerHint = 0.8f
            };
            graph.Nodes.Add(exit);
            graph.AddEdge(prev, exit);
            graph.Exit = exit;

            return graph;
        }

        private static GraphNode BuildSpineNode(int idx, float progress, System.Random rng, ref bool memoryPlaced)
        {
            // Early (0–0.3): safe / corridor
            // Mid (0.3–0.7): encounter / memory / traversal
            // Late (0.7–1.0): encounter / ritual / collapse

            RoomType type;
            float    danger;

            if (progress < 0.3f)
            {
                type   = rng.NextDouble() < 0.6 ? RoomType.Corridor : RoomType.Safe;
                danger = Mathf.Lerp(0.1f, 0.3f, (float)progress / 0.3f);
            }
            else if (progress < 0.7f)
            {
                double roll = rng.NextDouble();
                if (!memoryPlaced && roll < 0.4)
                {
                    type         = RoomType.Memory;
                    memoryPlaced = true;
                }
                else
                    type = roll < 0.5 ? RoomType.Encounter : RoomType.Corridor;

                danger = Mathf.Lerp(0.3f, 0.65f, (float)(progress - 0.3f) / 0.4f);
            }
            else
            {
                double roll = rng.NextDouble();
                type   = roll < 0.5 ? RoomType.Encounter : RoomType.Ritual;
                danger = Mathf.Lerp(0.65f, 0.9f, (float)(progress - 0.7f) / 0.3f);
            }

            var node = new GraphNode(idx, type)
            {
                DangerHint       = danger,
                MustHaveFragment = type == RoomType.Memory
            };

            if (type == RoomType.Memory) memoryPlaced = true;

            return node;
        }

        private static int Rng(System.Random rng, int min, int max) =>
            rng.Next(min, max + 1);
    }
}
