using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace WaveShaper.Core.Utilities
{
    public class EnumUtil
    {
        protected static readonly IDictionary<Type, EnumMappings> Enums = new Dictionary<Type, EnumMappings>();

        private static EnumMappings GetEnum(Type enumType)
        {
            if (!Enums.TryGetValue(enumType, out EnumMappings em))
            {
                em = new EnumMappings(enumType);
                Enums[enumType] = em;
            }

            return em;
        }

        public static IEnumerable<Enum> AllValues(Type enumType)
        {
            if (!typeof(Enum).IsAssignableFrom(enumType))
                throw new ArgumentException("enumType must be an Enum type");

            return GetEnum(enumType).Values;
        }

        public static IEnumerable<EnumListItem> AllValuesForDdl(Type enumType)
        {
            if (!typeof(Enum).IsAssignableFrom(enumType))
                throw new ArgumentException("enumType must be an Enum type");

            return GetEnum(enumType).Values
                                    .Select(e => new EnumListItem
                                    {
                                        Text = ToDisplayString(e),
                                        Value = e
                                    });
        }

        public static string ToDisplayString(Enum @enum)
        {
            var em = GetEnum(@enum.GetType());

            if (em.DisplayStrings.TryGetValue(@enum, out string displayString))
                return displayString;

            string stringEnum = @enum.ToString();
            var fi = @enum.GetType().GetField(stringEnum);

            displayString = fi.GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attr ? attr.Description : stringEnum;

            em.DisplayStrings[@enum] = displayString;

            return displayString;
        }


        protected class EnumMappings
        {
            private readonly Type enumType;

            private IList<Enum> values;
            private static IDictionary<Enum, string> enumToDisplayString;

            public EnumMappings(Type enumType)
            {
                this.enumType = enumType;
            }

            public IEnumerable<Enum> Values => values ?? (values = Enum.GetValues(enumType).OfType<Enum>().ToList());

            public IDictionary<Enum, string> DisplayStrings => enumToDisplayString ?? (enumToDisplayString = new Dictionary<Enum, string>());

        }

        public class EnumListItem
        {
            [UsedImplicitly]
            public string Text { get; set; }

            [UsedImplicitly]
            public object Value { get; set; }
        }
    }

    public class EnumUtil<TEnum> : EnumUtil
        where TEnum : struct, IConvertible // limit to enum
    {
        private static IList<TEnum> values;
        private static IDictionary<TEnum, string> enumToDisplayString;
        private static IDictionary<TEnum, string> enumToValueString;
        private static IDictionary<string, TEnum> valueStringToEnum;

        public static IEnumerable<TEnum> Values => values ?? (values = Enum.GetValues(typeof(TEnum)).OfType<TEnum>().ToList());

        #region Mappings

        private static IDictionary<TEnum, string> DisplayStrings => enumToDisplayString ?? (enumToDisplayString = new Dictionary<TEnum, string>());

        private static IDictionary<TEnum, string> ValueStrings => enumToValueString ?? (enumToValueString = new Dictionary<TEnum, string>());

        private static IDictionary<string, TEnum> ValueStringToEnum
        {
            get
            {
                if (valueStringToEnum != null)
                    return valueStringToEnum;

                valueStringToEnum = Values.Select(e => new
                                          {
                                              Enum = e,
                                              Attr = e.GetType()
                                                      .GetField(e.ToString(CultureInfo.InvariantCulture))
                                                      .GetCustomAttribute(typeof(EnumStringValueAttribute), false) as EnumStringValueAttribute
                                          })
                                          .Where(x => x.Attr != null)
                                          .ToDictionary(x => x.Attr.StringValue, x => x.Enum);

                return valueStringToEnum;
            }
        }

        #endregion

        #region To enum

        public static string ToDisplayString(TEnum @enum)
        {
            if (DisplayStrings.TryGetValue(@enum, out string displayString))
                return displayString;

            string stringEnum = @enum.ToString(CultureInfo.InvariantCulture);
            var fi = @enum.GetType().GetField(stringEnum);

            displayString = fi.GetCustomAttribute(typeof(DescriptionAttribute), false) is DescriptionAttribute attr ? attr.Description : stringEnum;

            DisplayStrings[@enum] = displayString;

            return displayString;
        }

        public static string ToDisplayString(TEnum? @enum)
        {
            return @enum == null ? null : ToDisplayString(@enum.Value);
        }

        public static string ToValueString(TEnum @enum)
        {
            if (ValueStrings.TryGetValue(@enum, out string valueString))
                return valueString;

            var stringEnum = @enum.ToString(CultureInfo.InvariantCulture);
            var fi = @enum.GetType().GetField(stringEnum);

            valueString = fi.GetCustomAttribute(typeof(EnumStringValueAttribute), false) is EnumStringValueAttribute attr ? attr.StringValue : stringEnum;

            ValueStrings[@enum] = valueString;

            return valueString;
        }

        public static string ToValueString(TEnum? @enum)
        {
            return @enum == null ? null : ToValueString(@enum.Value);
        }

        #endregion

        #region From enum

        public static TEnum FromValueString(string valueString)
        {
            if (!ValueStringToEnum.TryGetValue(valueString, out TEnum @enum))
                throw new InvalidCastException(valueString + " is not a defined string value for enum type " + typeof(TEnum).FullName);

            return @enum;
        }

        public static TEnum? FromValueStringSafe(string valueString)
        {
            if (!ValueStringToEnum.TryGetValue(valueString, out TEnum @enum))
                return null;

            return @enum;
        }

        public static TEnum FromRawValue(object rawValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), rawValue))
                throw new InvalidCastException(rawValue + " is not a defined value for enum type " + typeof(TEnum).FullName);

            return (TEnum)rawValue;
        }

        public static TEnum Parse(string enumValue)
        {
            var @enum = (TEnum)Enum.Parse(typeof(TEnum), enumValue);

            if (!IsDefined(@enum))
                throw new ArgumentException($"{enumValue} is not a defined value for enum type {typeof(TEnum).FullName}");

            return @enum;
        }

        #endregion

        public static bool IsDefined(TEnum @enum) => Enum.IsDefined(typeof(TEnum), @enum);
    }

    public static class EnumUtilExtensions
    {
        public static string ToDisplayString<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible // limit to enum
        {
            return EnumUtil<TEnum>.ToDisplayString(@enum);
        }

        public static string ToDisplayString<TEnum>(this TEnum? @enum)
            where TEnum : struct, IConvertible // limit to enum
        {
            return EnumUtil<TEnum>.ToDisplayString(@enum);
        }

        public static string ToValueString<TEnum>(this TEnum @enum)
            where TEnum : struct, IConvertible // limit to enum
        {
            return EnumUtil<TEnum>.ToValueString(@enum);
        }

        public static string ToValueString<TEnum>(this TEnum? @enum)
            where TEnum : struct, IConvertible // limit to enum
        {
            return EnumUtil<TEnum>.ToValueString(@enum);
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EnumStringValueAttribute : Attribute
    {
        public readonly string StringValue;

        public EnumStringValueAttribute(string stringValue)
        {
            this.StringValue = stringValue;
        }
    }
}
