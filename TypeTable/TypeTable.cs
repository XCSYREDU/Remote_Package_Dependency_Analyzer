/////////////////////////////////////////////////////////////////////////////
//  Typetable.cs -- Typetable detect type name from files and store them   //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project3                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: Typetable.cs created by Dr.Jim Fawcett                         //
/////////////////////////////////////////////////////////////////////////////
/*
 *   Module Operations
 *   -----------------
 *   This module defines the following class:
 *   TypeTable  - creat a container for typetable and get typetable from
 *   semiexpression
 *   BuildCodeAnalyzer - capture constructs defined by rules
 *   TestType  -  contains testtub
 *   
 *    * Public Interface
 * ======================
 * TypeTable show()                        // show the TypeTable
 * TypeTable getTypeTable()      // get back TypeTable for later using
 * TypeTable  add()   // add item into typetable
 *  TypeTable  add(Type type, File file, Namespace ns , T_name t_Name) // add item into typetable
 */
/*
 *   Build Process
 *   -------------
 *   - Required files: parser.cs RulesAndActions.cs Display.cs Semi.cs IRuleandAction.cs 
 *     ITokenCollection.cs Element.cs FileMgr.cs
 *     
 * 
 *   Maintenance History
 *   -------------------
 *   ver 1.0 : 29 October 2018
 *     - first release
 *     
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Lexer;
using CodeAnalysis;
using FileUtilities;


namespace TypeTables
{
    using File = String;
    using Type = String;
    using Namespace = String;
    using T_name = String;
    /////////////////////////////////////////////////////////
    // struct to store basic information for this type

    public struct TypeItem
    {
        public File file;
        public Namespace namesp;
        public T_name typename;
    }
    /////////////////////////////////////////////////////////
    // TypeTable Class implement a container to store type 
    // name, type type name, file name and name space name

    public class TypeTable 
    {
        
        public Dictionary<File, List<TypeItem>> table { get; set; } =
          new Dictionary<File, List<TypeItem>>();
        //--------<add(Type type, TypeItem ti) implement opreation to add type into table>-----

        public void add(Type type, TypeItem ti)
        {
            if (table.ContainsKey(type))
                table[type].Add(ti);
            else
            {
                List<TypeItem> temp = new List<TypeItem>();
                temp.Add(ti);
                table.Add(type, temp);
            }
        }
        //--------<add(Type type, TypeItem ti) implement opreation to add everything we need into table>-------

        public void add(Type type, File file, Namespace ns , T_name t_Name){
            TypeItem temp;
            temp.file = file;
            temp.namesp = ns;
            temp.typename = t_Name;
            add(type, temp);
        }
        //--------<show function implement opreation to show everything inside table>-------

        public void show(){
            foreach (var elem in table){
                Console.Write("\n  {0}", elem.Key);
                Console.WriteLine();
                foreach (var item in elem.Value){
                    //Console.WriteLine(elem);
                    Console.Write("\n    [{0}, {1} , {2}]", item.file, item.namesp , item.typename);
                }
            }
            Console.Write("\n");
        }

        public string tt_to_string(){
            string result = "";
            result = result + "\n TypeTable: \n";
            foreach (var elem in table){
                result = result + elem.Key;
                result = result + "\n";
                foreach (var value in elem.Value){
                    result = result + "\n [ " + value.file + " , " + value.namesp + " , " + value.typename + " ] ";
                }
                result = result + "\n====================================\n";
            }
            return result;
        }

        public List<string> print(){
            List<string> print = new List<string>();
            print.Add("TypeTable");
            foreach (var ele in table){
                print.Add(ele.Key);
                foreach (var item in ele.Value){
                    string temp = "";
                    temp = "[" + " " + item.typename + " , " + item.file + " , " + item.namesp + " " + "]";
                    print.Add(temp);
                }
            }
            return print;
        }

        //--------<GetTable implement opreation to get a typetable from semi and retrun a whole typetable>-----

        public TypeTable GetTable(string[] args){
            TestType tp = new TestType();
            TypeTable tt = new TypeTable();
            //FileFinder filef = new FileFinder();
            //args = filef.get_cs();
            foreach(var item in args){
                //Console.WriteLine(item);
            }
            foreach (string file in args){
                ITokenCollection semiexp = Factory.create();
                if (!semiexp.open(file as string)){
                    Console.Write("\n  Can't open {0}\n\n", args[0]);
                    break;
                }
                BuildCodeAnalyzers builder = new BuildCodeAnalyzers(semiexp);
                Parser parser = builder.build();
                try
                {
                    while (semiexp.get().Count > 0)
                        parser.parse(semiexp);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                string name = "";
                foreach (Elem e in table){
                    if (e.type == "namespace"){
                        name = e.name;
                    }
                    else{
                        if (e.type == "Alias" || e.type == "function") continue;
                        
                        tt.add(e.name, System.IO.Path.GetFileName(file), name, e.type);
                        //Console.WriteLine("Pass this route");
                    }
                }
                semiexp.close();
            }
            Console.WriteLine(tt.table.Count);
            return tt;
        }
    }
    
    public class Parsers
    {
        private List<CodeAnalysis.IRule> Rules;

        public Parsers()
        {
            Rules = new List<CodeAnalysis.IRule>();
        }
        public void add(CodeAnalysis.IRule rule)
        {
            Rules.Add(rule);
        }
        public void parse(Lexer.ITokenCollection semi)
        {
            // Note: rule returns true to tell parser to stop
            //       processing the current semiExp

            Display.displaySemiString(semi.ToString());

            foreach (CodeAnalysis.IRule rule in Rules)
            {
                if (rule.test(semi))
                    break;
            }
        }
    }

    public class BuildCodeAnalyzers
    {
        Repository repos = new Repository();

        public BuildCodeAnalyzers(Lexer.ITokenCollection semi)
        {
            repos.semi = semi;
        }
        public virtual Parser build()
        {
            Parser parser = new Parser();

            // decide what to show
            AAction.displaySemi = false;
            AAction.displayStack = false;  // false is default

            // action used for namespaces, classes, and functions
            PushStack push = new PushStack(repos);

            // capture namespace info
            DetectNamespace detectNS = new DetectNamespace();
            detectNS.add(push);
            parser.add(detectNS);

            // capture class info
            DetectClass detectCl = new DetectClass();
            detectCl.add(push);
            parser.add(detectCl);

            // capture delegate info
            DetectDelegate detectDE = new DetectDelegate();
            SaveDeclar print_1 = new SaveDeclar(repos);
            detectDE.add(print_1);
            parser.add(detectDE);

            // capture DetectAlias info
            DetectAlias detectAL = new DetectAlias();
            SaveDeclar print_2 = new SaveDeclar(repos);
            detectAL.add(print_2);
            parser.add(detectAL);

            // parser configured
            return parser;
        }
    }

    public class TestType
    {
        //----< Test Stub >--------------------------------------------------
#if (TEST_TypeTable)

        static void Main(string[] args)
        {
            FileFinder filef = new FileFinder();
            args = filef.get_cs();
            TypeTable tt = new TypeTable();
            tt = tt.GetTable(args);
            //Console.WriteLine(tt.table.Count);
            tt.show();
            Console.Write("\n\n");
            Console.ReadKey();
        }
#endif
    }
}
