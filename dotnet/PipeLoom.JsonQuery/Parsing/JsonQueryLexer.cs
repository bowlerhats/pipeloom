using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PipeLoom.JsonQuery.Parsing;


// AI generated


/// <summary>
/// Token kinds produced by <see cref="JsonQueryLexer"/>.
///
/// Deliberately simpler/flatter than JsonQuery.Net's own JsonQueryTokenType:
/// we don't distinguish PropertyName vs PropertyPath at the lexer level (both
/// are just a run of Dot+Segment pairs to us — the parser decides how many
/// segments constitute the property access), and we don't need a FunctionName
/// token distinct from Ident, since validating "is this a known function name"
/// is explicitly NOT this lexer's job (unlike JsonQuery.Net's reader, which
/// consults JsonQueryableRegistry mid-tokenization — see design note below).
/// </summary>
public enum TokType
{
    Eof,
    Ident,        // bareword: function name, unquoted property segment, or keyword operator (and/or/in/not)
    Dot,          // .
    QuotedString, // "..." — used both as a value and as a quoted property segment
    Number,
    True,
    False,
    Null,
    LParen,       // (
    RParen,       // )
    LBrace,       // {
    RBrace,       // }
    LBracket,     // [
    RBracket,     // ]
    Comma,        // ,
    Colon,        // :
    Pipe,         // |
    Op,           // symbolic operator: == != >= <= > < + - * / % ^
}

internal readonly struct Token
{
    public TokType Type { get; }
    public string Text { get; }          // raw text (identifier, operator symbol, etc.)
    public string? StringValue { get; }  // decoded string contents, only for QuotedString
    public decimal NumberValue { get; }  // decimal, not double — matches how numeric literals
                                          // are actually meant to be compared/stored for a query
                                          // language operating over JSON numbers; avoids binary
                                          // floating-point surprises for everyday literals like
                                          // 0.1, 18, 2.5 that a data pipeline will compare/filter on.
    public int Position { get; }

    public Token(TokType type, string text, int position, string? stringValue = null, decimal numberValue = 0)
    {
        this.Type = type;
        this.Text = text;
        this.Position = position;
        this.StringValue = stringValue;
        this.NumberValue = numberValue;
    }

    public override string ToString() => $"{this.Type}:'{this.Text}'@{this.Position}";
}

/// <summary>
/// Hand-written lexer for the JsonQuery text format
/// (https://jsonquerylang.org/docs/#text-format).
///
/// Fully self-contained: no dependency on JsonQuery.Net, no reflection, no
/// external libraries beyond BCL string/number parsing. Safe under Native AOT.
///
/// DESIGN NOTE — why this does NOT validate function/operator names against a
/// registry, unlike JsonQuery.Net's own JsonQueryReader:
///   JsonQuery.Net's reader calls JsonQueryableRegistry.TryGetQueryableType(name)
///   while tokenizing, and throws if the function name isn't registered. That
///   couples tokenization to a mutable, reflection-populated registry (see the
///   earlier finding: JsonQueryableRegistry entries are constructed via
///   Activator.CreateInstance on runtime-closed generics, which is what breaks
///   under Native AOT). This lexer makes NO such judgement — every bareword is
///   just an Ident token, and it is entirely up to the PARSER (or a later stage
///   of your pipeline) to decide whether "foo" in `foo(.x)` is a valid, known
///   function. This keeps the lexer itself simple, dependency-free, and
///   trivially extensible to custom functions/operators without touching any
///   registry at all.
/// </summary>
internal sealed class JsonQueryLexer
{
    private readonly string _s;
    private int _pos;

    // Multi-char operators must be tried longest-first so e.g. ">=" isn't
    // mis-lexed as ">" followed by a dangling "=".
    private static readonly string[] SymbolicOperators =
    {
        "==", "!=", ">=", "<=", ">", "<", "+", "-", "*", "/", "%", "^"
    };

    public JsonQueryLexer(string source)
    {
        _s = source ?? throw new ArgumentNullException(nameof(source));
        _pos = 0;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        Token t;
        do
        {
            t = this.NextToken();
            tokens.Add(t);
        } while (t.Type != TokType.Eof);

        return tokens;
    }

    private Token NextToken()
    {
        this.SkipWhitespace();

        if (_pos >= _s.Length)
            return new Token(TokType.Eof, string.Empty, _pos);

        int start = _pos;
        char c = _s[_pos];

        switch (c)
        {
            case '.': _pos++; return new Token(TokType.Dot, ".", start);
            case '(': _pos++; return new Token(TokType.LParen, "(", start);
            case ')': _pos++; return new Token(TokType.RParen, ")", start);
            case '{': _pos++; return new Token(TokType.LBrace, "{", start);
            case '}': _pos++; return new Token(TokType.RBrace, "}", start);
            case '[': _pos++; return new Token(TokType.LBracket, "[", start);
            case ']': _pos++; return new Token(TokType.RBracket, "]", start);
            case ',': _pos++; return new Token(TokType.Comma, ",", start);
            case ':': _pos++; return new Token(TokType.Colon, ":", start);
            case '|': _pos++; return new Token(TokType.Pipe, "|", start);
            case '"': return this.ReadQuotedString();
        }

        // Numbers are always lexed WITHOUT consuming a leading sign here — '-'
        // (and '+') are tokenized separately as Op("-")/Op("+") regardless of
        // context. Whether a leading '-' means "unary negate" or "binary
        // subtract" is resolved in the PARSER (JsonQueryParser.ParseUnary),
        // which combines a leading Op("-") with the following primary. This
        // keeps the lexer itself simple and fully context-free — it never has
        // to look at what came before to decide how to tokenize what comes next,
        // which is exactly the category of statefulness that caused the bug we
        // found in JsonQuery.Net's own reader (an operator-tracking stack that
        // could desync across certain token sequences). This lexer has no such
        // cross-call state at all: tokenizing is a pure function of position.
        if (char.IsDigit(c))
            return this.ReadNumber();

        if (IsIdentStart(c))
            return this.ReadIdentOrKeyword();

        foreach (var op in SymbolicOperators)
        {
            if (this.Matches(op))
            {
                _pos += op.Length;
                return new Token(TokType.Op, op, start);
            }
        }

        throw new JsonQueryParseException($"Unexpected character '{c}'", start);
    }

    private bool IsDigitAt(int i) => i < _s.Length && char.IsDigit(_s[i]);

    private bool Matches(string op)
    {
        if (_pos + op.Length > _s.Length) return false;
        for (int i = 0; i < op.Length; i++)
            if (_s[_pos + i] != op[i]) return false;
        return true;
    }

    private void SkipWhitespace()
    {
        while (_pos < _s.Length && char.IsWhiteSpace(_s[_pos]))
            _pos++;
    }

    private static bool IsIdentStart(char c) => char.IsLetter(c) || c == '_' || c == '$';
    private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c == '_' || c == '$';

    private Token ReadIdentOrKeyword()
    {
        int start = _pos;
        while (_pos < _s.Length && IsIdentPart(_s[_pos]))
            _pos++;

        string text = _s.Substring(start, _pos - start);

        return text switch
        {
            "true" => new Token(TokType.True, text, start),
            "false" => new Token(TokType.False, text, start),
            "null" => new Token(TokType.Null, text, start),
            // "and" / "or" / "in" / "not" are recognized as plain Ident here;
            // the PARSER decides (based on position/context) whether they act
            // as operators or "not in" forms a compound operator. Keeping them
            // as Ident in the lexer keeps the lexer simple and context-free —
            // see the design note on the class for why we deliberately don't
            // validate/special-case function or operator names during lexing.
            _ => new Token(TokType.Ident, text, start),
        };
    }

    private Token ReadNumber()
    {
        int start = _pos;
        while (_pos < _s.Length && char.IsDigit(_s[_pos])) _pos++;

        if (_pos < _s.Length && _s[_pos] == '.' && this.IsDigitAt(_pos + 1))
        {
            _pos++;
            while (_pos < _s.Length && char.IsDigit(_s[_pos])) _pos++;
        }

        if (_pos < _s.Length && (_s[_pos] == 'e' || _s[_pos] == 'E'))
        {
            int expStart = _pos;
            _pos++;
            if (_pos < _s.Length && (_s[_pos] == '+' || _s[_pos] == '-')) _pos++;
            if (this.IsDigitAt(_pos))
            {
                while (_pos < _s.Length && char.IsDigit(_s[_pos])) _pos++;
            }
            else
            {
                _pos = expStart; // not actually an exponent, back off
            }
        }

        string text = _s.Substring(start, _pos - start);

        // decimal.Parse over the exact substring, same rationale as storing
        // Token.NumberValue as decimal: avoids binary floating-point rounding
        // for ordinary query literals. NumberStyles.Float allows the optional
        // exponent part we may have just scanned. No AllowLeadingSign here
        // since '+'/'-' are never consumed as part of the number by this lexer
        // (see NextToken's comment above).
        decimal value = decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
        return new Token(TokType.Number, text, start, numberValue: value);
    }

    private Token ReadQuotedString()
    {
        int start = _pos;
        _pos++; // consume opening quote
        var sb = new StringBuilder();

        while (true)
        {
            if (_pos >= _s.Length)
                throw new JsonQueryParseException("Unterminated string literal", start);

            char c = _s[_pos];
            if (c == '"')
            {
                _pos++;
                break;
            }

            if (c == '\\')
            {
                _pos++;
                if (_pos >= _s.Length)
                    throw new JsonQueryParseException("Unterminated escape sequence", start);

                char esc = _s[_pos];
                switch (esc)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u':
                        if (_pos + 4 >= _s.Length)
                            throw new JsonQueryParseException("Invalid unicode escape", _pos);
                        string hex = _s.Substring(_pos + 1, 4);
                        sb.Append((char)ushort.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                        _pos += 4;
                        break;
                    default:
                        throw new JsonQueryParseException($"Invalid escape sequence '\\{esc}'", _pos);
                }
                _pos++;
            }
            else
            {
                sb.Append(c);
                _pos++;
            }
        }

        string raw = _s.Substring(start, _pos - start);
        return new Token(TokType.QuotedString, raw, start, stringValue: sb.ToString());
    }
}

internal sealed class JsonQueryParseException : Exception
{
    public int Position { get; }

    public JsonQueryParseException(string message, int position)
        : base($"{message} (at position {position})")
    {
        this.Position = position;
    }
}