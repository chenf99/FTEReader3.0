using FTEReader.DataBase;
using FTEReader.Models;
using FTEReader.ViewModels;
using FTEReader.WebRequest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace FTEReader
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ReadPage : Page
    {
        public bool isPlaying = false;        
        public string type;
        public string title;
        public string pages;
        public string id;
        public string bookId;
        public string nowChapter;
        public string content;
        const int wordsOnePage = 777;
        int numOfPage = 0;
        int currentPage = 1;
        Windows.Storage.StorageFile file;
        Encoding x;
        private ChapterItemViewModels chapters;
        public ReadPage()
        {
            this.InitializeComponent();
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Parchment.jpg", UriKind.Absolute));
            globe.Background = imageBrush;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (type == "0")
            {
                BookInShelfViewModels bookInShelfViewModel = BookInShelfViewModels.GetViewModel();
                bookInShelfViewModel.changePages(id, currentPage.ToString());
            }
            chapters.ChapterItems.Clear();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string[] book = e.Parameter as string[];
            type = book[0];
            chapters = ChapterItemViewModels.GetViewModel();
            if (type == "0")
            {
                chapters.ChapterItems.Clear();
                title = book[1];
                Debug.WriteLine(title);
                pages = book[2];
                id = book[3];
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFolder bookFolder = await localFolder.GetFolderAsync("books");
                file = await bookFolder.GetFileAsync(title + ".txt");

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var encoding = Encoding.GetEncoding(0);
                //encoding = Encoding.UTF8;
                x = encoding;
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, false))
                    {
                        string text = reader.ReadToEnd();
                        numOfPage = text.Length / wordsOnePage + 1;
                        currentPage = int.Parse(pages);
                        string result = text.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                        B1.Text = result;
                    }
                }
            }
            if (type == "1")
            {                
                bookId = (string)book[1];
                try
                {
                    List<string> chapterStrs = await BookService.GetChapterList(bookId);
                    chapters.add(chapterStrs);
                    nowChapter = (string)book[2];
                    content = (string)book[3];
                    string text = content;
                    numOfPage = text.Length / wordsOnePage + 1;
                    currentPage = 1;
                    string result = text.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                    B1.Text = result;
                }
                catch (Exception exception)
                {
                    B1.Text = "无法获得对应的书籍！";
                }
            }
            //Select.Visibility = Visibility.Collapsed;
        }

        /*private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail; //可通过使用图片缩略图创建丰富的视觉显示，以显示文件选取器中的文件  
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".txt");


            //选取单个文件  
            file = await picker.PickSingleFileAsync();


            //文件处理  
            if (file != null)
            {
                StorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var encoding = Encoding.GetEncoding(0);
                var dialog = new MessageDialog("txt文件的编码是？");
                dialog.Commands.Add(new UICommand("ANSI(默认)", cmd =>
                {
                    encoding = Encoding.GetEncoding(0);
                }, commandId: 0));
                dialog.Commands.Add(new UICommand("UTF-8", cmd =>
                {
                    encoding = Encoding.UTF8;
                }, commandId: 1));
                //设置默认按钮，不设置的话默认的确认按钮是第一个按钮
                dialog.DefaultCommandIndex = 0;
                dialog.CancelCommandIndex = 1;
                //获取返回值
                await dialog.ShowAsync();
                x = encoding;
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream, encoding, false))
                    {
                        string text = reader.ReadToEnd();
                        numOfPage = text.Length / wordsOnePage + 1;
                        //currentPage = ;
                        string result = text.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                        B1.Text = result;
                    }
                }
            }
            Select.Visibility = Visibility.Collapsed;
        }*/

        private async void playButton_Click(object sender, RoutedEventArgs e)
        {
            //SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            //SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(B1.Text);
            //media_element.SetSource(stream, stream.ContentType);
            //media_element.Play();
            await play();
        }

        private async Task<int> play()
        {
            SpeechSynthesizer synthesizer = new SpeechSynthesizer();
            SpeechSynthesisStream stream = await synthesizer.SynthesizeTextToStreamAsync(B1.Text);
            media_element.SetSource(stream, stream.ContentType);
            isPlaying = true;
            media_element.Play();
            return 0;
        }

        private async void preButton_Click(object sender, RoutedEventArgs e)
        {
            preButton.IsEnabled = false;
            nextButton.IsEnabled = false;
            media_element.Stop();            
            if (type == "0")
            {
                if (currentPage == 1)
                {
                    preButton.IsEnabled = true;
                    nextButton.IsEnabled = true;
                    if (isPlaying)
                    {
                        await play();
                    }
                    return;
                }
                currentPage--;
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream, x, false))
                    {
                        string text = reader.ReadToEnd();
                        numOfPage = text.Length / wordsOnePage + 1;
                        string result = text.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                        B1.Text = result;
                    }
                }
            }
            if (type == "1")
            {
                string result = null;
                if (currentPage == 1)
                {
                    if (nowChapter == "1")
                    {
                        MessageDialog message = new MessageDialog("已经是第一页", "提示");
                        await message.ShowAsync();
                        return;
                    }
                    int num = int.Parse(nowChapter);
                    nowChapter = (--num).ToString();
                    content = await BookService.GetChapterContent(bookId, nowChapter);
                    currentPage = content.Length / wordsOnePage + 1;
                    numOfPage = currentPage;
                    result = content.Substring((currentPage - 1) * wordsOnePage, content.Length % wordsOnePage);
                }
                else
                {
                    currentPage--;
                    result = content.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                }
                B1.Text = result; 
            }
            preButton.IsEnabled = true;
            nextButton.IsEnabled = true;
            if (isPlaying)
            {
                await play();
            }
        }
        private async void nextButton_Click(object sender, RoutedEventArgs e)
        {
            media_element.Stop();
            preButton.IsEnabled = false;
            nextButton.IsEnabled = false;
            if (type == "0")
            {
                if (currentPage == numOfPage)
                {
                    preButton.IsEnabled = true;
                    nextButton.IsEnabled = true;
                    if (isPlaying)
                    {
                        await play();
                    }
                    return;
                }
                currentPage++;
                using (Stream stream = await file.OpenStreamForReadAsync())
                {
                    using (StreamReader reader = new StreamReader(stream, x, false))
                    {
                        string text = reader.ReadToEnd();
                        numOfPage = text.Length / wordsOnePage + 1;
                        string result = text.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                        B1.Text = result;
                    }
                }
            }
            if (type == "1")
            {
                if (currentPage == numOfPage)
                {
                    int num = int.Parse(nowChapter);
                    nowChapter = (++num).ToString();
                    content = await BookService.GetChapterContent(bookId, nowChapter);                    
                    numOfPage = content.Length / wordsOnePage + 1;
                    currentPage = 1;
                    string result = content.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                    B1.Text = result;
                }
                else
                {
                    string result;
                    currentPage++;
                    if (currentPage == numOfPage)
                    {
                        result = content.Substring((currentPage - 1) * wordsOnePage, content.Length % wordsOnePage);
                    }
                    else
                    {
                        numOfPage = content.Length / wordsOnePage + 1;
                        result = content.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                    }
                    B1.Text = result;                    
                }                              
            }
            preButton.IsEnabled = true;
            nextButton.IsEnabled = true;
            if (isPlaying)
            {
                await play();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            media_element.Stop();
            isPlaying = false;
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                ChapterItem selectedChapter = e.ClickedItem as ChapterItem;
                int chapterNum = selectedChapter.Num;
                nowChapter = chapterNum.ToString();
                content = await BookService.GetChapterContent(bookId, nowChapter);
                numOfPage = content.Length / wordsOnePage + 1;
                currentPage = 1;
                string result = content.Substring((currentPage - 1) * wordsOnePage, wordsOnePage);
                B1.Text = result;
            }
            catch (Exception)
            {
                MessageDialog message = new MessageDialog("无法获取对应章节！", "提示");
                await message.ShowAsync();
            }            
        }
    }
}
