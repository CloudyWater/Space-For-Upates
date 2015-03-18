using iBoxDB.LocalServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace iBoxDB.Compatible
{

    public class ABinder
    {
        public readonly AutoBox Box;
        public readonly string TableName;
        public readonly object[] Key;

        public ABinder(AutoBox abox, string table, object[] key)
        {
            this.Box = abox;
            this.TableName = table;
            this.Key = key;
        }

        public override string ToString()
        {
            if (Key != null && Key.Length == 0) { return null; }
            var ld = Select();
            if (ld == null) { return null; }
            return DB.ToString(ld);
        }

        public Local Select(bool keep = false)
        {
            return Select<Local>(keep);
        }

        public V Select<V>(bool keep = false) where V : class,new()
        {
            return Box.SelectKey<V>(TableName, Key);
        }

        public bool Insert<V>(V v) where V : class
        {
            return Box.Insert(TableName, v);
        }
        public bool Update<V>(V v) where V : class
        {
            return Box.Update(TableName, Key, v);
        }

        public bool Delete()
        {
            return Box.Delete(TableName, Key);
        }
    }


}
