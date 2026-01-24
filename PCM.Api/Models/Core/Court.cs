namespace PCM.Api.Models.Core
{
    public class Court
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool IsActive { get; set; }

        public string? Description { get; set; }
    }
}
