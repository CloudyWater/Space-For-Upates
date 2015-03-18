using System;
using System.Collections.Generic;
using iBoxDB.Compatible;


namespace iBoxDB.LocalServer.Graph
{

    public interface INodeElement
    {
        object this[string name]
        {
            get;
            set;
        }
        bool Save();
        bool Delete();
    }
    public interface INode<K> : INodeElement
    {
        K ID { get; }
        IEnumerable<INodeRelationship<K>> GetRelationships(string where = null, params object[] args);

        INodeRelationship<K> GetRelationship(K rNodeID);
        INodeRelationship<K> CreateRelationship(K rNodeID);
    }
    public static class INodeKHelper
    {
        public static INodeRelationship<K> GetRelationship<K>(this INode<K> self, INode<K> rNode)
        {
            return self.GetRelationship(rNode.ID);
        }
        public static INodeRelationship<K> CreateRelationship<K>(this INode<K> self, INode<K> rNode)
        {
            return self.CreateRelationship(rNode.ID);
        }
    }
    public interface INodeRelationship<K> : INodeElement
    {
        K I { get; }
        K D { get; }

        INode<K> GetNode();
    }

    public class NodeGraph<K>
    {

        static readonly string defaultNodeTable = Nullable.GetUnderlyingType(typeof(K)) != null
                                                     ? "_N_" + Nullable.GetUnderlyingType(typeof(K)).Name
                                                     : "_G_" + typeof(K).Name;
        static readonly string defaultNodeRelationshipTable = defaultNodeTable + "_R";

        //----------------------------------------------------------------------//
        const int TableNameLength = 20;
        static readonly internal string IKey = "I";
        static readonly internal string DKey = "D";
        static readonly internal string IDKey = "ID";

        public readonly string NodeTable;
        public readonly string RelationshipTable;
        private readonly IBox Box;

        internal void EnsureMeta(DatabaseConfig self)
        {
            self.EnsureTable<Node>(NodeTable, IDKey + "(" + TableNameLength + ")");
            self.EnsureTable<Relationship>(RelationshipTable, IKey + "(" + TableNameLength + ")", DKey + "(" + TableNameLength + ")");
            self.EnsureIndex<Relationship>(RelationshipTable, true, DKey + "(" + TableNameLength + ")", IKey + "(" + TableNameLength + ")");
        }

        public NodeGraph(IBox box, string name)
        {
            this.NodeTable = name == null ? defaultNodeTable : name;
            this.RelationshipTable = name == null ? defaultNodeRelationshipTable : name + "_R";
            this.Box = box;
        }

        public INode<K> CreateNode(K id)
        {
            var n = new Node();
            n.ID = id;
            n.Graph = this;
            n.Box = Box;
            return n;
        }

        public IEnumerable<INode<K>> GetNodes(string where = null, params object[] args)
        {
            Couple.FixWhere(ref where, args);
            var ql = "from " + NodeTable + (where != null ? " " + where : "");
            foreach (var r in Box.BSelect<Node>(ql, args))
            {
                r.Graph = this;
                r.Box = Box;
                yield return r;
            }
        }

        public IEnumerable<INodeRelationship<K>> GetRelationships(string where = null, params object[] args)
        {
            Couple.FixWhere(ref where, args);
            var ql = "from " + RelationshipTable + (where != null ? " " + where : "");
            foreach (var r in Box.BSelect<Relationship>(ql, args))
            {
                r.Graph = this;
                r.Box = Box;
                yield return r;
            }
        }

        public INode<K> GetNode(K k)
        {
            var n = Box.Bind(NodeTable, k).Select<Node>();
            if (n != null)
            {
                n.Graph = this;
                n.Box = Box;
            }
            return n;
        }






        public class Node : Dictionary<string, object>, INode<K>
        {
            [NotColumn]
            public NodeGraph<K> Graph;
            [NotColumn]
            public IBox Box;
            private bool _IsNew = false;

            public K ID
            {
                get
                {
                    return (K)base[IDKey];
                }
                set
                {
                    base[IDKey] = value;
                    _IsNew = true;
                }
            }

            public bool Save()
            {
                if (this._IsNew)
                {
                    this._IsNew = !Box.Insert(Graph.NodeTable, this);
                    return !_IsNew;
                }
                else
                {
                    return Box.Bind(Graph.NodeTable, ID).Update(this);
                }
            }

            public bool Delete()
            {
                foreach (var r in
                   new List<object>(Box.Bind(Graph.NodeTable, ID).Link(Graph.RelationshipTable, IKey).GetDetails<object>())
                   )
                {
                    if (!Box.Bind(Graph.RelationshipTable, (object[])r).Delete())
                    {
                        return false;
                    }
                }
                foreach (var r in
                     new List<object>(Box.Bind(Graph.NodeTable, ID).Link(Graph.RelationshipTable, DKey).GetDetails<object>())
                   )
                {
                    var or = (object[])r;
                    var reverseR = new object[] { or[1], or[0] };
                    if (!Box.Bind(Graph.RelationshipTable, reverseR).Delete())
                    {
                        return false;
                    }
                }
                return Box.Bind(Graph.NodeTable, (object)ID).Delete();
            }



            public IEnumerable<INodeRelationship<K>> GetRelationships(string where = null, params object[] args)
            {
                Couple.FixWhere(ref where, args);
                foreach (var r in
                     Box.Bind(Graph.NodeTable, ID).Link(Graph.RelationshipTable, IKey).GetDetails<Relationship>(where, args)
                    )
                {
                    r.Graph = this.Graph;
                    r.Box = this.Box;
                    yield return r;
                }
            }

            public INodeRelationship<K> GetRelationship(K rNodeID)
            {
                foreach (var r in GetRelationships(DKey + "==?", rNodeID))
                {
                    return r;
                }
                return null;
            }
            public INodeRelationship<K> CreateRelationship(K rNodeID)
            {
                var r = new Relationship();
                r.Graph = this.Graph;
                r.I = this.ID;
                r.D = rNodeID;
                r.Box = Box;
                return r;
            }

        }

        public class Relationship : Dictionary<string, object>, INodeRelationship<K>
        {
            [NotColumn]
            public NodeGraph<K> Graph;
            [NotColumn]
            public IBox Box;
            private bool _IsNew = false;



            public K I
            {
                get
                {
                    return (K)base[IKey];
                }
                set
                {
                    base[IKey] = value;
                    this._IsNew = true;
                }
            }


            public K D
            {
                get
                {
                    return (K)base[DKey];
                }
                set
                {
                    base[DKey] = value;
                }
            }


            public INode<K> GetNode()
            {
                var n = Box.Bind(Graph.NodeTable, D).Select<Node>();
                n.Graph = Graph;
                n.Box = Box;
                return n;
            }

            public bool Save()
            {
                if (this._IsNew)
                {
                    this._IsNew = !Box.Insert(Graph.RelationshipTable, this);
                    return !_IsNew;

                }
                else
                {
                    return Box.Bind(Graph.RelationshipTable, I, D).Update(this);
                }
            }
            public bool Delete()
            {
                return Box.Bind(Graph.RelationshipTable, I, D).Delete();
            }
        }

    }
}
