using CerealPlayer.Models.Hoster;

namespace CerealPlayer.Models.Web
{
    public class WebModel
    {
        public WebModel(Models models)
        {
            VideoHoster = new VideoHosterModel(models);
            Html = new HtmlProvider(models.Settings.MaxChromiumInstances);
        }

        public HtmlProvider Html { get; }
        public VideoHosterModel VideoHoster { get; }

        public void Dispose()
        {
            Html.Dispose();
        }
    }
}