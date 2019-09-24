using System;
using System.Collections.Generic;
using System.Linq;

namespace AStarRouting
{
    public class AStar
    {
        public float TravelCost { get; set; }
        public bool AllowDiagonals { get; set; }
        public float DiagonalTravelCost { get; set; }

        public List<List<Node>> Grid { get; set; }

        /// <summary>
        /// Create an instance of the AStar class with the passed in grid and other parameters.
        /// </summary>
        /// <param name="grid">The 2D grid with the format [x][y] starting with [0][0] at the bottom left.</param>
        /// <param name="nodeSize">The size of each block on the grid, used for distance calculations etc.</param>
        /// <param name="travelCost">The cost to travel to a cardinally adjacent node.</param>
        /// <param name="allowDiagonals">Whether moving diagonally is allowed.</param>
        /// <param name="diagonalTravelCost">The cost to travel to a diagonally adjacent node.</param>
        public AStar(List<List<Node>> grid, Vector2D nodeSize, float travelCost = 1, bool allowDiagonals = false, float diagonalTravelCost = 1)
        {
            this.Grid = grid;
            Node.Size = nodeSize;
            this.TravelCost = travelCost;
            this.AllowDiagonals = allowDiagonals;
            this.DiagonalTravelCost = diagonalTravelCost;
        }

        /// <summary>
        /// Create an instance of the AStar class which constructs a grid from dimensions and passed in untraversable nodes.
        /// </summary>
        /// <param name="width">The width of the grid to create.</param>
        /// <param name="height">The height of the grid to create.</param>
        /// <param name="untraversableNodes">A list of node positions that are not traversable.</param>
        /// <param name="nodeSize">The size of each block on the grid, used for distance calculations etc.</param>
        /// <param name="travelCost">The cost to travel to a cardinally adjacent node.</param>
        /// <param name="allowDiagonals">Whether moving diagonally is allowed.</param>
        /// <param name="diagonalTravelCost">The cost to travel to a diagonally adjacent node.</param>
        public AStar(int width, int height, List<Vector2D> untraversableNodes, Vector2D nodeSize, float travelCost = 1, bool allowDiagonals = false, float diagonalTravelCost = 1)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Width should be greater than 0.");
            }
            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Height should be greater than 0.");
            }

            Node.Size = nodeSize;
            this.TravelCost = travelCost;
            this.AllowDiagonals = allowDiagonals;
            this.DiagonalTravelCost = diagonalTravelCost;

            Grid = new List<List<Node>>();
            for (int i = 0; i < width; i++)
            {
                var column = new List<Node>();
                for (int j = 0; j < height; j++)
                {
                    column.Add(
                        new Node(new Vector2D(i, j), true)
                    );
                }
                Grid.Add(column);
            }
            foreach (var curr in untraversableNodes)
            {
                Grid[curr.X][curr.Y].Traversable = false;
            }

        }

        /// <summary>
        /// Finds the shortes route from the start node to the end node using the A* search algorithm.
        /// </summary>
        /// <param name="start">The position of the node to begin from.</param>
        /// <param name="end">The position of the node to complete the path.</param>
        /// <returns></returns>
        public List<Node> FindRoute(Vector2D start, Vector2D end)
        {
            if (Grid.Count < 1)
            {
                throw new InvalidOperationException("The Grid must contain some number of columns.");
            }
            if (Grid[0].Count < 1)
            {
                throw new InvalidOperationException("The Grid must contain some number of rows.");
            }
            if (start.X > Grid.Count - 1 || start.Y > Grid[0].Count - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "The start node is not within the grid.");
            }
            if (end.X > Grid.Count - 1 || end.Y > Grid[0].Count - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "The end node is not within the grid.");
            }

            var startNode = Grid[start.X][start.Y];
            var endNode = Grid[end.X][end.Y];

            var sortedPotentialNodes = new LinkedList<Node>();
            var completedNodes = new HashSet<Node>();

            sortedPotentialNodes.AddFirst(startNode);
            startNode.CostToHere = 0;
            Node currentItem;

            while (sortedPotentialNodes.Count != 0 && sortedPotentialNodes.FirstOrDefault() != endNode)
            {
                currentItem = sortedPotentialNodes.First.Value;
                sortedPotentialNodes.RemoveFirst();
                completedNodes.Add(currentItem);

                AddNodesToPotentialList(GetCardinalAdjacents(currentItem), currentItem, sortedPotentialNodes, completedNodes, currentItem.CostToHere, TravelCost, endNode);
                if (AllowDiagonals)
                {
                    AddNodesToPotentialList(GetDiagonalAdjacents(currentItem), currentItem, sortedPotentialNodes, completedNodes, currentItem.CostToHere, DiagonalTravelCost, endNode);
                }
            }

            if (sortedPotentialNodes.FirstOrDefault() == endNode)
            {
                var route = new List<Node>();
                RecursivePathBuilder(endNode, startNode, route);
                return route;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Builds a path by recursively iterating from the end of the list to the start and then adding each node to the list.
        /// </summary>
        /// <param name="currNode">The current node we are visiting.</param>
        /// <param name="startNode">The node that we started at in the pathing algorithm.</param>
        /// <param name="list">The list to add the route to, beginning at the start and in order.</param>
        private void RecursivePathBuilder(Node currNode, Node startNode, List<Node> list)
        {
            if (currNode.PreviousNode == null && currNode == startNode)
            {
                list.Add(currNode);
            }
            else
            {
                RecursivePathBuilder(currNode.PreviousNode, startNode, list);
                list.Add(currNode);
            }
        }

        /// <summary>
        /// Adds the nodes in the passed in list to the list of potential nodes if they are traversable and new to the list or an improvement on an existing node.
        /// </summary>
        /// <param name="nodesToAdd">The list of candidate nodes to be added to the list.</param>
        /// <param name="parentNode">The node that is the parent of the nodes in the candidate list.</param>
        /// <param name="potentialList">The sorted list of nodes that are potentials to be selected in the the next loop of the algorithm.</param>
        /// <param name="completedNodes">The nodes that have been processed previously.</param>
        /// <param name="costSoFar">The cost of moving so far to the parent node of this node.</param>
        /// <param name="costForMovingToNodes">The cost for moving to each of the nodes in the the list of nodesToAdd.</param>
        /// <param name="endNode">The end node we are trying to find a path to.</param>
        private void AddNodesToPotentialList(List<Node> nodesToAdd, Node parentNode, LinkedList<Node> potentialList, HashSet<Node> completedNodes, float costSoFar, float costForMovingToNodes, Node endNode)
        {
            foreach (Node curr in nodesToAdd)
            {
                if (curr.Traversable && !completedNodes.Contains(curr)) //If the node is not a completed node and is one that can be moved to.
                {
                    float newCostToHere = costSoFar + costForMovingToNodes;

                    if (curr.CostToHere > 0 && curr.CostToHere > newCostToHere) //the node is already in the list of possibles and the new route is shorter
                    {
                        curr.CostToHere = newCostToHere;
                        curr.PreviousNode = parentNode;
                        AddNodeToListInPositionAndRemoveExisting(curr, potentialList);
                    }
                    else
                    {
                        curr.CostToHere = newCostToHere;
                        curr.PreviousNode = parentNode;
                        SetDistanceFromTarget(curr, endNode);
                        AddNodeToListInPosition(curr, potentialList);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a node to the sorted link list in the position of its nodeweight whilst also removing the existing copy of it that is in the list.
        /// </summary>
        /// <param name="node">The node to add (and remove) to the list.</param>
        /// <param name="list">The list of nodes, sorted ascendingly by nodeweight, where we are adding and removing nodes from.</param>
        private void AddNodeToListInPositionAndRemoveExisting(Node node, LinkedList<Node> list)
        {
            if (list.Last.Value == node)
            {
                if (list.Count == 1 || node.NodeWeight > list.Last.Previous.Value.NodeWeight) //the node remains the last item in the list even with its new weight.
                {
                    return;
                }
                else
                {
                    list.Remove(list.Last);
                    AddNodeToListInPosition(node, list);
                    return;
                }
            }

            var curr = list.First;
            var notAddedNew = true;
            var notRemoved = true;

            while (curr != null && (notAddedNew || notRemoved))
            {
                if (curr.Value == node)//because the node being removed will be after the node we add is, we don't need to worry about checking which of the 2 it is if we have already added it.
                {
                    curr = curr.Next;
                    list.Remove(curr.Previous);
                    notRemoved = false;
                }
                if (notAddedNew && curr.Value.NodeWeight >= node.NodeWeight)
                {
                    list.AddBefore(curr, node);
                    curr = curr.Next;
                    notAddedNew = false;
                }
                else
                {
                    curr = curr.Next;
                }
            }
        }

        /// <summary>
        /// Adds a node to the sorted link list in the correct position sorted by nodeweight
        /// </summary>
        /// <param name="node">The node to add to the sorted list.</param>
        /// <param name="list">The list of nodes, sorted ascendingly by nodeweight, where we are adding the node.</param>
        private void AddNodeToListInPosition(Node node, LinkedList<Node> list)
        {
            if (list.Count == 0)
            {
                list.AddLast(node);
                return;
            }

            var curr = list.First;
            while (curr != null)
            {
                if (curr.Value.NodeWeight >= node.NodeWeight)
                {
                    list.AddBefore(curr, node);
                    return;
                }
                curr = curr.Next;
            }
            list.AddLast(node); //if we've got here we haven't added the node yet so add to end.
        }

        /// <summary>
        /// Set the DistanceFromTarget of the curr node based upon its distance from the end node.
        /// </summary>
        /// <param name="curr">The node we want to set the distance from target to.</param>
        /// <param name="end">The end node to calculate the distance to.</param>
        private void SetDistanceFromTarget(Node curr, Node end)
        {
            if (curr.DistanceFromTarget < 0)
            {
                curr.DistanceFromTarget = CalculateHeuristicDistanceBetweenNodes(new Vector2D(curr.Position.X * Node.Size.X, curr.Position.Y * Node.Size.Y), new Vector2D(end.Position.X * Node.Size.X, end.Position.Y * Node.Size.Y));
            }
        }

        /// <summary>
        /// Calculates the linear distance between 2 vectors.
        /// </summary>
        /// <param name="start">The first vector.</param>
        /// <param name="end">The second vector.</param>
        /// <returns></returns>
        private float CalculateHeuristicDistanceBetweenNodes(Vector2D start, Vector2D end)
        {
            return (float)Math.Sqrt(Math.Pow(start.X - end.X, 2) + Math.Pow(start.Y - end.Y, 2));
        }

        /// <summary>
        /// Get the nodes to the north, south, east and west of the passed in node.
        /// </summary>
        /// <param name="node">The node to find the adjacents of.</param>
        /// <returns>A list containing the adjacent nodes.</returns>
        private List<Node> GetCardinalAdjacents(Node node)
        {
            var nodes = new List<Node>();

            int x = node.Position.X;
            int y = node.Position.Y;

            if (y + 1 < Grid[0].Count)
            {
                nodes.Add(Grid[x][y + 1]);
            }
            if (x + 1 < Grid.Count)
            {
                nodes.Add(Grid[x + 1][y]);
            }
            if (y - 1 >= 0)
            {
                nodes.Add(Grid[x][y - 1]);
            }
            if (x - 1 >= 0)
            {
                nodes.Add(Grid[x - 1][y]);
            }

            return nodes;
        }

        /// <summary>
        /// Get the nodes to the north east, south east, south west and north west of the passed in node.
        /// </summary>
        /// <param name="node">The node to find the diagonally adjacent nodes of.</param>
        /// <returns>A list containing the diagonally adjacent nodes.</returns>
        private List<Node> GetDiagonalAdjacents(Node node)
        {
            var nodes = new List<Node>();

            int x = node.Position.X;
            int y = node.Position.Y;

            if (y + 1 < Grid[0].Count)
            {
                if (x + 1 < Grid.Count)
                {
                    nodes.Add(Grid[x + 1][y + 1]);
                }
                if (x - 1 >= 0)
                {
                    nodes.Add(Grid[x - 1][y + 1]);
                }
            }
            if (y - 1 >= 0)
            {
                if (x + 1 < Grid.Count)
                {
                    nodes.Add(Grid[x + 1][y - 1]);
                }
                if (x - 1 >= 0)
                {
                    nodes.Add(Grid[x - 1][y - 1]);
                }
            }
            return nodes;
        }
    }
}
