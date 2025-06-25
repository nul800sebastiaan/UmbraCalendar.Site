using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;

namespace UmbraCalendar.DeliveryApi;

public class DateFromSort : ISortHandler, IContentIndexHandler
{
    private const string FieldAlias = "dateFrom";
    private const string SortOptionSpecifier = $"{FieldAlias}:";

    // Querying
    public bool CanHandle(string query)
        => query.StartsWith(SortOptionSpecifier, StringComparison.OrdinalIgnoreCase);

    public SortOption BuildSortOption(string sort)
    {
        var sortDirection = sort.Substring(SortOptionSpecifier.Length);

        return new SortOption
        {
            FieldName = FieldAlias,
            Direction = sortDirection.StartsWith("asc", StringComparison.OrdinalIgnoreCase)
                ? Direction.Ascending
                : Direction.Descending
        };
    }

    // Indexing
    public IEnumerable<IndexFieldValue> GetFieldValues(IContent content, string? culture)
    {
        var dateFrom = content.GetValue<DateTime?>(FieldAlias);

        if (dateFrom is null)
        {
            return Enumerable.Empty<IndexFieldValue>();
        }

        return new[]
        {
            new IndexFieldValue
            {
                FieldName = FieldAlias,
                Values = new object[] { dateFrom }
            }
        };
    }

    public IEnumerable<IndexField> GetFields()
        => new[]
        {
            new IndexField
            {
                FieldName = FieldAlias,
                FieldType = FieldType.Date,
                VariesByCulture = false
            }
        };
}