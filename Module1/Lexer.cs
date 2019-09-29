using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Policy;

namespace Lexer
{

    public class LexerException : System.Exception
    {
        public LexerException(string msg)
            : base(msg)
        {
        }

    }

    public class Lexer
    {

        protected int position;
        protected char currentCh; // очередной считанный символ
        protected int currentCharValue; // целое значение очередного считанного символа
        protected System.IO.StringReader inputReader;
        protected string inputString;

        public Lexer(string input)
        {
            inputReader = new System.IO.StringReader(input);
            inputString = input;
        }

        public void Error()
        {
            System.Text.StringBuilder o = new System.Text.StringBuilder();
            o.Append(inputString + '\n');
            o.Append(new System.String(' ', position - 1) + "^\n");
            o.AppendFormat("Error in symbol {0}", currentCh);
            throw new LexerException(o.ToString());
        }

        protected void NextCh()
        {
            this.currentCharValue = this.inputReader.Read();
            this.currentCh = (char) currentCharValue;
            this.position += 1;
        }

        public virtual bool Parse()
        {
            return true;
        }
    }

    public class IntLexer : Lexer
    {

        protected System.Text.StringBuilder intString;
        public int parseResult = 0;

        public IntLexer(string input)
            : base(input)
        {
            intString = new System.Text.StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();
            if (currentCh == '+' || currentCh == '-')
            {
                intString.Append(currentCh);
                NextCh();
            }

            if (char.IsDigit(currentCh))
            {
                intString.Append(currentCh);
                NextCh();
            }
            else
            {
                Error();
            }

            while (char.IsDigit(currentCh))
            {
                intString.Append(currentCh);
                NextCh();
            }


            if (currentCharValue != -1)
            {
                Error();
            }

            parseResult = int.Parse(intString.ToString());
            return true;

        }
    }

    public class IdentLexer : Lexer
    {
        private string parseResult;
        protected StringBuilder builder;

        public string ParseResult
        {
            get { return parseResult; }
        }

        public IdentLexer(string input) : base(input)
        {
            builder = new StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();
            if (char.IsLetter(currentCh))
            {
                builder.Append(currentCh);
                NextCh();
            }
            else
            {
                Error();
            }

            while (char.IsDigit(currentCh) || char.IsLetter(currentCh) || currentCh == '_')
            {
                builder.Append(currentCh);
                NextCh();
            }

            if (currentCharValue != -1)
            {
                Error();
            }

            parseResult = builder.ToString();
            return true;
        }

    }

    public class IntNoZeroLexer : IntLexer
    {
        public IntNoZeroLexer(string input)
            : base(input)
        {
        }

        public override bool Parse()
        {
             NextCh();
            if (currentCh == '+' || currentCh == '-')
            {
                NextCh();
            }

            if (char.IsDigit(currentCh) && currentCh != '0')
            {
                NextCh();
            }
            else
            {
                Error();
            }

            while (char.IsDigit(currentCh))
            {
                NextCh();
            }

            if (currentCharValue != -1)
            {
                Error();
            }

            return true;
        }
    }

    public class LetterDigitLexer : Lexer
    {
        protected StringBuilder builder;
        protected string parseResult;
        private char prev;

        public string ParseResult
        {
            get { return parseResult; }
        }

        public LetterDigitLexer(string input)
            : base(input)
        {
            builder = new StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();
            if (char.IsLetter(currentCh))
            {
                prev = currentCh;
                NextCh();
            }
            else
            {
                Error();
            }

            while (char.IsDigit(currentCh) && char.IsLetter(prev) || char.IsLetter(currentCh) && char.IsDigit(prev))
            {
                prev = currentCh;
                NextCh();
            }

            if (currentCharValue != -1)
            {
                Error();
            }
            parseResult = builder.ToString();
            return true;
        }

    }

    public class LetterListLexer : Lexer
    {
        protected List<char> parseResult;
        private char prev;

        public List<char> ParseResult
        {
            get { return parseResult; }
        }

        public LetterListLexer(string input)
            : base(input)
        {
            prev = ',';
            parseResult = new List<char>();
        }

        public override bool Parse()
        {
            NextCh();
            while ((char.IsLetter(currentCh) && (prev == ',' || prev == ';')) || ((currentCh == ',' || currentCh == ';') && char.IsLetter(prev)))
            {
                prev = currentCh;
                if (char.IsLetter(currentCh))
                {
                    parseResult.Add(currentCh);
                }
                NextCh();
            }

            if (currentCharValue != -1 || prev == ',' || prev == ';')
            {
                Error();
            }

            foreach (var c in parseResult)
            {
                System.Console.Write(c + ' ');
            }

            return true;
        }
    }

    public class DigitListLexer : Lexer
    {
        protected List<int> parseResult;
        private char prev;

        public List<int> ParseResult
        {
            get { return parseResult; }
        }

        public DigitListLexer(string input)
            : base(input)
        {
            prev = ' ';
            parseResult = new List<int>();
        }

        public override bool Parse()
        {
            NextCh();
            if (currentCharValue == -1)
            {
                Error();
            }

            if (char.IsDigit(currentCh))
            {
                prev = currentCh;
                parseResult.Add(int.Parse(currentCh.ToString()));
                NextCh();
            }
            else
            {
                Error();
            }

            while (char.IsDigit(currentCh) && prev == ' ' || (currentCh == ' '))
            {
                prev = currentCh;
                if (char.IsDigit(currentCh))
                {
                    parseResult.Add(int.Parse(currentCh.ToString()));
                }
                NextCh();
            }

            if (currentCharValue != -1 || prev == ' ')
            {
                Error();
            }

            foreach (var d in parseResult)
            {
                System.Console.Write(d + ' ');
            }
            return true;
        }
    }

    public class LetterDigitGroupLexer : Lexer
    {
        protected StringBuilder builder;
        protected string parseResult;

        public string ParseResult
        {
            get { return parseResult; }
        }

        public LetterDigitGroupLexer(string input)
            : base(input)
        {
            builder = new StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();
            if (currentCharValue == -1)
            {
                Error();
            }

            if (!char.IsLetter(currentCh))
            {
                Error();
            }

            while (builder.Length < 2 ||
                   char.IsDigit(currentCh) && (char.IsLetter(builder[builder.Length - 1]) || char.IsLetter(builder[builder.Length - 2])) ||
                   char.IsLetter(currentCh) && (char.IsDigit(builder[builder.Length - 1]) || char.IsDigit(builder[builder.Length - 2])))
            {
                builder.Append(currentCh);
                NextCh();
            }

            if (currentCharValue != -1)
            {
                Error();
            }

            parseResult = builder.ToString();
            return true;
        }

    }

    public class DoubleLexer : Lexer
    {
        private StringBuilder builder;
        private double parseResult;

        public double ParseResult
        {
            get { return parseResult; }

        }

        public DoubleLexer(string input)
            : base(input)
        {
            builder = new StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();

            if (char.IsDigit(currentCh))
            {
                builder.Append(currentCh);
                NextCh();
            }
            else
            {
                Error();
            }

            while (char.IsDigit(currentCh))
            {
                builder.Append(currentCh);
                NextCh();
            }

            if (currentCh == '.')
            {
                builder.Append(currentCh);
                NextCh();
            }
            else if (currentCharValue == -1)
            {
                parseResult = double.Parse(builder.ToString());
                return true;
            }
            else
            {
                Error();
            }

            if (currentCharValue == -1)
            {
                Error();
            }

            while (char.IsDigit(currentCh))
            {
                builder.Append(currentCh);
                NextCh();
            }

            if (currentCharValue != -1)
            {
                Error();
            }

            parseResult = double.Parse(builder.ToString(), System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }

    }

    public class StringLexer : Lexer
    {
        private StringBuilder builder;
        private string parseResult;

        public string ParseResult
        {
            get { return parseResult; }

        }

        public StringLexer(string input)
            : base(input)
        {
            builder = new StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();
            if (currentCh == '\'')
            {
                builder.Append(currentCh);
                NextCh();
            }
            else
            {
                Error();
            }

            if (currentCharValue == -1)
            {
                Error();
            }

            while (currentCh != '\'')
            {
                builder.Append(currentCh);
                NextCh();
            }

            builder.Append(currentCh);
            NextCh();

            if (currentCharValue != -1)
            {
                Error();
            }

            parseResult = builder.ToString();
            return true;
        }
    }

    public class CommentLexer : Lexer
    {
        private StringBuilder builder;
        private string parseResult;
        private char prev;

        public string ParseResult
        {
            get { return parseResult; }

        }

        public CommentLexer(string input)
            : base(input)
        {
            prev = ' ';
            builder = new StringBuilder();
        }

        public override bool Parse()
        {
            NextCh();
            if (currentCh == '/')
            {
                builder.Append(currentCh);
                NextCh();
            }
            else
            {
                Error();
            }

            if (currentCh == '*')
            {
                builder.Append(currentCh);
                NextCh();
            }
            else
            {
                Error();
            }

            if (currentCharValue == -1)
            {
                Error();
            }

            while (!((prev == '*') && (currentCh == '/')))
            {
                prev = currentCh;
                builder.Append(currentCh);
                NextCh();
                if (currentCharValue == -1)
                {
                    Error();
                }

            }

            builder.Append(currentCh);
            NextCh();

            if (currentCharValue != -1)
            {
                Error();
            }

            parseResult = builder.ToString();
            return true;
        }
    }

    public class IdentChainLexer : Lexer
    {
        private StringBuilder builder;
        private List<string> parseResult;

        public List<string> ParseResult
        {
            get { return parseResult; }

        }

        public IdentChainLexer(string input)
            : base(input)
        {
            builder = new StringBuilder();
            parseResult = new List<string>();
        }

        public override bool Parse()
        {
            NextCh();

            while (true)
            {
                var temp = "";

                if (char.IsLetter(currentCh))
                {
                    temp += currentCh;
                    NextCh();
                }
                else
                {
                    Error();
                }

                while (char.IsDigit(currentCh) || char.IsLetter(currentCh) || currentCh == '_')
                {
                    temp += currentCh;
                    NextCh();
                }

                if (currentCh == '.')
                {
                    parseResult.Add(builder.ToString());
                    builder = new StringBuilder();
                    NextCh();
                }
                else
                {
                    break;
                }
            }

            if (currentCharValue != -1)
            {
                Error();
            }

            return true;
        }
    }

    public class Program
    {
        public static void Main()
        {
            string input = "123.4";
            Lexer L = new DoubleLexer(input);
            try
            {
                L.Parse();
            }
            catch (LexerException e)
            {
                System.Console.WriteLine(e.Message);
            }

        }
    }
}
