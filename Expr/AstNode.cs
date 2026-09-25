using System;
using System.Globalization;

namespace HaileysSpreadsheats.Expr;

public class AstNode
{
    public TokenKind Kind;
    public double Value;
    public AstNode Left;
    public AstNode Right;
    public Roll Roll;

    public override string ToString()
    {
        return Kind switch
        {
            TokenKind.Number => Value.ToString(CultureInfo.InvariantCulture),
            TokenKind.Plus => $"({Left} + {Right})",
            TokenKind.Minus => $"({Left} - {Right})",
            TokenKind.Mul => $"({Left} * {Right})",
            TokenKind.Div => $"({Left} / {Right})",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}