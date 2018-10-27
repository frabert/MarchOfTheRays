using System.Collections.Generic;

namespace MarchOfTheRays.Core
{
    public static class Compiler
    {
        enum NodeColor
        {
            White,
            Gray,
            Black
        }

        /// <summary>
        /// Search for cycles in the node.
        /// Returns a list containing the nodes belonging to a cycle, or an empty list if none are found.
        /// </summary>
        /// <param name="outputNode">The node from which to start the search</param>
        /// <param name="nodes">The graph to scan</param>
        /// <returns>A list of nodes belonging to a cycle</returns>
        public static IList<INode> CheckForCycles(OutputNode outputNode, IList<INode> nodes)
        {
            var colors = new Dictionary<INode, NodeColor>();
            var parents = new Dictionary<INode, INode>();
            var cycle = new List<INode>();
            var stack = new Stack<INode>();
            INode cycleStart = null;

            foreach (var node in nodes)
            {
                colors.Add(node, NodeColor.White);
            }

            foreach(var node in nodes)
            {
                if(colors[node] == NodeColor.White) DFSVisit(node);
            }

            void DFSVisit(INode n)
            {
                colors[n] = NodeColor.Gray;
                var edges = new List<INode>();
                switch (n)
                {
                    case IUnaryNode u:
                        if (u.Input != null) edges.Add(u.Input);
                        break;
                    case IBinaryNode b:
                        if (b.Left != null) edges.Add(b.Left);
                        if (b.Right != null) edges.Add(b.Right);
                        break;
                }

                foreach (var edge in edges)
                {
                    if (colors[edge] == NodeColor.White)
                    {
                        colors[edge] = NodeColor.Gray;
                        parents[edge] = n;
                        DFSVisit(edge);
                    }
                    else if (colors[edge] == NodeColor.Gray)
                    {
                        cycleStart = edge;
                        parents[edge] = n;
                    }
                }
                colors[n] = NodeColor.Black;
            }

            if(cycleStart != null)
            {
                cycle.Add(cycleStart);
                var parent = parents[cycleStart];
                while(parent != cycleStart)
                {
                    cycle.Add(parent);
                    parent = parents[parent];
                }
            }

            return cycle;
        }
    }
}
