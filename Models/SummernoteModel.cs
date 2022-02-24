namespace AppMVC.Models
{
    public class SummernoteModel
    {
        public SummernoteModel(string id, bool isLoadScript = true)
        {
            Id = id;
            IsLoadScript = isLoadScript;
        }

        public string Id { get; set; }
        public bool IsLoadScript { get; set; }
        public int Height { get; set; } = 120;
        public string Toolbar { get; set; } = @"[
            ['style', ['style']],
            ['font', ['bold', 'underline', 'clear']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'video']],
            ['view', ['fullscreen', 'codeview', 'help']]
            ]";
    }
}