namespace AStarRouting
{
    public class Node
    {
        /// <summary>
        /// The node we visited this node from.
        /// </summary>
        public Node PreviousNode { get; set; }
        /// <summary>
        /// The position of the bottom left corner of this node on the grid.
        /// </summary>
        public Vector2D Position { get; set; }
        /// <summary>
        /// The width and height of the node on the grid.
        /// </summary>
        public static Vector2D Size { get; set; }
        /// <summary>
        /// Whether the node can be moved to or not.
        /// </summary>
        public bool Traversable { get; set; }
        /// <summary>
        /// The cost to move to the current node at this point
        /// </summary>
        public float CostToHere { get; set; }
        /// <summary>
        /// The distance of this node from the target.
        /// </summary>
        public float DistanceFromTarget { get; set; }
        /// <summary>
        /// The weight of the current node.
        /// </summary>
        public float NodeWeight
        {
            get
            {
                return CostToHere + DistanceFromTarget;
            }
        }


        public Node(Vector2D position, bool traversable)
        {
            this.Position = position;
            this.Traversable = traversable;

            this.CostToHere = -1;
            this.DistanceFromTarget = -1;

        }
    }

}
