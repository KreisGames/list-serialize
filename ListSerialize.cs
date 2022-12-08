public class ListNode
{
    //Better mark this fields as nullable
    public ListNode Prev;
    public ListNode Next;
    public ListNode Rand; // произвольный элемент внутри списка
    public string Data;
}

public class ListRand
{
    public ListNode Head;
    public ListNode Tail;
    public int Count;

    public void Serialize(FileStream s)
    {
        //Dict for better exec speed (hashmap)
        var map = new Dictionary<ListNode, int>(Count) { { Head, 0 } };
        var prevValue = Head;
        for (var i = 1; i < Count; i++)
        {
            map.Add(prevValue.Next, i);
            prevValue = prevValue.Next;
        }

        using var binaryWriter = new BinaryWriter(s);
        for (var node = Head; node != Tail; node = node.Next)
        {
            binaryWriter.Write(node.Data);
            binaryWriter.Write(map[node.Rand]);
        }

        binaryWriter.Write(Tail.Data);
        binaryWriter.Write(map[Tail.Rand]);
    }

    public void Deserialize(FileStream s)
    {
        //Dict of elements, int is required element index, list is all elements which require this rand element
        //Used to fill class in 1 iteration
        var randElems = new Dictionary<int, List<ListNode>>();
        var elements = new List<ListNode>(8);
        
        using var binaryReader = new BinaryReader(s);

        var first = new ListNode
        {
            Data = binaryReader.ReadString()
        };

        var firstRand = binaryReader.ReadInt32();
        if (firstRand == 0)
        {
            first.Rand = first;
        }
        else
        {
            randElems.Add(firstRand, new List<ListNode> { first });
        }
        
        elements.Add(first);
        Head = first;

        for (var i = 1; binaryReader.PeekChar() != -1; i++)
        {
            var current = new ListNode
            {
                Data = binaryReader.ReadString()
            };
            var rand = binaryReader.ReadInt32();

            elements.Add(current);
            
            //Check Rand and fill dependencies
            if (rand <= i)
            {
                current.Rand = elements[rand];
            }
            else
            {
                if (randElems.ContainsKey(rand))
                {
                    randElems[rand].Add(current);
                }
                else
                {
                    randElems.Add(rand, new List<ListNode> { current });
                }
            }

            if (randElems.ContainsKey(i))
            {
                foreach (var node in randElems[i])
                {
                    node.Rand = current;
                }

                randElems.Remove(i);
            }

            var prev = elements[i - 1];
            prev.Next = current;
            current.Prev = prev;
        }

        Tail = elements.Last();
        Count = elements.Count;
    }
}
