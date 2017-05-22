namespace Stardust.Interstellar.Rest.Client.Graph
{
    public class GraphQueryPart
    {
        public string Operator { get; set; }

        public string FieldName { get; set; }

        public string Term { get; set; }

        public GraphQueryPart Expression { get; set; }
    }
}