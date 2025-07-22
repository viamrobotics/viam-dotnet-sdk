using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

using System;
using System.Collections.Generic;
using System.Linq;

using Viam.Core.Resources;

namespace Viam.Core.Utils
{
    public static class GrpcExtensions
    {
        public static Struct ToStruct(this IDictionary<string, object?>? dict)
        {
            var @struct = new Struct();
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
                System.Enum val => Value.ForString(val.ToString()),
                Value val => val,
                IDictionary<string, object?> val => Value.ForStruct(val.ToStruct()),
                IList<object> val => Value.ForList(val.Select(ConvertToValue)
                    .ToArray()),
                null => Value.ForNull(),
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
            };
        }

        public static ViamDictionary ToDictionary(this Struct @struct)
        {
            return new ViamDictionary(@struct.Fields.ToDictionary(x => x.Key, x => ConvertFromValue(x.Value)));
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
                Value.KindOneofCase.StructValue => value.StructValue.ToDictionary(),
                Value.KindOneofCase.ListValue => value.ListValue.Values.Select(ConvertFromValue).ToList(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static T? ConvertFromValue<T>(this Value value) where T : class
        {
            var convertedValue = ConvertFromValue(value);
            return convertedValue as T;
        }

        public static Dictionary<string, object?> ToDictionary(this MapField<string, Value> map)
        {
            return map.ToDictionary(x => x.Key, x => ConvertFromValue(x.Value));
        }

        public static void Add(this MapField<string, Value> map, IDictionary<string, object?> dict)
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