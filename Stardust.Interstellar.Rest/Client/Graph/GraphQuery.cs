namespace Stardust.Interstellar.Rest.Client.Graph
{
    public class GraphQuery
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public GraphQueryPart Expression { get; set; }

        public GraphSortStatement SortStatement { get; set; }
    }

    public class GraphSortStatement
    {
        public string Field { get; set; }

        public bool Descending { get; set; }
    }

    public class GraphQuery<T> : GraphQuery
    {

    }
}