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
        static Node root;
        static int numOfNodes = 0;
        const int K = 8;  // min degree
        const int kvp_SIZE = (2 * K - 1);  // min degree
        const int children_SIZE = (2 * K);  // min degree
        const int ByteLength = 128;

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
                // Read in the root
                root = ReadTree(1);
                // Read Children
                for (int i = 0; i < NumberOfNodes; ++i)
                {
                    
                }
            }
            else
            {
                // create new root Node and write it to disk
                root = new Node();
                WriteTree(root);
            }
        }

        #region BTree Properties
        public Node Root
        {
            get
            {
                return root;
            }
            set
            {
                root = value;
            }
        }

        public static int NumberOfNodes
        {
            get
            {
                return numOfNodes;
            }
            set
            {
                numOfNodes = value;
                SetNumberOfNodes(numOfNodes);
            }
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

        /// <summary>
        /// Set number of nodes in a file
        /// </summary>
        /// <param name="num"></param>
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

        #endregion

        /// <summary>
        /// Internal Node class
        /// </summary>
        [Serializable()]
        public class Node
        {
            long id;
            bool leaf;
            int numberOfKeys = 0;
            KeyValuePair<float, string>[] kvPairs;
            Node[] children;
            long[] childIDs;

            public Node()
            {
                id = ++NumberOfNodes;
                leaf = true;
                kvPairs = new KeyValuePair<float, string>[kvp_SIZE];
                children = new Node[children_SIZE];
                childIDs = new long[children_SIZE];
            }

            #region Node Properties 

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

            public Node[] Children
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
            public long[] ChildIDs
            {
                get
                {
                    return childIDs;
                }
                set
                {
                    childIDs = value;
                }
            }

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
                    FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                    fs.Seek(id, SeekOrigin.Begin);
                    readNode.KVPairs = new KeyValuePair<float, string>[K];
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        readNode.Leaf = reader.ReadBoolean();
                        //readNode.Parent = reader.ReadInt64();
                        for (int kvi = 0; kvi <= K; ++kvi)
                        {
                            float key = reader.ReadSingle();
                            string val = reader.ReadString();
                            readNode.KVPairs[kvi] = new KeyValuePair<float, string>(key, val);
                        }
                        for (int ci = 0; ci <= (K + 1); ++ci)
                        {
                            readNode.ChildIDs[ci] = reader.ReadInt64();
                        }
                    } // end using
                } // end if
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            
            return readNode;
        }

        /// <summary>
        /// Read list of children of disk
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        private Node[] ReadChildren(long[] ids)
        {
            Node[] childs = new Node[children_SIZE];
            for (int i = 0; i < ids.Length; ++i)
            {
                childs[i] = ReadTree(ids[i]);
            }
            return childs;
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
                    for (int kvi = 0; kvi <= K; ++kvi)
                    {
                        writer.Write(node.KVPairs[kvi].Key);
                        writer.Write(node.KVPairs[kvi].Value);
                    }
                    if (node.ChildIDs != null)
                    {
                        foreach (var ck in node.ChildIDs)
                        {
                            writer.Write(ck);
                        }
                    }

                }
                byte[] bytes = ms.ToArray();
                fs.Seek(node.ID, SeekOrigin.Begin);
                fs.Write(bytes, (int)fs.Position, ByteLength);  // write the stream to file
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
        public string SearchTree(float key, long id)
        {
            // Get Tree
            Node temp = ReadTree(id);
            try
            {
                // Check keys
                foreach (var kvp in temp.KVPairs)
                {
                    if (key == kvp.Key)
                    {
                        return kvp.Value;
                    }
                }

                // Search Children
                if (temp.ChildIDs.Length > 0)
                {
                    string ck = null;
                    foreach (var cid in temp.ChildIDs)
                    {
                        // Block empty slots
                        if (cid != 0)
                        {
                            ck = SearchTree(key, cid);
                        }
                        if (ck != null) break;
                    }
                    return ck;
                }

            } catch (Exception e) { Console.WriteLine("Exception at: " + e); }

            return null;
        }


        /// <summary>
        /// Add key value pair to the tree
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Insert(float key, string value)
        {
            try
            {
                Node nd = this.Root;
                nd.Children = ReadChildren(nd.ChildIDs);
                KeyValuePair<float, string> kvp = new KeyValuePair<float, string>(key, value);

                // If node is not full insert, else split
                if (nd.NumberOfKeys < kvp_SIZE)
                {
                    InsertNonFull(nd, kvp);
                }
                else
                {
                    // New parent
                    Node p = new Node();
                    p.Leaf = false;
                    // Switch root IDs
                    long tempID = p.ID;
                    p.ID = nd.ID;
                    nd.ID = tempID;

                    p.Children[0] = nd;
                    p.ChildIDs[0] = p.Children[0].ID;

                    // Split nd into mini tree
                    SplitChild(p, nd, 0);

                    // Set root
                    Root = p;
                    WriteTree(Root);

                    // Move middle key to parent node, append this nodes children to the parents children
                    InsertNonFull(nd, kvp);

                } // End else
            }
            catch (Exception e) { Console.WriteLine("Error during BTree insert: " + e); }
        }

        /// <summary>
        /// Add key value pair to the tree
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void InsertNonFull(Node x, KeyValuePair<float, string> kvp)
        {
            int i = x.NumberOfKeys - 1;
            if (x.Leaf)
            {
                // Find correct key position and shuffle KVPairs
                while (i >= 0 && x.KVPairs[i].Key > kvp.Key)
                {
                    x.KVPairs[i + 1] = x.KVPairs[i];
                    --i;
                }
                // Insert the key
                x.KVPairs[i] = kvp;
                ++x.NumberOfKeys;
                WriteTree(x);
            }
            else
            {
                // Get x children from disk
                x.Children = ReadChildren(x.ChildIDs);
                // Find child to add new key
                while (i >= 0 && x.KVPairs[i].Key > kvp.Key)
                {
                    --i;
                }
                // Read where kvp is in x.KVPairs.Children[i]
                ++i;

                // Check if child node is full
                if (x.Children[i].NumberOfKeys == kvp_SIZE)
                {
                    // Split
                    SplitChild(x, x.Children[i], i);

                    // New children are x.Children[i] and x.Children[i + 1]
                    if (kvp.Key > x.KVPairs[i].Key)
                        ++i;
                }

                // Recursive insert now that we are not full
                InsertNonFull(x.Children[i], kvp);
            }

        }

        /// <summary>
        /// Split full node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="nd"></param>
        /// <param name="index"></param>
        public void SplitChild(Node parent, Node oldChild, int index)
        {
            // New child node
            Node newChild = new Node();
            // If old node was a leaf, new node is now a leaf
            newChild.Leaf = oldChild.Leaf;
            // New node will have K-1 keyvalue pairs due to old node splitting in half
            newChild.NumberOfKeys = K - 1;
            // Hand over back keys
            for (int i = 0; i < K - 1; ++i)
            {
                newChild.KVPairs[i] = oldChild.KVPairs[i + (K - 1)];
                oldChild.KVPairs[i + (K - 1)] = default(KeyValuePair<float, string>);
            }
            // old nodes keys
            oldChild.NumberOfKeys = K - 1;
            // Hand over Childs
            if (!oldChild.Leaf)
            {
                for (int i = 0; i < K; ++i)
                {
                    newChild.Children[i] = oldChild.Children[i + K];
                    newChild.ChildIDs[i] = oldChild.Children[i + K].ID;
                }
            }
            // Add the halved old node and new node as 
            // children of the parent we passed in
            for (int i = parent.NumberOfKeys; i > index + 1; --i)
            {
                parent.Children[i + 1] = parent.Children[i];
                parent.ChildIDs[i + 1] = parent.Children[i].ID;
            }
            parent.Children[index + 1] = newChild;
            parent.ChildIDs[index + 1] = newChild.ID;
            // Move keys also
            for (int i = parent.NumberOfKeys; i > index; --i)
            {
                parent.KVPairs[i + 1] = parent.KVPairs[i];
            }
            ++parent.NumberOfKeys;
            // Write to disk
            WriteTree(oldChild);
            WriteTree(newChild);
            WriteTree(parent);
        }
    }
}



/*
 * Not Used
 /// <summary>
        /// Split full node
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="nd"></param>
        /// <param name="index"></param>
        public Node SplitNode(Node nd)
        {
            // Create new child nodes
            Node childA = new Node();
            Node childB = new Node();
            childA.Leaf = true;
            childA.NumberOfKeys = K - 1;
            childB.Leaf = true;
            childB.NumberOfKeys = K - 1;

            // Hand off Keys
            for (int i = 0; i < (K - 1); ++i)
            {
                childA.KVPairs[i] = nd.KVPairs[i];
            }

            // Hand off Keys
            for (int i = kvp_SIZE; i > (K - 1); --i)
            {
                childB.KVPairs[i] = nd.KVPairs[i];
            }

            // Promote middle node 
            Node newParent = new Node();
            newParent.KVPairs[0] = nd.KVPairs[K - 1];

            // Clear nd
            nd = null;

            // Set children
            newParent.Children[0] = childA;
            newParent.Children[1] = childB;

            return newParent;
        }
 */

