using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HaileysSpreadsheats.Expr;

public class Lexer
{
    delegate Token TokenHandlerFunc(Lexer l, Match m);

    struct TokenHandler
    {
        public Regex Regex;
        public TokenHandlerFunc Handler;
    };

    private readonly List<Token> _tokens = new();

    private readonly List<TokenHandler> _tokenHandlers =
    [
        new()
        {
            Handler = HandleRandomCellValue,
            Regex = new Regex(@"(?<num>[0-9]+)d(?<sides>[0-9]+)", RegexOptions.IgnoreCase)
        },
        new() { Handler = HandleNumber, Regex = new Regex("[0-9]+(\\.[0-9]+)?", RegexOptions.IgnoreCase) },
        // new() { Handler = HandleCellRange, Regex = new Regex("[A-Z]+[0-9]+:[A-Z]+[0-9]+", RegexOptions.IgnoreCase) }, // A1:B2
        new() { Handler = SkipWhiteSpace, Regex = new Regex("\\s+", RegexOptions.IgnoreCase) },
        new() { Handler = HandleSymbP, Regex = new Regex("\\+", RegexOptions.IgnoreCase) },
        new() { Handler = HandleSymbM, Regex = new Regex("-", RegexOptions.IgnoreCase) },
        new() { Handler = HandleSymbD, Regex = new Regex("/", RegexOptions.IgnoreCase) },
        new() { Handler = HandleSymbMu, Regex = new Regex("\\*", RegexOptions.IgnoreCase) },
    ];

    private static Token HandleRandomCellValue(Lexer l, Match m)
    {
        string mv = m.Groups[0].Captures[0].Value;
        string s = l.vss.Read(mv.Length);
        return new Token
        {
            Kind = TokenKind.DiceRoll,
            DiceRoll = new Roll(s)
        };
    }

  

    private static Token SkipWhiteSpace(Lexer l, Match m)
    {
        while (char.IsWhiteSpace(l.vss.Current[0]))
        {
            l.vss.Skip(1);
        }


        return new Token() { Kind = TokenKind.SKIP };
    }

    private static Token HandleNumber(Lexer l, Match m)
    {
        string mv = m.Groups[0].Captures[0].Value;
        string s = l.vss.Read(mv.Length);
        return new Token()
        {
            Kind = TokenKind.Number,
            NumberValue = Double.Parse(s)
        };
    }

    private static Token HandleSymbP(Lexer l, Match m) => HandleSymb(l, m, TokenKind.Plus);
    private static Token HandleSymbM(Lexer l, Match m) => HandleSymb(l, m, TokenKind.Minus);
    private static Token HandleSymbD(Lexer l, Match m) => HandleSymb(l, m, TokenKind.Div);
    private static Token HandleSymbMu(Lexer l, Match m) => HandleSymb(l, m, TokenKind.Mul);

    private static Token HandleSymb(Lexer lexer, Match match, TokenKind tk)
    {
        lexer.vss.Skip(1);
        return new Token() { Kind = tk };
    }

    private readonly ViewableStringStreamer vss;

    private Token Lex()
    {
        RESET:
        Token ret = new();
        bool found = false;
        foreach (var handler in _tokenHandlers)
        {
            var match = handler.Regex.Match(vss.Current);
            if (match.Success && match.Groups[0].Captures[0].Index == 0)
            {
                // var caller = handler.Handler.GetMethodInfo().Name;
                ret = handler.Handler(this, match);
                found = true;
                if (ret.Kind == TokenKind.SKIP)
                    goto RESET;

                break;
            }
        }

        if (found) return ret;
        throw new Exception("Invalid token");
    }

    public Lexer(string expr)
    {
        vss = new(expr);

        while (!string.IsNullOrWhiteSpace(vss.Current))
        {
            var tok = Lex();
            _tokens.Add(tok);
        }

        _tokens.Add(new Token { Kind = TokenKind.EOF });
    }

    public int Pos { get; set; }

    public Token Peek() => _tokens[Pos];
    public bool More() => _tokens.Count > Pos;

    public Token Consume()
    {
        var pk = Peek();
        Pos += 1;
        return pk;
    }
}

public struct Roll
{
    public int Sides;
    public int Rolls;

    public override string ToString() => $"{Rolls}d{Sides}";

    public Roll(string s)
    {
        var parts = s.Split('d');
        Rolls = int.Parse(parts[0]);
        Sides = int.Parse(parts[1]);
    }
}