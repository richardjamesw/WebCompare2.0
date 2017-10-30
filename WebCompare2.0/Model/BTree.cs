using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using WebCompare2_0.Model;

namespace WebCompare2_0.Model
{
    [Serializable()]
    public class BTree
    {
        const int ByteLength = 128;
        const int k = 64;  // num of children
        static int numOfNodes = 0;
        static Node root;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Root </param>
        public BTree()
        {
            // Get number of nodes
            numOfNodes = GetNumberOfNodes();
            // if number of nodes is not zero get root (1)
            if (numOfNodes > 0)
            {
                // read in the root
                root = ReadTree(1);
            }
            // else 
            else
            {
                // create new root Node and write it to disk
                root = new Node();
                WriteTree(root);
            }
        }

        /// <summary>
        /// Internal Node class
        /// </summary>
        [Serializable()]
        public class Node
        {
            bool leaf;
            long id;
            int numberOfKeys;
            KeyValuePair<float, string>[] kvPairs;
            long[] children;

            public Node()
            {
                id = ++numOfNodes;
                leaf = true;
                kvPairs = new KeyValuePair<float, string>[k];
                children = new long[k + 1];
                
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
            //// Name property
            //public long Parent
            //{
            //    get
            //    {
            //        return parent;
            //    }
            //    set
            //    {
            //        parent = value;
            //    }
            //}
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
            public int NumberOfKeys
            {
                get
                {
                    return numberOfKeys;
                }
                set
                {
                    numberOfKeys = value;
                }
            }
            #endregion
        }

        /// <summary>
        /// Read number of nodes from file
        /// </summary>
        /// <returns></returns>
        private static int GetNumberOfNodes()
        {
            try
            {
                string FileName = $"treebin\\numberofnodes.bin";
                return Int32.Parse(File.ReadAllText(FileName));
            }
            catch { return 0; }
        }


        private static void SetNumberOfNodes(int num)
        {
            try
            {
                string FileName = $"treebin\\numberofnodes.bin";
                File.WriteAllText(FileName, num.ToString());
            }
            catch
            {

            }
        }

        /// <summary>
        /// Read a Node from disk
        /// </summary>
        /// <param name="id">Node identification number.</param>
        public static Node ReadTree(long id)
        {
            string FileName = $"treebin\\tree.bin";
            Node readNode = null;
            try
            {
                if (File.Exists(FileName))
                {
                    readNode.KVPairs = new KeyValuePair<float, string>[k];
                    using (BinaryReader reader = new BinaryReader(File.Open(FileName, FileMode.Open)))
                    {
                        readNode.Leaf = reader.ReadBoolean();
                        //readNode.Parent = reader.ReadInt64();
                        for (int kvi = 0; kvi <= k; ++kvi)
                        {
                            float key = reader.ReadSingle();
                            string val = reader.ReadString();
                            readNode.KVPairs[kvi] = new KeyValuePair<float, string>(key, val);
                        }
                        for (int ci = 0; ci <= (k + 1); ++ci)
                        {
                            readNode.Children[ci] = reader.ReadInt64();
                        }
                    } // end using
                } // end if
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            
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
                    //writer.Write(node.Parent);
                    for (int kvi = 0; kvi <= k; ++kvi)
                    {
                        writer.Write(node.KVPairs[kvi].Key);
                        writer.Write(node.KVPairs[kvi].Value);
                    }
                    for (int ci = 0; ci <= (k + 1); ++ci)
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
        public KeyValuePair<long, string> SearchTree(int key)
        {
            return new KeyValuePair<long, string>(0, "");
        }


        /// <summary>
        /// Add key value pair to the tree
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Insert(long key, string value)
        {
            Node rt = ReadTree(root.ID);
            KeyValuePair<long, string> kvp = new KeyValuePair<long, string>;
            if (rt.NumberOfKeys == (k - 1))
            {
                Node nd = new Node();
                root = nd;
                nd.Leaf = false;
                nd.Children[0] = rt;
            }
        }


        /// <summary>
        /// Split full node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        /// <param name="index"></param>
        public void SplitNode(Node parent, Node child, int index)
        {
            Node childB = new Node();
            childB.Leaf = child.Leaf;

            // hand off keys
            for (int i = 0; i < (k-1)/2; ++i)
            {
                childB.KVPairs[i] = child.KVPairs[k-1-i];
                child.KVPairs[k-1-i] = default(KeyValuePair<float, string>);
            }
            // hand off children
            if (!child.Leaf)
            {
                for (int i = 0; i < (k-1)/2; ++i)
                {
                    childB.Children[i] = child.Children[k-1-i];
                    child.Children[k-1-i] = default(long);
                }
            }


        }
    }
}
