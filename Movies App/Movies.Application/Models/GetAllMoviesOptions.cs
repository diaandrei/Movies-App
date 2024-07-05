using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Application.Models
{
    [NotMapped]
    public class GetAllMoviesOptions
    {
        public string? Title { get; set; }
        public int? YearOfRelease { get; set; }
        public Guid? UserId { get; set; }
        public SortField? SortField { get; set; }
        public SortOrder? SortOrder { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public enum SortOrder
    {
        Unsorted,
        Ascending,
        Descending
    }

    public enum SortField
    {
        Title,
        YearOfRelease
    }
}
