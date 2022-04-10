namespace LCDonald.Core.Model
{
    public record LCDGameInput
    {
        
        public string Name { get; init; }

        public string Description { get; init; }

        public int KeyCode { get; set; }

    }

}