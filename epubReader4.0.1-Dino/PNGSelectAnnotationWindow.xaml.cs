﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.IO;

using System.Runtime.InteropServices;
using System.Management;

namespace epubReader4._0_Dino_master
{
    /// <summary>
    /// PNGShowAnnotationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PNGSelectAnnotationWindow : Window
    {
        public PNGSelectAnnotationWindow()
        {
            InitializeComponent();
        }

        //自分のキャプチャ一覧を表示する
        string captureDirectory;
        string searchImageFileName;
        string epubDirectory;
        string epubFileName;
        string[] files;
        User user;

        //初期処理
        public void init(string searchImageFileName, string epubDirectory, string epubFileName, User user)
        {
            this.searchImageFileName = searchImageFileName;
            this.epubFileName = epubFileName;
            this.epubDirectory = epubDirectory;
            this.user = user;

            //ファイル共有するならこっち
            if (Directory.Exists(GetUniversalName(@"\\MCDYNA20\ContentsData")))
            {
                captureDirectory =
                        @"\\MCDYNA20\ContentsData\Annotation\" + user.GetId() + "\\" + epubFileName.Replace(".epub", "");

                string unc_path = GetUniversalName(captureDirectory);

                //自分のアノテーションファイルの置き場がなければつくる
                if (!Directory.Exists(unc_path))
                {
                    Directory.CreateDirectory(unc_path);
                }

                //保存先にページ.pngが何枚保存されているか調べる
                files = System.IO.Directory.GetFiles(captureDirectory, searchImageFileName + "*" + ".png", System.IO.SearchOption.TopDirectoryOnly);
            }

            //しないならこっち
            else
            {
                captureDirectory =
                    epubDirectory.Replace("epub", "Annotation\\") + user.GetId() + "\\" + epubFileName.Replace(".epub", "");


                //自分のアノテーションファイルの置き場がなければつくる
                if (!Directory.Exists(captureDirectory))
                {
                    Directory.CreateDirectory(captureDirectory);
                }

                //保存先にページ.pngが何枚保存されているか調べる
                files = System.IO.Directory.GetFiles(captureDirectory, searchImageFileName + "*" + ".png", System.IO.SearchOption.TopDirectoryOnly);
            }

            if(files.Length == 0)
            {
                MessageBox.Show("きろくがありません。");
                this.Close();
            }

            //ボタンを生成
            Button[] btn = new Button[1024];

            int j = 0; //グリッドの列要素の位置
            int k = 0; //グリッドの行要素の位置

            int i = 0;
            foreach (string f in files)
            {

                btn[i] = new Button() { Content = f };
                btn[i].Background = new ImageBrush(new BitmapImage(new Uri(f, UriKind.Relative)));

                if (j < 5)
                {
                    ColumnDefinition cd1 = new ColumnDefinition() { Width = new GridLength(200) };
                    grid1.ColumnDefinitions.Add(cd1);
                    j++;
                }
                else
                {
                    RowDefinition rd1 = new RowDefinition() { Height = new GridLength(200) };
                    grid1.RowDefinitions.Add(rd1);
                    j = 1;
                    k++;
                }
                btn[i].Content = string.Format("{0}." + f, i + 1);
                Grid.SetColumn(btn[i], j);
                Grid.SetRow(btn[i], k);
                grid1.Children.Add(btn[i]);
                btn[i].VerticalAlignment = VerticalAlignment.Stretch;
                btn[i].HorizontalAlignment = HorizontalAlignment.Stretch;
                btn[i].Width = double.NaN;  //Autoという意味
                btn[i].Height = double.NaN; //Autoという意味

                btn[i].Click += new RoutedEventHandler(btn_Click);
                i++;
            }
        }

        //それぞれのボタンを押したときの処理
        public void btn_Click(object sender, RoutedEventArgs e)
        {
            //senderからクリックしたファイル名を取得
            string picPath = sender.ToString();
            picPath = picPath.Replace("System.Windows.Controls.Button: ", "");
            int x = picPath.IndexOf(".");
            picPath = picPath.Remove(0, x + 1);

            PNGShowAnnotationWindow pshaw = new PNGShowAnnotationWindow();
            pshaw.Show();
            pshaw.init(picPath, files, x);

            this.Close();
        }

        //以下、ファイル共有関係のコード（おれもよくわかんない笑）
        /* 
        * WNetGetUniversalNameをインポートする
        */
        [DllImport("mpr.dll", CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U4)]
        static extern int
            WNetGetUniversalName(
            string lpLocalPath,                                 // ネットワーク資源のパス 
            [MarshalAs(UnmanagedType.U4)] int dwInfoLevel,      // 情報のレベル
            IntPtr lpBuffer,                                    // 名前バッファ
            [MarshalAs(UnmanagedType.U4)] ref int lpBufferSize  // バッファのサイズ
        );


        /*
         * dwInfoLevelに指定するパラメータ
         *  lpBuffer パラメータが指すバッファで受け取る構造体の種類を次のいずれかで指定
         */
        const int UNIVERSAL_NAME_INFO_LEVEL = 0x00000001;
        const int REMOTE_NAME_INFO_LEVEL = 0x00000002; //こちらは、テストしていない


        /*
         * lpBufferで受け取る構造体
         */
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct UNIVERSAL_NAME_INFO
        {
            public string lpUniversalName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct _REMOTE_NAME_INFO  //こちらは、テストしていない
        {
            string lpUniversalName;
            string lpConnectionName;
            string lpRemainingPath;
        }

        /* エラーコード一覧
        * WNetGetUniversalName固有のエラーコード
        *   http://msdn.microsoft.com/ja-jp/library/cc447067.aspx
        * System Error Codes (0-499)
        *   http://msdn.microsoft.com/en-us/library/windows/desktop/ms681382(v=vs.85).aspx
        */
        const int NO_ERROR = 0;
        const int ERROR_NOT_SUPPORTED = 50;
        const int ERROR_MORE_DATA = 234;
        const int ERROR_BAD_DEVICE = 1200;
        const int ERROR_CONNECTION_UNAVAIL = 1201;
        const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        const int ERROR_EXTENDED_ERROR = 1208;
        const int ERROR_NO_NETWORK = 1222;
        const int ERROR_NOT_CONNECTED = 2250;

        /*
        * UNC変換ロジック本体
        */
        public static string GetUniversalName(string path_src)
        {
            string unc_path_dest = path_src; //解決できないエラーが発生した場合は、入力されたパスをそのまま戻す
            int size = 1;

            /*
             * 前処理
             *   意図的に、ERROR_MORE_DATAを発生させて、必要なバッファ・サイズ(size)を取得する。
             */
            //1バイトならば、確実にERROR_MORE_DATAが発生するだろうという期待。
            IntPtr lp_dummy = Marshal.AllocCoTaskMem(size);

            //サイズ取得をトライ
            int apiRetVal = WNetGetUniversalName(path_src, UNIVERSAL_NAME_INFO_LEVEL, lp_dummy, ref size);

            //ダミーを解放
            Marshal.FreeCoTaskMem(lp_dummy);
            /*
                        * UNC変換処理
                        */
            switch (apiRetVal)
            {
                case ERROR_MORE_DATA:
                    //受け取ったバッファ・サイズ(size)で再度メモリ確保
                    IntPtr lpBufUniversalNameInfo = Marshal.AllocCoTaskMem(size);

                    //UNCパスへの変換を実施する。
                    apiRetVal = WNetGetUniversalName(path_src, UNIVERSAL_NAME_INFO_LEVEL, lpBufUniversalNameInfo, ref size);

                    //UNIVERSAL_NAME_INFOを取り出す。
                    UNIVERSAL_NAME_INFO a = (UNIVERSAL_NAME_INFO)Marshal.PtrToStructure(lpBufUniversalNameInfo, typeof(UNIVERSAL_NAME_INFO));

                    //バッファを解放する
                    Marshal.FreeCoTaskMem(lpBufUniversalNameInfo);

                    if (apiRetVal == NO_ERROR)
                    {
                        //UNCに変換したパスを返す
                        unc_path_dest = a.lpUniversalName;
                    }
                    else
                    {
                        //MessageBox.Show(path_src +"ErrorCode:" + apiRetVal.ToString());
                    }
                    break;

                case ERROR_BAD_DEVICE: //すでにUNC名(\\servername\test)
                case ERROR_NOT_CONNECTED: //ローカル・ドライブのパス(C:\test)
                    //MessageBox.Show(path_src +"\nErrorCode:" + apiRetVal.ToString());
                    break;
                default:
                    //MessageBox.Show(path_src + "\nErrorCode:" + apiRetVal.ToString());
                    break;

            }

            return unc_path_dest;
        }
    }
}