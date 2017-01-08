using AppStudio.Common.Commands;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Data.Html;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace FindIFSC
{
    class MainVM : INotifyPropertyChanged
    {
        public Action<string> Update { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public void Raise(string propertyname)
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(propertyname)); }
        }

        private bool enableFavButton = false;

        public bool EnableFavButton
        {
            get { return enableFavButton; }
            set { enableFavButton = value; Raise("EnableFavButton"); }
        }


        private List<string> banks;

        public List<string> Banks
        {
            get { return banks; }
            set { banks = value; Raise("Banks"); }
        }
        private List<string> states;

        public List<string> States
        {
            get { return states; }
            set { states = value; Raise("States"); }
        }
        private List<string> dsts;

        public List<string> Dsts
        {
            get { return dsts; }
            set { dsts = value; Raise("Dsts"); }
        }

        private List<BDetail> favouirtes = new List<BDetail>();

        public List<BDetail> Favourites
        {
            get { return favouirtes; }
            set { favouirtes = value; Raise("Favourites"); }
        }


        private BDetail bankDetail;

        public BDetail BankDetail
        {
            get { return bankDetail; }
            set { bankDetail = value; Raise("BankDetail"); }
        }
        private List<string> branches;

        public List<string> Branches
        {
            get { return branches; }
            set { branches = value; Raise("Branches"); }
        }
        public string SelectedBranch { get; set; }
        public string SelectedDst { get; set; }
        public string SelectedBank { get; set; }
        public string SelectedState { get; set; }
        public string Result { get; set; }
        public ICommand GetStateCommand
        {
            get
            {
                return new RelayCommand<object>(async (x) =>
                {
                    Update("PROGRESS");
                    var urlpath = "http://www.ifsc-code.com/ajax/fetchState.php";
                    var result = await GetResponse(urlpath, "bankName=" + x.ToString());
                    States = result.Split('\n').Where(y => y != "" && y != "--State--").Select(z => z.Trim()).ToList();
                    SelectedBank = x.ToString();
                    Update("STATES");
                });
            }
        }

        public ICommand GetDistrictCommand
        {
            get
            {
                return new RelayCommand<object>(async (x) =>
                {
                    Update("PROGRESS");
                    SelectedState = x.ToString();
                    var urlpath = "http://www.ifsc-code.com/ajax/fetchDistrict.php";
                    var result = await GetResponse(urlpath, "stateName=" + x.ToString() + "&bankName=" + SelectedBank);
                    Dsts = result.Split('\n').Where(y => y != "" && y != "--State--").Select(z => z.Trim()).ToList();

                    Update("DSTS");
                });
            }
        }

        public ICommand ShowFavDetail
        {
            get
            {
                return new RelayCommand<object>((x) =>
                {
                    EnableFavButton = false;
                    BankDetail = (x as ItemClickEventArgs).ClickedItem as BDetail;
                    Update("WEB");

                });
            }
        }

        public ICommand GetBranchCommand
        {
            get
            {
                return new RelayCommand<object>(async (x) =>
                {
                    Update("PROGRESS");
                    SelectedDst = x.ToString();
                    var urlpath = "http://www.ifsc-code.com/ajax/fetchBranch.php";
                    var result = await GetResponse(urlpath, "district=" + x.ToString() + "&bankName=" + SelectedBank);
                    Branches = result.Split('\n').Where(y => y != "" && y != "--State--").Select(z => z.Trim()).ToList();

                    Update("BRCNS");
                });
            }
        }
        public ICommand GetDetailCommand
        {
            get
            {
                return new RelayCommand<object>(async (y) =>
                    {
                        if (null != y && y is string)
                        {

                            bankDetail = new BDetail();
                            Update("PROGRESS");
                            SelectedBranch = y.ToString();//HDFC+BANK+LTD&stateName=KERALA&district=ALAPPUZHA&branch=ALAPPUZHA+-+KERALA&submit.x=26&submit.y=1
                            var urlpath = string.Format("http://www.ifsc-code.com/search.php", SelectedBank, SelectedState, SelectedDst, SelectedBranch);
                            var str = await GetResponse(urlpath,
                                string.Format("bankName={0}&stateName={1}&district={2}&branch={3}&submit.x=26&submit.y=1", SelectedBank, SelectedState, SelectedDst, SelectedBranch), true);
                            if (!string.IsNullOrEmpty(str))
                            {
                                HtmlDocument doc = new HtmlDocument();
                                doc.LoadHtml(str);
                                var node = doc.DocumentNode.Descendants().Where(x => x.Name == "div" && x.Attributes.Contains("class") && x.Attributes["class"].Value.Split().Contains("txtbox1")).FirstOrDefault();
                                if (node != null)
                                {
                                    var dt = node.InnerHtml.Replace("\r", "").Replace("\t", "").Split('\n').Where(x => x != "" && x != " " && x.Trim() != "").ToList();
                                    for (int i = 0; i < dt.Count; i++)
                                    {
                                        if (i != 5)
                                        {
                                            var st = HtmlUtilities.ConvertToText(dt[i]);
                                            var split = st.Split(':');
                                            if (split.Length == 1)
                                            {
                                                BankDetail.BankNameDetail = split[0];
                                            }
                                            else
                                            {
                                                switch (split[0])
                                                {
                                                    case "Bank":
                                                        BankDetail.Bank = split[1].Trim();
                                                        break;
                                                    case "Address":
                                                        BankDetail.Address = split[1].Trim();
                                                        break;
                                                    case "State":
                                                        BankDetail.State = split[1].Trim();
                                                        break;
                                                    case "District":
                                                        BankDetail.District = split[1].Trim();
                                                        break;
                                                    case "Branch":
                                                        BankDetail.Branch = split[1].Trim();
                                                        break;
                                                    case "IFSC Code":
                                                        BankDetail.IFSCCode = split[1].Split('(')[0].Trim();
                                                        break;
                                                    case "Branch Code":
                                                        BankDetail.BranchCode = split[1].Trim();
                                                        break;
                                                    case "MICR Code":
                                                        BankDetail.MicrCode = split[1].Trim();
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                    }
                                    EnableFavButton = true;
                                    // Result = Regex.Replace(node.InnerHtml, @"\<a\s[^\>]+\></a\>", "");
                                    Raise("BankDetail");
                                    Update("WEB");
                                }
                                //("//div[@id='thumbs']");

                            }
                        }
                    });
            }
        }

        public void GetBankNames()
        {
            ResourceLoader ld = new ResourceLoader();
            string banknames = ld.GetString("BANKS");
            Banks = banknames.Split('|').Select(x => x.Trim()).ToList();
            Update("BANKS");

        }
        private async Task<string> GetResponse(string url, string post, bool isdetail = false)
        {
            try
            {

                Uri uri = new Uri(url);
                //var handler=new Http
                var httpclient = new HttpClient();
                var dt = await httpclient.PostAsync(uri, new HttpStringContent(post, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"));
                if (isdetail)
                {
                    //string rsp = "http://www.ifsc-code.com" + dt.Headers["location"];
                    var dtres = await httpclient.GetStringAsync((dt.RequestMessage.RequestUri));
                    return dtres;
                }
                if (dt != null)
                {
                    string result = await dt.Content.ReadAsStringAsync();
                    return result = Regex.Replace(result, "<[^>]+>", "");

                    // XElement ele = XElement.Parse("<?xml version=\"1.0\" encoding=\"utf-8\" ?> <options>" + result + "</options>");
                }
                return "";
                //  httpclient.PostAsync(uri,)
            }
            catch (Exception)
            {

                return "";
            }
        }
        public string Publisher
        {
            get
            {
                return "Jashif | contact: jashif@live.com";
            }
        }

        public string AppVersion
        {
            get
            {
                return string.Format("{0}.{1}.{2}.{3}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build, Package.Current.Id.Version.Revision);
            }
        }

        public string AboutText
        {
            get
            {
                return "The application  will help to get the IFSC code numbers of banks in india";
            }
        }

        public void RateMe()
        {
            Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + "12dae2b2-91bc-4223-9fc4-82a63a526017"));
        }

        public void ShowDonate()
        {
            Launcher.LaunchUriAsync(new Uri("https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=jashif@live.com&item_name=Help for creating windows phone application&item_number=Windows phone development support&amount=2&currency_code=USD"));
            //var wnd = Window.Current.Content as Frame;
            //wnd.Navigate(typeof(DonatePage), this);
        }

        public void ShowAbout()
        {
            var wnd = Window.Current.Content as Frame;
            wnd.Navigate(typeof(AbouthisApp), this);
        }

        public void AddFavourite()
        {

            if (BankDetail != null && !string.IsNullOrEmpty(BankDetail.IFSCCode))
            {
                var ser = JsonConvert.SerializeObject(BankDetail);
                AddKeyValue(BankDetail.IFSCCode, ser);
            }
        }

        public static void AddKeyValue(string key, object value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public static T GetKeyValueFromJson<T>(string key) where T : class
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                return JsonConvert.DeserializeObject<T>(ApplicationData.Current.LocalSettings.Values[key].ToString());
            return null;
        }

        public static string GetKeyValue(string key)
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
                return ApplicationData.Current.LocalSettings.Values[key].ToString();
            return "";
        }

        public void LoadFavourites()
        {
            try
            {
                favouirtes = new List<BDetail>();
                var values = ApplicationData.Current.LocalSettings.Values;
                foreach (var val in values.Keys)
                {
                    favouirtes.Add(GetKeyValueFromJson<BDetail>(val));
                }
                Raise("Favourites");
            }
            catch (Exception)
            {

            }
        }
    }
    public class BDetail
    {
        public string BankNameDetail { get; set; }
        public string Bank { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string Branch { get; set; }
        public string IFSCCode { get; set; }
        public string BranchCode { get; set; }
        public string MicrCode { get; set; }
    }
}
