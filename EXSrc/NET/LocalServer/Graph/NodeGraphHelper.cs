using System.Collections.Generic;

/*
  public class C1Config : BoxFileStreamConfig
  {
        public C1Config()
            : base()
        {
            this.EnsureGraph<string>();
        }
  }
  using (var box = db.Cube())
  {
        var graph = box.Graph<string>();
        var memberNode = graph.CreateNode( "Name" );
  }
 */
namespace iBoxDB.LocalServer.Graph
{

    public static class NodeGraphHelper
    {
        public static NodeGraph<K> EnsureGraph<K>(this DatabaseConfig self, string graphName = null)
        {
            var x = new NodeGraph<K>(null, graphName);
            x.EnsureMeta(self);
            return x;
        }

        public static NodeGraph<K> Graph<K>(this IBox self, string graphName = null)
        {
            return new NodeGraph<K>(self, graphName);
        }
    }

    public struct Couple
    {
        public readonly Binder Master;

        internal readonly string DetailTable;
        internal readonly string ForeignKey;
                
        internal Couple(Binder master, string detail, string foreignKey)
        {
            // TODO: Complete member initialization
            this.Master = master;
            this.DetailTable = detail;
            this.ForeignKey = foreignKey;
        }

        private readonly static char[] argFlgs = new char[] { '?', ']' };
        internal static void FixWhere(ref string where, object[] args)
        {
            if (where != null && args != null && args.Length == 1)
            {
                if (where.LastIndexOfAny(argFlgs) < 0)
                {
                    where += '?';
                }
            }
        }


        public IEnumerable<V> GetDetails<V>( 
            string where = null, params object[] args
            )
            where V:class,new()
        {
            var select = "from " + DetailTable + " where " + ForeignKey + "==?"
                          + (where != null ? "&(" + where +")" : "");
            var list = new List<object>();
            list.Add(Master.Key);
            if (args != null && args.Length > 0)
            {
                list.AddRange(args);
            }
            return Master.Box.Select<V>(select, list.ToArray()  );         
        }



    }        
    
    public static class DatabaseServerHalper2
    {

        public static Couple Link(this Binder master, string detail, string foreignKey )
        {
            return new Couple(master, detail, foreignKey);
        }
       
    }


}
