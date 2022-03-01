namespace Contracts.Catalog
{
    public class SampleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public virtual DateTime CreatedDate { get; set; }
    }
}