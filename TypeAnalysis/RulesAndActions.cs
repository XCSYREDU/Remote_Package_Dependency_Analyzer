﻿/////////////////////////////////////////////////////////////////////////////
//  RulesAndActions.cs - Parser rules specific to an application           //
//  ver 1.0                                                                //
//  Language:     C#, VS 2017                                              //
//  Platform:     AlienWare 17R5, WIN10, Visual Studio 2017                //
//  Application:  CSE681 Project3                                          //
//  Author:       Xiao Chen, Computer Engineering Syracuse University      //
//                (315) 560-7375, xchen149@syr.edu                         //
//  Source: RulesAndActions.cs created by Dr.Jim Fawcett                   //
/////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 * 
 * public interface:
 * Repository Repository()    //provides all code access to Repository
 * Repository getInstance()   //provides all code access to Repository
 * lineCount()    //semi gets line count from toker who counts lines
 * PushStack PushStack()   // pushes scope info on stack when entering new scope
 * PushStack doAction()     
 * PopStack PopStack()
 * PopStack doAction()
 * PrintFunction public PrintFunction(Repository repo)
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Element.cs, ScopeStack.cs,
 *   Semi.cs, Display.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * 
 * ver 1.0 : 29 October 2018
 * - first release
 * ver 1.1 : 1 November 2018
 * - add Rules to detect Aliases
 *
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Lexer;

namespace CodeAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // Repository class
    // - Specific to each application
    // - holds results of processing
    // - ScopeStack holds current state of scope processing
    // - List<Elem> holds start and end line numbers for each scope
    ///////////////////////////////////////////////////////////////////

    public class Repository
    {
        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();

        static Repository instance;

        public Repository()
        {
            instance = this;
        }

        //----< provides all code access to Repository >-------------------

        public static Repository getInstance()
        {
            return instance;
        }

        //----< provides all actions access to current semiExp >-----------

        public ITokenCollection semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount(); }
        }
        public int prevLineCount  // not used in this demo
        {
            get;
            set;
        }

        //----< enables recursively tracking entry and exit from scopes >--

        public int scopeCount
        {
            get;
            set;
        }

        public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
        {
            get { return stack_; }
        }

        // the locations table is the result returned by parser's actions
        // in this demo

        public List<Elem> locations
        {
            get { return locations_; }
            set { locations_ = value; }
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Define Actions
    ///////////////////////////////////////////////////////////////////
    // - PushStack
    // - PopStack
    // - PrintFunction
    // - PrintSemi
    // - SaveDeclar

    ///////////////////////////////////////////////////////////////////
    // pushes scope info on stack when entering new scope
    // - pushes element with type and name onto stack
    // - records starting line number

    public class PushStack : AAction
    {
        public PushStack(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action PushStack");
            ++repo_.scopeCount;
            Elem elem = new Elem();
            elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
            elem.name = semi[1];     // expects name
            elem.beginLine = repo_.semi.lineCount() - 1;
            elem.endLine = 0;        // will be set by PopStack action
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = 0;  // will be set by PopStack action
            repo_.stack.push(elem);

            // display processing details if requested

            if (AAction.displayStack)
                repo_.stack.display();
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }

            // add starting location if namespace, type, or function

            if (elem.type == "control" || elem.name == "anonymous")
                return;
            repo_.locations.Add(elem);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope
    // - records end line number and scope count

    public class PopStack : AAction
    {
        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action SaveDeclar");
            Elem elem;
            try
            {
                // if stack is empty (shouldn't be) pop() will throw exception

                elem = repo_.stack.pop();

                // record ending line count and scope level

                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {
                            if ((repo_.locations[i]).endLine == 0)
                            {
                                (repo_.locations[i]).endLine = repo_.semi.lineCount();
                                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                                break;
                            }
                        }
                    }
                }
            }
            catch
            {
                return;
            }

            if (AAction.displaySemi)
            {
                Lexer.ITokenCollection local = Factory.create();
                local.add(elem.type).add(elem.name);
                if (local[0] == "control")
                    return;

                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount());
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }
    ///////////////////////////////////////////////////////////////////
    // action to print function signatures - not used in demo

    public class PrintFunction : AAction
    {
        public PrintFunction(Repository repo)
        {
            repo_ = repo;
        }
        public override void display(Lexer.ITokenCollection semi)
        {
            Console.Write("\n    line# {0}", repo_.semi.lineCount() - 1);
            Console.Write("\n    ");
            for (int i = 0; i < semi.size(); ++i)
            {
                if (semi[i] != "\n")
                    Console.Write("{0} ", semi[i]);
            }
        }
        public override void doAction(ITokenCollection semi)
        {
            this.display(semi);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // ITokenCollection printing action, useful for debugging

    public class PrintSemi : AAction
    {
        public PrintSemi(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Console.Write("\n  line# {0}", repo_.semi.lineCount() - 1);
            this.display(semi);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // display public declaration

    public class SaveDeclar : AAction
    {
        public SaveDeclar(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Display.displayActions(actionDelegate, "action SaveDeclar");
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.beginLine = repo_.lineCount;
            elem.endLine = elem.beginLine;
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = elem.beginScopeCount;
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            repo_.locations.Add(elem);
        }
    }
    ///////////////////////////////////////////////////////////////////
    // Define Rules
    ///////////////////////////////////////////////////////////////////
    // - DetectNamespace
    // - DetectClass
    // - DetectFunction
    // - DetectAnonymousScope
    // - DetectPublicDeclaration
    // - DetectLeavingScope

    ///////////////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectNamespace");
            int index;
            semi.find("namespace", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }


    ///////////////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectClass");
            int indexCL;
            semi.find("class", out indexCL);
            int indexIF;
            semi.find("interface", out indexIF);
            int indexST;
            semi.find("struct", out indexST);
            int indexEN;
            semi.find("enum", out indexEN);
            
            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);
            
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // local semiExp with tokens for type and name
                
                local.add(semi[index]).add(semi[index + 1]);
                
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect Delegate definitions

    public class DetectDelegate : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectDelegate");
            int indexDE;
            semi.find("delegate", out indexDE);

            int index = indexDE;
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // local semiExp with tokens for type and name
                semi.find("(", out index);
                local.add(semi[index-3]).add(semi[index - 1]);
                
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect Delegate definitions

    public class DetectAlias : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectAlias");
            int index;
            semi.find("using", out index);
            if (index != -1)
            {
                semi.find("=", out index);
                if (index != -1)
                {
                    ITokenCollection local = Factory.create();
                    local.add("Alias").add(semi[index - 1]);
                    doActions(local);
                    return true;
                }
                return false;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to dectect function definitions

    public class DetectFunction : ARule
    {
        public static bool isSpecialToken(string token)
        {
            string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
            foreach (string stoken in SpecialToken)
                if (stoken == token)
                    return true;
            return false;
        }
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectFunction");
            if (semi[semi.size() - 1] != "{")
                return false;

            int index;
            semi.find("(", out index);
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                ITokenCollection local = Factory.create();
                local.add("function").add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those

    public class DetectAnonymousScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
            int index;
            semi.find("{", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add("control").add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect public declaration

    public class DetectPublicDeclar : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectPublicDeclar");
            int index;
            semi.find(";", out index);
            if (index != -1)
            {
                semi.find("public", out index);
                if (index == -1)
                    return true;
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                //local.displayNewLines = false;
                local.add("public " + semi[index + 1]).add(semi[index + 2]);

                semi.find("=", out index);
                if (index != -1)
                {
                    doActions(local);
                    return true;
                }
                semi.find("(", out index);
                if (index == -1)
                {
                    doActions(local);
                    return true;
                }
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
            int index;
            semi.find("}", out index);
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }
}

