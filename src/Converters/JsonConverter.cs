using Kros.Utils;
using System;
using System.Text.Json;

namespace Kros.KORM.Converter
{
    /// <summary>
    /// Converter for converting JSON value from DB to entity.
    /// </summary>
    /// <seealso cref="IConverter" />
    public class JsonConverter<T> : IConverter
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverter{T}"/> class with default serialization options.
        /// </summary>
        public JsonConverter() : this(new JsonSerializerOptions())
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverter{T}"/> class.
        /// </summary>
        /// <param name="options">Serialization options.</param>
        /// <exception cref="ArgumentNullException">The value of <paramref name="options"/> is null.</exception>
        public JsonConverter(JsonSerializerOptions options)
        {
            _options = Check.NotNull(options, nameof(options));
        }

        /// <summary>
        /// Converts specified JSON value from DB to entity.
        /// </summary>
        /// <param name="value">JSON value.</param>
        /// <returns>Entity.</returns>
        public object Convert(object value)
            => JsonSerializer.Deserialize((string)value, typeof(T), _options);

        /// <summary>
        /// Converts entity to JSON value for DB.
        /// </summary>
        /// <param name="value">Entity.</param>
        /// <returns>Converted JSON value for DB.</returns>
        public object ConvertBack(object value)
            => JsonSerializer.Serialize(value, typeof(T), _options);
    }
}
