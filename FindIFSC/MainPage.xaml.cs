using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace FindIFSC
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MainVM vm=null;
        public MainPage()
        {
            this.InitializeComponent();
            vm = new MainVM();
            this.DataContext = vm;
            //this.NavigationCacheMode = NavigationCacheMode.Required;
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (Detail.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                Detail.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {

            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
            //vm = (MainVM)this.DataContext;
            vm.Update += (x) => 
            {
                switch (x)
                {
                    case "BANKS":
            //            banks.ItemsSource = vm.Banks;
                        break;
                    case"STATES":
                        prg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                       // states.ItemsSource = vm.States;
                        break;
                    case "DSTS":
                        prg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                      //  dst.ItemsSource = vm.Dsts;
                        break;
                    case"BRCNS":
                        prg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                      //  brn.ItemsSource = vm.Branches;
                        break;
                    case "PROGRESS":
                        prg.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        break;
                    case "WEB": 
                        //brn.ItemsSource = null;

                       // dst.ItemsSource = null;

                        prg.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        Detail.Visibility = Windows.UI.Xaml.Visibility.Visible;
                       // web.NavigateToString(vm.Result);
                        break;
                    default:
                        break;
                }
            };
            vm.GetBankNames();
        }

       
    }
}
