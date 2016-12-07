

namespace ConsoleApplication
{
    public class WebPage
    {
        public string URL {get; set;}
        public string content {get; set;}
        public bool OK {get; set;}

        public WebPage()
        {
            this.URL = "";
            this.content = "";
            this.OK = false;
        }
        public WebPage(string URL)
        {
            this.URL = URL;
            this.content = "";
            this.OK = false;
        }
        public WebPage(WebPage a)
        {
            this.URL = a.URL;
            this.content = a.content;
            this.OK = a.OK;
        }

        public void Valide()
        {
            if (this.content.Length >= 300)
                this.OK = true;
            else this.OK = false;
        }
    }
}