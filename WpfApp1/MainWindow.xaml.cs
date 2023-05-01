using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded( object sender, RoutedEventArgs e )
        {
            startTheDataExtraction();
        }
        private static string APIUrl( string APIProcedure )
        {

            return "http://" + "localhost" + ":" + "47765" + "/api/" + APIProcedure;// + "/" + Globals.version;

        }

        private async Task startTheDataExtraction()
        {
            Task<JObject> task = Task<JObject>.Run( async () =>
            {
                JObject r = new JObject();
                try
                {
                    r = await getControllersData();
                    Debug.WriteLine( 3 );
                }
                catch( Exception ex )
                {

                }

                return r;

            });

            Task UITask = task.ContinueWith( ( ret ) =>
            {
                // refresh the UI
                var jO = ret.Result;

                try
                {
                    if( jO != null )
                    {
                        if( jO.HasValues )
                        {
                            //refreshTheUI( jO );
                        }
                    }
                    else
                    {
                        // problematic execution or data 
                    }
                }
                catch( Exception ex )
                {

                }

                // reshedule next interaction
                resheduleDataExtraction();
            },
            TaskScheduler.FromCurrentSynchronizationContext() );
        }

        public static DateTime lastSuccessfulDataExtraction = DateTime.MinValue;

        public void resheduleDataExtraction()
        {
            Task.Delay( 1000 ).ContinueWith( async ( d ) =>
            {
                await startTheDataExtraction();
            } );
        }

        public static async Task<JObject> getControllersData()
        {
            JObject r = new JObject();

            JObject request = new JObject();

            request.Add( "GUIDS", new JArray(  ) );

            try
            {
                r = await postJSON( request, "Readings" );

                if( r != null )// there is a data
                {
                    lastSuccessfulDataExtraction = DateTime.Now;
                }
            }
            catch( Exception e )
            {
            }
            return r;
        }



        public static async Task<JObject> postJSON( JObject jo, string APIProcedure )
        {
            JObject r = new JObject();

            jo.Add( "sessionID", "ddd" );
            jo.Add( "version", "1" );

            try
            {
                var webRequest = WebRequest.Create( APIUrl( APIProcedure ) );
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";

                using( Stream postStream = await webRequest.GetRequestStreamAsync() )
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes( jo.ToString() );
                    await postStream.WriteAsync( byteArray, 0, byteArray.Length );
                    await postStream.FlushAsync();
                }
                try
                {
                    string Response;
                    using( var response = ( HttpWebResponse ) await webRequest.GetResponseAsync() )
                    using( Stream streamResponse = response.GetResponseStream() )
                    using( StreamReader streamReader = new StreamReader( streamResponse ) )
                    {
                        Response = await streamReader.ReadToEndAsync();
                    }
                    if( Response == "" )
                    {
                        //show some error msg to the user        
                        Debug.WriteLine( "ERROR" );

                    }
                    else
                    {
                        //Your response will be available in "Response" 
                        //Debug.WriteLine( Response );
                        r = JObject.Parse( Response );
                    }
                }
                catch( WebException )
                {
                    //error    
                }
            }
            catch( Exception ex )
            {
                Debug.WriteLine( "x" );
            }
            return r;
        }

    }
}
