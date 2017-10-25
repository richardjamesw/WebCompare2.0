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
        const int k = 100;
        static int numOfNodes = 0;
        static Node root;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="id">Root </param>
        public BTree()
        {
            root = new Node(null);
            WriteNode(root);
        }

        /// <summary>
        /// Internal Node class
        /// </summary>
        [Serializable()]
        public class Node
        {
            long id;
            KeyValuePair<long, string>[] kvPairs;
            Node[] children;
            Node parent;
            bool leaf;

            public Node(Node p)
            {
                id = ++numOfNodes;
                kvPairs = new KeyValuePair<long, string>[k - 1];
                children = new Node[k];
                parent = p;
                leaf = true;
            }

            // Name property
            public long ID
            {
                get
                {
                    return id;
                }
            }
        }

        /// <summary>
        /// Read a Node from disk
        /// </summary>
        /// <param name="id">Node identification number.</param>
        public Node ReadNode(long id)
        {
            string FileName = $"treebin\\node{id}.bin";
            try
            {
                if (File.Exists(FileName))
                {
                    Stream filestream = File.OpenRead(FileName);
                    BinaryFormatter deserializer = new BinaryFormatter();
                    Node node = (Node) deserializer.Deserialize(filestream);
                    filestream.Close();
                    return node;
                }
            }
            catch (Exception e) { Console.WriteLine("Error in ReadNode: " + e); }
            return null;
        }


        /// <summary>
        /// Write a node to disk
        /// </summary>
        /// <param name="node">Node to be written.</param>
        /// <param name="id">Node identification number.</param>
        public void WriteTree()
        {
            try
            {
                string FileName = $"treebin\\tree.bin";
                Stream TestFileStream = File.Create(FileName);
                BinaryFormatter serializer = new BinaryFormatter();
                serializer.Serialize(TestFileStream, this);
                TestFileStream.Close();
            }
            catch (Exception e) { Console.WriteLine("Error in WriteNode: " + e); }
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
