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

    public List<CompExprOp> Compile()
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
                case AstNodeKind.FnCall:
                {
                    ret.Add(new CompExprOp() { Kind = CompExprOp.Op.Call, FnName = n.Name });
                    var aComp = new Compiler(n.Arg);
                    var compile = aComp.Compile();
                    for (int i = compile.Count - 1; i >= 0; i--) ret.AddRange(compile[i]);
                    break;
                }
                default: throw new Exception($"Unknown token {n.Kind}");
            }
        }

        ret.Reverse();
        return ret;
    }
}