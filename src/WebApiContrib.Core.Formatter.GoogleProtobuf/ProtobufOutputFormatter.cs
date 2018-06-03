using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace WebApiContrib.Core.Formatter.GoogleProtobuf
{
    public class ProtobufOutputFormatter :  OutputFormatter
    {
        private readonly ProtobufFormatterOptions _options;

        public ProtobufOutputFormatter(ProtobufFormatterOptions protobufFormatterOptions)
        {
            ContentType = "application/x-protobuf";
            _options = protobufFormatterOptions ?? throw new ArgumentNullException(nameof(protobufFormatterOptions));
            foreach (var contentType in protobufFormatterOptions.SupportedContentTypes)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(contentType));
            }
        }

        public string ContentType { get; private set; }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            var response = context.HttpContext.Response;

            if (_options.UseDelimitedFormat)
                WriteDelemited(context, response);
            else
                WriteRegular(context, response);

            return Task.FromResult(response);
        }

        private static void WriteDelemited(OutputFormatterWriteContext context, HttpResponse response)
        {
            var objects = GetIMessageArray(context);
            foreach (IMessage message in objects)
            {
                message.WriteDelimitedTo(response.Body);
            }
        }

        private static void WriteRegular(OutputFormatterWriteContext context, HttpResponse response)
        {
            if (context.ObjectType.IsArray)
                throw new NotSupportedException("Writing of `IMessage` arrays is only supported in delemited mode.");

            var message = (IMessage) context.Object;
            message.WriteTo(response.Body);
        }

        private static Array GetIMessageArray(OutputFormatterWriteContext context)
        {

            if (context.ObjectType.IsArray)
                return (Array) context.Object;
            
            return new[] {context.Object};
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(IMessage).IsAssignableFrom(type.GetElementType() ?? type);
        }
    }
}
