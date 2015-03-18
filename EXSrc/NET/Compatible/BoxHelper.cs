using System;
using System.Collections.Generic;
using iBoxDB.LocalServer;
using iBoxDB.LocalServer.Helper;
using iBoxDB.LocalServer.Replication;

namespace iBoxDB.Compatible
{
    public static class BoxHelper
    {
        public static ABinder Bind(this AutoBox abox, string table, params object[] key)
        {
            return new ABinder(abox, table, key);
        }

        public static V Find<V>(this AutoBox abox, String table, params Object[] key) where V : class, new()
        {
            return abox.SelectKey<V>(table, key);
        }

        public static Dictionary<string, object> Find(this AutoBox abox, String table, params Object[] key)
        {
            var t = abox.SelectKey(table, key);
            return t != null ? t.Clone() : null;
        }

        public static bool Insert<V>(this IBox self, string tableName, V v) where V : class
        {
            return ((ILocalBox)self).Insert(tableName, v);
        }
        public static bool Update<K, V>(this IBox self, string tableName, K k, V v)
            where V : class
            where K : class
        {
            return ((ILocalBox)self).Update(tableName, k, v);
        }
        public static bool UpdateNoIndex<V>(this IBox self, string tableName, V v)
            where V : class
        {
            return ((ILocalBox)self).Update(tableName, v, v);
        }
        public static bool UpdateNoIndex<V>(this Binder self, V v)
          where V : class
        {
            return self.Update(v);
        }
        public static bool UpdateNoIndex<V>(this ABinder self, V v)
          where V : class
        {
            return self.Update(v);
        }

        public static bool UpdateNoIndex<V>(this AutoBox self, string tableName, V v)
          where V : class
        {
            return self.Update(tableName, v);
        }
        public static bool ReplaceNoIndex<V>(this AutoBox self, string tableName, V v)
          where V : class
        {
            return self.Replace(tableName, v);
        }


        public static bool Delete<K>(this IBox self, string tableName, K k)
            where K : class
        {
            return ((ILocalBox)self).Delete(tableName, k);
        }

        public static ILocalBox Services(this IBox box)
        {
            return (ILocalBox)box;
        }
        public static IBoxRecycler GetBoxRecycler(this IDatabase db)
        {
            return ((ILocalDatabase)db).GetBoxRecycler();
        }
        public static int NewIntId(this IBox self, byte name, int step)
        {
            return (int)self.NewId(name, step);
        }

        public static IEnumerable<V> BSelect<V>(this IBox self, string QL, params object[] args)
            where V : class,new()
        {
            return self.Select<V>(QL, args);
        }

        // LocalDictionary is inner object , for readonly purpose
        public static IEnumerable<Local> BSelect(this IBox self, string QL, params object[] args)
        {
            return self.Select<Local>(QL, args);
        }

        public static Binder<K> BindProperty<K>(this IBox self, string tableName, K k) where K : class
        {
            return new Binder<K>((ILocalBox)self, tableName, k);
        }
        public static object BGetValue(this Dictionary<string, object> self, string name)
        {
            return DictionaryHelper.BGetValue(self, name);
        }


    }
}
