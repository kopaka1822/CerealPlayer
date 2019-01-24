using CerealPlayer.Models.Hoster;

namespace CerealPlayer.Models.Web
{
    public class WebModel
    {
        public WebModel(Models models)
        {
            VideoHoster = new VideoHosterModel(models);
        }

        public HtmlProvider Html { get; } = new HtmlProvider();
        public VideoHosterModel VideoHoster { get; }

        public void Dispose()
        {
            Html.Dispose();
        }
    }
}