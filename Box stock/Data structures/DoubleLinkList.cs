using System;
using System.Collections.Generic;
using System.Text;
using Box_stock.Data_models;

namespace Box_stock.Data_structures
{
    public class DoubleLinkList<T>
    {
        Node start;
        Node end;
        internal Node End { get => end; }
        internal Node Start { get => start; } 

        public void AddFirst(T val)
        {
            Node n = new Node(val);
            n.next = start;
            start = n;
            if (end == null) //in a case of empty list
            {
                end = n;
                return;
            }
            start.next.previus = start;
        }
        public bool RemoveFirst(out T SaveFirstData)
        {
            SaveFirstData = default(T);
            if (start == null) return false;

            SaveFirstData = start._data;
            start = start.next;
            if (start == null) end = null; //for removing the only (and last) item
            else start.previus = null;
            return true;
        }
        public void AddLast(T val)
        {
            if (start == null) //no elements
            {
                AddFirst(val); //private case
                return;
            }
            Node n = new Node(val);
            end.next = n;
            n.previus = end;
            end = n;
        }
        public bool RemoveLast(out T SaveLastData)
        {
            SaveLastData = default(T);
            if (start == null) return false; //list is empty

            SaveLastData = end._data;
            end = end.previus;
            if (end != null) end.next = null;
            else start = null;    //there was only one element and now list is empty (end.previus=null)
            return true;
        }

        internal void DeleteByNode(Node node) 
        {
            if (node == start)
            {
                RemoveFirst(out node._data);
                return;
            }
               
            if (node == end) 
            {
                RemoveLast(out node._data);
                return;
            }
            node.previus.next = node.next;
            node.next.previus = node.previus;
        }

        internal void ReplaceByNode(Node node) 
        {
            if (node.next == null) return;
            if (node.previus != null)
            {
                node.previus.next = node.next;
                node.next.previus = node.previus;
            }
            else
            {
                start = node.next;
                start.previus = null;
            }
            end.next = node;
            node.next = null;
            node.previus = end;
            end = node;
        }
        internal class Node
        {
            public T _data;
            public Node next;
            public Node previus;
            public Node(T data)
            {
                previus = null;
                next = null;
                _data = data;
            }
        }

        public bool AddAt(int position, T value) //add a value from a specific position
        {
            if (position < 0) return false;  //position can't be negative

            if (position == 0)  //if temp sits on the first element, just send it to addFirst function (private case)
            {
                AddFirst(value);
                return true; //you can always add first item
            }
            Node newValue = new Node(value);
            Node temp = start;
            int i = 0;
            while (temp != null && i < position)
            {
                if (temp.next == null && i == position - 1) //if temp sits on the last element, just send it to addLast function (private case)
                {
                    AddLast(value);
                    return true; //you can always add last item
                }
                temp = temp.next;
                i++;
            }
            if (temp == null) return false; //position is bigger than the numbers of elements

            newValue.next = temp; //if temp isn't null, our node sits between 2 other nodes and we need to navigate the pointers in order to add the new node 
            newValue.previus = temp.previus;
            temp.previus = newValue;
            newValue.previus.next = newValue;
            return true;
        }
        public bool GetAt(int position, out T value) //get value of index in a specific position
        {
            value = default(T);
            if (position < 0) return false; //position can't be negative

            Node temp = start;
            int i = 0;
            while (temp != null && i < position)
            {
                i++;
                temp = temp.next;
            }
            if (temp == null) return false; //position is bigger than the numbers of elements

            value = temp._data;
            return true;
        }
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            Node temp = start;
            while (temp != null)
            {
                sb.AppendLine(temp._data.ToString());
                temp = temp.next;
            }
            return sb.ToString();
        }
    }
}
