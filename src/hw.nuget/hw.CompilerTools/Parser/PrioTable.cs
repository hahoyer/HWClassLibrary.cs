using System;
using System.Collections.Generic;
using System.Data;
using hw.Helper;
using System.Linq;
using hw.Forms;
using JetBrains.Annotations;

namespace hw.Parser
{
    /// <summary>
    ///     Priority table used in parsing to create the syntax tree.
    /// </summary>
    public sealed class PrioTable
    {
        public string Title;
        public const string Any = "(any)";
        public const string EndOfText = "(eot)";
        public const string BeginOfText = "(bot)";
        public const string Error = "(err)";

        readonly string[] _token;
        readonly ValueCache<char[,]> _dataCache;

        public override bool Equals(object obj)
        {
            var x = obj as PrioTable;
            if(x == null)
                return false;
            return _token == x._token && _dataCache == x._dataCache;
        }

        public static bool operator ==(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return ReferenceEquals(y, null);
            return x.Equals(y);
        }

        public override int GetHashCode()
        {
            return _token.GetHashCode() + _dataCache.GetHashCode();
        }

        public static bool operator !=(PrioTable x, PrioTable y)
        {
            if(ReferenceEquals(x, null))
                return !ReferenceEquals(y, null);
            return !x.Equals(y);
        }

        /// <summary>
        ///     shows the table in table form.
        ///     The characters have the following meaning:
        ///     Plus: New token is higher the found token,
        ///     Minus: Found token is higher than new token
        ///     Equal sign: New token and found token are matching
        /// </summary>
        /// <returns> </returns>
        public string Dump()
        {
            var maxlen = _token.Max(t => t.Length);
            var head0 = "";
            head0 = head0.PadLeft(maxlen);
            head0 += "    ";
            var head1 = head0;
            var result = "";
            for(var i = 0; i < _token.Length; i++)
            {
                var ii = Convert.ToString(i + 10000);
                head0 += ii[3];
                head1 += ii[4];
                result += _token[i].PadLeft(maxlen) + " " + ii.Substring(3, 2) + " ";
                for(var j = 0; j < Length; j++)
                    result += _dataCache.Value[i, j];
                result += "\n";
            }


            result = head0 + "\n" + head1 + "\n" + result;
            if(Title != null)
                result = Title + "\n" + result;
            return result;
        }

        public override string ToString() { return Title ?? Dump(); }

        public static PrioTable FromText(string text) { return FromText(text.Split('\n', '\r')); }

        static PrioTable FromText(string[] text)
        {
            return FromText(text.Select(l => l.Split(' ', '\t')).ToArray());
        }

        static PrioTable FromText(string[][] text)
        {
            var result = Left(Any);
            foreach(var line in text)
            {
                var data = line.Skip(1).ToArray();
                var tokenCount = data.Length / 2;
                switch(line[0].ToLowerInvariant())
                {
                    case "left":
                        result += Left(data);
                        break;
                    case "right":
                        result += Right(data);
                        break;
                    case "parlevel":
                        result = result.ParenthesisLevelLeft
                            (
                                data.Take(tokenCount).ToArray(),
                                data.Skip(tokenCount).Take(tokenCount).ToArray());
                        break;
                    case "-":
                    case "+":
                    case "=":
                        result.Correct(data[0], data[1], line[0][0]);
                        break;
                }
            }

            result = result.ParenthesisLevel(BeginOfText, EndOfText);
            result.Sort();
            return result;
        }

        char[,] AllocData()
        {
            var data = new char[Length, Length];
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                    data[i, j] = ' ';
            return data;
        }
        static string[] AllocTokens(params string[][] tokenArrayList)
        {
            return tokenArrayList
                .SelectMany(tokenArray => tokenArray)
                .ToArray();
        }

        /// <summary>
        ///     Returns number of token in table
        /// </summary>
        int Length { get { return _token.Length; } }

        static int FindGroup(int i, params string[][] x)
        {
            for(var j = 0; j < x.Length; j++)
            {
                i -= x[j].Length;
                if(i < 0)
                    return j;
            }
            return x.Length;
        }

        PrioTable() { _dataCache = new ValueCache<char[,]>(AllocData); }

        PrioTable(char data, string[] token)
            : this()
        {
            _token = AllocTokens(token);
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                    _dataCache.Value[i, j] = data;
        }

        PrioTable(PrioTable x, PrioTable y)
            : this()
        {
            _token = AllocTokens(x._token, y._token);
            for(var i = 0; i < Length; i++)
                if(i < x.Length)
                    for(var j = 0; j < Length; j++)
                    {
                        _dataCache.Value[i, j] = '+';
                        if(j < x.Length)
                            _dataCache.Value[i, j] = x._dataCache.Value[i, j];
                    }
                else
                    for(var j = 0; j < Length; j++)
                    {
                        _dataCache.Value[i, j] = '-';
                        if(j >= x.Length)
                            _dataCache.Value[i, j] = y._dataCache.Value[i - x.Length, j - x.Length];
                    }
        }

        PrioTable(PrioTable x, string[] data, string[] left, string[] right)
            : this()
        {
            _token = AllocTokens(left, x._token, right);
            for(var i = 0; i < Length; i++)
                for(var j = 0; j < Length; j++)
                    _dataCache.Value[i, j] = PrioChar(x, data, left, i, j);
        }

        static char PrioChar(PrioTable x, string[] data, string[] left, int i, int j)
        {
            var iGroup = FindGroup(i, left, x._token);
            var jGroup = FindGroup(j, left, x._token);

            if(iGroup == 1 && jGroup == 1)
                return x._dataCache.Value[i - left.Length, j - left.Length];

            if(iGroup == 2 && jGroup == 0)
                switch(Math.Sign(left.Length + x.Length - i + j))
                {
                    case -1:
                        return '-';
                    case 0:
                        return '=';
                    case 1:
                        return '+';
                    default:
                        throw new InvalidExpressionException();
                }
            return data[iGroup][jGroup];
        }
        /// <summary>
        /// </summary>
        //     Obtain the index in token list. 
        //     Empty string is considered as "end" in angle brackets. 
        //     If name is not found the entry "common" in angle brackets is used
        int Index(string name)
        {
            for(var i = 0; i < Length; i++)
                if(_token[i] == name)
                    return (i);

            for(var i = 0; i < Length; i++)
                if(_token[i] == Any)
                    return (i);

            throw new NotImplementedException("missing " + Any + " entry in priority table");
        }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are left associative
        /// </summary>
        /// <param name="x"> </param>
        /// <returns> </returns>
        public static PrioTable Left(params string[] x)
        {
            return new PrioTable('-', x);
        }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are right associative
        /// </summary>
        /// <param name="x"> </param>
        /// <returns> </returns>
        public static PrioTable Right(params string[] x)
        {
            return new PrioTable('+', x);
        }

        /// <summary>
        ///     Define a prio table with tokens that have the same priority and are like a list
        /// </summary>
        /// <param name="x"> </param>
        /// <returns> </returns>
        public static PrioTable List(params string[] x)
        {
            return new PrioTable('=', x);
        }

        /// <summary>
        ///     Define a prio table that adds a parenthesis level.
        ///     LToken and RToken should have the same number of elements.
        ///     Elements of these lists that have the same index are considered as matching
        /// </summary>
        /// <param name="data">
        ///     contains a 3 by 3 character table.
        ///     The characters have the following meaning:
        ///     0,0: left TokenClass finds left TokenClass;
        ///     0,1: left TokenClass finds TokenClass defined so far;
        ///     0,2: left TokenClass finds right TokenClass;
        ///     1,0: TokenClass defined so far finds left TokenClass;
        ///     1,1: ignored, Table for already defined Tokens used (use '?');
        ///     1,2: TokenClass defined so far finds right TokenClass;
        ///     2,0: ignored, "-=+"-Table generated (use '?');
        ///     2,1: right TokenClass finds TokenClass defined so far;
        ///     2,2: right TokenClass finds right TokenClass
        /// </param>
        /// <param name="lToken"> list of strings that play the role of left parenthesis </param>
        /// <param name="rToken"> list of strings that play the role of right parenthesis </param>
        /// <returns> </returns>
        public PrioTable Level(string[] data, string[] lToken, string[] rToken)
        {
            return new PrioTable(this, data, lToken, rToken);
        }

        static readonly string[] _parenthesisTableLeft =
        {
            "++-",
            "+?-",
            "?--"
        };

        static readonly string[] _parenthesisTableRight =
        {
            "+++",
            "+?+",
            "?--"
        };

        public PrioTable ParenthesisLevelLeft(string[] lToken, string[] rToken)
        {
            return Level(_parenthesisTableLeft, lToken, rToken);
        }
        public PrioTable ParenthesisLevelRight(string[] lToken, string[] rToken)
        {
            return Level(_parenthesisTableRight, lToken, rToken);
        }
        public PrioTable ParenthesisLevel(string lToken, string rToken)
        {
            return Level(_parenthesisTableLeft, new[] {lToken}, new[] {rToken});
        }

        public static PrioTable operator +(PrioTable x, PrioTable y) { return new PrioTable(x, y); }

        /// <summary>
        ///     Combines two prioritity tables. The tokens contained in left operand are considered as higher priority operands
        /// </summary>
        /// <param name="x"> higher priority tokens </param>
        /// <param name="y"> lower priority tokens </param>
        /// <returns> </returns>
        [UsedImplicitly]
        public static PrioTable Add(PrioTable x, PrioTable y)
        {
            return new PrioTable(x, y);
        }

        /// <summary>
        ///     Manual correction of table entries
        /// </summary>
        /// <param name="n"> </param>
        /// <param name="t"> </param>
        /// <param name="d"> </param>
        [UsedImplicitly]
        public void Correct(string n, string t, char d)
        {
            _dataCache.Value[Index(n), Index(t)] = d;
        }

        /// <summary>
        ///     List of names, without the special tokens "frame", "end" and "else" in angle brackets
        /// </summary>
        [UsedImplicitly]
        public string[] GetNameList()
        {
            var result = new string[NormalNameLength()];
            var k = 0;
            for(var i = 0; i < Length; i++)
                if(IsNormalName(_token[i]))
                    result[k++] = _token[i];
            return result;
        }

        int NormalNameLength()
        {
            var n = 0;
            for(var i = 0; i < Length; i++)
                if(IsNormalName(_token[i]))
                    n++;
            return n;
        }

        static bool IsNormalName(string name)
        {
            return name != BeginOfText && name != EndOfText && name != Any && name != Error;
        }

        /// <summary>
        ///     Returns the priority information of a pair of tokens
        ///     The characters have the following meaning:
        ///     Plus: New token is higher the recent token,
        ///     Minus: Recent token is higher than new token
        ///     Equal sign: New token and recent token are matching
        /// </summary>
        /// <param name="newTokenName"> </param>
        /// <param name="recentTokenName"> </param>
        /// <returns> </returns>
        public char Relation(string newTokenName, string recentTokenName)
        {
            if(newTokenName == BeginOfText || recentTokenName == EndOfText)
                return ' ';

            //Tracer.FlaggedLine("\"" + _token[New] + "\" on \"" + _token[recentToken] + "\" --> \"" + _dataCache[New, recentToken] + "\"");
            return _dataCache.Value[Index(newTokenName), Index(recentTokenName)];
        }

        //For debug only
        [Node]
        public string[] Token { get { return _token; } }

        public IEnumerable<string> NewToken { get { return _token.Where(t => t != BeginOfText); } }
        public IEnumerable<string> RecentToken { get { return _token.Where(t => t != EndOfText); } }

        //For debug only
        [Node]
        public string[] Data
        {
            get
            {
                var result = new string[_dataCache.Value.GetLength(0)];
                for(var i = 0; i < _dataCache.Value.GetLength(0); i++)
                {
                    result[i] = "";
                    for(var j = 0; j < _dataCache.Value.GetLength(1); j++)
                        result[i] += _dataCache.Value[i, j];
                }
                return result;
            }
        }

        void Sort()
        {
            while(SortOne())
                continue;
        }

        bool SortOne()
        {
            var comparer = new PrioComparer(this);
            var newOrder = Length.Select().OrderBy(d => d, comparer);
            var toDo = newOrder.Select
                (
                    (iOld, iNew) => new
                    {
                        iOld,
                        iNew
                    }).FirstOrDefault(x => x.iOld != x.iNew);
            if(toDo == null)
                return false;

            Exchange(toDo.iOld, toDo.iNew);
            return true;
        }

        void Exchange(int iOld, int iNew)
        {
            Exchange(ref _token[iOld], ref _token[iNew]);
            var data = _dataCache.Value;
            var olddata = data.Clone();
            for(var i = 0; i < Length; i++)
                Exchange(ref data[i, iOld], ref data[i, iNew]);
            for(var i = 0; i < Length; i++)
                Exchange(ref data[iOld, i], ref data[iNew, i]);
        }

        static void Exchange<T>(ref T iOld, ref T iNew)
        {
            var t = iOld;
            iOld = iNew;
            iNew = t;
        }


        sealed class PrioComparer : Comparer<int>
        {
            readonly PrioTable _parent;
            public PrioComparer(PrioTable parent) { _parent = parent; }
            public override int Compare(int x, int y)
            {
                var result = _parent.NoPluses(x).CompareTo(_parent.NoPluses(y));
                if(result != 0)
                    return result;
                result = _parent.NoMinuses(x).CompareTo(_parent.NoMinuses(y));
                if(result != 0)
                    return result;
                return String.Compare(_parent.Token[x], _parent.Token[y], StringComparison.Ordinal);
            }
        }

        int NoPluses(int x) { return Data[x].Count(d => d != '+'); }
        int NoMinuses(int x) { return Data[x].Count(d => d != '-'); }
    }
}