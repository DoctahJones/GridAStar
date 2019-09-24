using AStarRouting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AStarTests
{
    [TestClass]
    public class AStarTests
    {
        [TestMethod]
        public void TestGridConstructorAcceptsValidEmptyGrid()
        {
            var grid = new List<List<Node>>();
            var star = new AStar(grid, new Vector2D(1, 1));

            Assert.IsNotNull(star);
            Assert.IsNotNull(star.Grid);
        }

        [TestMethod]
        public void TestGridConstructorAcceptsValidGrid()
        {
            var grid = new List<List<Node>>();
            for (int i = 0; i < 3; i++)
            {
                var list = new List<Node>();
                for (int j = 0; j < 4; j++)
                {
                    list.Add(new Node(new Vector2D(i, j), true));
                }
                grid.Add(list);
            }
            var star = new AStar(grid, new Vector2D(1, 1));

            Assert.IsNotNull(star);
            Assert.IsNotNull(star.Grid);
            Assert.AreEqual(3, star.Grid.Count);
            Assert.AreEqual(4, star.Grid[0].Count);
        }

        [TestMethod]
        public void TestGridConstructorArguments()
        {
            var grid = new List<List<Node>>();
            for (int i = 0; i < 3; i++)
            {
                var list = new List<Node>();
                for (int j = 0; j < 4; j++)
                {
                    list.Add(new Node(new Vector2D(i, j), true));
                }
                grid.Add(list);
            }
            var star = new AStar(grid, new Vector2D(5, 4), 3, true, 4);

            Assert.AreEqual(5, Node.Size.X);
            Assert.AreEqual(4, Node.Size.Y);
            Assert.AreEqual(3, star.TravelCost);
            Assert.IsTrue(star.AllowDiagonals);
            Assert.AreEqual(4, star.DiagonalTravelCost);
        }

        [TestMethod]
        public void TestBuildGridConstructorBuildsValidGrid()
        {
            var star = new AStar(3, 3, new List<Vector2D>(), new Vector2D(1, 1));

            Assert.IsNotNull(star);
            Assert.IsNotNull(star.Grid);
            Assert.AreEqual(3, star.Grid.Count);
            Assert.AreEqual(3, star.Grid[0].Count);
        }

        [TestMethod]
        public void TestBuildGridConstructorUntraversableNodes()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 1),
                new Vector2D(2, 2)
            };

            var star = new AStar(3, 3, untraversables, new Vector2D(1, 1));

            Assert.IsTrue(star.Grid[0][0].Traversable);
            Assert.IsFalse(star.Grid[0][1].Traversable);
            Assert.IsTrue(star.Grid[0][2].Traversable);
            Assert.IsFalse(star.Grid[2][2].Traversable);
        }


        [TestMethod]
        public void TestBuildGridConstructorArguments()
        {
            var star = new AStar(7, 4, new List<Vector2D>(), new Vector2D(4, 3), 4, true, 6);


            Assert.AreEqual(7, star.Grid.Count);
            Assert.AreEqual(4, star.Grid[0].Count);

            Assert.AreEqual(4, Node.Size.X);
            Assert.AreEqual(3, Node.Size.Y);

            Assert.AreEqual(4, star.TravelCost);
            Assert.IsTrue(star.AllowDiagonals);
            Assert.AreEqual(6, star.DiagonalTravelCost);
        }

        [TestMethod]
        public void TestFindRouteSmallNoUntraversablesNoDiagonalsSimpleStraightLine()
        {
            var star = new AStar(4, 4, new List<Vector2D>(), new Vector2D(1, 1));

            var route = star.FindRoute(new Vector2D(2, 0), new Vector2D(2, 2));

            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Count);
            Assert.AreEqual(new Vector2D(2, 0), route[0].Position);
            Assert.AreEqual(new Vector2D(2, 1), route[1].Position);
            Assert.AreEqual(new Vector2D(2, 2), route[2].Position);
        }

        [TestMethod]
        public void TestCantFindRouteSmalloDiagonalsSimpleStraightLine()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 1),
                new Vector2D(1, 1),
                new Vector2D(2, 1),
                new Vector2D(3, 1)
            };

            var star = new AStar(4, 4, untraversables, new Vector2D(1, 1));

            var route = star.FindRoute(new Vector2D(2, 0), new Vector2D(2, 2));

            Assert.IsNull(route);
        }

        [TestMethod]
        public void TestFindRouteSmallNoUntraversablesDiagonalsSimpleStraightLine()
        {
            var star = new AStar(4, 4, new List<Vector2D>(), new Vector2D(1, 1), 1, true, 1);

            var route = star.FindRoute(new Vector2D(0, 1), new Vector2D(2, 3));

            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Count);
            Assert.AreEqual(new Vector2D(0, 1), route[0].Position);
            Assert.AreEqual(new Vector2D(1, 2), route[1].Position);
            Assert.AreEqual(new Vector2D(2, 3), route[2].Position);
        }

        [TestMethod]
        public void TestFindZigZagRoute()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 1),
                new Vector2D(1, 1),
                new Vector2D(2, 1),
                new Vector2D(3, 1),
                new Vector2D(1, 3),
                new Vector2D(2, 3),
                new Vector2D(3, 3),
                new Vector2D(4, 3)
            };

            var star = new AStar(5, 5, untraversables, new Vector2D(1, 1));

            var route = star.FindRoute(new Vector2D(0, 0), new Vector2D(4, 4));

            Assert.IsNotNull(route);
            Assert.AreEqual(17, route.Count);
        }

        [TestMethod]
        public void TestDoesntFavorDiagonalsWithLargeCost()
        {
            var star = new AStar(4, 4, new List<Vector2D>(), new Vector2D(1, 1), 1, true, 3);

            var route = star.FindRoute(new Vector2D(0, 0), new Vector2D(1, 3));

            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Count);
        }

        [TestMethod]
        public void TestJumpsDiagonalsWhenBlockedOtherwise()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 2),
                new Vector2D(1, 2),
                new Vector2D(2, 1),
                new Vector2D(2, 0)
            };

            var star = new AStar(5, 5, untraversables, new Vector2D(1, 1), 1, true, 1);

            var route = star.FindRoute(new Vector2D(0, 0), new Vector2D(3, 2));

            Assert.IsNotNull(route);
            Assert.AreEqual(4, route.Count);
        }

        [TestMethod]
        public void TestFindsRouteWhenStartAndEndAreTheSame()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(2, 0),
                new Vector2D(2, 1),
                new Vector2D(2, 2),
                new Vector2D(1, 2),
                new Vector2D(0, 2),
                new Vector2D(0, 1)
            };

            var star = new AStar(5, 5, untraversables, new Vector2D(1, 1));

            var route = star.FindRoute(new Vector2D(1, 1), new Vector2D(1, 1));

            Assert.IsNotNull(route);
            Assert.AreEqual(1, route.Count);
        }

        [TestMethod]
        public void TestFindsRouteHeadingSouthWest()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 1),
                new Vector2D(1, 1),
                new Vector2D(0, 2),
                new Vector2D(1, 2),
                new Vector2D(0, 3),
                new Vector2D(1, 3),
                new Vector2D(1, 4),
                new Vector2D(1, 4)
            };

            var star = new AStar(5, 5, untraversables, new Vector2D(1, 1));

            var route = star.FindRoute(new Vector2D(4, 3), new Vector2D(1, 0));

            Assert.IsNotNull(route);
            Assert.AreEqual(7, route.Count);
        }

        [TestMethod]
        public void TestDoesntFindRouteWhenBlockedIn()
        {
            var untraversables = new List<Vector2D>
            {
                new Vector2D(0, 0),
                new Vector2D(1, 0),
                new Vector2D(2, 0),
                new Vector2D(2, 1),
                new Vector2D(2, 2),
                new Vector2D(1, 2),
                new Vector2D(0, 2),
                new Vector2D(0, 1)
            };

            var star = new AStar(5, 5, untraversables, new Vector2D(1, 1), 1, true, 2);

            var route = star.FindRoute(new Vector2D(1, 1), new Vector2D(4, 3));

            Assert.IsNull(route);
        }


    }
}
