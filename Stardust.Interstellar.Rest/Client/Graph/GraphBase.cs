using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Stardust.Interstellar.Rest.Client.Graph
{
    public abstract class GraphBase : IGraphItem, IInternalGraphHelper, IValidatableObject
    {
        protected string baseUrl;

        protected IGraphItem parent;

        protected IGraphCollection<TChild> CreateGraphCollection<TChild>()
        {
            if (string.IsNullOrWhiteSpace(this.baseUrl)) return null;
            var collection = new GraphCollection<TChild>();
            collection.Initialize(this);
            return collection;
        }

        protected IGraphItem<TChild> CreateGraphItem<TChild>()
        {
            if (string.IsNullOrWhiteSpace(this.baseUrl)) return null;
            var collection = new GraphItem<TChild>();
            collection.Initialize(this);
            return collection;
        }

        protected IGraphItem<TChild> CreateGraphItem<TChild>(string id)
        {
            if (string.IsNullOrWhiteSpace(this.baseUrl)) return null;
            var collection = new GraphItem<TChild>() { Id = id };
            collection.Initialize(this);
            return collection;
        }




        protected IGraphCollection<TChild> CreateGraphCollection<TChild>(string navigationNodeName, string id)
        {
            if (string.IsNullOrWhiteSpace(this.baseUrl)) return null;
            var collection = new GraphCollection<TChild>();
            collection.Initialize(this);
            collection.SetFilter(navigationNodeName, id);
            return collection;
        }


        string IInternalGraphHelper.BaseUrl
        {
            get
            {
                return InternalBaseUrl;
            }
            set
            {
                InternalBaseUrl = value;
            }
        }

        IGraphItem IInternalGraphHelper.Parent
        {
            get
            {
                return parent;
            }
            set
            {
                parent = value;
            }
        }

        protected string InternalBaseUrl
        {
            get
            {
                return baseUrl;
            }
            set
            {
                baseUrl = value;
            }
        }

        public virtual IGraphItem Initialize(IGraphItem parent)
        {
            this.parent = parent;
            this.InternalBaseUrl = ((IInternalGraphHelper)parent).BaseUrl;
            return this;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            return new List<ValidationResult> {};
        }
    }
}