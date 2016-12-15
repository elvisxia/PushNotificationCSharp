using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Newtonsoft;
using System.Net;
using System.Text;
using Windows.Networking.PushNotifications;
using System.Net.Http.Headers;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PushNotificationCSharp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private HttpClient _client;
        public HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                }
                return _client;
            }
        }
        public Model ModelData { get; set; }
        public MainPage()
        {
            this.InitializeComponent();
            ModelData = new Model();
            ModelData.BaseUri = "https://login.live.com";
            ModelData.RequestUri = "https://login.live.com/accesstoken.srf";
            ModelData.ClientId = WebUtility.UrlEncode("ms-app://s-1-15-2-3406259235-1763561558-611599895-3551440097-2866007150-2621388571-3259634121");
            ModelData.ClientSecret = WebUtility.UrlEncode("Wha3nuNWisa5VSafsqr6rUEkmGpM9bra");
            //Client.BaseAddress = new Uri(ModelData.BaseUri);
        }

        private async void btnAuth_Click(object sender, RoutedEventArgs e)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(ModelData.RequestUri);
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(ModelData.Body, Encoding.UTF8,"application/x-www-form-urlencoded");
            HttpResponseMessage response = await Client.SendAsync(request);
            if (response.IsSuccessStatusCode == true)
            {
                String JsonStr = await response.Content.ReadAsStringAsync();
                AccessToken token = JsonConvert.DeserializeObject<AccessToken>(JsonStr);
                tbAuth.Text = token.access_token;
                ModelData.AccessToken = token.access_token;
                ModelData.TokenType = token.token_type;
            }
            else
            {
                tbAuth.Text = "Auth Failed";
            }
        }

        private async void btnChannel_Click(object sender, RoutedEventArgs e)
        {
            PushNotificationChannel channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            ModelData.ChannelUri = channel.Uri;
            tbChannel.Text = channel.Uri;
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            String xmlData=null;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Notifications/ToastNotification.xml"));
            using (Stream stream=await file.OpenStreamForReadAsync())
            {
                StreamReader reader = new StreamReader(stream);
                xmlData=await reader.ReadToEndAsync();
            }

            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(ModelData.ChannelUri);
            request.Headers.Authorization = new AuthenticationHeaderValue(ModelData.TokenType, ModelData.AccessToken);
            request.Headers.TryAddWithoutValidation("X-WNS-Type", "wns/toast");
            request.Method = HttpMethod.Post;
            request.Content = new StringContent(xmlData,Encoding.UTF8, "text/xml");
            HttpResponseMessage response = await Client.SendAsync(request);
            
         }
    }

    public class AccessToken
    {
        public String access_token;
        public String token_type;
    }

    public class Model
    {
        public String BaseUri { get; set; }
        public String RequestUri { get; set; }
        public String AccessToken { get; set; }
        public String TokenType { get; set; }
        public String ChannelUri { get; set; }
        public String NotificationXml { get; set; }
        public String ClientId { get; set; }
        public String ClientSecret { get; set; }

        public String Body
        {
            get
            {
                return "grant_type=client_credentials&client_id=clientId&client_secret=clientSecret&scope=notify.windows.com".Replace("clientId", this.ClientId).Replace("clientSecret", this.ClientSecret);
            }
        }
    }
}
