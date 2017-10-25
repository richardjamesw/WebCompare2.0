using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WebCompare2_0.Model;

namespace WebCompare2._0.Model
{
    public class BTree
    {
        const int ByteLength = 100;
        const int k = 16;
        static int numOfNodes = 0;
        static Node root;
        long id;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Root </param>
        public BTree()
        {
            id = 0;
            root = new Node(null);
            WriteTree(root);
        }

        /// <summary>
        /// Internal Node class
        /// </summary>
        [Serializable()]
        public class Node
        {
            long id;
            public KeyValuePair<float, string>[] kvPairs;
            long[] children;
            long parent;
            bool leaf;

            public Node(Node p)
            {
                id = 0;
                kvPairs = new KeyValuePair<float, string>[k];
                children = new long[k + 1];
                leaf = true;
                ++numOfNodes;
            }
         #region Node Properties 
         // ID property
         public long ID
            {
                get
                {
                    return id;
                }
                set
                {
                    id = value;
                }
            }
           // Name property
            public KeyValuePair<float, string>[] KVPairs
            {
                get
                {
                    return kvPairs;
                }
                set
                {
                    kvPairs = value;
                }
            }
            // Name property
            public long[] Children
            {
                get
                {
                    return children;
                }
                set
                {
                    children = value;
                }
            }
            // Name property
            public long Parent
            {
                get
                {
                    return parent;
                }
                set
                {
                    parent = value;
                }
            }
            // Name property
            public bool Leaf
            {
                get
                {
                    return leaf;
                }
                set
                {
                    leaf = value;
                }
            }
         #endregion
      }

      /// <summary>
      /// Read a Node from disk
      /// </summary>
      /// <param name="id">Node identification number.</param>
      public static Node ReadTree(long id)
        {
            string FileName = $"treebin\\tree.bin";
            FileStream fs = null;
            Node readNode = null;
            try
            {
                if (File.Exists(FileName))
                {
                    readNode.KVPairs = new KeyValuePair<float, string>[k];
                    using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open)))
                    {
                       readNode.Leaf = reader.ReadBoolean();
                       readNode.Parent = reader.ReadInt64();
                       for (int kvi = 0; kvi <= k; ++kvi)
                       {
                            float key = reader.ReadSingle();
                            string val = reader.ReadString();
                            readNode.KVPairs[kvi] = new KeyValuePair<float, string>(key, val);
                       }
                       for (int ci = 0; ci <= (k+1); ++ci)
                       {
                           readNode.Children[ci] = reader.ReadInt64();
                       }
                    } // end using
                } // end if
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            finally { fs.Close(); }
            return readNode;
        }


        /// <summary>
        /// Write a node to disk
        /// </summary>
        /// <param name="node">Node to be written.</param>
        /// <param name="id">Node identification number.</param>
        public void WriteTree(Node node)
        {
         string FileName = $"treebin\\tree.bin";
         FileStream fs = new FileStream(FileName, FileMode.Append, FileAccess.Write);
         try
            {
                MemoryStream ms = new MemoryStream();
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    writer.Write(node.Leaf);
                    writer.Write(node.Parent);
                    for (int kvi = 0; kvi <= k; ++kvi)
                    {
                         writer.Write(node.KVPairs[kvi].Key);
                         writer.Write(node.KVPairs[kvi].Value);
                    }
                    for (int ci = 0; ci <= (k+1); ++ci)
                    {
                        writer.Write(node.Children[ci]);
                    }
                    
                }
                byte[] bytes = ms.ToArray();
                fs.Write(bytes, (int)fs.Position, ByteLength);  // write the stream to file, already should be at the end
                fs.Close();
            }
            catch (Exception e) { Console.WriteLine("Error in WriteNode: " + e); }
            finally { fs.Close(); }
        }


        /// <summary>
        /// Search tree using key
        /// </summary>
        /// <param name="node">Start search here.</param>
        /// <param name="key"></param
        /// <param name="min">Index.</param>
        /// <param name="max">Index.</param>
        public KeyValuePair<> SearchTree(int key)
        {

        }




    }
}
