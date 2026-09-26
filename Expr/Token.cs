namespace HaileysSpreadsheats.Expr;

public struct Token
{
    public double NumberValue;
    public string Text;

    public TokenKind Kind;
    public Roll DiceRoll;
}


public enum TokenKind
{
    Plus,
    Minus,
    Mul,
    Div,

    Number, // 123
    Cell, // A1
    DiceRoll, // 1d20
    
    OpenParen,
    CloseParen,
    Cama,

    Constant,
    FnIdentifier,
    
    SKIP, //used by parts of lexer to communicate
    EOF,
}