using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace Timesheeter.Server
{
    /// <summary>
    /// Writes records as XML for the ".xml" endpoints: one element per record, named after its class,
    /// with one child element per public property (the same fields the JSON has), e.g.
    /// &lt;customers&gt;&lt;Customer&gt;&lt;ID&gt;…&lt;/ID&gt;&lt;Name&gt;Acme&lt;/Name&gt;…&lt;/Customer&gt;&lt;/customers&gt;.
    /// </summary>
    /// <remarks>
    /// Written by hand rather than with XmlSerializer, which can't handle DateOnly / TimeOnly.
    /// </remarks>
    public static class XmlFormat
    {
        public const string ContentType = "application/xml";

        /// <summary>A list of records under a <paramref name="root"/> element.</summary>
        public static string Write<T>(IEnumerable<T> items, string root) where T : class =>
            Document(new XElement(root, items.Select(Element)));

        /// <summary>A single record.</summary>
        public static string Write<T>(T item) where T : class =>
            Document(Element(item));

        // XDocument.ToString leaves out the <?xml ... ?> declaration, so it's added here.
        private static string Document(XElement root) =>
            $"{new XDeclaration("1.0", "utf-8", null)}{Environment.NewLine}{new XDocument(root)}";

        private static XElement Element<T>(T item) where T : class
        {
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);

            return new XElement(typeof(T).Name,
                properties.Select(p => new XElement(p.Name, Text(p.GetValue(item)))));
        }

        // The same text formats the database uses, culture-independent.
        private static string? Text(object? value) => value switch
        {
            null => null,
            DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TimeOnly t => t.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            DateTime dt => dt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            Guid g => g.ToString("D"),
            bool b => b ? "true" : "false",
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString(),
        };
    }
}
