using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace WebApiContrib.Core.Formatter.GoogleProtobuf
{
    public class ProtobufInputFormatter : InputFormatter
    {
        private readonly ProtobufFormatterOptions _options;

        public ProtobufInputFormatter(ProtobufFormatterOptions protobufFormatterOptions)
        {
            _options = protobufFormatterOptions ?? throw new ArgumentNullException(nameof(protobufFormatterOptions));
            foreach (var contentType in protobufFormatterOptions.SupportedContentTypes)
            {
                SupportedMediaTypes.Add(new MediaTypeHeaderValue(contentType));
            }
        }

        public override Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var type = context.ModelType;
            var request = context.HttpContext.Request;
            var stream = request.Body;
            MediaTypeHeaderValue requestContentType = null;
            MediaTypeHeaderValue.TryParse(request.ContentType, out requestContentType);
            
            if (_options.UseDelimitedFormat)
                return ReadDelemited(type, stream);
            else
                return ReadRegular(type, stream);
        }

        private static Task<InputFormatterResult> ReadDelemited(Type type, Stream stream)
        {
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                Debug.Assert(elementType != null, nameof(elementType) + " != null");

                var resultList = new List<IMessage>();
                while (stream.Position < stream.Length)
                {
                    var result = (IMessage) Activator.CreateInstance(elementType);
                    result.MergeDelimitedFrom(stream);
                    resultList.Add(result);
                }

                return InputFormatterResult.SuccessAsync(resultList.ToArray());
            }
            else
            {
                var result = (IMessage) Activator.CreateInstance(type);
                result.MergeDelimitedFrom(stream);
                return InputFormatterResult.SuccessAsync(result);
            }
        }

        private static Task<InputFormatterResult> ReadRegular(Type type, Stream stream)
        {
            if (type.IsArray)
            {
                throw new NotSupportedException("Reading from `IMessage` arrays is only supported in delemited mode.");
            }

            var result = (IMessage) Activator.CreateInstance(type);
            result.MergeFrom(stream);
            return InputFormatterResult.SuccessAsync(result);
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var type = context.ModelType;
            return typeof(IMessage).IsAssignableFrom(type.GetElementType() ?? type);
        }
    }
}
