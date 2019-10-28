/////////////////////////////////////////////////////////////////////////////
//  Strongconpo.cs --  find strong conponent of a set of files             //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project3                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: Graph.cs created by Dr.Jim Fawcett                             //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   The modeule get dependency table from dependency analysis and gernerate 
 *   a new dependency graph. By analysis strong conponent, I use tarjan 
 *   algorithm.
 *   
 *    * Public Interface
 * ======================
 * CsEdge<V, E>            //define the edge
 * CsNode<V, E>           // define the node
 * CsGraph<V, E>                    // define the graph
 * SC_finder()                      // the algorithm for finding the SCC
 *   showDependencies() // show denpendency graph
 *   SC_tostring // transform SC graph in to string
 *   
 */
/*
 *   Build Process
 *   -------------
 *   - Required files: FileMgr.cs   DepenAnalysis.cs
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 31 October 2018
 *     - first release
 *     
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DepAnalysis;
using FileUtilities;

namespace StrongConpo
{
    /////////////////////////////////////////////////////////////////////////
    // CsEdge<V,E> and CsNode<V,E> classes

    public class CsEdge<V, E> // holds child node and instance of edge type E
    {
        public CsNode<V, E> targetNode { get; set; } = null;
        public E edgeValue { get; set; }

        public CsEdge(CsNode<V, E> node, E value)
        {
            targetNode = node;
            edgeValue = value;
        }
    };

    public class CsNode<V, E>
    {
        public V nodeValue { get; set; }
        public string name { get; set; }
        public List<CsEdge<V, E>> children { get; set; }
        public bool visited { get; set; }
        public int t_normalnum { get; set; } = -1;
        public int min_num { get; set; } = -1;

        //----< construct a named node >---------------------------------------

        public CsNode(string nodeName)
        {
            name = nodeName;
            children = new List<CsEdge<V, E>>();
            visited = false;
        }
        //----< add child vertex and its associated edge value to vertex >-----

        public void addChild(CsNode<V, E> childNode, E edgeVal)
        {
            children.Add(new CsEdge<V, E>(childNode, edgeVal));
        }
        //----< find the next unvisited child >--------------------------------

        public CsEdge<V, E> getNextUnmarkedChild()
        {
            foreach (CsEdge<V, E> child in children)
            {
                if (!child.targetNode.visited)
                {
                    child.targetNode.visited = true;
                    return child;
                }
            }
            return null;
        }
        //----< has unvisited child? >-----------------------------------

        public bool hasUnmarkedChild()
        {
            foreach (CsEdge<V, E> child in children)
            {
                if (!child.targetNode.visited)
                {
                    return true;
                }
            }
            return false;
        }
        public void unmark()
        {
            visited = false;
        }
        public override string ToString()
        {
            return name;
        }
    }
    /////////////////////////////////////////////////////////////////////////
    // Operation<V,E> class
    
    public class Operation<V, E>
    {
        //----< graph.walk() calls this on every node >------------------------

        virtual public bool doNodeOp(CsNode<V, E> node)
        {
            Console.Write("\n  {0}", node.ToString());
            return true;
        }
        //----< graph calls this on every child visitation >-------------------

        virtual public bool doEdgeOp(E edgeVal)
        {
            Console.Write(" {0}", edgeVal.ToString());
            return true;
        }
    }
    /////////////////////////////////////////////////////////////////////////
    // CsGraph<V,E> class

    public class CsGraph<V, E>
    {
        public CsNode<V, E> startNode { get; set; }
        public string name { get; set; }
        public bool showBackTrack { get; set; } = false;
        List<string> SClist = new List<string>();
        private List<CsNode<V, E>> adjList { get; set; }  // node adjacency list
        private Operation<V, E> gop = null;


        Stack<CsNode<V, E>> node_stack = new Stack<CsNode<V, E>>();
        static int dfn_index = 0;

        //public static int dfn_index = 0;

        //----< construct a named graph >--------------------------------------

        public CsGraph(string graphName)
        {
            name = graphName;
            adjList = new List<CsNode<V, E>>();
            gop = new Operation<V, E>();
            startNode = null;
        }
        //----< register an Operation with the graph >-------------------------

        public Operation<V, E> setOperation(Operation<V, E> newOp)
        {
            Operation<V, E> temp = gop;
            gop = newOp;
            return temp;
        }
        //----< add vertex to graph adjacency list >---------------------------

        public void addNode(CsNode<V, E> node)
        {
            adjList.Add(node);
        }
        //----< clear visitation marks to prepare for next walk >--------------

        public void clearMarks()
        {
            foreach (CsNode<V, E> node in adjList)
                node.unmark();
        }
        //----< find Strong conponent based on tarjan alforithm >--------------

        public void SC_finder(CsNode<V, E> node)
        {
            node.t_normalnum = node.min_num = ++dfn_index;
            node_stack.Push(node);
            foreach (var item in node.children)
            {
                if (item.targetNode.t_normalnum < 0)
                {
                    SC_finder(item.targetNode);
                    node.min_num = Math.Min(item.targetNode.min_num, node.min_num);
                }
                else if (node_stack.Contains(item.targetNode))
                {
                    node.min_num = Math.Min(node.min_num, item.targetNode.min_num);
                }
            }
            if(node.t_normalnum == node.min_num)
            {
                SClist.Add("\n  Find a Strong Conponent: ");
                Console.Write("\n    Find a Strong Conponent: ");
                CsNode<V, E> new_node;
                do
                {
                    new_node = node_stack.Pop();
                    SClist.Add(new_node.name + ", ");
                    Console.Write(new_node.name + ", ");
                }
                while (new_node != node);
            }
            
        }

        public void sc_finder()
        {
            foreach(var elem in adjList)
            {
                if (elem.t_normalnum < 0)
                {
                    SC_finder(elem);
                }
            }
        }
        public void gett(Dictionary<string, List<string>> a, List<CsNode<string, string>> node_container)
        {
            foreach (var item in a)
            {
                foreach (var elem in node_container)
                {
                    if (elem.name == item.Key)
                    {
                        foreach (var item_1 in item.Value)
                        {
                            foreach (var elem_1 in node_container)
                            {
                                if (elem_1.name == item_1)
                                {
                                    elem.addChild(elem_1, "node");
                                }
                            }
                        }
                    }
                }
            }

        }

        //----< Generate dependency graph based on dependency table >--------------

        public CsGraph<string, string> Creat_Graph(string[] args){

            DepenAnalysis dep = new DepenAnalysis();
            dep = dep.DepenAnalysises(args);
            CsGraph<string, string> Depen_Graph = new CsGraph<string, string>("Dep_Table");
            CsNode<string, string> Depen_node_one = new CsNode<string, string>("node");
            List<CsNode<string, string>> node_container = new List<CsNode<string, string>>();
            List<string> temp = new List<string>();
            foreach (var files in args){
                if (!temp.Contains(System.IO.Path.GetFileName(files))){
                    temp.Add(System.IO.Path.GetFileName(files));
                }
                else continue;
            }
            foreach (var file in temp){
                CsNode<string, string> nodes =
                    new CsNode<string, string>(file);
                Depen_node_one = nodes;
                node_container.Add(nodes);
            }
            gett(dep.DepTable, node_container);
            foreach (var elem in node_container){
                Depen_Graph.addNode(elem);
            }
            Depen_Graph.startNode = Depen_node_one;
            return Depen_Graph;
        }
        //----< Show Dependency Graph >--------------

        public void showDependencies(){
            foreach (var node in adjList){
                Console.Write("\n ");
                Console.Write("\n  {0}", node.name);
                Console.Write("\n =========================");
                for (int i = 0; i < node.children.Count; ++i){
                    Console.Write("\n    {0}", node.children[i].targetNode.name);
                }
            }
        }



        public void show()
        {
            foreach(var item in SClist)
            {
                Console.WriteLine(item);
            }
        }


        public string SC_tostring()
        {
            string result = "";
            foreach (var item in SClist)
            {
                result = result + item;
            }
            return result;
        }
    }

    /////////////////////////////////////////////////////////////////////////
    // Test class
    class Demo_SC
    {
        static void Main(string[] args)
        {
            FileFinder filef = new FileFinder();
            args = filef.get_cs();
            CsGraph<string, string> csGraph = new CsGraph<string, string>("Dep_Table");
            csGraph = csGraph.Creat_Graph(args);
            csGraph.showDependencies();
            Console.Write("\n\n Strong Conponent Shows Below: ");
            csGraph.sc_finder();
            csGraph.show();
            Console.Write("\n\n");
            Console.ReadKey();
        }
    }
}
