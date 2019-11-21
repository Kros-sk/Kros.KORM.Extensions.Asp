using Kros.Utils;
using System;
using System.Text.Json;

namespace Kros.KORM.Converter
{
    /// <summary>
    /// Converter for converting JSON value from DB to entity.
    /// </summary>
    /// <seealso cref="IConverter" />
    public class JsonConverter : IConverter
    {
        private readonly Type _entityType;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverter"/> class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <exception cref="ArgumentNullException">The value of <paramref name="entityType"/> is null.</exception>
        public JsonConverter(Type entityType)
        {
            Check.NotNull(entityType, nameof(entityType));
            _entityType = entityType;
        }

        /// <summary>
        /// Converts specified JSON value from DB to entity.
        /// </summary>
        /// <param name="value">JSON value.</param>
        /// <returns>Entity.</returns>
        public object Convert(object value)
            => JsonSerializer.Deserialize((string)value, _entityType);

        /// <summary>
        /// Converts entity to JSON value for DB.
        /// </summary>
        /// <param name="value">Entity.</param>
        /// <returns>Converted JSON value for DB.</returns>
        public object ConvertBack(object value)
            => JsonSerializer.Serialize(value, _entityType);
    }
}
