using System;
using System.Collections.Generic;
using System.Linq;
using Viam.Contracts.Resources;
using Value = Google.Protobuf.WellKnownTypes.Value;

namespace Viam.Contracts
{
    public static class GrpcExtensions
    {
        public static Google.Protobuf.WellKnownTypes.Struct ToProto(this IDictionary<string, object?>? dict)
        {
            var @struct = new Google.Protobuf.WellKnownTypes.Struct();
            if (dict == null)
            {
                return @struct;
            }

            foreach (var kvp in dict)
            {
                @struct.Fields[kvp.Key] = ConvertToValue(kvp.Value);
            }

            return @struct;
        }

        public static Value ConvertToValue(this object? value)
        {
            return value switch
            {
                string val => Value.ForString(val),
                bool val => Value.ForBool(val),
                double val => Value.ForNumber(val),
                float val => Value.ForNumber(val),
                short val => Value.ForNumber(val),
                ushort val => Value.ForNumber(val),
                int val => Value.ForNumber(val),
                uint val => Value.ForNumber(val),
                long val => Value.ForNumber(val),
                ulong val => Value.ForNumber(val),
                Enum val => Value.ForString(val.ToString()),
                Value val => val,
                IDictionary<string, object?> val => Value.ForStruct(val.ToProto()),
                IList<object> val => Value.ForList(val.Select(ConvertToValue)
                    .ToArray()),
                null => Value.ForNull(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static object? ConvertFromValue(this Value value)
        {
            return value.KindCase switch
            {
                Value.KindOneofCase.None => null,
                Value.KindOneofCase.NullValue => null,
                Value.KindOneofCase.NumberValue => value.NumberValue,
                Value.KindOneofCase.StringValue => value.StringValue,
                Value.KindOneofCase.BoolValue => value.BoolValue,
                Value.KindOneofCase.StructValue => value.StructValue,
                Value.KindOneofCase.ListValue => value.ListValue.Values.Select(ConvertFromValue).ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static void Add(this Google.Protobuf.Collections.MapField<string, Value> map, IDictionary<string, object?> dict)
        {
            foreach (var kvp in dict)
            {
                map.Add(kvp.Key, ConvertToValue(kvp.Value));
            }
        }

        public static DateTime? ToDeadline(this TimeSpan? timeSpan) =>
            timeSpan.HasValue
                ? DateTime.UtcNow.Add(timeSpan.Value)
                : null;

        public static DateTime? ToDeadline(this TimeSpan timeSpan) =>
            DateTime.UtcNow.Add(timeSpan);

        public static TimeSpan? ToTimeout(this DateTime? dateTime) =>
            dateTime.HasValue
                ? dateTime - DateTime.UtcNow
                : null;

        public static TimeSpan ToTimeout(this DateTime dateTime) =>
            dateTime - DateTime.UtcNow;

        public static ViamResourceName ToResourceName(this string resourceName)
        {
            var parts = resourceName.Split('/');
            if (parts.Length != 2)
            {
                throw new ArgumentException($"{resourceName} is not a valid ResourceName");
            }

            var subType = SubType.FromString(parts[0]);
            return new ViamResourceName(subType, parts[1]);
        }
    }
}