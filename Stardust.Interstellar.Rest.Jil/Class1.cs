using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jil;

namespace Stardust.Interstellar.Rest.Jil
{
    public class JilSerializerFormatter : MediaTypeFormatter
    {
        public JilSerializerFormatter()
        {
            SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
        }
        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return Task<object>.Factory.StartNew(() =>Deserialize(type, readStream, formatterLogger));
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger,CancellationToken cancellationToken)
        {
            return Task<object>.Factory.StartNew(() => Deserialize(type, readStream, formatterLogger),cancellationToken);
        }

        private static object Deserialize(Type type, Stream readStream, IFormatterLogger formatterLogger)
        {
            try
            {
                using (var reader = new StreamReader(readStream))
                {
                    return JSON.Deserialize(reader, type,Options.ISO8601ExcludeNulls);
                }
            }
            catch (Exception ex)
            {
                formatterLogger?.LogError("", ex);
                throw;
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,
            TransportContext transportContext)
        {
            var task = Task.Factory.StartNew(() => Serialize(value, writeStream));
            return task;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content,TransportContext transportContext, CancellationToken cancellationToken)
        {
            var task = Task.Factory.StartNew(() => Serialize(value, writeStream),cancellationToken);
            return task;
        }

        private static void Serialize(object value, Stream writeStream)
        {
            using (var writer = new StreamWriter(writeStream))
            {
                JSON.Serialize(value, writer, Options.ISO8601ExcludeNulls);
            }
        }

        
    }
}
