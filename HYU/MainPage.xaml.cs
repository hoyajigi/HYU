using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

using System.Xml.Linq;
using System.ServiceModel.Syndication;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;

// EUCKR <--> Unicode String conversion library for Windows Phone 7 Mango
// Written by Kyle Koh
// Oct. 26, 2011
// kyle at hcil dot snu dot ac dot kr
//
// All codes are provided 'as-is', with NO warranty.
//
// ConversionTable.dat is made from cp949.txt written by
// Shawn.Steele at microsoft dot com
// and has been modified into a binary form to be read faster in a mobile environment
//
// You are free to modify, use, or distribute this code for both commercial and non-commercial use.
// However, if you want to distribute the source code of the modified version to the public,
//		please indicate the original author and briefly explain how you modified the original code for educational purpose.
// If you don't want to distribute your modified code, then you are free to distribute the program w/o credit to the original author

using System.IO.IsolatedStorage;
using System.Text;
using System.Windows.Resources;

namespace EUCKR_Unicode_Library
{
    public class EUCKR_Unicode_Converter
    {
        private static List<Byte[]> unicodeToEucKrTable;
        private static List<Char> eucKrToUnicodeTable;

        /// <summary>
        /// Initialize the conversion table
        /// </summary>
        private static void Initialize()
        {
            if (unicodeToEucKrTable == null)
            {
                unicodeToEucKrTable = new List<Byte[]>(65536);
            }
            else
            {
                unicodeToEucKrTable.Clear();
            }

            if (eucKrToUnicodeTable == null)
            {
                eucKrToUnicodeTable = new List<Char>(65536);
            }
            else
            {
                eucKrToUnicodeTable.Clear();
            }


            IsolatedStorageFile isolatedStorageFile = IsolatedStorageFile.GetUserStoreForApplication();
            StreamResourceInfo streamInfo = Application.GetResourceStream(new Uri("ConversionTable.dat", UriKind.Relative));
            BinaryReader br = new BinaryReader(streamInfo.Stream, Encoding.Unicode);

            for (Int32 i = 0; i < 65536; ++i)
            {
                Byte[] newData = new Byte[2];
                newData[1] = br.ReadByte();
                newData[0] = br.ReadByte();
                Char c = BitConverter.ToChar(newData, 0);
                eucKrToUnicodeTable.Add(c);

            }
            for (Int32 i = 0; i < 65536; ++i)
            {
                Byte[] newData = new Byte[2];
                newData[0] = br.ReadByte();
                newData[1] = br.ReadByte();
                unicodeToEucKrTable.Add(newData);
            }

            streamInfo.Stream.Close();
            br.Close();
        }

        /// <summary>
        /// Take an EUC-KR byte array and return a corresponding unicode string
        /// </summary>
        /// <param name="euckrStringBytes">EUC-KR byte array</param>
        /// <returns>Unicode String</returns>
        public static String GetUnicodeString(Byte[] euckrStringBytes)
        {
            if (eucKrToUnicodeTable == null)
            {
                Initialize();
            }
            StringBuilder stringBuilder = new StringBuilder();

            Int32 movingIndex = 0;
            while (movingIndex < euckrStringBytes.Length)
            {
                if (euckrStringBytes[movingIndex] < 129)
                {
                    Byte[] euckrChar = { euckrStringBytes[movingIndex] };
                    if(GetUnicodeChar(euckrChar)!=0)
                        stringBuilder.Append(GetUnicodeChar(euckrChar));
                    movingIndex += 1;
                }
                else
                {
                    Byte[] euckrChar = { euckrStringBytes[movingIndex], euckrStringBytes[movingIndex + 1] };
                    if (GetUnicodeChar(euckrChar) != 0)
                        stringBuilder.Append(GetUnicodeChar(euckrChar)); 
                    movingIndex += 2;
                }
            }

            return stringBuilder.ToString();
        }

        private static Char GetUnicodeChar(Byte[] euckrBytes)
        {
            Byte[] newBytes = new Byte[2];
            if (euckrBytes.Length == 1)
            {
                newBytes[0] = euckrBytes[0];
                newBytes[1] = 0;
            }
            else
            {
                newBytes[0] = euckrBytes[1];
                newBytes[1] = euckrBytes[0];
            }
            Int32 lookupIndex = (Int32)BitConverter.ToUInt16(newBytes, 0);

         //   if (eucKrToUnicodeTable[lookupIndex] == 0)
         //       return '*';
            return eucKrToUnicodeTable[lookupIndex];
        }

        /// <summary>
        /// Take a unicode string and return a corresponding EUC-KR byte array
        /// </summary>
        /// <param name="unicodeString"></param>
        /// <returns></returns>
        public static Byte[] GetEucKRString(String unicodeString)
        {
            if (unicodeToEucKrTable == null)
            {
                Initialize();
            }

            List<Byte> euckrStringBytes = new List<Byte>(unicodeString.Length * 2);

            foreach (Char c in unicodeString)
            {
                foreach (Byte b in GetEucKR(c))
                {
                    euckrStringBytes.Add(b);
                }
            }

            return euckrStringBytes.ToArray();
        }

        private static Byte[] GetEucKR(Char unicodeChar)
        {
            Byte[] unicodeBytes = BitConverter.GetBytes(unicodeChar);

            Int32 lookupIndex = (Int32)BitConverter.ToUInt16(unicodeBytes, 0);

            Byte[] euckrBytes = unicodeToEucKrTable[lookupIndex];

            if (lookupIndex < 33024)
            { // if leading header is less than 0x81
                Byte[] newReturnValue = { euckrBytes[1] };
                return newReturnValue;
            }
            else
            {
                return euckrBytes;
            }
        }
    }
}


namespace HYU
{
    public partial class MainPage : PhoneApplicationPage
    {
        List<Book> books1 = new List<Book>(), books2 = new List<Book>();
        Stream stream=null;
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
         //   DataContext = App.ViewModel;
           // this.Loaded += new RoutedEventHandler(MainPage_Loaded);


            WebClient WC1 = new WebClient(), WC2 = new WebClient();
            WC1.DownloadStringAsync(new Uri("http://library.hanyang.ac.kr/paiknam/jsp/svp/rss/RSSController.jsp?act=RSSAction&model=RSSModel&method=bbs&bbsTypeId=000000000001&bbsId=000000000101"));
            
            WC1.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(Completed1);

            WC2.OpenReadCompleted += delegate(object sender, OpenReadCompletedEventArgs e)
            {
                stream = e.Result;
                byte[] buffer = new byte[1000000];
                stream.BeginRead(buffer, 0, 1000000, new AsyncCallback(StreamingReadCallBack), null);

                string rssContent;
                rssContent = EUCKR_Unicode_Library.EUCKR_Unicode_Converter.GetUnicodeString(buffer);
                //                rssContent = e.Result;
                //rssContent = HttpUtility.HtmlDecode(Regex.Replace(rssContent, "euc-kr", "utf-8"));
                rssContent = Regex.Replace(rssContent, "euc-kr", "utf-8");
                
                XDocument rssParser = XDocument.Parse(rssContent);
                //LINQ
                var rssList = from rssTree in rssParser.Descendants("item")
                              select new Book
                              {
                                  Title = HttpUtility.HtmlDecode(Regex.Replace(rssTree.Element("title").Value, "<[^>]+?>", "")),

                                  Contents = rssTree.Element("author").Value,
                                  Img = new Uri(rssTree.Element("link").Value, UriKind.Absolute),
                                  Url = rssTree.Element("link").Value

                              };


                FirstListBox.ItemsSource = rssList;

                books2 = rssList.ToList();
            };
            WC2.OpenReadAsync(new Uri("http://www.hanyang.ac.kr/rss/notice.html"));
           
        }
        private void StreamingReadCallBack(IAsyncResult asyncResult)
        {
            int read = stream.EndRead(asyncResult);
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //     WebBrowserTask blogViewer = new WebBrowserTask();
            //   blogViewer.URL = books[listBox1.SelectedIndex].Url;
            //   blogViewer.Show();
            NavigationService.Navigate(new Uri("/Page1.xaml?id="+Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(books2[FirstListBox.SelectedIndex].Url)), UriKind.Relative));
        }
        private void listBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //     WebBrowserTask blogViewer = new WebBrowserTask();
            //   blogViewer.URL = books[listBox1.SelectedIndex].Url;
            //   blogViewer.Show();
            NavigationService.Navigate(new Uri("/Page1.xaml?id=" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(books1[SecondListBox.SelectedIndex].Url)), UriKind.Relative));
        }

        void Completed1(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            string rssContent;
            try
            {
                rssContent = e.Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No Internet Connection");
                return;
            }
            XDocument rssParser = XDocument.Parse(rssContent);
            //LINQ
            var rssList = from rssTree in rssParser.Descendants("item")
                          select new Book
                          {
                              Title = HttpUtility.HtmlDecode(Regex.Replace(rssTree.Element("title").Value, "<[^>]+?>", "")),

                              Contents = rssTree.Element("author").Value,
                              Img = new Uri(rssTree.Element("link").Value, UriKind.Absolute),
                              Url = rssTree.Element("link").Value

                          };


            SecondListBox.ItemsSource = rssList;

            books1 = rssList.ToList();

            //progressBar1.Visibility = System.Windows.Visibility.Collapsed;
        }
        void Completed2(object sender, System.Net.DownloadStringCompletedEventArgs e)
        {
            string rssContent;
            try
            {
    //            rssContent = EUCKR_Unicode_Library.EUCKR_Unicode_Converter.GetUnicodeString(e.Result);
                rssContent = e.Result;
                rssContent=HttpUtility.HtmlDecode(Regex.Replace(rssContent, "euc-kr", "utf-8"));
                MessageBox.Show(rssContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No Internet Connection");
                return;
            }
            

            //progressBar1.Visibility = System.Windows.Visibility.Collapsed;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }
    }
    public class Book
    {
        public string Title { get; set; }
        public string Contents { get; set; }
        public Uri Img { get; set; }
        public string Url { get; set; }
    }
}