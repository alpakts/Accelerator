namespace Litium.Accelerator.ViewModels.Search
{
    public class ListItem
    {
        public ListItem(string text, string value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }
}
