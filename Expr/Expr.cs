using System;
using System.Collections.Generic;

namespace HaileysSpreadsheats.Expr;

public struct CompiledExpression(string expr, List<CompExprOp> ops)
{
    public readonly string Expr = expr;
    public readonly List<CompExprOp> Ops = ops;
}

public struct CompExprOp
{
    public enum Op
    {
        Plus,
        Minus,
        Mul,
        Div,

        PushNumber,
        PushDiceRoll
    }

    public Op Kind;
    public double Number;
    public Roll Roll;

    public override string ToString()
    {
        return Kind switch
        {
            Op.Plus => $"+",
            Op.Minus => $"-",
            Op.Mul => $"*",
            Op.Div => $"/",
            Op.PushNumber => $"PUSH {Number}",
            Op.PushDiceRoll => $"ROLL AND PUSH {Roll}",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public static class CompExpr
{

    public delegate double RollDelegate(Roll spec);

    public static double Evaluate(CompiledExpression expr,  RollDelegate roll)
    {
        Stack<double> stack = new();
        foreach (var op in expr.Ops)
        {
            switch (op.Kind)
            {
                case CompExprOp.Op.Plus:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a + b);
                    break;
                }
                case CompExprOp.Op.Minus:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a - b);
                    break;
                }
                case CompExprOp.Op.Mul:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a * b);
                    break;
                }
                case CompExprOp.Op.Div:
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(a / b);
                    break;
                }
                case CompExprOp.Op.PushNumber:
                    stack.Push(op.Number);
                    break;
                case CompExprOp.Op.PushDiceRoll:
                    stack.Push(roll(op.Roll));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return stack.Pop();
    }

    public static CompiledExpression FromString(string expr)
    {
        Lexer l = new(expr);
        Parser p = new(l);
        AstNode root = p.GetRoot();
        Compiler c = new(root);
        var res = c.Compile();
        if (res.error != null) throw new Exception(res.error);
        return new CompiledExpression(expr, res.output!);
    }
}