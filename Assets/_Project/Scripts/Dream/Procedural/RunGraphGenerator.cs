using System.Collections.Generic;
using UnityEngine;

namespace Restless.Dream.Procedural
{
    /// <summary>
    /// Builds the abstract graph for a run.
    /// No prefabs, no geometry — only nodes and edges.
    ///
    /// Graph shape every run:
    ///   Entrance fans out to 2-3 rooms immediately.
    ///   Each of those may branch further (depth 1 → 1-2 children).
    ///   Depth 2+ nodes are mostly leaves (exploration dead ends).
    ///   Exit attaches to the highest-danger leaf.
    ///   At least 1 Memory node guaranteed.
    /// </summary>
    public static class RunGraphGenerator
    {
        public static RunGraph Generate(int seed, int targetRooms)
        {
            var rng = new System.Random(seed);
            var graph = new RunGraph();
            int idx = 0;

            targetRooms = Mathf.Clamp(targetRooms, 7, 14);

            // ── Entrance ──────────────────────────────────────────────────
            var entrance = new GraphNode(idx++, RoomType.Safe, RoomSize.Medium)
            {
                IsEntrance = true,
                DangerHint = 0f
            };
            graph.Nodes.Add(entrance);

            // ── BFS expansion ─────────────────────────────────────────────
            // Budget = total interior rooms (entrance and exit not counted).
            int budget = targetRooms - 2;
            bool memoryPlaced = false;

            var frontier = new Queue<(GraphNode node, int depth)>();
            frontier.Enqueue((entrance, 0));

            while (frontier.Count > 0 && budget > 0)
            {
                var (parent, depth) = frontier.Dequeue();

                int children = Mathf.Min(ChildCount(depth, budget, rng), budget);

                for (int i = 0; i < children; i++)
                {
                    if (budget <= 0) break;

                    // Progress 0→1 based on depth (max depth ~3)
                    float progress = Mathf.Clamp01(depth * 0.28f + 0.15f +
                                                   (float)rng.NextDouble() * 0.12f);

                    var node = BuildNode(idx++, progress, rng, ref memoryPlaced);
                    graph.Nodes.Add(node);
                    graph.AddEdge(parent, node);
                    budget--;

                    frontier.Enqueue((node, depth + 1));
                }
            }

            // ── Guarantee memory node ─────────────────────────────────────
            if (!memoryPlaced)
            {
                int midIdx = Rng(rng, 1, graph.Nodes.Count - 1);
                graph.Nodes[midIdx].Type             = RoomType.Memory;
                graph.Nodes[midIdx].MustHaveFragment = true;
            }

            // ── Guarantee ritual node ──────────────────────────────────────
            // With shallow graphs (≤10 rooms) progress never exceeds 0.65, so
            // Ritual nodes are never generated organically. Convert the highest-
            // danger non-special interior node to Ritual to ensure one always exists.
            bool ritualPlaced = false;
            foreach (var n in graph.Nodes)
                if (n.Type == RoomType.Ritual) { ritualPlaced = true; break; }

            if (!ritualPlaced)
            {
                GraphNode best      = null;
                float     bestScore = -1f;
                foreach (var n in graph.Nodes)
                {
                    if (n.IsEntrance || n.IsExit) continue;
                    if (n.Type == RoomType.Memory || n.Type == RoomType.Landmark) continue;
                    float score = n.DangerHint + (float)rng.NextDouble() * 0.05f;
                    if (score > bestScore) { bestScore = score; best = n; }
                }
                if (best != null) best.Type = RoomType.Ritual;
            }

            // ── Exit ──────────────────────────────────────────────────────
            // Attach to the highest-danger leaf that isn't the entrance.
            // A leaf has only 1 neighbour in the graph (no children yet).
            var exitParent = FindExitParent(graph, rng);
            var exit = new GraphNode(idx++, RoomType.Landmark, RoomSize.Large)
            {
                IsExit     = true,
                DangerHint = 0.9f
            };
            graph.Nodes.Add(exit);
            graph.AddEdge(exitParent, exit);
            graph.Exit = exit;

            return graph;
        }

        // ── Child count per depth ─────────────────────────────────────────

        // depth 0 (entrance): 2-3 exits — wide open start.
        // depth 1: 55 % chance of 2 children, 45 % of 1 — creates branch clusters.
        // depth 2: 25 % chance of 1 child, otherwise leaf — mostly closes out.
        // depth 3+: always leaf.
        private static int ChildCount(int depth, int budget, System.Random rng)
        {
            if (budget <= 0) return 0;
            return depth switch
            {
                0 => Rng(rng, 2, 3),
                1 => rng.NextDouble() < 0.55 ? 2 : 1,
                2 => rng.NextDouble() < 0.25 ? 1 : 0,
                _ => 0
            };
        }

        // ── Node builder ──────────────────────────────────────────────────

        private static GraphNode BuildNode(int idx, float progress,
                                           System.Random rng, ref bool memoryPlaced)
        {
            RoomType type;
            if (progress < 0.3f)
            {
                type = RoomType.Safe;
            }
            else if (progress < 0.65f)
            {
                double roll = rng.NextDouble();
                if (!memoryPlaced && roll < 0.35)
                    type = RoomType.Memory;
                else
                    type = roll < 0.55 ? RoomType.Encounter : RoomType.Safe;
            }
            else
            {
                type = rng.NextDouble() < 0.5 ? RoomType.Encounter : RoomType.Ritual;
            }

            if (type == RoomType.Memory) memoryPlaced = true;

            float danger = Mathf.Clamp01(progress * 0.85f + (float)rng.NextDouble() * 0.1f);
            RoomSize size = progress >= 0.3f && rng.NextDouble() < 0.3
                ? RoomSize.Large : RoomSize.Medium;

            return new GraphNode(idx, type, size)
            {
                DangerHint       = danger,
                MustHaveFragment = type == RoomType.Memory
            };
        }

        // ── Exit parent selection ─────────────────────────────────────────

        // Prefer leaf nodes (1 neighbour = no children) with high danger,
        // with a small random nudge to vary the chosen branch each run.
        private static GraphNode FindExitParent(RunGraph graph, System.Random rng)
        {
            GraphNode best      = null;
            float     bestScore = -1f;

            foreach (var n in graph.Nodes)
            {
                if (n.IsEntrance) continue;

                float leafBonus = n.Neighbours.Count == 1 ? 0.5f : 0f;
                float score     = n.DangerHint + leafBonus + (float)rng.NextDouble() * 0.15f;

                if (score > bestScore) { bestScore = score; best = n; }
            }

            return best ?? graph.Nodes[graph.Nodes.Count - 1];
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static int Rng(System.Random rng, int min, int max) =>
            rng.Next(min, max + 1);
    }
}
