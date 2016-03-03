using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using KimchinityCore;
using Microsoft.Win32;
using System.Windows.Input;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Ookii.Dialogs.Wpf;

namespace Kimchinity
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class KimchinityNetwork
        : MetroWindow
        , INetworkClient
    {
        public int ImgIndex;
        public string file;
        public string HeroImagePath;
        public List<Image> list = new List<Image>();
        public List<Image> Elist = new List<Image>();
        public List<Map> MList = new List<Map>();
        public List<del> DList = new List<del>();
        public SaveFile SaveList = new SaveFile();

        public struct Map
        {
            public int Type { get; set; }
            public int MaxHp { get; set; }
            public int Hp { get; set; }
            public int Attack { get; set; }
            public string Name { get; set; }
            public int Number { get; set; }
            public int Count { get; set; }
        };

        struct Do
        {
            public Image Img { get; set; }
            public bool Mod { get; set; }
            public bool Copy { get; set; }
            public bool Delete { get; set; }
            public int Fx { get; set; }
            public int Fy { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int Type { get; set; }
            public int MaxHp { get; set; }
            public int Hp { get; set; }
            public int Attack { get; set; }
            public string Name { get; set; }
            public int Number { get; set; }
            public int Count { get; set; }
            public int zIndex { get; set; }
        };

        Do[] RedoStack = new Do[1000];
        Do[] UndoStack = new Do[1000];
        Do TempDo = new Do();
        Map[,] map;
        string[,] MonsterMap;
        string[,] m_ImageCopy;
        public static int xp, yp;
        public static int Fxp, Fyp;

        public static string StartSound { get; set; }
        public static string MainSound { get; set; }
        public static string HeroAttackSound { get; set; }
        public static string HeroMoveSound { get; set; }
        public static string HeroHitedSound { get; set; }
        public static string HeroDieSound { get; set; }

        public static string EnemyHitedSound { get; set; }
        public static string EnemyDieSound { get; set; }

        public static string ObstacleSound { get; set; }

        public static string EndingSound { get; set; }
        public static string ClearSound { get; set; }

        public static string TitleImg { get; set; }
        public static string EndImage { get; set; }
        public static string ClearImage { get; set; }

        //
        public static bool FStartSound { get; set; }
        public static bool FMainSound { get; set; }
        public static bool FHeroAttackSound { get; set; }
        public static bool FHeroMoveSound { get; set; }
        public static bool FHeroHitedSound { get; set; }
        public static bool FHeroDieSound { get; set; }

        public static bool FEnemyHitedSound { get; set; }
        public static bool FEnemyDieSound { get; set; }

        public static bool FObstacleSound { get; set; }

        public static bool FEndingSound { get; set; }
        public static bool FClearSound { get; set; }

        public static bool FTitleImg { get; set; }
        public static bool FEndImage { get; set; }
        public static bool FClearImage { get; set; }
        //

        public string SavePath { get; set; }
        public string ProjectPath { get; set; }
        public string AIFlag;

        public int zindex;
        public int MapSize;

        public Image ImageTemp;
        public bool ListToCanvas = false;
        public static bool CopyFlag = false;
        public static bool SelectMod = true;
        public static bool DeleteFlag = false;

        public int HeroAttack;
        public int HeroHp;
        public int IsHero;
        public int IsDoor;

        public int HeroX;
        public int HeroY;

        public int DoorX;
        public int DoorY;

        public int RedoTop;
        public int UndoTop;

        //Network 변수들
        public static bool IsConnected { get; set; }
        public static List<ICommand> Commands { get; private set; }
        public static List<IPeer> s_server;

        public string m_RoomKey { get; set; }
        public string m_Roomname { get; set; }
        public string m_Nickname { get; set; }
        public int currentFileNum = 0;
        private Image SelectedImageTemp;
        private bool canDisConnect = false;
        private DispatcherTimer mTimer;

        //미니맵 변수
        public bool IsMinimapClicked { get; set; }
        private DispatcherTimer mDrawTimer;

        public KimchinityNetwork(int mapX=50, int mapY=50, string path="Default", int preId = 0)
        {
            System.IO.Directory.CreateDirectory(path);

            map = new Map[mapY, mapX];
            MonsterMap = new string[mapY, mapX];
            m_ImageCopy = new string[mapY, mapX];
            ProjectPath = Path.GetFullPath(path);
            currentFileNum = preId * 50000;

            Init();

            m_Minimap.Height *= (mapY / 100f);
            m_Minimap.Width *= (mapX / 100f);
        }

        public void Init()
        {
            ThemeManager.ChangeTheme(App.Current, ThemeManager.DefaultAccents[0], Theme.Light);

            this.Closing += (s, e) =>
            {
                if (s_server == null)
                    return;

                if (!canDisConnect)
                {
                    CPacket msg = CPacket.Create((short)PROTOCOL.END);
                    s_server[0].Send(msg);
                    e.Cancel = true;
                }
            };

            this.Closed += (s, e) =>
            {
                mDrawTimer.Stop();
            };

            InitializeComponent();
            Loaded += new RoutedEventHandler(Window1_Loaded);

            Fxp = -1;
            Fyp = -1;
            zindex = 1;
            RedoTop = -1;
            UndoTop = -1;
            HeroX = -1;
            HeroY = -1;
            DoorX = -1;
            DoorY = -1;
            MapSize = 50;
            CopyFlag = false;
            radioButton1.IsChecked = true;
            mycanv.Width = MapSize * map.GetLength(1);
            mycanv.Height = MapSize * map.GetLength(0);

            StartSound = "'undefined'";
            MainSound = "'undefined'";
            HeroAttackSound = "'undefined'";
            HeroMoveSound = "'undefined'";
            HeroHitedSound = "'undefined'";
            HeroDieSound = "'undefined'";
            EnemyHitedSound = "'undefined'";
            EnemyDieSound = "'undefined'";
            ObstacleSound = "'undefined'";
            EndingSound = "'undefined'";
            ClearSound = "'undefined'";

            TitleImg = "'undefined'";
            EndImage = "'undefined'";
            ClearImage = "'undefined'";

            Commands = new List<ICommand>();

            mTimer = new DispatcherTimer
            (
               TimeSpan.FromTicks(1),
               DispatcherPriority.ApplicationIdle,// Or DispatcherPriority.SystemIdle
               (s, e) => { this.OnIdle(s, e); }, // or something similar
               Application.Current.Dispatcher
            );

            mDrawTimer = new DispatcherTimer
            (
                TimeSpan.FromSeconds(3),
               DispatcherPriority.ApplicationIdle,// Or DispatcherPriority.SystemIdle
               (s, e) => { this.DrawMinimap(); }, // or something similar
               Application.Current.Dispatcher
            );
        }

        public void CloseOK()
        {
            canDisConnect = true;
            mTimer.Stop();
            IsConnected = false;
        }

        private void RedoPush(Do Redo)
        {
            RedoStack[++RedoTop] = Redo;
        }

        private Do RedoPop()
        {
            return RedoStack[RedoTop--];
        }

        private void UndoPush(Do Undo)
        {
            UndoStack[++UndoTop] = Undo;
        }

        private Do UndoPop()
        {
            return UndoStack[UndoTop--];
        }

        /// <summary>
        /// 접속 성공시 호출될 콜백 매소드.
        /// </summary>
        /// <param name="serverToken"></param>
        static void OnConnected(CUserToken serverToken)
        {
            lock (s_server)
            {
                Console.WriteLine("Connected!");
            }
        }

        private void btn_Open(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "dat|*.dat";
            saveDialog.AddExtension = true;

            if (saveDialog.ShowDialog() == true)
            {
                SavePath = saveDialog.SafeFileName;
            }
            else
            {
                return;
            }

            foreach (FrameworkElement fe in mycanv.Children)
            {

                // example 0
                double top = (double)fe.GetValue(Canvas.TopProperty);
                double left = (double)fe.GetValue(Canvas.LeftProperty);
                /*
                System.Console.WriteLine("마지막 좌표");
                System.Console.WriteLine(top);
                System.Console.WriteLine(left);
                */
                Image tmg = new Image();
                save s = new save();
                Point pp = new Point();
                pp.X = top;
                pp.Y = left;
                s.p = pp;
                tmg = (Image)fe;
                //s.im = tmg;
                s.path = ((BitmapImage)tmg.Source).UriSource.AbsolutePath;
                s.z_index = Canvas.GetZIndex(fe);
                if (s.z_index > 9999)
                {
                    s.Type = map[((int)top / 50), ((int)left / 50)].Type;
                    s.MaxHp = map[((int)top / 50), ((int)left / 50)].MaxHp;
                    s.Hp = map[((int)top / 50), ((int)left / 50)].Hp;
                    s.Attack = map[((int)top / 50), ((int)left / 50)].Attack;
                    s.Name = map[((int)top / 50), ((int)left / 50)].Name;
                    s.Number = map[((int)top / 50), ((int)left / 50)].Number;
                    s.Count = map[((int)top / 50), ((int)left / 50)].Count;
                }

                SaveList.List.Add(s);
                //System.Console.WriteLine(fe.ToString());
                // example 1
                //double top1 = Canvas.GetTop(fe);
                //double left1 = Canvas.GetLeft(fe);


            }
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    MList.Add(map[i, j]);
                }
            }
            //SaveList.map = map;

            using (Stream st =
                new FileStream(SavePath, FileMode.Create))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                binFormatter.Serialize(st, SaveList);
                st.Close();
            }
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            Dragg.SetWindow(this.mycanv);
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            ListToCanvas = true;
            ListBox lb = e.Source as ListBox;
            ImageTemp = lb.SelectedItem as Image;
            object data = ImageTemp;
            if (data != null)
            {
                var filename = Path.GetFileName(((BitmapImage)ImageTemp.Source).UriSource.AbsolutePath);
                CPacket msg = CPacket.Create((short)PROTOCOL.SELECTION_IMAGE_REQ);
                msg.Push(filename);
                s_server[0].Send(msg);

                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        private void OnIdle(object s, EventArgs e)
        {
            OnMessageReceive();
        }

        private void OnMessageReceive()
        {
            if (false == IsConnected) return;

            if (0 < Commands.Count)
            {
                ICommand command = Commands[0];
                if (command != null)
                {
                    command.Excute(this);
                    command.Examine();
                    Commands.Remove(command);
                }
            }
        }

        public void image_load(BitmapImage pic)
        {
            if (null == pic) return;

            Image bg = new Image();
            bg.Source = pic;
            bg.Width = 50;
            bg.Height = 50;

            Dragg.SetDrag(bg, true);
            listBox1.Items.Add(bg);
            list.Add(bg);

        }

        private void cnv_drop(object sender, DragEventArgs e)
        {
            if (ListToCanvas)
            {
                Point p = e.GetPosition(mycanv);
                xp = (int)p.X / 50;
                yp = (int)p.Y / 50;

                //네트워크 함수
                {
                    CPacket msg = CPacket.Create((short)PROTOCOL.IMAGE_ATTACH_REQ);
                    msg.Push(xp);
                    msg.Push(yp);
                    msg.Push((!SelectMod ? 0 : 1));
                    s_server[0].Send(msg);
                }

                Image ig = new Image();
                ig.Source = ImageTemp.Source;
                ImageTemp = null;
                ig.Width = 50;
                ig.Height = 50;
                Dragg.SetDrag(ig, true);
                mycanv.Children.Add(ig);
                ListToCanvas = false;
                Canvas.SetLeft(ig, xp * 50);
                Canvas.SetTop(ig,  yp * 50);
                Canvas.SetZIndex(ig, zindex + ImgIndex);

                listBox1.UnselectAll();
                ListToCanvas = false;
                if (!SelectMod)
                {
                    Elist.Add(ig);
                    map[yp, xp].Number = ++ImgIndex;
                }
                else
                {
                    list.Add(ig);
                }
            }
            else
            {
                e.Handled = true;

                string[] strArrays = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
                int fileCount = strArrays.Length;

                foreach (string fileName in strArrays)
                {
                    if (FileNmaeContains(fileName) == "false")
                    {
                        System.Windows.MessageBox.Show("파일의 형식이 다릅니다.", "형식에러", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK, System.Windows.MessageBoxOptions.DefaultDesktopOnly);

                        return;
                    }

                    file = fileName;
                    //this.Topmost = true;
                    //this.Topmost = false;

                    // 이미지 로드
                    // 네트워크 관련
                    {
                        String name = System.IO.Path.GetFileName(file);

                        var buffers = File.ReadAllBytes(file);
                        FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);

                        var msg = CPacket.Create((short)PROTOCOL.FILE_SEND_REQ);
                        msg.Push(name);
                        msg.Push(buffers.Length);

                        var nCeiling = (int)Math.Ceiling(buffers.Length / (double)CNetworkService.BufferSize);

                        msg.Push(nCeiling);
                        msg.Push(currentFileNum);
                        msg.Push(0);

                        s_server[0].Send(msg);

                        int currentLength = 0;
                        short count = 1;
                        while (currentLength < buffers.Length)
                        {
                            var bytes = buffers.Skip(currentLength).Take(CNetworkService.BufferSize - 12).ToArray();
                            var nIndex = bytes.Length;
                            var packet = CPacket.Create((short)PROTOCOL.FILE_SEND);
                            currentLength += nIndex;

                            packet.Push(currentFileNum);
                            packet.PushInt16(count++);
                            packet.Push(bytes);
                            s_server[0].Send(packet);
                        }

                        Console.WriteLine("Filenum : {0}", currentFileNum);
                        currentFileNum++;
                    }
                }
            }
        }

        private string FileNmaeContains(String sfilename)
        {
            String filenameToupper = sfilename.ToUpper();

            if (filenameToupper.Contains(".PNG") == true || filenameToupper.Contains(".BMP") == true ||
               filenameToupper.Contains(".JPG") == true || filenameToupper.Contains(".GIF") == true)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }

        private void mycanv_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void mycanv_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(mycanv);
            xp = (int)p.X / 50;
            yp = (int)p.Y / 50;
            Fxp = xp;
            Fyp = yp;
            if (!SelectMod)
            {
                if (map[yp, xp].Number == 0)
                {
                    e.Handled = true;
                    if (Dragg.al != null)
                        Dragg.al.Remove(Dragg.myAdorner);
                    comboBox1.SelectedIndex = -1;
                    textBox1.Text = null;
                    textBox2.Text = null;
                    comboBox2.SelectedIndex = -1;
                    comboBox1.IsEnabled = false;
                    textBox1.IsEnabled = false;
                    textBox2.IsEnabled = false;
                    comboBox2.IsEnabled = false;
                    return;
                }
                else
                {
                    comboBox1.IsEnabled = true;
                    textBox1.IsEnabled = true;
                    textBox2.IsEnabled = true;
                    comboBox2.IsEnabled = true;
                    comboBox1.SelectedIndex = map[yp, xp].Type - 1;
                    textBox1.Text = map[yp, xp].MaxHp.ToString();
                    textBox2.Text = map[yp, xp].Attack.ToString();
                    comboBox2.SelectedIndex = map[yp, xp].Hp;

                }
            }
        }

        private void mycanv_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(mycanv);
            xp = (int)p.X / 50;
            yp = (int)p.Y / 50;

            if (!(xp == Fxp && yp == Fyp) && !CopyFlag && Dragg.TempImg != null)
            {
                TempDo.Img = Dragg.TempImg;
                TempDo.Mod = SelectMod;
                TempDo.Copy = CopyFlag;
                TempDo.Delete = DeleteFlag;
                TempDo.Fx = Fxp;
                TempDo.Fy = Fyp;
                TempDo.x = xp;
                TempDo.y = yp;
                CopyMapInfo(xp, yp);
                RedoPush(TempDo);

            }

            bool isbool = (!(xp == Fxp && yp == Fyp) && Fxp != -1 && Fyp != -1) && !CopyFlag && Dragg.TempImg != null;

            if(isbool)
            {
                int zOrder = Canvas.GetZIndex(Dragg.TempImg);
                CPacket msg = CPacket.Create((short)PROTOCOL.IMAGE_MOVE_REQ);
                msg.Push(zOrder);

                msg.Push(Fxp);//과거
                msg.Push(Fyp);

                msg.Push(xp);// 현재
                msg.Push(yp);
                s_server[0].Send(msg);
            }

            if (CopyFlag && Dragg.TempImg != null)
            {
                if (map[yp, xp].Number == 0)
                {
                    CPacket msg = CPacket.Create((short)PROTOCOL.IMAGE_COPY_REQ);
                    msg.Push(Fxp); //lastIndex
                    msg.Push(Fyp);
                    int zOrder = Canvas.GetZIndex(Dragg.TempImg);
                    msg.Push(zOrder);

                    msg.Push(xp); //index
                    msg.Push(yp);
                    ImgIndex++;
                    msg.Push(zindex + ImgIndex);
                    s_server[0].Send(msg);
                    Dragg.TempImg = null;
                }
                else
                {
                    MessageBox.Show("이벤트는 겹칠 수 없습니다.");
                }
            }
            else
            {
                if (!SelectMod)
                {
                    if ((Fxp != xp || Fyp != yp) && map[yp, xp].Number != 0)
                    {
                        MessageBox.Show("이벤트는 겹칠 수 없습니다.");
                        Canvas.SetLeft(Dragg.CurrentElement, Fxp * 50);
                        Canvas.SetTop(Dragg.CurrentElement, Fyp * 50);
                        return;
                    }
                    else
                    {
                        {
                            CPacket msg = CPacket.Create((short)PROTOCOL.SWAP_PROPERTY_REQ);
                            msg.Push(Fxp);
                            msg.Push(Fyp);
                            msg.Push(xp);
                            msg.Push(yp);
                            s_server[0].Send(msg);
                        }
                    }
                    if (map[yp, xp].Number == 0)
                    {
                        //if (Drag.al != null)
                        //    Drag.al.Remove(Drag.myAdorner);
                        comboBox1.SelectedIndex = -1;
                        textBox1.Text = null;
                        textBox2.Text = null;
                        comboBox2.SelectedIndex = -1;
                        return;
                    }
                    if (!SelectMod)
                    {
                        comboBox1.SelectedIndex = map[yp, xp].Type - 1;
                        textBox1.Text = map[yp, xp].MaxHp.ToString();
                        textBox2.Text = map[yp, xp].Attack.ToString();
                        comboBox2.SelectedIndex = map[yp, xp].Hp;

                       
                    }
                }
            }
        }


        public void ImageCopy(int lx, int ly, int lzOrder,int x, int y, int zOrder)
        {
            Image findImage = null;
            foreach (FrameworkElement fe in mycanv.Children)
            {
                var top = (double)fe.GetValue(Canvas.TopProperty);
                var left = (double)fe.GetValue(Canvas.LeftProperty);

                if ((left / 50) == lx && (top / 50) == ly)
                {
                    if (Canvas.GetZIndex((Image)fe) == lzOrder)
                    {
                        findImage = fe as Image;
                    }
                }
            }

            if (null == findImage)
                return;

            Image ig = new Image();
            ig.Source = findImage.Source;
            ig.Width = 50;
            ig.Height = 50;

            Dragg.SetDrag(ig, true);
            mycanv.Children.Add(ig);
            Canvas.SetLeft(ig, x * 50);
            Canvas.SetTop(ig, y * 50);
            Canvas.SetZIndex(ig, zindex + ImgIndex);
            Elist.Add(ig);

            TempDo.Img = ig;
            TempDo.Mod = SelectMod;
            TempDo.Copy = CopyFlag;
            TempDo.Delete = DeleteFlag;
            TempDo.Fx = Fxp;
            TempDo.Fy = Fyp;
            TempDo.x = xp;
            TempDo.y = yp;
            RedoPush(TempDo);

            CopyMapInfo(xp, yp);
            CopyFlag = false;
            //SetDefaultMapInfo(xp, yp);
            if (!SelectMod)
            {
                map[yp, xp] = map[Fyp, Fxp];
                map[yp, xp].Name += ImgIndex;
            }
        }

        private void mycanv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                CopyFlag = true;
                //System.Console.WriteLine("컨트롤키 누름");
            }
            if (e.Key == Key.Delete)
            {
                if (xp > -1 && yp > -1)
                {
                    CPacket msg = CPacket.Create((short)PROTOCOL.DELETE_REQ);
                    msg.Push(xp);
                    msg.Push(yp);
                    s_server[0].Send(msg);
                }
            }
        }

        private void mycanv_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                CopyFlag = false;
                Dragg.TempImg = null;
            }
        }

        private void SetDefaultMapInfo(int x, int y)
        {
            /*
            public int Type { get; set; }
            public int MaxHp { get; set; }
            public int Hp { get; set; }
            public int Attack { get; set; }
            public string Name { get; set; }
            public int Number { get; set; }
            public int Count { get; set; }
            */
            map[y, x].Type = 0;
            map[y, x].MaxHp = 0;
            map[y, x].Hp = 0;
            map[y, x].Attack = 0;
            map[y, x].Name = "";
            map[y, x].Number = 0;
            map[y, x].Count = 0;
        }

        private void SetMapInfo(int x, int y)
        {
            map[y, x].Type = TempDo.Type;
            map[y, x].MaxHp = TempDo.MaxHp;
            map[y, x].Hp = TempDo.Hp;
            map[y, x].Attack = TempDo.Attack;
            map[y, x].Name = TempDo.Name;
            map[y, x].Number = TempDo.Number;
            map[y, x].Count = TempDo.Count;
        }

        private void SwapMapInfo(int x1, int y1, int x2, int y2)
        {
            //1은 덮혀쓰일곳, 2는 기존
            int tInt;
            string Tst;
            tInt = map[y1, x1].Type;
            map[y1, x1].Type = map[y2, x2].Type;
            map[y2, x2].Type = tInt;

            tInt = map[y1, x1].MaxHp;
            map[y1, x1].MaxHp = map[y2, x2].MaxHp;
            map[y2, x2].MaxHp = tInt;

            tInt = map[y1, x1].Hp;
            map[y1, x1].Hp = map[y2, x2].Hp;
            map[y2, x2].Hp = tInt;

            tInt = map[y1, x1].Attack;
            map[y1, x1].Attack = map[y2, x2].Attack;
            map[y2, x2].Attack = tInt;

            Tst = map[y1, x1].Name;
            map[y1, x1].Name = map[y2, x2].Name;
            map[y2, x2].Name = Tst;

            tInt = map[y1, x1].Number;
            map[y1, x1].Number = map[y2, x2].Number;
            map[y2, x2].Number = tInt;
        }

        private void OverMapInfo(int x2, int y2, int x1, int y1)
        {
            map[y1, x1] = map[y2, x2];
            /*
            map[x1, y1].Type = map[x2, y2].Type;
 
            map[x1, y1].MaxHp = map[x2, y2].MaxHp;
            map[x2, y2].MaxHp = 0;
 
            map[x1, y1].Hp = map[x2, y2].Hp;
            map[x2, y2].Hp = 0;
 
            map[x1, y1].Attack = map[x2, y2].Attack;
            map[x2, y2].Attack = 0;
 
            map[x1, y1].Name = map[x2, y2].Name;
            map[x2, y2].Name = "";
            */

        }

        public void SwapProperty(int fx, int fy, int px, int py)
        {
            SwapMapInfo(fx, fy, px, py);

            var type = map[py, px].Type;
            var name = map[py, px].Name;

            switch (type)
            {
                case 3:
                    IsHero = 1;
                    HeroX = px;
                    HeroY = py;
                    break;

                case 4:
                    IsDoor = 1;
                    DoorX = xp;
                    DoorY = yp;
                    break;
            }
        }

        public void SetTypeName(int px, int py, int type, string name)
        {
            map[py, px].Type = type;
            map[py, px].Name = name;

            switch (type)
            {
                case 3:
                    IsHero = 1;
                    HeroX = px;
                    HeroY = py;
                    break;

                case 4:
                    IsDoor = 1;
                    DoorX = xp;
                    DoorY = yp;
                    break;
            }
        }

        private void comboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;

            if (HeroX == xp && HeroY == yp)
                return;


            int TempType = map[yp, xp].Type;
            map[yp, xp].Type = comboBox1.SelectedIndex + 1;
            map[yp, xp].Name = "";

            switch (map[yp, xp].Type)
            {
                case 1:
                    map[yp, xp].Name = "enemy" + map[yp, xp].Number;
                    textBox1.IsEnabled = true;
                    textBox2.IsEnabled = true;
                    comboBox2.IsEnabled = true;
                    break;
                case 2:
                    map[yp, xp].Name = "obstacle" + map[yp, xp].Number;
                    textBox1.IsEnabled = false;
                    textBox2.IsEnabled = false;
                    comboBox2.IsEnabled = false;
                    break;
                case 3:
                    if (IsHero == 1)
                    {
                        for (int i = 0; i < map.GetLength(0); i++)
                        {
                            for (int j = 0; j < map.GetLength(1); j++)
                            {
                                if ((HeroX == j) && (HeroY == i))
                                    continue;
                                if (map[i, j].Type == 3)
                                {
                                    MessageBox.Show("주인공은 하나만 됩니다.");
                                    comboBox1.SelectedIndex = TempType - 1;
                                    map[yp, xp].Type = TempType;
                                    textBox1.IsEnabled = true;
                                    textBox2.IsEnabled = true;
                                    comboBox2.IsEnabled = false;
                                    return;
                                }
                            }
                        }

                    }
                    HeroX = xp;
                    HeroY = yp;
                    map[HeroY, HeroX].Name = "Hero";
                    IsHero = 1;
                    textBox1.IsEnabled = true;
                    textBox2.IsEnabled = true;
                    comboBox2.IsEnabled = false;

                    break;
                case 4:
                    if (IsDoor == 1)
                    {
                        for (int i = 0; i < map.GetLength(0); i++)
                        {
                            for (int j = 0; j < map.GetLength(1); j++)
                            {
                                if ((DoorX == j) && (DoorY == i))
                                    continue;
                                if (map[i, j].Type == 4)
                                {
                                    MessageBox.Show("문은 하나만 됩니다.");
                                    comboBox1.SelectedIndex = TempType - 1;
                                    map[yp, xp].Type = TempType;
                                    map[yp, xp].Name = "Door";
                                    textBox1.IsEnabled = false;
                                    textBox2.IsEnabled = false;
                                    comboBox2.IsEnabled = false;
                                    return;
                                }
                            }
                        }

                    }
                    IsDoor = 1;
                    DoorX = xp;
                    DoorY = yp;
                    textBox1.IsEnabled = false;
                    textBox2.IsEnabled = false;
                    comboBox2.IsEnabled = false;

                    break;
                case 5:
                    map[yp, xp].Name = "obstacle" + map[yp, xp].Number;
                    textBox1.IsEnabled = false;
                    textBox2.IsEnabled = false;
                    comboBox2.IsEnabled = false;
                    break;
                default:
                    break;
            }

            {
                CPacket msg = CPacket.Create((short)PROTOCOL.SET_TYPENAME_REQ);
                msg.Push(xp);
                msg.Push(yp);
                msg.Push(map[yp, xp].Type);
                msg.Push(map[yp, xp].Name);
                s_server[0].Send(msg);
            }
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            SelectMod = true;
            comboBox1.IsEnabled = false;
            textBox1.IsEnabled = false;
            textBox2.IsEnabled = false;
            comboBox2.IsEnabled = false;
            zindex = 1;
            for (int i = 0; i < Elist.Count; i++)
            {
                Elist[i].Visibility = Visibility.Hidden;
            }
        }

        private void radioButton1_Checked2(object sender, RoutedEventArgs e)
        {
            /*
            SelectMod = false;
            comboBox1.IsEnabled = true;
            textBox1.IsEnabled = true;
            textBox2.IsEnabled = true;
            textBox3.IsEnabled = true;
            textBox4.IsEnabled = true;
            textBox5.IsEnabled = true;
             */
            SelectMod = false;
            zindex = 10000;
            for (int i = 0; i < Elist.Count; i++)
            {
                Elist[i].Visibility = Visibility.Visible;
            }
        }

        private SaveFile Deserialize(string path)
        {
            Stream readStream = new FileStream(path, FileMode.Open);

            BinaryFormatter formatter = new BinaryFormatter();
            SaveFile sav = formatter.Deserialize(readStream) as SaveFile;

            readStream.Close();

            return sav;
        }

        private static RenderTargetBitmap ConverterBitmapImage(FrameworkElement element)
        {
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();

            drawingContext.DrawRectangle(new VisualBrush(element), null,
                new Rect(new Point(0, 0), new Point(element.ActualWidth, element.ActualHeight)));

            drawingContext.Close();

            RenderTargetBitmap target =
                new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight,
                96, 96, System.Windows.Media.PixelFormats.Pbgra32);

            target.Render(drawingVisual);
            return target;
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                CPacket msg = CPacket.Create((short)PROTOCOL.SET_PROPERTY_REQ);
                msg.Push(xp);
                msg.Push(yp);
                msg.Push(int.Parse(textBox1.Text));
                msg.Push(int.Parse(textBox2.Text));
                s_server[0].Send(msg);
            }
        }

        public void PrintChatLog(string nickname, string inputText)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("[{0}]", System.DateTime.Now.ToString("hh:mm:ss"));
            builder.AppendFormat("[{0}({1})] : ", nickname, (m_Nickname.Equals(nickname) ? "You" : "Other"));
            builder.AppendFormat(inputText);
            MessageTextBox.Text += builder.ToString() + "\n";
            MessageTextBox.ScrollToEnd();
        }

        private void MessageSend(object sender, RoutedEventArgs e)
        {
            MessageRealSend();
        }

        private void MessageRealSend()
        {
            var size = MessageSendBox.GetLineLength(0);
            if (size == 0)
                return;

            string input_text = MessageSendBox.Text;
            string nick_name = m_Nickname;
            MessageSendBox.Text = "";

            CPacket msg = CPacket.Create((short)PROTOCOL.CHAT_MSG_REQ);
            msg.Push(nick_name);
            msg.Push(input_text);
            s_server[0].Send(msg);
        }

        private void MessageSendBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                MessageRealSend();
            }
        }

        public bool IsOther(string nickname)
        {
            return m_Nickname.Equals(nickname);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".dat";
            dlg.Filter = "dat|*.dat";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                SavePath = dlg.FileName;
            }

            mycanv.Children.Clear();
            string ss = "";


            using (Stream st =
                new FileStream(SavePath, FileMode.Open))
            {
                BinaryFormatter binFormatter = new BinaryFormatter();
                SaveFile sf =
                    binFormatter.Deserialize(st) as SaveFile;
                for (int i = 0; i < sf.List.Count; i++)
                {
                    Image im = new Image();
                    BitmapImage bit = new BitmapImage();
                    ss = sf.List[i].path;
                    bit.BeginInit();
                    bit.UriSource = new Uri(Uri.UnescapeDataString(ss), UriKind.RelativeOrAbsolute);
                    bit.DecodePixelHeight = 50;
                    bit.DecodePixelWidth = 50;
                    bit.EndInit();
                    im.Source = bit;

                    Dragg.SetDrag(im, true);
                    mycanv.Children.Add(im);
                    Canvas.SetLeft(im, sf.List[i].p.Y);
                    Canvas.SetTop(im, sf.List[i].p.X);
                    Canvas.SetZIndex(im, sf.List[i].z_index);

                    if (sf.List[i].z_index > 9999)
                    {
                        Elist.Add(im);
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].Type = sf.List[i].Type;
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].MaxHp = sf.List[i].MaxHp;
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].Hp = sf.List[i].Hp;
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].Attack = sf.List[i].Attack;
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].Name = sf.List[i].Name;
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].Number = sf.List[i].Number;
                        map[(int)sf.List[i].p.X / 50, (int)sf.List[i].p.Y / 50].Count = sf.List[i].Count;
                    }
                }
                st.Close();
                for (int i = 0; i < Elist.Count; i++)
                {
                    Elist[i].Visibility = Visibility.Hidden;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine(RedoTop);
            e.Handled = true;
            if (RedoTop < 0)
            {
                MessageBox.Show("ㄴㄴ");
                return;
            }
            CopyDo(RedoPop());
            UndoPush(TempDo);

            Point Fp = new Point();
            Fp.X = TempDo.Fx * 50;
            Fp.Y = TempDo.Fy * 50;

            Point p = new Point();
            p.X = TempDo.x * 50;
            p.Y = TempDo.y * 50;

            if (!TempDo.Delete && !TempDo.Copy)
            {
                Canvas.SetLeft(TempDo.Img, Fp.X);
                Canvas.SetTop(TempDo.Img, Fp.Y);
                if (!TempDo.Mod)
                {
                    SetMapInfo(TempDo.Fx, TempDo.Fy);
                    SetDefaultMapInfo(TempDo.x, TempDo.y);
                }
            }
            if (TempDo.Delete)
            {
                Image _img = new Image();
                _img = TempDo.Img;
                Dragg.SetDrag(_img, true);
                mycanv.Children.Add(_img);
                Canvas.SetLeft(_img, p.X);
                Canvas.SetTop(_img, p.Y);
                if (!TempDo.Mod)
                {
                    SetMapInfo(TempDo.x, TempDo.y);
                }
            }
            if (TempDo.Copy)
            {
                mycanv.Children.Remove(TempDo.Img);
                if (!TempDo.Mod)
                {
                    SetDefaultMapInfo(TempDo.x, TempDo.y);
                }
            }
        }

        private void CopyDo(Do b)
        {
            TempDo.Copy = b.Copy;
            TempDo.Img = b.Img;
            TempDo.Mod = b.Mod;
            TempDo.Delete = b.Delete;
            TempDo.Fx = b.Fx;
            TempDo.Fy = b.Fy;
            TempDo.x = b.x;
            TempDo.y = b.y;
        }

        private void CopyMapInfo(int x, int y)
        {
            TempDo.Type = map[y, x].Type;
            TempDo.MaxHp = map[y, x].MaxHp;
            TempDo.Hp = map[y, x].Hp;
            TempDo.Attack = map[y, x].Attack;
            TempDo.Name = map[y, x].Name;
            TempDo.Number = map[y, x].Number;
            TempDo.Count = map[y, x].Count;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            System.Console.WriteLine(UndoTop);
            if (UndoTop < 0)
            {
                MessageBox.Show("ㄴㄴ");
                return;
            }
            CopyDo(UndoPop());

            Point Fp = new Point();
            Fp.X = TempDo.Fx * 50;
            Fp.Y = TempDo.Fy * 50;

            Point p = new Point();
            p.X = TempDo.x * 50;
            p.Y = TempDo.y * 50;



            if (!TempDo.Delete && !TempDo.Copy)
            {
                Canvas.SetLeft(TempDo.Img, p.X);
                Canvas.SetTop(TempDo.Img, p.Y);
                if (!TempDo.Mod)
                {
                    SetMapInfo(TempDo.x, TempDo.y);
                    SetDefaultMapInfo(TempDo.Fx, TempDo.Fy);
                }
            }
            if (TempDo.Delete)
            {
                mycanv.Children.Remove(TempDo.Img);
                if (!TempDo.Mod)
                {
                    SetDefaultMapInfo(TempDo.Fx, TempDo.Fy);
                }
                /*
                Image _img = new Image();
                _img = TempDo.Img;
                Drag.SetDrag(_img, true);
                mycanv.Children.Add(_img);
                Canvas.SetLeft(_img, p.X);
                Canvas.SetTop(_img, p.Y);
                if (!TempDo.Mod)
                {
                    SetMapInfo(TempDo.Fx, TempDo.Fy);
                }
                */

            }
            if (TempDo.Copy)
            {
                //mycanv.Children.Remove(TempDo.Img);
                Image _img = new Image();
                _img = TempDo.Img;
                Dragg.SetDrag(_img, true);
                mycanv.Children.Add(_img);
                Canvas.SetLeft(_img, p.X);
                Canvas.SetTop(_img, p.Y);
                if (!TempDo.Mod)
                {
                    SetMapInfo(TempDo.Fx, TempDo.Fy);
                }
            }
            /*
            StreamReader objReader = new StreamReader("d:\\test.js");
            string sLine = "";
            ArrayList arrText = new ArrayList();
 
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                    arrText.Add(sLine);
            }
            objReader.Close();
 
            foreach (string sOutput in arrText) 
                Console.WriteLine(sOutput);
            Console.ReadLine();
            */
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ProjectPath = System.IO.Path.GetFullPath(ProjectPath);

            Random rnd = new Random();
            int ts = rnd.Next(1, 999999999);

            string tempPath = ProjectPath + "\\" + ts.ToString() + ".png";
            //string tempPath = ProjectPath + "\\" + "aa.png";

            for (int i = 0; i < Elist.Count; i++)
            {
                Elist[i].Visibility = Visibility.Hidden;
            }

            BitmapEncoder encoder = null;

            RenderTargetBitmap bitmap = ConverterBitmapImage(mycanv);

            FileStream stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write);


            encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            encoder.Save(stream);

            stream.Close();

            tempPath = System.IO.Path.GetFileName(tempPath);


            for (int i = 0; i < Elist.Count; i++)
            {
                Elist[i].Visibility = Visibility.Visible;
            }

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j].Type == 1 || map[i, j].Type == 2 || map[i, j].Type == 5)
                    {
                        foreach (FrameworkElement fe in mycanv.Children)
                        {
                            double top = (double)fe.GetValue(Canvas.TopProperty); //y
                            double left = (double)fe.GetValue(Canvas.LeftProperty); //x
                            if (left / 50 == j && top / 50 == i)
                            {
                                MonsterMap[i, j] = System.IO.Path.GetFileName(((BitmapImage)((Image)fe).Source).UriSource.AbsolutePath);
                            }
                        }
                    }
                    if (map[i, j].Type == 3)
                    {
                        foreach (FrameworkElement fe in mycanv.Children)
                        {
                            double top = (double)fe.GetValue(Canvas.TopProperty); //y
                            double left = (double)fe.GetValue(Canvas.LeftProperty); //x
                            if (left / 50 == j && top / 50 == i)
                            {
                                //MonsterMap[i, j] = System.IO.Path.GetFileName(((BitmapImage)((Image)fe).Source).UriSource.AbsolutePath);
                                MonsterMap[i, j] = ((BitmapImage)((Image)fe).Source).UriSource.AbsolutePath;
                            }
                        }
                        HeroImagePath = System.IO.Path.GetFileName(MonsterMap[i, j]);
                        HeroAttack = map[i, j].Attack;
                        HeroHp = map[i, j].MaxHp;
                    }
                }
            }


            string gameField = "[";
            for (int i = 0; i < map.GetLength(0); i++)
            {
                gameField += "[";
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    switch (map[i, j].Type)
                    {
                        case 0:
                            gameField += "0";
                            break;
                        case 1:
                            if (map[i, j].Hp == 0)
                                AIFlag = "true";
                            if (map[i, j].Hp == 1)
                                AIFlag = "false";
                            gameField += "{name : '" + map[i, j].Name + @"',src : decodeURI('" + MonsterMap[i, j] + @"'),isAI : " + AIFlag + " ,health : " + map[i, j].MaxHp + @", attack : " + map[i, j].Attack + @" }";
                            break;
                        case 2:
                            gameField += "{name : '" + map[i, j].Name + @"',src : decodeURI('" + MonsterMap[i, j] + "'),isMovable : 'movable'  }";
                            break;
                        case 3:
                            gameField += "'hero'";
                            break;
                        case 4:
                            gameField += "'door'";
                            break;
                        case 5:
                            gameField += "{name : '" + map[i, j].Name + @"',src : decodeURI('" + MonsterMap[i, j] + "'),isMovable : 'unmovable'  }";
                            break;
                    }
                    if (j != map.GetLength(1) - 1)
                        gameField += ",";
                }
                gameField += "]";
                if (i != map.GetLength(0) - 1)
                    gameField += ",";
            }
            gameField += "];";

            //string path = @"C:\Users\SongHyunIl\Desktop\Kimchinity\송현일\TOOL\kimchinity_수정본\WPF_ObjectDrag\bin\Debug\RealGame.js";


            if (FStartSound)
            {
                CopyFiles(StartSound, ProjectPath + "\\" + System.IO.Path.GetFileName(StartSound));
                StartSound = "'" + System.IO.Path.GetFileName(StartSound) + "'";
            }
            if (FMainSound)
            {
                CopyFiles(MainSound, ProjectPath + "\\" + System.IO.Path.GetFileName(MainSound));
                MainSound = "'" + System.IO.Path.GetFileName(MainSound) + "'";
            }
            if (FHeroAttackSound)
            {
                CopyFiles(HeroAttackSound, ProjectPath + "\\" + System.IO.Path.GetFileName(HeroAttackSound));
                HeroAttackSound = "'" + System.IO.Path.GetFileName(HeroAttackSound) + "'";
            }
            if (FHeroMoveSound)
            {
                CopyFiles(HeroMoveSound, ProjectPath + "\\" + System.IO.Path.GetFileName(HeroMoveSound));
                HeroMoveSound = "'" + System.IO.Path.GetFileName(HeroMoveSound) + "'";
            }
            if (FHeroHitedSound)
            {
                CopyFiles(HeroHitedSound, ProjectPath + "\\" + System.IO.Path.GetFileName(HeroHitedSound));
                HeroHitedSound = "'" + System.IO.Path.GetFileName(HeroHitedSound) + "'";
            }
            if (FHeroDieSound)
            {
                CopyFiles(HeroDieSound, ProjectPath + "\\" + System.IO.Path.GetFileName(HeroDieSound));
                HeroDieSound = "'" + System.IO.Path.GetFileName(HeroDieSound) + "'";
            }
            if (FEnemyHitedSound)
            {
                CopyFiles(EnemyHitedSound, ProjectPath + "\\" + System.IO.Path.GetFileName(EnemyHitedSound));
                EnemyHitedSound = "'" + System.IO.Path.GetFileName(EnemyHitedSound) + "'";
            }
            if (FEnemyDieSound)
            {
                CopyFiles(EnemyDieSound, ProjectPath + "\\" + System.IO.Path.GetFileName(EnemyDieSound));
                EnemyDieSound = "'" + System.IO.Path.GetFileName(EnemyDieSound) + "'";
            }
            if (FObstacleSound)
            {
                CopyFiles(ObstacleSound, ProjectPath + "\\" + System.IO.Path.GetFileName(ObstacleSound));
                ObstacleSound = "'" + System.IO.Path.GetFileName(ObstacleSound) + "'";
            }
            if (FEndingSound)
            {
                CopyFiles(EndingSound, ProjectPath + "\\" + System.IO.Path.GetFileName(EndingSound));
                EndingSound = "'" + System.IO.Path.GetFileName(EndingSound) + "'";
            }
            if (FClearSound)
            {
                CopyFiles(ClearSound, ProjectPath + "\\" + System.IO.Path.GetFileName(ClearSound));
                ClearSound = "'" + System.IO.Path.GetFileName(ClearSound) + "'";
            }


            if (FTitleImg)
            {
                CopyFiles(TitleImg, ProjectPath + "\\" + System.IO.Path.GetFileName(TitleImg));
                TitleImg = "'" + System.IO.Path.GetFileName(TitleImg) + "'";
            }

            if (FEndImage)
            {
                CopyFiles(EndImage, ProjectPath + "\\" + System.IO.Path.GetFileName(EndImage));
                EndImage = "'" + System.IO.Path.GetFileName(EndImage) + "'";
            }

            if (FClearImage)
            {
                CopyFiles(ClearImage, ProjectPath + "\\" + System.IO.Path.GetFileName(ClearImage));
                ClearImage = "'" + System.IO.Path.GetFileName(ClearImage) + "'";
            }


            #region 입출력
            string value = @"
var heroImage = '" + HeroImagePath + @"';
var playerAttack = " + map[HeroY, HeroX].Attack + @";
var playerHeart = " + map[HeroY, HeroX].MaxHp + @";
     
function onGameInit(test) {
 isConnected = test;

//iftest
 
//sub
     blockWidth = 50;
     real_canvasWidth = blockWidth*" + map.GetLength(1) + @";
     real_canvasHeight = blockWidth*" + map.GetLength(0) + @";
    
     closedDoorSrc = 'cloesedGate.png';
     openDoorSrc = 'openGate.png';
    
     game = new Game('KimchynityGame', 'GameCanvas');
     playState = new PlayGameState();
     logoState =  new GameLogoState();
    
     mapBasesrc = '" + tempPath + @"';
     gameoversrc = " + EndImage + @";
     gameclearsrc = " + ClearImage + @";
     heroHittedsrc = " + HeroHitedSound + @";
     heroStepsrc = " + HeroMoveSound + @";
     heroDiesrc = " + HeroDieSound + @";
     gameoversound = " + EndingSound + @";
     gameclearsound = " + ClearSound + @";
     backgroundSound = " + MainSound + @";
     titleSound = " + StartSound + @";
     effectSound = " + HeroAttackSound + @";
     enemyDieSound = " + EnemyDieSound + @";
     titleImage = " + TitleImg + @";
    
    
     gametitleState =  new GameTitleState(titleImage);
     heroObject = new HeroSprite(heroImage,playerHeart,playerAttack);
      
        gameField = " + gameField + @";


//tmp
//elstt
                                                                                                           
                                                                                                            
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
                                                                                                           
 
//insert
                                                                                             
                                                                                             
                                                                                             
                                                                                             
                                                                                             
    if(gameoversrc == 'undefined')
    gameoversrc ='http://tooopen.github.io/KimOEngine/images/over.jpg';    
 
    if(gameclearsrc == 'undefined')
    gameclearsrc ='http://tooopen.github.io/KimOEngine/images/gameclear.jpg'; 

    if(titleImage == 'undefined')
    titleImage ='http://tooopen.github.io/KimOEngine/images/title.jpg'; 


    resourcePreLoader.AddImage(titleImage);
    resourcePreLoader.AddImage(gameoversrc);
    resourcePreLoader.AddImage(gameclearsrc);
    resourcePreLoader.AddImage(mapBasesrc);
    soundSystem.AddSound(backgroundSound, 2);
    soundSystem.AddSound(heroHittedsrc);
    soundSystem.AddSound(heroStepsrc);
    soundSystem.AddSound(heroDiesrc,2);
    soundSystem.AddSound(gameoversound,2);
    soundSystem.AddSound(gameclearsound,2);
    soundSystem.AddSound(effectSound);
    soundSystem.AddSound(enemyDieSound);
    soundSystem.AddSound(titleSound,2);
    
    for (var i = 0; i < gameField.length; ++i) {
        for (var j = 0; j < gameField[0].length; ++j) {
            if ( typeof gameField[i][j] == 'string') {
                if(gameField[i][j] == 'hero')
                {
                    heroObject.left = blockWidth * j;
                    heroObject.top = blockWidth * i;
                    heroObject.x = j;
                    heroObject.y = i;
                    heroObject.Invalid();
                    gameField[i][j] = 0;
                    
                }
                else if(gameField[i][j]=='door'){
                    
                    doorsprite.y = i;
                    doorsprite.x = j;
                    doorsprite.top = blockWidth * i;
                    doorsprite.left = blockWidth * j;
                    doorsprite.Invalid();
                }
            }
            else if(typeof gameField[i][j] == 'object')
            {
            if(/enemy+/.test(gameField[i][j].name)){
            
                    var data = gameField[i][j];
                    var temp = new EnemySprite(data.name,data.src,data.isAI,data.health,data.attack);
                    temp.left = blockWidth * j;
                    temp.top = blockWidth * i;
                    temp.x = j;
                    temp.y = i;
                    temp.Invalid();
                    temp.AIUpdate();
                    gameField[i][j] = data.name;
                    enemyManagerObject.AddObject(temp);
                }
                else if(/obstacle+/.test(gameField[i][j].name))
                {
                    var data = gameField[i][j];
                    var temp = new ObstacleSprite(data.name,data.src,data.isMovable);
                    temp.left = blockWidth * j;
                    temp.top = blockWidth * i;
                    temp.x = j;
                    temp.y = i;
                    temp.Invalid();
                    playState.AddSprite(temp);
                    gameField[i][j] = data.name;;
                }
            }
        }
    }
    
    inputSystem.AddKeyListener({
        key : 'up arrow',
        listener : function() {
        
                direction = 'up';
                game.heroMoveCollisionCheck();
            
        }
    },
    {
        key : 'down arrow',
        listener : function() {
                           direction = 'down';
                game.heroMoveCollisionCheck();
                
            

        }
    },
    {
        key : 'right arrow',
        listener : function() {
           
                direction = 'right';
                game.heroMoveCollisionCheck();
            

        }
    },
    {
        key : 'left arrow',
        listener : function() {
           
                direction = 'left';
                game.heroMoveCollisionCheck();
            

        }
     },
     {
        key : 'enter',
        listener : function() {
            if (nowGameStateis == 3) {
                soundSystem.PauseSound(titleSound);
                ChangeGameState(new TransitionFadeOut(game_state,new TransitionFadeIn(game_state,playState,3.0),5.0));
                nowGameStateis = 4;
            }
        }
    },
    {
        key : 'space',
        listener : function() {
            if (nowGameStateis == 4) {
                game.heroAttack();
              
            }

        }
    },
    {
        key : 'p',
        listener : function() {
            if (nowGameStateis == 4) {
            console.log('nope');
              game.paused =! game.paused;
            }

        }
    }
    ); 
      
     game.start();
}

window.addEventListener('load', onGameInit(false), false);/////////this
";
            tempPath = ProjectPath + "\\" + "RealGame.js";
            System.IO.File.WriteAllText(tempPath, value, Encoding.Default);

            string htmlsorce = @"<!DOCTYPE html>
<html lang='ko'>
   <head>
      <meta charset='UTF-8' />
      <title>aptana test</title>

      <style>
         #GameCanvas {
            position: absolute;
            left: 10%;
            top: 10%;
            width: 60%;
            height: 80%;
            padding-left: 0;
            padding-right: 0;
            margin-left: auto;
            margin-right: auto;
            display: block;
            -webkit-box-shadow: 4px 4px 8px rgba(0,0,0,0.5);
            -moz-box-shadow: 4px 4px 8px rgba(0,0,0,0.5);
            box-shadow: 4px 4px 8px rgba(0,0,0,0.5);
            z-index: 2;
         }
         #GUICanvas {
            position: absolute;
            left: 10%;
            top: 10%;
            width: 60%;
            height: 80%;
            padding-left: 0;
            padding-right: 0;
            margin-left: auto;
            margin-right: auto;
            display: block;
            z-index: 3;
         }
         #GameTransition {
            position: absolute;
            left: 10%;
            top: 10%;
            width: 60%;
            height: 80%;
            padding-left: 0;
            padding-right: 0;
            margin-left: auto;
            margin-right: auto;
            display: block;
            z-index: 4;
         }
         #logo {
            position: absolute;
            left: 80%;
            top: 65%;
            width: 20%;
            height: 25%;
            padding-left: 0;
            padding-right: 0;
            margin-top: 20px;
            margin-left: auto;
            margin-right: auto;
            display: block;
            z-index: 3;
         }
      </style>
   </head>
   <body>

      <canvas id ='GameCanvas' >
         캔버스를 지원안하네
      </canvas>

      <canvas id ='GUICanvas' >
         캔버스를 지원안하네
      </canvas>

      <canvas id ='GameTransition' >
         캔버스를 지원안하네
      </canvas>

      <script src='http://tooopen.github.io/KimOEngine/javascripts/inputSystem.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/SoundManager.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/SlashSprite.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/ResourcePreLoader.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/sprite.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/HeroSprite.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/enemySprite.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/doorSprite.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/enemyManager.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/GUI.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/GameLogoState.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/gameTransition.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/obstacleSprite.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/transition.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/GamePlayState.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/GameTitleState.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/RequsetNewAnimation.js'></script>
      <script src='http://tooopen.github.io/KimOEngine/javascripts/Game.js'></script>
      <script src='RealGame.js'></script>
      <script src='http://cdn.rawgit.com/satcy/centiscript/master/js/release/centi.min.0.4.9b.js'></script>

      <img  id='back' src  = 'http://tooopen.github.io/KimOEngine/images/back.png' style='position:absolute; z-index: 1; top:0px;left:0px'width='100%'height='100%'/>
      <a href = 'https://ko-kr.facebook.com/monkeyhandle'> <img  id='logo' src  = 'http://tooopen.github.io/KimOEngine/images/logo.png' align='middle' /> </a>
   </body>
</html>";
            #endregion
            tempPath = ProjectPath + "\\" + "Game.html";
            System.IO.File.WriteAllText(tempPath, htmlsorce, Encoding.Default);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (map[i, j].Type != 0)
                    {
                        foreach (FrameworkElement fe in mycanv.Children)
                        {
                            double top = (double)fe.GetValue(Canvas.TopProperty); //y
                            double left = (double)fe.GetValue(Canvas.LeftProperty); //x
                            if (left / 50 == j && top / 50 == i)
                            {
                                CopyFiles(((BitmapImage)((Image)fe).Source).UriSource.AbsolutePath, ProjectPath + "\\" + System.IO.Path.GetFileName(((BitmapImage)((Image)fe).Source).UriSource.AbsolutePath));
                            }
                        }
                    }
                }
            }


            MessageBox.Show("Export 완료");
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            string path = "";
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG|*.png|JPG|*.jpg|GIF|*.gif|BMP|*.bmp";

            Nullable<bool> result = dlg.ShowDialog();


            if (result == true)
            {
                path = dlg.FileName;
                string file = path;
               
                // 이미지 로드
                // 네트워크 관련
                {
                    String name = System.IO.Path.GetFileName(file);

                    var buffers = File.ReadAllBytes(file);
                    FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);

                    var msg = CPacket.Create((short)PROTOCOL.FILE_SEND_REQ);
                    msg.Push(name);
                    msg.Push(buffers.Length);

                    var nCeiling = (int)Math.Ceiling(buffers.Length / (double)CNetworkService.BufferSize);

                    msg.Push(nCeiling);
                    msg.Push(currentFileNum);
                    msg.Push(1);

                    s_server[0].Send(msg);

                    int currentLength = 0;
                    short count = 1;
                    while (currentLength < buffers.Length)
                    {
                        var bytes = buffers.Skip(currentLength).Take(CNetworkService.BufferSize - 12).ToArray();
                        var nIndex = bytes.Length;
                        var packet = CPacket.Create((short)PROTOCOL.FILE_SEND);
                        currentLength += nIndex;

                        packet.Push(currentFileNum);
                        packet.PushInt16(count++);
                        packet.Push(bytes);
                        s_server[0].Send(packet);
                    }

                    Console.WriteLine("Filenum : {0}", currentFileNum);
                    currentFileNum++;
                }
            }
          
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            EventPage win = new EventPage();
            win.Loaded += OnLoadedEventPage;
            win.Closed += OnClosedEventPage;
            win.Show();
        }

        private void OnLoadedEventPage(object sender, EventArgs e)
        {
            Console.WriteLine("OnLoadedEventPage");

            CPacket msg = CPacket.Create((short) PROTOCOL.EVENT_PAGE_OPEN_REQ);
            s_server[0].Send(msg);
        }

        private void OnClosedEventPage(object sender, EventArgs e)
        {
            Console.WriteLine("OnClosedEventPage");
            CPacket msg = CPacket.Create((short)PROTOCOL.EVENT_PAGE_CLOSE_REQ);

            Console.WriteLine(StartSound              );
            Console.WriteLine(MainSound               );
            Console.WriteLine(HeroAttackSound         );
            Console.WriteLine(HeroMoveSound           );
            Console.WriteLine(HeroHitedSound          );
            Console.WriteLine(HeroDieSound            );
            Console.WriteLine(EnemyHitedSound         );
            Console.WriteLine(EnemyDieSound           );
            Console.WriteLine(ObstacleSound           );
            Console.WriteLine(EndingSound             );
            Console.WriteLine(ClearSound              );
            Console.WriteLine(TitleImg                );
            Console.WriteLine(EndImage                );
            Console.WriteLine(ClearImage              );

            msg.Push(StartSound);
            msg.Push(MainSound);
            msg.Push(HeroAttackSound);
            msg.Push(HeroMoveSound);
            msg.Push(HeroHitedSound);
            msg.Push(HeroDieSound);
            msg.Push(EnemyHitedSound);
            msg.Push(EnemyDieSound);
            msg.Push(ObstacleSound);
            msg.Push(EndingSound);
            msg.Push(ClearSound);
            msg.Push(TitleImg);
            msg.Push(EndImage);
            msg.Push(ClearImage);
            s_server[0].Send(msg);

            // 이미지 로드
            // 네트워크 관련
            {                                    
                SendResourceFile(StartSound      , 2);
                SendResourceFile(MainSound       , 2);
                SendResourceFile(HeroAttackSound , 2);
                SendResourceFile(HeroMoveSound   , 2);
                SendResourceFile(HeroHitedSound  , 2);
                SendResourceFile(HeroDieSound    , 2);
                SendResourceFile(EnemyHitedSound , 2);
                SendResourceFile(EnemyDieSound   , 2);
                SendResourceFile(ObstacleSound   , 2);
                SendResourceFile(EndingSound     , 2);
                SendResourceFile(ClearSound      , 2);
                SendResourceFile(TitleImg        , 2);
                SendResourceFile(EndImage        , 2);
                SendResourceFile(ClearImage      , 2);
            }
        }

        private void SendResourceFile(string filename, int mode)
        {
            string file = filename.Replace("'","");
            if (file.Equals("undefined"))
                return;

            String name = System.IO.Path.GetFullPath(file);

            var buffers = File.ReadAllBytes(name);
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            if (null == stream)
                return;


            var msg = CPacket.Create((short)PROTOCOL.FILE_SEND_REQ);
            msg.Push(file);
            msg.Push(buffers.Length);

            var nCeiling = (int)Math.Ceiling(buffers.Length / (double)CNetworkService.BufferSize);

            msg.Push(nCeiling);
            msg.Push(currentFileNum);
            msg.Push(mode);

            s_server[0].Send(msg);

            int currentLength = 0;
            short count = 1;
            while (currentLength < buffers.Length)
            {
                var bytes = buffers.Skip(currentLength).Take(CNetworkService.BufferSize - 12).ToArray();
                var nIndex = bytes.Length;
                var packet = CPacket.Create((short)PROTOCOL.FILE_SEND);
                currentLength += nIndex;

                packet.Push(currentFileNum);
                packet.PushInt16(count++);
                packet.Push(bytes);
                s_server[0].Send(packet);
            }

            Console.WriteLine("Filenum : {0}", currentFileNum);
            currentFileNum++;
        }

        private void comboBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            map[yp, xp].Hp = comboBox2.SelectedIndex;
        }

        public void SelectingImage(string p)
        {
            var fullpath = Path.GetFullPath(p);
            fullpath = fullpath.Replace("\\", "/");
            fullpath = fullpath.Replace(" ", "%20");

            var items = listBox1.Items;

            foreach(var item in items)
            {
                Image image = item as Image;
                BitmapImage bit = image.Source as BitmapImage;

                if (bit.UriSource.AbsolutePath.Equals(fullpath) == false)
                    continue;

                SelectedImageTemp = image;
                //image.IsEnabled = false;
            }
        }

        public void ImageAttach(int x, int y, int selMod)
        {
            if (null != SelectedImageTemp)
            {
                Image ig = new Image();
                ig.Source = SelectedImageTemp.Source;
                SelectedImageTemp = null;

                ig.Width = 50;
                ig.Height = 50;

                Dragg.SetDrag(ig, true);
                mycanv.Children.Add(ig);

                Canvas.SetLeft(ig, x * 50);
                Canvas.SetTop(ig, y * 50);

                int zIndex = (selMod == 0 ? 10000 : 0);

                Canvas.SetZIndex(ig, zIndex + ImgIndex);

                if (selMod == 0)
                {
                    Elist.Add(ig);
                    map[y, x].Number = ++ImgIndex;
                }
                else
                {
                    list.Add(ig);
                }
            }
        }

        public void ImageMove(int zOrder, int fx, int fy, int px, int py)
        {
            foreach (FrameworkElement fe in mycanv.Children)
            {
                var top = (double)fe.GetValue(Canvas.TopProperty);
                var left = (double)fe.GetValue(Canvas.LeftProperty);

                if((left/50) == fx && (top/50) == fy )
                {
                    if (Canvas.GetZIndex((Image)fe) == zOrder)
                    {
                        Canvas.SetLeft((Image)fe, px * 50);
                        Canvas.SetTop((Image)fe, py * 50);

                        return;
                    }
                }
            }
        }

        public void ImageBG(string filename)
        {
            string path = Path.GetFullPath(filename);

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Image im = new Image();
                    BitmapImage bit = new BitmapImage();
                    bit.BeginInit();
                    bit.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                    bit.DecodePixelHeight = 50;
                    bit.DecodePixelWidth = 50;
                    bit.EndInit();
                    im.Source = bit;
                    im.Height = 50;
                    im.Width = 50;

                    Dragg.SetDrag(im, true);
                    mycanv.Children.Add(im);
                    Canvas.SetLeft(im, j * 50);
                    Canvas.SetTop(im, i * 50);
                    Canvas.SetZIndex(im, zindex + ImgIndex);
                }
            }
        }

        public void DeleteImage(int xp , int yp)
        {
            Point pt = new Point();
            pt.X = xp;
            pt.Y = yp;
            foreach (FrameworkElement fe in mycanv.Children)
            {
                double top = (double)fe.GetValue(Canvas.TopProperty);
                double left = (double)fe.GetValue(Canvas.LeftProperty);
                del s = new del();
                Point pp = new Point();
                pp.X = top;
                pp.Y = left;
                s.p = pp;
                s.im = (Image)fe;
                s.z_index = Canvas.GetZIndex(fe);

                DList.Add(s);
            }
            for (int i = 0; i < DList.Count; i++)
            {
                if (DList[i].p.Y / 50 == pt.X && DList[i].p.X / 50 == pt.Y)
                {
                    DeleteFlag = true;
                    TempDo.Img = DList[i].im;
                    TempDo.Mod = SelectMod;
                    TempDo.Copy = CopyFlag;
                    TempDo.Delete = DeleteFlag;
                    TempDo.Fx = Fxp;
                    TempDo.Fy = Fyp;
                    TempDo.x = xp;
                    TempDo.y = yp;
                    CopyMapInfo(xp, yp);
                    RedoPush(TempDo);
                    mycanv.Children.Remove(DList[i].im);
                    DeleteFlag = false;
                }
            }
            SaveList.List.Clear();
            if (!SelectMod)
            {
                if (map[xp, yp].Type == 3)
                {
                    IsHero = 0;
                    HeroX = -1;
                    HeroY = -1;
                }
                if (map[xp, yp].Type == 4)
                {
                    IsDoor = 0;
                    DoorX = -1;
                    DoorY = -1;
                }
                SetDefaultMapInfo(xp, yp);
            }
        }

        public void SetProperty(int px, int py, int maxHp, int attack)
        {
            map[py, px].MaxHp = maxHp;
            map[py, px].Attack = attack;
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process ps = new System.Diagnostics.Process();
            ps.StartInfo.FileName = System.IO.Path.GetFullPath(ProjectPath) + "\\Game.html";
            ps.Start();
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Creadit \n\n 게임  엔진 : 高 변순항 (땔감) \n 게임     툴 : 古 송현일 (곶감) \n 게임  서버 : 酤 송시윤 (나감)");
        }

        private void OnMinimap(object sender, RoutedEventArgs e)
        {
            DrawMinimap();
        }

        private void DrawMinimap()
        {
            var renderTarget = ConverterBitmapImage(mycanv);
            m_Minimap.Source = renderTarget;
        }

        private void m_ScrollViewOfMiddleBorder_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var mapSizeX = m_Minimap.Width;
            var mapSizeY = m_Minimap.Height;

            var borderX = m_BorderOfMiddle.ActualWidth;
            var borderY = m_BorderOfMiddle.ActualHeight;

            var canvasX = mycanv.ActualWidth;
            var canvasY = mycanv.ActualHeight;

            var ratioX = borderX / canvasX;
            var ratioY = borderY / canvasY;

            var offsetX = m_ScrollViewOfMiddleBorder.HorizontalOffset/(m_ScrollViewOfMiddleBorder.ExtentWidth);
            var offsetY = m_ScrollViewOfMiddleBorder.VerticalOffset  /(m_ScrollViewOfMiddleBorder.ExtentHeight);

            Console.WriteLine(offsetX);
            Console.WriteLine(offsetY);
            Console.WriteLine(ratioX * mapSizeX);
            Console.WriteLine(ratioY * mapSizeY);

            Canvas.SetLeft(m_RectMapSizeSelector, offsetX * mapSizeX);
            Canvas.SetTop(m_RectMapSizeSelector, offsetY * mapSizeY);
            m_RectMapSizeSelector.Width = ratioX * mapSizeX;
            m_RectMapSizeSelector.Height = ratioY * mapSizeY;
        }

        private void m_CanvasOfMinimap_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsMinimapClicked = true;
        }

        private void m_CanvasOfMinimap_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMinimapClicked)
            {
                Point p = Mouse.GetPosition(m_CanvasOfMinimap);

                var pxx = p.X - m_RectMapSizeSelector.Width / 2;
                var pyy = p.Y - m_RectMapSizeSelector.Height / 2;

                var minX = 0;
                var minY = 0;
                var maxX = m_Minimap.ActualWidth - m_RectMapSizeSelector.Width;
                var maxY = m_Minimap.ActualHeight - m_RectMapSizeSelector.Height;

                if (pxx < minX)
                    pxx = minX;

                if (pxx > maxX)
                    pxx = maxX;

                if (pyy < minY)
                    pyy = minY;

                if (pyy > maxY)
                    pyy = maxY;

                Canvas.SetLeft(m_RectMapSizeSelector, pxx);
                Canvas.SetTop(m_RectMapSizeSelector, pyy);

                var ratioX = pxx / m_Minimap.ActualWidth;
                var ratioY = pyy / m_Minimap.ActualHeight;
                m_ScrollViewOfMiddleBorder.ScrollToHorizontalOffset(ratioX * m_ScrollViewOfMiddleBorder.ExtentWidth);
                m_ScrollViewOfMiddleBorder.ScrollToVerticalOffset(ratioY * m_ScrollViewOfMiddleBorder.ExtentHeight);
            }
        }

        private void m_CanvasOfMinimap_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_CanvasOfMinimap_MouseLeave(sender, e);
        }

        private void m_CanvasOfMinimap_MouseLeave(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                IsMinimapClicked = false;
            }
        }

        public void ApplyEventSheet(string[] p)
        {
            StartSound       =  p[0];
            MainSound        =  p[1];
            HeroAttackSound  =  p[2];
            HeroMoveSound    =  p[3];
            HeroHitedSound   =  p[4];
            HeroDieSound     =  p[5];
            EnemyHitedSound  =  p[6];
            EnemyDieSound    =  p[7];
            ObstacleSound    =  p[8];
            EndingSound      =  p[9];
            ClearSound       =  p[10];
            TitleImg         =  p[11];
            EndImage         =  p[12];
            ClearImage       =  p[13];
        }

        private void CopyFiles(string gu, string sin)
        {
            FileStream fsSrc;
            FileStream fsDest;
            try
            {
                fsSrc = new FileStream(gu, FileMode.Open, FileAccess.Read);
                fsDest = new FileStream(sin, FileMode.Create, FileAccess.Write);
            }
            catch (Exception ee)
            {
                MessageBox.Show("경로나 파일명에 한글이 포함되어 에러가 발생하였습니다.\n가급적 영문 경로와 폴더를 이용해 주세요.");
                return;
            }
            byte[] bts = new byte[4096];

            int valCurrent = 0;

            int seekPointer = 0;

            while (valCurrent < 100)
            {
                fsSrc.Seek(4096 * seekPointer, SeekOrigin.Begin);
                fsSrc.Read(bts, 0, 4096);

                fsDest.Seek(4096 * seekPointer, SeekOrigin.Begin);
                fsDest.Write(bts, 0, 4096);

                seekPointer++;

                // 진행률
                valCurrent = (int)(fsDest.Length * 100 / fsSrc.Length);

            }

            fsSrc.Dispose();
            fsDest.Dispose();
        }
    }

    [Serializable]
    public class save
    {

        public string path { get; set; }
        public Point p { get; set; }
        public int z_index { get; set; }
        //[NonSerialized]
        //public Image im { get; set; }
        public int Type { get; set; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int Count { get; set; }
    }

    [Serializable]
    public class SaveFile
    {
        public List<save> List = new List<save>();
        //public Map[,] map = new Map[17, 22];
        //public List<Map> MList = new List<Map>();
    }

    public class del
    {

        public string path { get; set; }
        public Point p { get; set; }
        public int z_index { get; set; }
        public Image im { get; set; }
        public int Type { get; set; }
        public int MaxHp { get; set; }
        public int Hp { get; set; }
        public int Attack { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public int Count { get; set; }
    }
}
