using System;

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

    private AstNode Nud(Token tk)
    {
        return tk.Kind switch
        {
            TokenKind.Number => new AstNode() { Kind = TokenKind.Number, Value = tk.NumberValue },
            TokenKind.DiceRoll => new AstNode() { Kind = TokenKind.DiceRoll, Roll = tk.DiceRoll },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}