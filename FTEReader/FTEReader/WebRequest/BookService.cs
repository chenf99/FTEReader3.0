using FTEReader.DataBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTEReader.WebRequest
{
    class BookService
    {
        public async static void initTheDataBase()
        {
            string[] tag = { "小说同人", "动漫同人", "影视同人", "游戏同人", "耽美同人" };
            for (int i = 3; i < 5; i++)
            {


                for (int page = 0; page < 1; page++)
                {
                    string Catalog = "同人";
                    string Tags = tag[i];
                    string compatibeMen = "female";
                    string start = (page * 50).ToString();
                    BooksObject myNewBook = await GetNewBooks.GetNewBook("hot", Catalog, Tags, start, "15", compatibeMen);

                    for (int bookcount = 0; bookcount < 10; bookcount++)
                    {
                        string bookid = myNewBook.books[bookcount]._id;
                        BookDetailObject myBookDetail = await BookDetail.GetBookDetail(bookid);
                        ChapterObject myChapter = await Chapter.GetChapter(bookid);
                        //string link = myChapter.mixToc.chapters[0].link;
                        string Title = myNewBook.books[bookcount].title;
                        string Info = myNewBook.books[bookcount].shortIntro;
                        string Image = "http://statics.zhuishushenqi.com" + myNewBook.books[bookcount].cover;
                        string author = myNewBook.books[bookcount].author;
                        string nowChac = "1";
                        BookDB.addToBookStore(Title, Catalog, Tags, Info, Image,
                                                    bookid, author, compatibeMen, nowChac);
                    }
                }
            }
        }

        public async static Task<string> GetChapterContent(string bookid, string chapterNum)
        {
            string contentText;
            int num = int.Parse(chapterNum);
            try
            {
                ChapterObject myChapter = await Chapter.GetChapter(bookid);
              
                string link = myChapter.mixToc.chapters[num-1].link;
                link = link.Replace("/", "%2F");
                link = link.Replace("?", "%3F");
                ChapterDetailObject myChapterContent = await ChapterDetail.GetChapterDetail(link);
                contentText = myChapter.mixToc.chapters[num-1].title + "\n" +  myChapterContent.chapter.body;
            }
            catch (Exception e)
            {
                contentText = "Can not get the content!";
            }
            return contentText;
        }

        public async static Task<int> GetTotalChapterNumber(string bookid)
        {
            ChapterObject myChapter = await Chapter.GetChapter(bookid);
            return myChapter.mixToc.chaptersCount1;
        }

        public async static Task<List<string>> GetChapterList(string bookid)
        {
            ChapterObject myChapter = await Chapter.GetChapter(bookid);
            List<string> myList = new List<string>();
            //int myChapterNumber = myChapter.mixToc.chaptersCount1;
            int myChapterNumber = myChapter.mixToc.chaptersCount1;
            for (int i = 0; i < myChapterNumber; i++)
            {
                myList.Add(myChapter.mixToc.chapters[i].title);
            }
            //Debug.WriteLine(myChapterNumber);
            return myList;
        }

    }
}
