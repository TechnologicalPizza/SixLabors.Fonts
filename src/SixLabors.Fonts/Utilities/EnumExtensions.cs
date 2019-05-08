using System;
using System.Linq.Expressions;

namespace SixLabors
{
    internal static class EnumExtensions
    {
        /// <summary>
        /// Determines whether the specified value has flags.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="flags">The flag.</param>
        /// <returns>
        ///  <c>true</c> if the specified value has flags; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFlags<TEnum>(this TEnum value, TEnum flags) where TEnum : Enum
        {
            return EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flags);
        }

        /// <summary>
        /// Determines whether the specified value has flags.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="flag0">The first flag.</param>
        /// <param name="flag1">The second flag.</param>
        /// <returns>
        ///  <c>true</c> if the specified value has flags; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFlags<TEnum>(
            this TEnum value, TEnum flag0, TEnum flag1) where TEnum : Enum
        {
            if (EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag0) ||
                EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag1))
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the specified value has flags.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="flag0">The first flag.</param>
        /// <param name="flag1">The second flag.</param>
        /// <param name="flag2">The second flag.</param>
        /// <returns>
        ///  <c>true</c> if the specified value has flags; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFlags<TEnum>(
            this TEnum value, TEnum flag0, TEnum flag1, TEnum flag2) where TEnum : Enum
        {
            if (EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag0) ||
                EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag1) ||
                EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flag2))
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether the specified value has multiple flags.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>
        ///  <c>true</c> if the specified value has any flag; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFlags<TEnum>(this TEnum value, params TEnum[] flags) where TEnum : Enum
        {
            for (int i = 0; i < flags.Length; i++)
            {
                if (EnumExtensionsInternal<TEnum>.HasFlagsDelegate(value, flags[i]))
                    return true;
            }
            return false;
        }

        private static class EnumExtensionsInternal<TEnum> where TEnum : Enum
        {
            public static readonly Func<TEnum, TEnum, bool> HasFlagsDelegate = CreateHasFlagDelegate();

            private static Func<TEnum, TEnum, bool> CreateHasFlagDelegate()
            {
                ParameterExpression valueExpression = Expression.Parameter(typeof(TEnum));
                ParameterExpression flagExpression = Expression.Parameter(typeof(TEnum));
                ParameterExpression flagValueVariable = Expression.Variable(
                    Type.GetTypeCode(typeof(TEnum)) == TypeCode.UInt64 ? typeof(ulong) : typeof(long));

                var body = Expression.Block(
                    new[] { flagValueVariable },

                    Expression.Assign(
                        flagValueVariable,
                        Expression.Convert(
                            flagExpression, flagValueVariable.Type)),

                    Expression.Equal(
                        Expression.And(
                            Expression.Convert(
                                valueExpression, flagValueVariable.Type),
                            flagValueVariable),
                        flagValueVariable)
                );

                var lambda = Expression.Lambda<Func<TEnum, TEnum, bool>>(
                    body, valueExpression, flagExpression);

                return lambda.Compile();
            }
        }
    }
}
