using System;
using System.Collections.Generic;

namespace HaileysSpreadsheats.Expr;

public class Compiler(AstNode root)
{
    private static void Walk(AstNode n, Action<AstNode> f)
    {
        if (n.Left != null) Walk(n.Left, f);
        if (n.Right != null) Walk(n.Right, f);
        f(n);
    }

    public (List<CompExprOp>? output, string? error) Compile()
    {
        Stack<AstNode> stack = new();
        List<CompExprOp> ret = new();
        Walk(root, stack.Push);

        while (stack.Count > 0)
        {
            var n = stack.Pop();
            switch (n.Kind)
            {
                case AstNodeKind.Number:
                    ret.Add(new CompExprOp { Kind = CompExprOp.Op.PushNumber, Number = n.Value });
                    break;
                case AstNodeKind.DiceRoll:
                    ret.Add(new CompExprOp { Kind = CompExprOp.Op.PushDiceRoll, Roll = n.Roll });
                    break;
                case AstNodeKind.Plus:
                    ret.Add(new CompExprOp { Kind = CompExprOp.Op.Plus });
                    break;
                case AstNodeKind.Minus:
                    ret.Add(new CompExprOp { Kind = CompExprOp.Op.Minus });
                    break;
                case AstNodeKind.Mul:
                    ret.Add(new CompExprOp { Kind = CompExprOp.Op.Mul });
                    break;
                case AstNodeKind.Div:
                    ret.Add(new CompExprOp { Kind = CompExprOp.Op.Div });
                    break;
                
                default:
                    return (null, "Unknown token kind");
            }
        }

        ret.Reverse();
        return (ret, null);
    }
}