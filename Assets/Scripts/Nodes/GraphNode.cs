public class GraphNode<T> : Node<T>
{
	
	public GraphNode() : base() { }
	public GraphNode(T value) : base(value) { }
	public GraphNode(T value, NodeList<T> neighbors) : base(value, neighbors) { }
	
	new public NodeList<T> Neighbors
	{
		get
		{
			if (base.Neighbors == null)
				base.Neighbors = new NodeList<T>();
			
			return base.Neighbors;
		}            
	}
}


