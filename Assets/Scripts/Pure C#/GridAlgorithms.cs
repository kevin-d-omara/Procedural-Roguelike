using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Priority_Queue;

namespace ProceduralRoguelike
{
	public static class GridAlgorithms
	{
        public static List<Vector2> AdjacentOffsets { get; private set; }
        public static List<Vector2> DiagonalOffsets { get; private set; } 
        public static List<Vector2> SurroundingOffsets { get; private set; } 

        static GridAlgorithms()
        {
            AdjacentOffsets = new List<Vector2>
            {
                new Vector2(1,0), new Vector2(0,1), new Vector2(-1,0), new Vector2(0,-1)
            };
            DiagonalOffsets = new List<Vector2>
            {
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
            public bool Diagonal { get; private set; }
            public int Cost { get; private set; }

            /// <param name="position">The position relative to origin of this node.</param>
            /// <param name="diagonal">Was this node reached by a 1-cost diagonal move?</param>
            /// <param name="cost">The cumulative cost to reach this node.</param>
            public Node(Vector2 position, bool diagonal, int cost)
            {
                this.Position = position;
                this.Diagonal = diagonal;
                this.Cost = cost;
            }

            public List<Vector2> Get(List<Vector2> relativeOffsets)
            {
                var offsets = new List<Vector2>();
                foreach (Vector2 offset in relativeOffsets)
                {
                    offsets.Add(new Vector2(Position.x + offset.x, Position.y + offset.y));
                }
            }
        }

        /// <summary>
        /// Calculates the offsets contained in a grid-based circle of the specified radius. Uses
        /// D&D movement rules: adjacent cost = 1, diagonal cost = 1,2,1,2..
        /// </summary>
        /// <returns>List of offsets contained in the grid-based circle.</returns>
		public static List<Vector2> CircleFill(int radius)
        {
            var offsets = new List<Vector2>();

            // Frontier is the open set of nodes to be expanded from.
            var frontier = new SimplePriorityQueue<Node, int>();
            frontier.Enqueue(new Node(Vector2.zero, false, 0), 0);

            while (frontier.Count > 0)
            {
                var currentNode = frontier.Dequeue();
                offsets.Add(currentNode.Position);
                if (currentNode.Cost >= radius) { continue; }

                foreach (Vector2 offset in GridAlgorithms.surroundingOffset)
                {
                    
                }
            }

            return offsets;
        }


	}
}
