using System.Collections.ObjectModel;
using System.Collections.Generic;

public class NodeList<T>
{
	List<Node<T>> Items;

	public NodeList() { Items = new List<Node<T>> (); }
	
	public NodeList(int initialSize)
	{
		// Add the specified number of items
		Items = new List<Node<T>> ();
		for (int i = 0; i < initialSize; i++)
			Items.Add(default(Node<T>));
	}
	
	public Node<T> FindByValue(T value)
	{
		// search the list for the value
		foreach (Node<T> node in Items)
			if (node.Value.Equals(value))
				return node;
		
		// if we reached here, we didn't find a matching node
		return null;
	}

	public void Add(Node<T> node)
	{
		Items.Add (node);
	}
}
