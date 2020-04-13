using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace huffmanCode
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //i like like like java do you like a java
            string str = "as";
            //获取字符出现频率统计表
            Byte[] source = Encoding.UTF8.GetBytes(str);
            //重要的源数据
            Console.WriteLine("原始长度:" + str.Length);
            CreatedHuffmanData huffmanCode = new CreatedHuffmanData();
            //进行压缩，压缩数据存放在_huffmanData中
            huffmanCode.CreHuffmanData(source);
            // for (int i = 0; i < huffmanCode.HuffmanData.Length; i++)
            // {
            //     Console.WriteLine(huffmanCode.HuffmanData[i]);
            // }
            Console.WriteLine("解压");
            Unzip unzip = new Unzip();
            var zip=unzip.UnzIpBytes(huffmanCode.GetHuffmanCodeMap(),huffmanCode.HuffmanData);
            Console.WriteLine(zip);
            //FileZip("D:\\数据结构与算法\\树\\huffmanCode\\baseCode.txt","D:\\数据结构与算法\\树\\huffmanCode\\base.zip");
            UnFileZip("D:\\数据结构与算法\\树\\huffmanCode\\base.zip",
                "D:\\数据结构与算法\\树\\huffmanCode\\bases.txt");
        }
        
        /// <summary>
        /// 对文件进行压缩
        /// </summary>
        /// <param name="inPutFileName">输入流:此文件需要被压缩</param>
        /// <param name="outputFileName">输出流:输出一个.zip压缩文件</param>
        public static void FileZip(string inPutFileName,string outputFileName)
        {
            try
            {
                using(Stream file = new FileStream(inPutFileName,FileMode.Open,FileAccess.Read) )
                {
                    using (BinaryReader bin=new BinaryReader(file,Encoding.UTF8))
                    {
                        var bytes = bin.ReadBytes((int)file.Length);
                        //根据文件的字节数组压缩文件
                        CreatedHuffmanData cre= new CreatedHuffmanData();
                        cre.CreHuffmanData(bytes);
                        BinaryFormatter binaryFormatter = new BinaryFormatter();
                        using (Stream output=new FileStream(outputFileName,FileMode.Create,FileAccess.Write))
                        {   
                            //将关键的经过哈夫曼编码后的字节文件，序列化写入到输出流中
                            binaryFormatter.Serialize(output,cre.HuffmanData);
                            //将关键的经过哈夫曼编码表写入到输出流中
                            binaryFormatter.Serialize(output,cre.GetHuffmanCodeMap());
                        }
                    }   
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(SerializationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// 对文件进解压
        /// </summary>
        /// <param name="inPutFileName">输入流:需要解压的文件</param>
        /// <param name="outputFileName">输出流:解压状态</param>
        public static void UnFileZip(string inPutFileName,string outputFileName)
        {
            try
            {
                using (Stream file = new FileStream(inPutFileName, FileMode.Open, FileAccess.Read))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    //反序列为对应类型输出
                    var sbytes = (sbyte[]) binaryFormatter.Deserialize(file);
                    var list = (Dictionary<Byte, string>) binaryFormatter.Deserialize(file);
                    Unzip unzip = new Unzip();
                    var cur = unzip.UnzIpBytes(list, sbytes);
                    using (Stream output = new FileStream(outputFileName, FileMode.Create, FileAccess.Write))
                    {
                        using (StreamWriter writer = new StreamWriter(output))
                        {
                            writer.Write(cur);
                        }
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (SerializationException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        
        /// <summary>
        /// 此类封装
        /// 1.统计频率，转换为权值集合
        /// 2.根据权值集合创建哈夫曼树
        /// 3.根据哈夫曼树创建哈夫曼表
        /// </summary>
        public class HuffmanCode
        {
            protected Dictionary<Byte, string> HuffmanCodeMap { get; set; }

            public HuffmanCode()
            {
                HuffmanCodeMap = new Dictionary<byte, string>();
            }

            //1.将字符串中的每个字符进行统计，获取他们的ASCll码值和他们出现的次数作为权值放到一个节点中
            /// <summary>
            /// 将数据转换为带权值节点Node集合
            /// </summary>
            /// <param name="buff">数据字节数组</param>
            /// <returns>返回一个Node集合，其中存有权值，真实数据</returns>
            protected static List<Node> GetDataNodes(Byte[] buff)
            {
                List<Node> nodes = new List<Node>();
                //利用可空值类型进行判断(C#中没有泛型哈希表，选择字典表代替，内部实现于哈希表类似)
                Dictionary<Byte, int?> dictionary = new Dictionary<byte, int?>();
                int? init = null;
                for (int i = 0; i < buff.Length; i++)
                {
                    init = dictionary.ContainsKey(buff[i]) ? dictionary[buff[i]] : null;
                    if (init == null)
                    {
                        dictionary.Add(buff[i], 1);
                    }
                    else
                    {
                        dictionary[buff[i]] += 1;
                    }
                }

                //将字典表中的内容添加到集合中返回
                foreach (var item in dictionary)
                {
                    nodes.Add(new Node(item.Key, (int) item.Value));
                }

                return nodes;
            }

            //2.根据我们统计的信息创建一个哈夫曼树。从而生成正确的前缀编码
            /// <summary>
            /// 将我们统计的个字符串出现次数，用于生成哈夫曼树
            /// </summary>
            /// <param name="nodes">Node集合</param>
            /// <returns>返回一个哈夫曼树根节点</returns>
            protected static Node CreatedHuffmanTree(List<Node> nodes)
            {
                //排序
                nodes.Sort();
                //取出两个节点，合并成一个树，再将其加入到集合中再次排序
                //循环执行，直到集合中只剩下一个元素
                while (nodes.Count > 1)
                {
                    var left = nodes[0];
                    var right = nodes[1];
                    var parent = new Node(null, left.Weigth + right.Weigth) {Left = left, Right = right};
                    nodes.Remove(left);
                    nodes.Remove(right);
                    nodes.Add(parent);
                    nodes.Sort();
                }
                //获取完整哈夫曼树，并清空节点表。最后返回
                Node root = nodes[0];
                nodes.Clear();
                return root;
            }
            //3.创建字符编码表（可能因为排序方式的不同，哈夫曼树不同导致字符编码不同）
            //但是WPL值是一样的，所以不会影响我们的压缩
            /// <summary>
            /// 创建哈夫曼编码表,最终存于HuffmanCodeMap表中(注意我们知道找到一个叶子节点然后将编码加入到集合中)
            /// 左节点编码为:0
            /// 右节点编码为:1
            /// </summary>
            /// <param name="root">哈夫曼树，根据此树进行创建</param>
            /// <param name="code">左节点编码为:0 右节点编码为:1</param>
            /// <param name="builder">拼接编码</param>
            private void _CreHuffmanCode(Node root, string code,
                StringBuilder builder)
            {
                //首先必须清楚，每个StringBuilder在内存中都是独立的(我们并没有引用任何StringBuilder)
                //这样就保证了，当调用函数执行完成后跳转回到返回地址时，StringBuilder是不变的，没有覆盖StringBuilder
                StringBuilder strBuilder = new StringBuilder(builder.ToString());
                strBuilder.Append(code);
                //如果data是空 则说明了当前节点不是叶子节点，还需要深入递归
                if (root.Data == null)
                {
                    if (root.Left != null)
                    {
                        _CreHuffmanCode(root.Left, "0", strBuilder);
                    }

                    if (root.Right != null)
                    {
                        _CreHuffmanCode(root.Right, "1", strBuilder);
                    }
                }
                else
                {
                    //找到了一个叶子节点。将编码与字符信息加入到Map中
                    HuffmanCodeMap.Add((Byte) root.Data, strBuilder.ToString());
                }
            }

            /// <summary>
            /// 创建哈夫曼编码表:注意如果传递的root为空，则HuffmanCodeMap = null
            /// </summary>
            /// <param name="root">哈夫曼树</param>
            protected void CreHuffmanCode(Node root)
            {
                if (root != null)
                {
                    if (root.Data != null && root.Left == null && root.Right == null)
                    {
                        //但是可能字符串中只有一个字符，那么这样考虑的话应该传递一个0
                        _CreHuffmanCode(root, "0", new StringBuilder());
                    }
                    else
                    {
                        //为减少编码数量，头节点无编码
                        _CreHuffmanCode(root, "", new StringBuilder());
                    }
                }
                else
                {
                    throw new ArgumentException("哈夫曼树不能为空");
                }
            }
        }

        //4.根据哈夫曼表，生成哈夫曼数据
        //此类采用继承，封装创建哈夫曼表方法，直接调用此类创建最终数据即可
        /// <summary>
        /// 压缩类
        /// </summary>
        public class CreatedHuffmanData : HuffmanCode
        {
            //sbyte是8位有符号类型;而byte是8位无符号类型
            private sbyte[] _huffmanData;

            //生成的哈夫曼编码数据
            public sbyte[] HuffmanData
            {
                get { return _huffmanData; }
            }

            public CreatedHuffmanData()
            {

            }
            /// <summary>
            /// 获取HuffmanCodeMap
            /// </summary>
            /// <returns>HuffmanCodeMap</returns>
            public Dictionary<Byte, string> GetHuffmanCodeMap()
            {
                return HuffmanCodeMap;
            }

            /// <summary>
            /// 创建HuffmanMap
            /// </summary>
            /// <param name="root">哈夫曼树</param>
            public void CreHuffmanCodeMap(Node root)
            {
                CreHuffmanCode(root);
            }

            /// <summary>
            /// 1.将原始字节数组根据哈夫曼编码表，生成相应字符的哈夫曼编码
            /// 2.将字符生成的哈夫曼编码根据8位1字节，生成一个字节数组
            /// </summary>
            /// <param name="sourceData">原数据二进制数组</param>
            private void _CreHuffmanData(Byte[] sourceData)
            {
                //1.首先获取字符串的哈夫曼编码
                string data = "";
                for (int i = 0; i < sourceData.Length; i++)
                {
                    data += HuffmanCodeMap[sourceData[i]];
                }
                //由于他现在是字符串，我们不能直接转换为字节数组，如果直接转化的话它反而会变得更长，而不是压缩了
                //10001111010011110000111101001111000011110100111100001110111010010101011100000110
                //11100100111101001111010011110000110101110111010010101
                //2.所以我们需要按照1字节=8位的原则，取8位10001111(补)->10001111-1(反码)->~(10001111-1)(原码)
                //按照这样的方式存储存储到字节的数组中
                //首先根据编码长度计算出按照每字节8位，字节数组需要多长
                int length = data.Length % 8 == 0 ? data.Length /8 +1: data.Length / 8 + 2;
                //将编码按照8位1字节方式存储到Byte[]数组中
                sbyte[] codesBytes = new sbyte[length];
                int lastbyteLength=0;
                for (int i = 0, j = 0; i < data.Length; i += 8, j++)
                {
                    string str = "";
                    //因为不能保证编码长度就是8的倍数，所以我们+8判断截取是否会溢出
                    if (i + 8 > data.Length)
                    {
                        str = data.Substring(i);
                        lastbyteLength = str.Length;
                    }
                    else
                    {
                        str = data.Substring(i, 8);
                        lastbyteLength = str.Length;
                    }
                    //将截取的8位转换位有符号整数再将其转换为字节存入到字节数组中
                    codesBytes[j] = Convert.ToSByte(str, 2);
                }
                //最后一个元素存放了最后的长度
                codesBytes[codesBytes.Length - 1] =  Convert.ToSByte(lastbyteLength);
                _huffmanData = codesBytes;
                //压缩率=(1-(length*8/sourceData.Length*8))*100=xxx%;
                //压缩率=(sourceData.Length-length/sourceData.Length)*100=xxx%
            }
            /// <summary>
            /// 如果HuffmanCodeMap == null将根据传递来的哈夫曼树。创建哈夫曼编码表
            /// 但是如果树也为空则报告ArgumentException("哈夫曼树不能为空");
            /// </summary>
            /// <param name="root">树</param>
            /// <exception cref="ArgumentException">哈夫曼树不能为空</exception>
            private void CreHuffmanData(Node root, Byte[] sourceData)
            {
                if (HuffmanCodeMap == null)
                {
                    if (root != null)
                    {
                        CreHuffmanCodeMap(root);
                    }
                    else
                    {
                        throw new ArgumentException("哈夫曼树不能为空");
                    }
                }

                _CreHuffmanData(sourceData);
            }
            /// <summary>
            /// 创建字符串哈夫曼编码版本的入口
            /// 1.统计
            /// 2.根据统计创建哈夫曼二叉树
            /// 3.生成对应哈夫曼编码表
            /// 4.压缩数据,压缩后的数据存放在此类_huffmanData中
            /// </summary>
            /// <param name="sourceData">需要压缩的目标Byte数组</param>
            public void CreHuffmanData( Byte[] source)
            {
                //统计Byte数组中，各个出现频率，作为权值返回List
                var nodes = GetDataNodes(source);
                //将我们的字符出现统计表，转换成一颗哈夫曼树
                var root = CreatedHuffmanTree(nodes);
                //创建哈夫曼编码表
                CreHuffmanCodeMap(root);
                //将原数据于哈夫曼表一一对应转换，生成应用哈夫曼编码后的版本(存于_huffmanData)
                CreHuffmanData(root, source);
            }
        }

        //5.数据解压
        public class Unzip
        {
            /// <summary>
            /// 将哈夫曼编码版本sbyte转换为哈夫曼编码二进制版本
            /// </summary>
            /// <param name="flag">确定是否需要补高位</param>
            /// <param name="b">sbyte数据</param>
            /// <returns></returns>
            public string ByteToBitString(bool flag, sbyte b,int bit)
            {
                //首先将我们的sbyte数据转换为整数类型(注意压缩时是有符号类型，解压时也必须是同等类型)
                int temp = b;
                //根据所传递的位数进行截取
                //如果是无符号数，那么值如果太小必然无法得到8位二进制，因为我们进行或运算高位补0，最后在截取,并不会影响最终结果
                var x = 1 << bit;
                temp |= x ;//进行或运算，补高位
                string byteStr = Convert.ToString(temp, 2);
                //因为我们压缩式是取8位进行压缩，所以其每个sbyte
                //大于Tmax=(2^w-1)-1,不会小于Tmin=-2^w-1;
                //但因为Convert.Tostirng()转换为16位二进制或更高
                //byteStr.Length - bit=所需要截取的下标位置
                return byteStr.Substring(byteStr.Length - bit);
            }
            /// <summary>
            //将压缩的字节解压为相对应的哈夫曼编码
            /// </summary>
            /// <param name="huffmanMap">哈夫曼编码表</param>
            /// <param name="bytes">经过哈夫曼编码后得到的sbyte数组</param>
            /// <returns></returns>
            public string UnzIpBytes(Dictionary<Byte,string> huffmanMap,sbyte[] bytes)
            {
                string str = "";
                for (int i = 0; i < bytes.Length-2; i++)
                {
                    str += ByteToBitString(true, bytes[i],8);
                }
                //根据传递的位数，补高位，截取bit位
                str += ByteToBitString(true, bytes[bytes.Length-2],bytes[bytes.Length - 1]);
                //将得到的字符串，按照我们之前得到的哈夫曼编码表，进行解码
                Dictionary<string,Byte> OldValueMap=new Dictionary <string,byte>();
                //根据哈夫曼编码表的value=哈夫曼编码;key=字符(ascll码值)
                foreach (var item in huffmanMap)
                {
                    //进行反转加入到新的表中
                    OldValueMap.Add(item.Value,item.Key);                    
                }
                List<Byte> codeList=new List<byte>();
                string tempCode = "";
                string oldval = "";
                // for (int i = 0; i < str.Length; i++)
                // {
                //     tempCode += str[i];
                //     if (OldValueMap.ContainsKey(tempCode))
                //     {
                //         codeList.Add(OldValueMap[tempCode]);
                //         tempCode = "";
                //     }
                // }
                for (int i = 0; i < str.Length; i++)
                {
                    tempCode += str[i];
                    if (OldValueMap.ContainsKey(tempCode))
                    {
                        oldval+=(char)OldValueMap[tempCode];
                        tempCode = "";
                    }
                }
                tempCode = "";
                return oldval;
            }
        }
        public class Node : IComparable<Node>
        {
            //数据
            public Byte? Data { get; set; }

            //权值
            public int Weigth { get; set; }

            //左
            public Node Left { get; set; }

            //右
            public Node Right { get; set; }

            public Node(byte? ch, int weigth)
            {
                Data = ch;
                Weigth = weigth;
            }

            public Node()
            {

            }

            //cmp 比较(从小到大排序)
            public int CompareTo(Node other)
            {
                return Weigth - other.Weigth;
            }

            public override string ToString()
            {
                return "Data:" + Data + "Weight:" + Weigth;
            }

            private void _PreOrder(Node node)
            {
                if (node.Data == null)
                {
                    Console.WriteLine(node.Weigth);
                }
                else
                {
                    Console.WriteLine("char\t" + (char) node.Data + "\t" + node.Weigth);
                }

                if (node.Left != null)
                {
                    _PreOrder(node.Left);
                }

                if (node.Right != null)
                {
                    _PreOrder(node.Right);
                }
            }

            public void PerOrder(Node root)
            {
                if (root != null)
                {
                    _PreOrder(root);
                }
            }
        }
        
    }
}