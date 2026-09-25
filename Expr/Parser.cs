using System;
using System.Globalization;

namespace HaileysSpreadsheats.Expr;

public class Parser
{
    private Lexer _l;
    public Parser(Lexer l) => _l = l;

    public static int LeftBindingPower(TokenKind tk) => tk switch
    {
        TokenKind.Number => 0,
        TokenKind.Cell => 0,
        TokenKind.DiceRoll => 0,
        TokenKind.Plus => 2,
        TokenKind.Minus => 2,
        TokenKind.Mul => 3,
        TokenKind.Div => 3,
        TokenKind.OpenParen => 1,
        TokenKind.CloseParen => -1,
        TokenKind.SKIP => -1,
        TokenKind.EOF => -1,
        _ => throw new ArgumentOutOfRangeException(nameof(tk), tk, null)
    };

    public AstNode GetRoot()
    {
        return Parse(0);
    }

    private AstNode Parse(int limit)
    {
        var tk = _l.Consume();
        var left = Nud(tk);

        while (LeftBindingPower(_l.Peek().Kind) > limit)
        {
            tk = _l.Consume();
            left = Led(tk, left);
        }

        return left;
    }


    private AstNode Led(Token tk, AstNode left)
    {
        var right = Parse(LeftBindingPower(tk.Kind));
        return new AstNode()
        {
            Kind = tk.Kind,
            Left = left,
            Right = right
        };
    }

    private static double GetConstantValue(string text) => text.ToLower(CultureInfo.InvariantCulture) switch
    {
        "false" => double.NegativeZero, //hehe any negative is false
        "true" => 0, // its positive?
        "pi" => Math.PI,
        "e" => Math.E,
        "tau" => Math.Tau,
        _ => throw new Exception($"Invalid Constant '{text}'")
    };

    private AstNode Nud(Token tk)
    {
        return tk.Kind switch
        {
            TokenKind.Number => new AstNode() { Kind = TokenKind.Number, Value = tk.NumberValue },
            TokenKind.DiceRoll => new AstNode() { Kind = TokenKind.DiceRoll, Roll = tk.DiceRoll },
            TokenKind.OpenParen => ParseOpenParen(),
            TokenKind.Constant => new AstNode() { Kind = TokenKind.Constant, Value = GetConstantValue(tk.Text) },
            TokenKind.FnIdentifier => ParseFnCall(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private AstNode ParseFnCall()
    {
        var inner = Parse(0);
        var next = _l.Consume();
        // while next == , parse(0)
        if (next.Kind != TokenKind.CloseParen) throw new Exception($"Expected ) but got {next.Kind}");        
        
        
    }

    private AstNode ParseOpenParen()
    {
        var inner = Parse(0);
        var next = _l.Consume();
        if (next.Kind != TokenKind.CloseParen) throw new Exception($"Expected ) but got {next.Kind}");
        return inner;
    }
}