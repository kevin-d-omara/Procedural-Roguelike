using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

namespace ProceduralRoguelike
{
	public static class GridAlgorithms
	{
        public static List<Vector2> OrthogonalOffsets { get; private set; }
        public static List<Vector2> DiagonalOffsets { get; private set; } 
        public static List<Vector2> SurroundingOffsets { get; private set; } 

        static GridAlgorithms()
        {
            OrthogonalOffsets = new List<Vector2>
            {
                // Counter-clockwise from +x-axis.
                new Vector2(1,0), new Vector2(0,1), new Vector2(-1,0), new Vector2(0,-1)
            };
            DiagonalOffsets = new List<Vector2>
            {
                // Counter-clockwise from 1st quadrant.
                new Vector2(1,1), new Vector2(-1,1), new Vector2(-1,-1), new Vector2(1,-1)
            };
            SurroundingOffsets = new List<Vector2>
            {
                new Vector2(1,0), new Vector2(0,1), new Vector2(-1,0), new Vector2(0,-1),
                new Vector2(1,1), new Vector2(-1,1), new Vector2(-1,-1), new Vector2(1,-1)
            };
        }

        private class Node
        {
            public Vector2 Position { get; private set; }
            public bool usedDiagonalDiscount { get; private set; }
            public int Cost { get; private set; }

            /// <param name="position">The position relative to origin of this node.</param>
            /// <param name="usedDiagonalDiscount">Was this node reached by a 1-cost diagonal move?
            /// </param>
            /// <param name="cost">The cumulative cost to reach this node.</param>
            public Node(Vector2 position, bool usedDiagonalDiscount, int cost)
            {
                this.Position = position;
                this.usedDiagonalDiscount = usedDiagonalDiscount;
                this.Cost = cost;
            }

            /// <summary>
            /// Finds the positions relative to this node based on the offsets provided.
            /// </summary>
            /// <param name="relativeOffsets">A list of offsets (i.e. 
            /// GridAlgorithms.OrthogonalOffsets</param>
            /// <returns>A list of positions offset from this node.</returns>
            public List<Vector2> GetPositionsFrom(List<Vector2> relativeOffsets)
            {
                var offsets = new List<Vector2>();
                foreach (Vector2 offset in relativeOffsets)
                {
                    offsets.Add(new Vector2(Position.x + offset.x, Position.y + offset.y));
                }
                return offsets;
            }
        }

        /// <summary>
        /// Calculates the offsets defining a circle of a specific radius. Uses Dungeons & Dragons
        /// movement rules: adjacent moves cost 1, diagonal moves alternate between cost 1 and 2.
        /// </summary>
        /// <returns>The list of offsets defining a circle of the specified radius.</returns>
		public static List<Vector2> CircleFill(int radius)
        {
            var offsets = new List<Vector2>();

            var visited = new Dictionary<Vector2, Node>();
            var frontier = new SimplePriorityQueue<Node, int>();
            frontier.Enqueue(new Node(Vector2.zero, false, 0), 0);

            while (frontier.Count > 0)
            {
                var currentNode = frontier.Dequeue();
                offsets.Add(currentNode.Position);

                var newCost = currentNode.Cost + 1;
                // Stop expanding from this node if it has surpassed the radius.
                if (newCost > radius) { continue; }

                // Expand into orthogonal posiitons.
                foreach (Vector2 newPosition in currentNode.GetPositionsFrom(OrthogonalOffsets))
                {
                    Node nextNode;
                    if (visited.TryGetValue(newPosition, out nextNode))
                    {
                        if (newCost < nextNode.Cost)
                        {
                            frontier.Remove(nextNode);
                            nextNode = new Node(newPosition, false, newCost);
                            frontier.Enqueue(nextNode, newCost);
                        }
                    }
                    else
                    {
                        nextNode = new Node(newPosition, false, newCost);
                        frontier.Enqueue(nextNode, newCost);
                    }
                }

                bool usedDiagonalDiscount = true;
                if (currentNode.usedDiagonalDiscount)
                {
                    ++newCost;
                    // Stop expanding from this node if it has surpassed the radius.
                    if (newCost > radius) { continue; }

                    usedDiagonalDiscount = false;
                }
                // Expand into diagonal positions.
                foreach (Vector2 newPosition in currentNode.GetPositionsFrom(DiagonalOffsets))
                {
                    Node nextNode;
                    if (visited.TryGetValue(newPosition, out nextNode))
                    {
                        if (newCost < nextNode.Cost)
                        {
                            frontier.Remove(nextNode);
                            nextNode = new Node(newPosition, usedDiagonalDiscount, newCost);
                            frontier.Enqueue(nextNode, newCost);
                        }
                    }
                    else
                    {
                        nextNode = new Node(newPosition, usedDiagonalDiscount, newCost);
                        frontier.Enqueue(nextNode, newCost);
                    }
                }
            }

            return offsets;
        }


	}
}
