using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Viam.Net.Sdk.Core.Utils
{
    internal static class GrpcExtensions
    {
        public static Struct ToStruct(this IDictionary<string, object> dict)
        {
            var @struct = new Struct();
            foreach (var (key, value) in dict)
            {
                @struct.Fields[key] = ConvertToValue(value);
            }

            return @struct;
        }

        public static Value ConvertToValue(this object value)
        {
            return value switch
                   {
                       string val                      => Value.ForString(val),
                       bool val                        => Value.ForBool(val),
                       double val                      => Value.ForNumber(val),
                       float val                       => Value.ForNumber(val),
                       int val                         => Value.ForNumber(val),
                       long val                        => Value.ForNumber(val),
                       uint val                        => Value.ForNumber(val),
                       ulong val                       => Value.ForNumber(val),
                       Value val                       => val,
                       IDictionary<string, object> val => Value.ForStruct(val.ToStruct()),
                       IList<object> val => Value.ForList(val.Select(ConvertToValue)
                                                             .ToArray()),
                       null => Value.ForNull(),
                       _    => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                   };
        }

        public static IDictionary<string, object?> ToDictionary(this Struct @struct)
        {
            return @struct.Fields.ToDictionary(x => x.Key, x => ConvertFromValue(x.Value));
        }

        public static object? ConvertFromValue(this Value value)
        {
            return value.KindCase switch
                   {
                       Value.KindOneofCase.None        => null,
                       Value.KindOneofCase.NullValue   => null,
                       Value.KindOneofCase.NumberValue => value.NumberValue,
                       Value.KindOneofCase.StringValue => value.StringValue,
                       Value.KindOneofCase.BoolValue   => value.BoolValue,
                       Value.KindOneofCase.StructValue => value.StructValue.ToDictionary(),
                       Value.KindOneofCase.ListValue   => value.ListValue.Values.Select(ConvertFromValue).ToList(),
                       _                               => throw new ArgumentOutOfRangeException()
                   };
        }

        public static T? ConvertFromValue<T>(this Value value) where T : class
        {
            var convertedValue = ConvertFromValue(value);
            return convertedValue as T;
        }

        public static IDictionary<string, object?> ToDictionary(this MapField<string, Value> map)
        {
            return map.ToDictionary(x => x.Key, x => ConvertFromValue(x.Value));
        }

        public static DateTime? ToDeadline(this TimeSpan? timeSpan) =>
            timeSpan.HasValue
                ? DateTime.UtcNow.Add(timeSpan.Value)
                : null;
    }
}
