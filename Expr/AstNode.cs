using System;
using System.Globalization;

namespace HaileysSpreadsheats.Expr;

public enum AstNodeKind
{
    Number,
    DiceRoll,
    Plus,
    Minus,
    Mul,
    Div,
    FnCall
}

public class AstNode
{
    public AstNodeKind Kind;

    public AstNode Left = null!;
    public AstNode Right = null!;
    public double Value;
    public Roll Roll;

    public AstNode Arg = null!;
    public string Name = null!;

    public AstNode()
    {
    }

    public AstNode(AstNodeKind kind, AstNode left, AstNode right)
    {
        Kind = kind;
        Left = left;
        Right = right;
    }

    public AstNode(AstNodeKind valueKind, double value)
    {
        Kind = valueKind;
        Value = value;
    }

    public AstNode(AstNodeKind rollKind, Roll tkDiceRoll)
    {
        Roll = tkDiceRoll;
        Kind = rollKind;
    }

    public override string ToString() => Kind switch
    {
        AstNodeKind.Number => Value.ToString(CultureInfo.InvariantCulture),
        AstNodeKind.Plus => $"({Left} + {Right})",
        AstNodeKind.Minus => $"({Left} - {Right})",
        AstNodeKind.Mul => $"({Left} * {Right})",
        AstNodeKind.Div => $"({Left} / {Right})",
        AstNodeKind.FnCall => $"{Name}({Arg})", // sus there is no left or right usage
        _ => throw new ArgumentOutOfRangeException()
    };

    public static AstNode CreateFrom(TokenKind tkKind, AstNode left, AstNode right) => tkKind switch
    {
        TokenKind.Plus => new AstNode(AstNodeKind.Plus, left, right),
        TokenKind.Minus => new(AstNodeKind.Minus, left, right),
        TokenKind.Mul => new AstNode(AstNodeKind.Mul, left, right),
        TokenKind.Div => new(AstNodeKind.Div, left, right),
        _ => throw new ArgumentOutOfRangeException(nameof(tkKind), $"Unknown tkKind {tkKind}")
    };

    public static AstNode NewFn(string tkText, AstNode inner)
    {
        return new AstNode()
        {
            Name = tkText, Arg = inner, Kind = AstNodeKind.FnCall
        };
    }
}