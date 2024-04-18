﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang
{
    public class Program
    {
       private List<Definition> Definitions = new List<Definition>();
        private Expression TopExpression;
        static void Main(string[] args)
        {
            var linesOfCode = new string[]
            {
                "(let y 4)",
                "(let x 7)",
                "(let z 9)",
                "(* x (- z y))"

             };
            try
            {
                var tokens = Lexer.GetTokens(linesOfCode);
                var program = Parse(tokens);
                var rez = program.TopExpression.Evaluate();
                Console.WriteLine(rez + Environment.NewLine);
            } catch (Exception e)
            {
                Console.WriteLine(e + Environment.NewLine);
            } finally
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        public Definition GetDefinition(string tok)
        {
            var def = Definitions.SingleOrDefault(x => (x.Identifier as string) == tok);
            return def;
        }

        static Program Parse(List<Token> tokens)
        {
            var program = new Program();
            program.TopExpression = new Expression(program);
            dynamic current = null;
            foreach (var token in tokens)
            {
                switch (token.Type)
                {
                    case TokenType.Let:
                        if (current != null)
                        {
                            throw new Exception("Invalid syntax, definitions must be of form (let x (expr))");
                        }
                        else
                        {
                            current = new Definition();
                        }
                        break;
                    case TokenType.Identifier:
                        if (current is Expression)
                        {
                            var curExpr = (Expression)current;
                            if (curExpr.LeftOperand == null)
                            {
                                curExpr.LeftOperand = new Operand()
                                {
                                    Identifier = token.Value,
                                    Expression = curExpr
                                };
                            }
                            else if (curExpr.RightOperand == null)
                            {
                                curExpr.RightOperand = new Operand()
                                {
                                    Identifier = token.Value,
                                    Expression = curExpr
                                };
                                current = curExpr.Parent;
                            }
                            else
                            {
                                throw new Exception("Could not recognize { token.Value.ToString() } in expression");
                            }
                        }
                        else if (current is Definition)
                        {
                            var curDef = (Definition)current;
                            if (curDef.Identifier == null)
                            {
                                curDef.Identifier = token.Value;
                            }
                            else
                            {
                                throw new Exception("Invalid identifier { token.Value }");
                            }
                        }
                        break;
                    case TokenType.Number:
                        if (current is Expression)
                        {
                            var curExpr = (Expression)current;
                            if (curExpr.LeftOperand == null)
                            {
                                curExpr.LeftOperand = new Operand()
                                {
                                    Literal = Convert.ToString(token.Value),
                                    Expression = curExpr
                                };
                            }
                            else if (curExpr.RightOperand == null)
                            {
                                curExpr.RightOperand = new Operand()
                                {
                                    Literal = Convert.ToString(token.Value),
                                    Expression = curExpr
                                };
                                current = curExpr.Parent;
                            }
                            else
                            {
                                throw new Exception("Could not recognize { token.Value.ToString() } as number");
                            }
                        }
                        else if (current is Definition)
                        {
                            var curDef = (Definition)current;
                            curDef.Value = int.Parse(token.Value.ToString());
                            program.Definitions.Add(curDef);
                            current = null;
                        }
                        else
                        {
                            throw new Exception("Could not recognize { token.Value.ToString() } as number");
                        }
                        break;
                    case TokenType.Operator:
                        if (current == null)
                        {
                            current = program.TopExpression;
                            (current as Expression).Operator = token.Value;
                        }
                        else if (current is Expression)
                        {
                            var curExp = current as Expression;
                            if (curExp.Operator == null)
                            {
                                curExp = token.Value;
                            }
                            else if (curExp.LeftOperand == null)
                            {
                                curExp.LeftOperand = new Operand()
                                {
                                    Expression = new Expression(program)
                                    {
                                        Operator = token.Value,
                                        Parent = current
                                    }
                                };
                                current = curExp.LeftOperand.Expression;
                            }
                            else if (curExp.RightOperand == null)
                            {
                                curExp.RightOperand = new Operand()
                                {
                                    Expression = new Expression(program)
                                    {
                                        Operator = token.Value,
                                        Parent = current
                                    }
                                };
                                current = curExp.RightOperand.Expression;
                            }
                        }
                        break;
                }
            }
            return program;
        }
    }
  }
