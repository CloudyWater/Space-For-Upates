using iBoxDB.LocalServer;
using System.Text;
using System.Collections.Generic;
using iBoxDB.LocalServer.Helper;
using System;
using System.Dynamic;
using iBoxDB.LocalServer.Replication;
using iBoxDB.Compatible;
using System.IO;
/*
  var select = box.QueryFrom( "Product" )
           .Where( "GameType==?GT")
            .Select();
   foreach (var p in select.SetArg("?GT", "ACT").Cast<Product>())
   {                               
   }    
*/
namespace iBoxDB.Query
{

    public static class BoxQueryHelper
    {


        public static SQueryFrom QueryFrom(this IBox box, string tableName)
        {
            return new SQueryFrom(box, tableName);
        }

        #region HC
        public class InFun : IFunction
        {
            private Array values;
            public InFun(Array _values)
            {
                this.values = _values;
            }
            public object Execute(int argCount, object[] args)
            {
                var v = args[0];
                for (var i = 0; i < values.Length; i++)
                {
                    var e = values.GetValue(i);
                    if (Object.Equals(v, e))
                    {
                        return true;
                    }
                }
                return false;
            }
            public override string ToString()
            {
                return " in " + ObjectStringHelper.ToObjectString(values);
            }
        }
        public static bool InOptimize(string columnName, Array values, out string condition, out object[] args)
        {
            if (values == null || values.Length < 1)
            {
                condition = "?==?";
                args = new object[] { 1, 2, 3 };
                return false;
            }
            Array.Sort(values);

            condition = "(" + columnName + ">=? & ?>=" + columnName + "&[" + columnName + "])";
            args = new object[] { values.GetValue(0), values.GetValue(values.Length - 1), new InFun(values) };
            return true;
        }

        public struct SQueryFrom
        {

            internal readonly IBox box;
            internal readonly StringBuilder sb;

            internal object[] args;
            internal List<string> argNames;

            internal bool isSet;

            internal SQueryFrom(SQueryFrom self)
            {
                box = self.box;
                sb = new StringBuilder(self.sb.ToString());
                args = self.args;
                argNames = self.argNames;
                isSet = self.isSet;
            }

            internal SQueryFrom(IBox box, string tableName)
            {
                this.isSet = false;
                this.args = null;
                this.argNames = null;
                this.box = box;
                sb = new StringBuilder();
                sb.Append(BoxSelect.keies[0])
                 .Append(tableName);
            }
            internal void ProcessCondition()
            {
                if (isSet) { return; }
                if (argNames != null) { return; }

                if (argNames == null) { argNames = new List<string>(); }
                var ql = sb.ToString() + " ";
                StringBuilder pname = null;
                for (var i = 0; i < ql.Length; i++)
                {
                    var c = ql[i];
                    switch (c)
                    {
                        case '?':
                            if (pname != null)
                            {
                                throw new ArgumentException(pname.ToString());
                            }
                            else
                            {
                                pname = new StringBuilder("?");
                            }
                            break;
                        case ' ':
                        case '[':
                        case ']':
                        case '&':
                        case '|':
                        case '(':
                        case ')':
                        case '>':
                        case '<':
                        case '=':
                        case '!':
                            if (pname != null)
                            {
                                argNames.Add(pname.ToString());
                                pname = null;
                            }
                            break;
                        default:
                            if (pname != null)
                            {
                                pname.Append(c);
                            }
                            break;
                    }
                }
            }

            public SQueryWhere Where(string condition, params object[] _args)
            {
                return new SQueryWhere(this, condition, _args);
            }

            public SQueryWhere WhereForIn(string condition, Array values)
            {
                condition = condition.Trim();
                string name = condition;

                var pos = condition.IndexOf(' ');
                if (pos > 0)
                {
                    name = condition.Substring(0, pos);
                }
                object[] args;
                InOptimize(name, values, out condition, out args);
                return Where(condition, args);
            }


            public SQueryOrderBy OrderBy(string condition, bool ascending, params object[] _args)
            {
                return new SQueryOrderBy(this, condition, ascending, _args);
            }

            public SQueryLimit Limit(int start, int length = -1)
            {
                return new SQueryLimit(this, start, length);
            }

            public BoxSelect Select()
            {
                return new BoxSelect(this);
            }
        }

        public struct SQueryWhere
        {
            internal readonly SQueryFrom from;
            internal SQueryWhere(SQueryFrom self, string condition, params object[] _args)
            {
                this.from = self;
                from.sb.Append(BoxSelect.keies[1])
                    .Append(condition);

                if (_args != null && _args.Length > 0)
                {
                    from.isSet = true;
                    from.args = _args;
                }
            }

            public SQueryOrderBy OrderBy(string condition, bool ascending, params object[] _args)
            {
                return new SQueryOrderBy(from, condition, ascending, _args);
            }

            public SQueryLimit Limit(int start, int length = -1)
            {
                return new SQueryLimit(from, start, length);
            }

            public BoxSelect Select()
            {
                return new BoxSelect(from);
            }
        }

        public struct SQueryOrderBy
        {
            internal readonly SQueryFrom from;

            internal SQueryOrderBy(SQueryFrom self, string condition, bool ascending,
                params object[] _args)
                : this(self, true, condition, ascending, _args)
            {
            }
            private SQueryOrderBy(SQueryFrom self, bool first, string condition, bool ascending,
                 params object[] _args)
            {
                this.from = self;
                if (first)
                {
                    from.sb.Append(BoxSelect.keies[2]);
                }
                else
                {
                    from.sb.Append(',');
                }
                from.sb.Append(condition);
                if (!ascending)
                {
                    from.sb.Append(BoxSelect.desc);
                }
                if (_args != null && _args.Length > 0)
                {
                    from.isSet = true;
                    if (from.args == null)
                    {
                        from.args = _args;
                    }
                    else
                    {
                        var t = new List<object>(from.args);
                        t.AddRange(_args);
                        from.args = t.ToArray();
                    }
                }
            }

            public SQueryOrderBy OrderBy(string condition, bool ascending, params object[] _args)
            {
                return new SQueryOrderBy(from, false, condition, ascending, _args);
            }

            public SQueryLimit Limit(int start, int length = -1)
            {
                return new SQueryLimit(from, start, length);
            }

            public BoxSelect Select()
            {
                return new BoxSelect(from);
            }
        }

        public struct SQueryLimit
        {
            internal readonly SQueryFrom from;
            public SQueryLimit(SQueryFrom self, int start, int length)
            {
                this.from = self;
                from.sb.Append(BoxSelect.keies[3])
                    .Append(start)
                    .Append(',')
                    .Append(length);
            }
            public BoxSelect Select()
            {
                return new BoxSelect(from);
            }
        }
        #endregion
    }

    #region BoxSelect
    public struct BoxSelect
    {

        internal static string[] keies = new string[] { "from ", " where ", " order by ", " limit " };
        internal static string desc = " desc";

        internal BoxQueryHelper.SQueryFrom srcfrom;
        public BoxSelect(BoxQueryHelper.SQueryFrom self)
        {
            this.srcfrom = self;
            this.srcfrom.ProcessCondition();
        }

        public BoxSelect SetArg<V>(string name, V v)
        {
            if (this.srcfrom.isSet)
            {
                throw new ArgumentException("SET");
            }
            var from = new BoxQueryHelper.SQueryFrom(srcfrom);

            if (name[0] != '?')
            {
                name = "?" + name;
            }
            var argNames = from.argNames;

            if (from.args == null)
            {
                from.args = new object[argNames.Count];
            }
            bool setted = false;
            for (var i = 0; i < argNames.Count; i++)
            {
                if (name == argNames[i])
                {
                    setted = true;
                    from.args[i] = v;
                }
            }
            if (!setted)
            {
                throw new ArgumentException(name + " " + ObjectStringHelper.ToObjectString(argNames));
            }

            return new BoxSelect(from);
        }


        public override string ToString()
        {
            return GetQL() + "\r\n  : " + ObjectStringHelper.ToObjectString(srcfrom.args);
        }

        public string GetIndexName()
        {
            IBox bx = (IBox)srcfrom.box;
            return bx.Services().GetIndexName(GetQL());
        }
        public string GetQL()
        {
            return srcfrom.sb.ToString();
        }
        #region CAST
        public IEnumerable<V> Cast<V>()
             where V : class,new()
        {
            var ql = GetQL();
            return srcfrom.box.Select<V>(ql,
                srcfrom.args);
        }
        public IEnumerable<Local> CastReadonly()
        {
            return Cast<Local>();
        }
        public IEnumerable<object> CastIndex()
        {
            return Cast<object>();
        }
        public IEnumerable<BEntity> CastDelay()
        {
            return Cast<BEntity>();
        }

        public IEnumerable<ExpandoObject> CastDynamic()
        {
            return Cast<ExpandoObject>();
        }
        #endregion
    }
    #endregion

}
