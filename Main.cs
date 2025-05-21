using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.ImageFile;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.ToolBlock;
using Microsoft.WindowsAPICodePack.Dialogs;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using ViDi2;
using ViDi2.VisionPro;

namespace FolderEvent
{
    public partial class Main : Form
    {
        private int ModelCount = 0;
        private int ModelNameCount = 0;
        public int ModelLoad = 0;
        public bool ModelLoad_State = false;
        private int NgImagecnt = 0;

        public DateTime StartTime;
        private TimeSpan UpTimeSpan;

        private string DB_Ng_Num = "";

        public bool Inspection1_State = false;
        public bool Inspection2_State = false;
        public bool Inspection3_State = false;
        public bool Inspection4_State = false;
        public bool Inspection5_State = false;

        public bool Image1_State = false;
        public bool Image2_State = false;
        public bool Image3_State = false;
        public bool Image4_State = false;
        public bool Image5_State = false;

        public int Inspection1 = 0;
        public int Inspection2 = 0;
        public int Inspection3 = 0;
        public int Inspection4 = 0;
        public int Inspection5 = 0;

        public bool Trigger_State = false;

        private string[] dirname;
        private string[] PartNo = new string[100];
        private string[] carrierindex = new string[100];
        private string[] PartName = new string[100];
        private string[] ALC = new string[100];
        private string[] Front_Exist = new string[100];
        private string[] Front_PartName = new string[100];
        private string[] Front_ALC = new string[100];
        private string[] Rear_Exist = new string[100];
        private string[] Rear_PartName = new string[100];
        private string[] Rear_ALC = new string[100];

        private bool stop = false;

        public CogAcqFifoTool Center_Aquire = new CogAcqFifoTool();
        public CogAcqFifoTool Front_LH_Aquire = new CogAcqFifoTool();
        public CogAcqFifoTool Front_RH_Aquire = new CogAcqFifoTool();

        public CogImage24PlanarColor C_Aquire_Image = new CogImage24PlanarColor();
        public CogImage24PlanarColor FRH_Aquire_Image = new CogImage24PlanarColor();
        public CogImage24PlanarColor FLH_Aquire_Image = new CogImage24PlanarColor();
        public CogImage24PlanarColor RRH_Aquire_Image = new CogImage24PlanarColor();
        public CogImage24PlanarColor RLH_Aquire_Image = new CogImage24PlanarColor();

        public CogImage24PlanarColor Center_Aquire_Image = new CogImage24PlanarColor();
        public CogImage24PlanarColor Front_LH_Aquire_Image = new CogImage24PlanarColor();
        public CogImage24PlanarColor Front_RH_Aquire_Image = new CogImage24PlanarColor();

        private int Number = 0;

        public static List<string> imagepath = null;

        private FileSystemWatcher fileSystemWatcher1 = new FileSystemWatcher();

        public Thread CenterThread = null;
        public Thread loadThread = null;
        public Thread Front_LHThread = null;
        public Thread Front_RHThread = null;
        public Thread Rear_LHThread = null;
        public Thread Rear_RHThread = null;
        public Thread Cam_Thread = null;

        private int Trigger1 = 0;
        private int Trigger2 = 0;
        private int Trigger3 = 0;
        private int Trigger4 = 0;
        private int Trigger5 = 0;

        private bool grap_state_center = false;
        private bool grap_state_lh = false;
        private bool grap_state_rh = false;

        private bool m_nStop = false;
        private string DirPath = "";

        private string _DB_Start = "";
        private string _DB_Body = "";
        private string _DB_result = "";
        private string _DB_index = "";
        private string _DB_img = "";
        private string _DB_path = "";

        private string _server = "localhost"; //DB 서버 주소, 로컬일 경우 localhost
        private int _port = 3308; //DB 서버 포트
        private string _database = "new_schema"; //DB 이름
        private string _id = "root"; //계정 아이디
        private string _pw = "root"; //계정 비밀번호
        private string DB_cs = "";
        private string DB_cs_send = "";
        private int _DeleteFoldor = 30;
        private int _NGTime = 3000;
        private int _SiS = 0;

        private string _server_send = "localhost"; //DB 서버 주소, 로컬일 경우 localhost
        private int _port_send = 3308; //DB 서버 포트
        private string _database_send = "new_schema"; //DB 이름
        private string _id_send = "root"; //계정 아이디
        private string _pw_send = "root"; //계정 비밀번호

        private string _server_ftp = "localhost"; //DB 서버 주소, 로컬일 경우 localhost
        private int _port_ftp = 3308; //DB 서버 포트
        private string _database_ftp = "new_schema"; //DB 이름
        private string _id_ftp = "root"; //계정 아이디
        private string _pw_ftp = "root"; //계정 비밀번호

        private string _Finalpath = ""; //파이널 경로

        private object LockImageSave = new object();
        
        private CogImageFileTool imagefileTool = new CogImageFileTool();
        private CogImageFileTool imagefileTool_Center = new CogImageFileTool();
        private CogImageFileTool imagefileTool_Front_LH = new CogImageFileTool();
        private CogImageFileTool imagefileTool_Front_RH = new CogImageFileTool();
        private CogImageFileTool imagefileTool_Rear_LH = new CogImageFileTool();
        private CogImageFileTool imagefileTool_Rear_RH = new CogImageFileTool();

        private CogImage24PlanarColor Center_image = new CogImage24PlanarColor();
        private CogIPOneImageTool imagep = new CogIPOneImageTool();
        private CogToolBlock S1_F41 = new CogToolBlock();
        private CogToolBlock Q1_Center = new CogToolBlock();

        private string Center_Result = "";
        private string Front_LH_Result = "";
        private string Front_RH_Result = "";
        private string Rear_LH_Result = "";
        private string Rear_RH_Result = "";

        private bool garp_state = false;
        private bool garp_complete = true;

        [DllImport("uio.dll")]
        private static extern bool usb_io_output(int pID, int cmd, int io1, int io2, int io3, int io4);

        #region ViDi
        private ViDi2.Runtime.Local.Control control = null;
        private ViDi2.Runtime.IWorkspace[] workspace = new ViDi2.Runtime.IWorkspace[24];
        private ViDi2.Runtime.IStream[] stream = new ViDi2.Runtime.IStream[8];
        private ViDi2.ISample[] sample = new ISample[8];
        private ViDi2.IImage[] image = new IImage[8];
        private ViDi2.IMarking marking = null;
        private ViDi2.IView view = null;
        private ViDi2.IRedView redView = null;
        private ViDi2.IBlueMarking Bluemarking = null;
        private ViDi2.IBlueView BlueView = null;
        private ViDi2.IGreenMarking Greenmarking = null;
        private ViDi2.IGreenView GreenView = null;
        private ViDi2.IRedMarking Redmarking = null;
        private ViDi2.IRedView RedView = null;
        #endregion

        public Main()
        {
            InitializeComponent();

            imagepath = new List<string>();

            fileSystemWatcher1.Path = @"\\Vision-pc\image\";
            fileSystemWatcher1.Filter = "*.*";
            fileSystemWatcher1.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.Size | NotifyFilters.FileName;
            fileSystemWatcher1.Created += fileSystemWatcher1_Created;
            fileSystemWatcher1.IncludeSubdirectories = true;

            StartTime = DateTime.Now;
            try
            {
                if (!Directory.Exists(ProcessDefine._MODEL_FILE_ROOT))
                {
                    Directory.CreateDirectory(ProcessDefine._MODEL_FILE_ROOT);
                }

                CogFrameGrabbers CogFrameGrabbers = new CogFrameGrabbers();

                if (CogFrameGrabbers.Count != 3)
                {
                    if (ProcessFunc.OnMessageDlg("카메라 연결 실패!! 연결 상태를 확인해주세요", (int)ProcessDefine.WARNING_TYPE.WARNING) != 2)
                    {
                        this.Close();
                        Application.Exit();
                        return;
                    }
                }

                Center_Aquire = CogSerializer.LoadObjectFromFile(ProcessDefine._MODEL_FILE_ROOT + "Center.vpp") as CogAcqFifoTool;
                Front_LH_Aquire = CogSerializer.LoadObjectFromFile(ProcessDefine._MODEL_FILE_ROOT + "Front_LH.vpp") as CogAcqFifoTool;
                Front_RH_Aquire = CogSerializer.LoadObjectFromFile(ProcessDefine._MODEL_FILE_ROOT + "Front_RH.vpp") as CogAcqFifoTool;
                Center_Aquire.Run();
                Front_LH_Aquire.Run();
                Front_RH_Aquire.Run();
            }
            catch
            {
                if (ProcessFunc.OnMessageDlg("카메라 연결 실패!! 연결 상태를 확인해주세요", (int)ProcessDefine.WARNING_TYPE.WARNING) != 2)
                {
                    this.Close();
                    Application.Exit();
                    return;
                }
            }
        }

        private void fileSystemWatcher1_Created(object sender,FileSystemEventArgs e)
        {
            string directoryname = string.Empty;
            string bodyno = "";
            DirPath = Path.GetDirectoryName(e.FullPath);

            dirname = DirPath.Split('\\');

            bodyno = lb_BodyNo.Text;
            if (bodyno != dirname[dirname.Length - 1])
            {
                if (garp_state == false && garp_complete == true)
                {
                    garp_state = true;
                    //231216 스레드 대신 fileSystemWatcher1_Created 내에서 촬영
                    Cam_Thread = new Thread(new ThreadStart(Cam_Start));
                    if (Cam_Thread.IsAlive != true)
                    {
                        Cam_Thread.Start();
                    }
                }
                this.Invoke(new MethodInvoker(delegate ()
                {
                    lb_CarType.Text = dirname[dirname.Length - 2];
                    lb_BodyNo.Text = dirname[dirname.Length - 1];
                }));
                DB_Ng_Num = "";
                if (lb_CarType.Text == "C1"|| lb_CarType.Text == "Q1" || lb_CarType.Text == "S1")
                {
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        dataGridView2.Rows.Clear();
                        dataGridView3.Rows.Clear();
                    }));

                    if (SelectDB_Partorder_Complete(lb_BodyNo.Text) == ProcessDefine._TRUE)
                    {
                        for (int i = 0; i < 100; i++)
                        {
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                dataGridView2.Rows.Add(new object[] { Front_PartName[i], Front_ALC[i], Front_Exist[i] });
                                dataGridView3.Rows.Add(new object[] { Rear_PartName[i], Rear_ALC[i], Rear_Exist[i] });
                            }));
                        }

                        //Thread.Sleep(2000);
                        Trigger1 = 1;
                        Trigger2 = 1;
                        Trigger3 = 1;
                        Trigger4 = 1;
                        Trigger5 = 1;
                        Inspection1 = 0;
                        Inspection2 = 0;
                        Inspection3 = 0;
                        Inspection4 = 0;
                        Inspection5 = 0;
                        garp_state = false;
                        garp_complete = true;

                        #region 스레드
                        stop = false;
                        loadThread = new Thread(new ThreadStart(loadStart));
                        if (loadThread.IsAlive != true)
                        {
                            loadThread.Start();
                        }
                        
                        #endregion

                    }
                    else
                    {
                        fileSystemWatcher1.EnableRaisingEvents = false;
                        ProcessFunc.OnMessageDlg("DB 통신 확인!!",1);
                    }
                }
            }
        }
        
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            fileSystemWatcher1.EnableRaisingEvents = false;
            m_nStop = true;
            Thread.Sleep(1000);
            foreach (Cognex.VisionPro.ICogFrameGrabber fg in new Cognex.VisionPro.CogFrameGrabbers())
            {
                fg.Disconnect(false);
            }
        }

        #region 쓰레드 함수
        public void loadStart()
        {
            try
            {
                bool load = false;
                string str = "";
                int camnum = 0;
                int i = 0;
               
                while (true)
                {
                    if (stop == true)
                    {
                        loadThread.Interrupt();
                        loadThread.Abort();
                        return;
                    }
                    switch (camnum)
                    {
                        case 0://센터
                            i = 0;
                            load = false;
                            while (i < 10)
                            {
                                Thread.Sleep(1000);
                                //FileInfo생성
                                FileInfo fi = new FileInfo(DirPath + "\\" + "CENTER_2.jpg");
                                FileInfo fi2 = new FileInfo(DirPath + "\\" + "CENTER_2.bmp");
                                //FileInfo.Exists로 파일 존재유무 확인 "
                                if (fi.Exists)
                                {
                                    str = DirPath + "\\" + "CENTER_2.jpg";
                                    try
                                    {
                                        imagefileTool_Center.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Center.Run();
                                        C_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Center.OutputImage;
                                        cogRecordDisplay_CENTER.Image = C_Aquire_Image;
                                        cogRecordDisplay_CENTER.Fit(false);
                                        camnum = 1;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if(i>=10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Center.Text = "이미지 로딩 NG";
                                                lb_Center.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                }
                                else if (fi2.Exists)
                                {
                                    str = DirPath + "\\" + "CENTER_2.bmp";
                                    try
                                    {
                                        imagefileTool_Center.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Center.Run();
                                        C_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Center.OutputImage;
                                        cogRecordDisplay_CENTER.Image = C_Aquire_Image;
                                        cogRecordDisplay_CENTER.Fit(false);
                                        camnum = 1;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Center.Text = "이미지 로딩 NG";
                                                lb_Center.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                }
                                else
                                {
                                    i++;
                                    if (i >= 10)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Center.Text = "이미지 로딩 NG";
                                            lb_Center.BackColor = Color.Red;
                                        }));
                                        load = true;
                                    }
                                }
                            }
                            if (load == true)
                            {
                                stop = true;
                            }
                            break;
                        case 1://Front_LH
                            i = 0;
                            load = false;
                            while (i < 10)
                            {
                                Thread.Sleep(500);
                                //FileInfo생성
                                FileInfo fi = new FileInfo(DirPath + "\\" + "FRONT_LH_0.jpg");
                                FileInfo fi2 = new FileInfo(DirPath + "\\" + "FRONT_LH_0.bmp");
                                //FileInfo.Exists로 파일 존재유무 확인 "
                                if (fi.Exists)
                                {
                                    str = DirPath + "\\" + "FRONT_LH_0.jpg";
                                    try
                                    {
                                        imagefileTool_Front_LH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Front_LH.Run();
                                        FLH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Front_LH.OutputImage;
                                        cogRecordDisplay_FRONT_LH.Image = FLH_Aquire_Image;
                                        cogRecordDisplay_FRONT_LH.Fit(false);
                                        camnum = 2;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Front_LH.Text = "이미지 로딩 NG";
                                                lb_Front_LH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    
                                }
                                else if (fi2.Exists)
                                {
                                    str = DirPath + "\\" + "FRONT_LH_0.bmp";
                                    try
                                    {
                                        imagefileTool_Front_LH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Front_LH.Run();
                                        FLH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Front_LH.OutputImage;
                                        cogRecordDisplay_FRONT_LH.Image = FLH_Aquire_Image;
                                        cogRecordDisplay_FRONT_LH.Fit(false);
                                        camnum = 2;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Front_LH.Text = "이미지 로딩 NG";
                                                lb_Front_LH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    i++;
                                    if (i >= 10)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_LH.Text = "이미지 로딩 NG";
                                            lb_Front_LH.BackColor = Color.Red;
                                        }));
                                        load = true;
                                    }
                                }
                            }
                            if (load == true)
                            {
                                stop = true;
                            }
                            break;
                        case 2://Front_RH
                            i = 0;
                            load = false;
                            while (i < 10)
                            {
                                Thread.Sleep(500);
                                //FileInfo생성
                                FileInfo fi = new FileInfo(DirPath + "\\" + "FRONT_RH_1.jpg");
                                FileInfo fi2 = new FileInfo(DirPath + "\\" + "FRONT_RH_1.bmp");
                                //FileInfo.Exists로 파일 존재유무 확인 "
                                if (fi.Exists)
                                {
                                    str = DirPath + "\\" + "FRONT_RH_1.jpg";
                                    try
                                    {
                                        imagefileTool_Front_RH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Front_RH.Run();
                                        FRH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Front_RH.OutputImage;
                                        cogRecordDisplay_FRONT_RH.Image = FRH_Aquire_Image;
                                        cogRecordDisplay_FRONT_RH.Fit(false);
                                        camnum = 3;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Front_RH.Text = "이미지 로딩 NG";
                                                lb_Front_RH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                }
                                else if (fi2.Exists)
                                {
                                    str = DirPath + "\\" + "FRONT_RH_1.bmp";
                                    try
                                    {
                                        imagefileTool_Front_RH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Front_RH.Run();
                                        FRH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Front_RH.OutputImage;
                                        cogRecordDisplay_FRONT_RH.Image = FRH_Aquire_Image;
                                        cogRecordDisplay_FRONT_RH.Fit(false);
                                        camnum = 3;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Front_RH.Text = "이미지 로딩 NG";
                                                lb_Front_RH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    i++;
                                    if (i >= 10)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_RH.Text = "이미지 로딩 NG";
                                            lb_Front_RH.BackColor = Color.Red;
                                        }));
                                        load = true;
                                    }
                                }
                            }
                            if (load == true)
                            {
                                stop = true;
                            }
                            break;
                        case 3://Rear_LH
                            i = 0;
                            load = false;
                            while (i < 10)
                            {
                                Thread.Sleep(500);
                                //FileInfo생성
                                FileInfo fi = new FileInfo(DirPath + "\\" + "REAR_LH_3.jpg");
                                FileInfo fi2 = new FileInfo(DirPath + "\\" + "REAR_LH_3.bmp");
                                //FileInfo.Exists로 파일 존재유무 확인 "
                                if (fi.Exists)
                                {
                                    str = DirPath + "\\" + "REAR_LH_3.jpg";
                                    try
                                    {
                                        imagefileTool_Rear_LH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Rear_LH.Run();
                                        RLH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Rear_LH.OutputImage;
                                        cogRecordDisplay_REAR_LH.Image = RLH_Aquire_Image;
                                        cogRecordDisplay_REAR_LH.Fit(false);
                                        camnum = 4;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Rear_LH.Text = "이미지 로딩 NG";
                                                lb_Rear_LH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    break;
                                }
                                else if (fi2.Exists)
                                {
                                    str = DirPath + "\\" + "REAR_LH_3.bmp";
                                    try
                                    {
                                        imagefileTool_Rear_LH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Rear_LH.Run();
                                        RLH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Rear_LH.OutputImage;
                                        cogRecordDisplay_REAR_LH.Image = RLH_Aquire_Image;
                                        cogRecordDisplay_REAR_LH.Fit(false);
                                        camnum = 4;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Rear_LH.Text = "이미지 로딩 NG";
                                                lb_Rear_LH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    i++;
                                    if (i >= 10)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_LH.Text = "이미지 로딩 NG";
                                            lb_Rear_LH.BackColor = Color.Red;
                                        }));
                                        load = true;
                                    }
                                }
                            }
                            if (load == true)
                            {
                                stop = true;
                            }
                            break;
                        case 4://Rear_RH
                            i = 0;
                            load = false;
                            while (i < 10)
                            {
                                Thread.Sleep(500);
                                //FileInfo생성
                                FileInfo fi = new FileInfo(DirPath + "\\" + "REAR_RH_4.jpg");
                                FileInfo fi2 = new FileInfo(DirPath + "\\" + "REAR_RH_4.bmp");
                                //FileInfo.Exists로 파일 존재유무 확인 "
                                if (fi.Exists)
                                {
                                    str = DirPath + "\\" + "REAR_RH_4.jpg";
                                    try
                                    {
                                        imagefileTool_Rear_RH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Rear_RH.Run();
                                        RRH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Rear_RH.OutputImage;
                                        cogRecordDisplay_REAR_RH.Image = RRH_Aquire_Image;
                                        cogRecordDisplay_REAR_RH.Fit(false);
                                        camnum = 5;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Rear_RH.Text = "이미지 로딩 NG";
                                                lb_Rear_RH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    break;
                                }
                                else if (fi2.Exists)
                                {
                                    str = DirPath + "\\" + "REAR_RH_4.bmp";
                                    try
                                    {
                                        imagefileTool_Rear_RH.Operator.Open(str, CogImageFileModeConstants.Read);
                                        imagefileTool_Rear_RH.Run();
                                        RRH_Aquire_Image = (CogImage24PlanarColor)imagefileTool_Rear_RH.OutputImage;
                                        cogRecordDisplay_REAR_RH.Image = RRH_Aquire_Image;
                                        cogRecordDisplay_REAR_RH.Fit(false);
                                        camnum = 5;
                                        break;
                                    }
                                    catch (System.Exception ex)
                                    {
                                        i++;
                                        if (i >= 10)
                                        {
                                            this.Invoke(new MethodInvoker(delegate ()
                                            {
                                                lb_Rear_RH.Text = "이미지 로딩 NG";
                                                lb_Rear_RH.BackColor = Color.Red;
                                            }));
                                            load = true;
                                        }
                                    }
                                    break;
                                }
                                else
                                {
                                    i++;
                                    if (i >= 10)
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_RH.Text = "이미지 로딩 NG";
                                            lb_Rear_RH.BackColor = Color.Red;
                                        }));
                                        load = true;
                                    }
                                }
                            }
                            if (load == true)
                            {
                                stop = true;
                            }
                            break;
                        case 5://검사스레드
                            Inspection1 = 0;
                            Inspection2 = 0;
                            Inspection3 = 0;
                            Inspection4 = 0;
                            Inspection5 = 0;

                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                //lb_CarType.Text = dirname[dirname.Length - 2];
                                //lb_BodyNo.Text = dirname[dirname.Length - 1];
                                lb_Center.Text = "...";
                                lb_Center.BackColor = Color.White;
                                lb_Front_LH.Text = "...";
                                lb_Front_LH.BackColor = Color.White;
                                lb_Front_RH.Text = "...";
                                lb_Front_RH.BackColor = Color.White;
                                lb_Rear_LH.Text = "...";
                                lb_Rear_LH.BackColor = Color.White;
                                lb_Rear_RH.Text = "...";
                                lb_Rear_RH.BackColor = Color.White;
                            }));

                            CenterThread = new Thread(new ThreadStart(CenterStart));
                            if (CenterThread.IsAlive != true)
                            {
                                CenterThread.Start();
                            }

                            Front_LHThread = new Thread(new ThreadStart(Front_LHStart));
                            if (Front_LHThread.IsAlive != true)
                            {
                                Front_LHThread.Start();
                            }

                            Front_RHThread = new Thread(new ThreadStart(Front_RHStart));
                            if (Front_RHThread.IsAlive != true)
                            {
                                Front_RHThread.Start();
                            }

                            Rear_LHThread = new Thread(new ThreadStart(Rear_LHStart));
                            if (Rear_LHThread.IsAlive != true)
                            {
                                Rear_LHThread.Start();
                            }

                            Rear_RHThread = new Thread(new ThreadStart(Rear_RHStart));
                            if (Rear_RHThread.IsAlive != true)
                            {
                                Rear_RHThread.Start();
                            }
                            camnum = 6;
                            break;
                        case 6://결과
                            #region 결과 이미지 캡쳐 저장
                            if (Inspection1 == 1 && Inspection2 == 1 && Inspection3 == 1 && Inspection4 == 1 && Inspection5 == 1)
                            {
                                Inspection1 = 0;
                                Inspection2 = 0;
                                Inspection3 = 0;
                                Inspection4 = 0;
                                Inspection5 = 0;

                                str = string.Format("{0:HH시mm분ss초}", DateTime.Now) + "," + lb_CarType.Text + "," + lb_BodyNo.Text + "," + Center_Result + "," + Front_LH_Result + "," + Front_RH_Result + ","
                                    + Rear_LH_Result + "," + Rear_RH_Result + ",";
                                SaveDataLogFile(str, true);

                                string gh = "";
                                Thread.Sleep(1000);
                                if (Front_LH_Result != "OK" || Front_RH_Result != "OK" || Center_Result != "OK" || Rear_LH_Result != "OK" || Rear_RH_Result != "OK")
                                {
                                    #region 결과처리
                                    if (Front_LH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_LH.Text = Front_LH_Result;
                                            lb_Front_LH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_LH.Text = "OK";
                                            lb_Front_LH.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Front_RH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_RH.Text = Front_RH_Result;
                                            lb_Front_RH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_RH.Text = "OK";
                                            lb_Front_RH.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Center_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Center.Text = Center_Result;
                                            lb_Center.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Center.Text = "OK";
                                            lb_Center.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Rear_LH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_LH.Text = Rear_LH_Result;
                                            lb_Rear_LH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_LH.Text = "OK";
                                            lb_Rear_LH.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Rear_RH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_RH.Text = Rear_RH_Result;
                                            lb_Rear_RH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_RH.Text = "OK";
                                            lb_Rear_RH.BackColor = Color.Green;
                                        }));
                                    }
                                    #endregion

                                    if (Front_LH_Result != "OK")
                                    {
                                        gh = Front_LH_Result + ",";
                                    }
                                    if (Front_RH_Result != "OK")
                                    {
                                        gh = gh + Front_RH_Result + ",";
                                    }
                                    if (Center_Result != "OK")
                                    {
                                        gh = gh + Center_Result + ",";
                                    }
                                    if (Rear_LH_Result != "OK")
                                    {
                                        gh = gh + Rear_LH_Result + ",";
                                    }
                                    if (Rear_RH_Result != "OK")
                                    {
                                        gh = gh + Rear_RH_Result;
                                    }
                                    SaveDataLogFile(str, false);
                                    Buzzer(_NGTime);

                                    this.Invoke(new MethodInvoker(delegate ()
                                    {
                                        lb_Rear_LH.Refresh();
                                        lb_Rear_RH.Refresh();
                                        lb_Center.Refresh();
                                        lb_Front_LH.Refresh();
                                        lb_Front_RH.Refresh();
                                    }));

                                    while(true)
                                    {
                                        if (lb_Rear_LH.Text != "..." && lb_Rear_RH.Text != "..." && lb_Center.Text != "..." && lb_Front_LH.Text != "..." && lb_Front_RH.Text != "...")
                                        {
                                            Thread.Sleep(500);
                                            ResultImageSave(lb_CarType.Text, false, gh);

                                            DeleteFolder(_DeleteFoldor);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    #region 결과처리
                                    if (Front_LH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_LH.Text = Front_LH_Result;
                                            lb_Front_LH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_LH.Text = "OK";
                                            lb_Front_LH.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Front_RH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_RH.Text = Front_RH_Result;
                                            lb_Front_RH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Front_RH.Text = "OK";
                                            lb_Front_RH.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Center_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Center.Text = Center_Result;
                                            lb_Center.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Center.Text = "OK";
                                            lb_Center.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Rear_LH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_LH.Text = Rear_LH_Result;
                                            lb_Rear_LH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_LH.Text = "OK";
                                            lb_Rear_LH.BackColor = Color.Green;
                                        }));
                                    }
                                    if (Rear_RH_Result != "OK")
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_RH.Text = Rear_RH_Result;
                                            lb_Rear_RH.BackColor = Color.Red;
                                        }));
                                    }
                                    else
                                    {
                                        this.Invoke(new MethodInvoker(delegate ()
                                        {
                                            lb_Rear_RH.Text = "OK";
                                            lb_Rear_RH.BackColor = Color.Green;
                                        }));
                                    }
                                    #endregion

                                    this.Invoke(new MethodInvoker(delegate ()
                                    {
                                        lb_Rear_LH.Refresh();
                                        lb_Rear_RH.Refresh();
                                        lb_Center.Refresh();
                                        lb_Front_LH.Refresh();
                                        lb_Front_RH.Refresh();
                                    }));

                                    while (true)
                                    {
                                        if (lb_Rear_LH.Text != "..." && lb_Rear_RH.Text != "..." && lb_Center.Text != "..." && lb_Front_LH.Text != "..." && lb_Front_RH.Text != "...")
                                        {
                                            Thread.Sleep(500);
                                            ResultImageSave(lb_CarType.Text, true, " ");

                                            DeleteFolder(_DeleteFoldor);
                                            break;
                                        }
                                    }
                                }

                                GC.Collect();
                                stop = true;
                            }
                            #endregion
                            break;
                    }
                    Thread.Sleep(1);
                }
                
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }
        public void CenterStart()
        {
            try
            {
                CENTER(lb_CarType.Text, cogRecordDisplay_CENTER, C_Aquire_Image, 0);
                ImageSave(C_Aquire_Image, lb_CarType.Text, 0);
                ImageSave(Center_Aquire_Image, lb_CarType.Text, 5);
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }

        public void Front_LHStart()
        {
            try
            {
                FRONT_LH(lb_CarType.Text, cogRecordDisplay_FRONT_LH, FLH_Aquire_Image, 0);
                ImageSave(FLH_Aquire_Image, lb_CarType.Text, 1);
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }

        public void Front_RHStart()
        {
            try
            {
                FRONT_RH(lb_CarType.Text, cogRecordDisplay_FRONT_RH, FRH_Aquire_Image, 0);
                ImageSave(FRH_Aquire_Image, lb_CarType.Text, 2);
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }

        public void Rear_LHStart()
        {
            try
            {
                REAR_LH(lb_CarType.Text, cogRecordDisplay_REAR_LH, RLH_Aquire_Image, 0);
                ImageSave(RLH_Aquire_Image, lb_CarType.Text, 3);
                ImageSave(Front_LH_Aquire_Image, lb_CarType.Text, 6);
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }

        public void Rear_RHStart()
        {
            try
            {
                REAR_RH(lb_CarType.Text, cogRecordDisplay_REAR_RH, RRH_Aquire_Image, 0);
                ImageSave(RRH_Aquire_Image, lb_CarType.Text, 4);
                ImageSave(Front_RH_Aquire_Image, lb_CarType.Text, 7);
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }

        public void Cam_Start()
        {
            try
            {
                string str = "";
                if (garp_state == true)
                {
                    garp_complete = false;
                    if (Aquire(cogRecordDisplay_REAR_LH2, 1) == ProcessDefine._FALSE)
                    {
                        Aquire(cogRecordDisplay_REAR_LH2, 1);
                    }
                    if (Aquire(cogRecordDisplay_REAR_RH2, 2) == ProcessDefine._FALSE)
                    {
                        Aquire(cogRecordDisplay_REAR_RH2, 2);
                    }
                    if (Aquire(cogRecordDisplay_CENTER2, 0) == ProcessDefine._FALSE)
                    {
                        Aquire(cogRecordDisplay_CENTER2, 0);
                    }
                    garp_state = false;
                }
            }
            catch (ThreadInterruptedException ex)
            {

            }
        }
        #endregion

        #region 딥러닝 검사
        private int FRONT_LH(string Cartype, CogRecordDisplay cogRecordDisplay, CogImage24PlanarColor _image,int _type)
        {
            try
            {
                bool Result = true;
                Front_LH_Result = "";
                int classnum = 0;
                int workspacenum = 0;
                if (Cartype == "C1")
                {
                    classnum = 15;
                    workspacenum = 0;
                }
                else if (Cartype == "Q1")
                {
                    classnum = 12;
                    workspacenum = 8;
                }
                else
                {
                    classnum = 28;
                    workspacenum = 16;
                }

                string DeepName = "";
                cogRecordDisplay.InteractiveGraphics.Clear();
                stream[0] = workspace[workspacenum].Streams["default"];
                ViDi2.VisionPro.Image image = new ViDi2.VisionPro.Image(_image);
                sample[0] = stream[0].Process(image);
                for (int i = 0; i < classnum; i++)
                {
                    if (Cartype == "C1")
                    {
                        int renum = i + 2;
                        if (renum == 5 || renum == 6 || renum == 7 || renum == 8 || renum == 9 || renum == 10 || renum == 13)
                        {

                        }
                        else
                        {
                            DeepName = "R" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[0].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[0].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;

                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if(renum == 2)
                                {
                                    if (Rear_Exist[2] != "0" || Rear_Exist[29] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 3)
                                {
                                    if (Rear_Exist[3] != "0" || Rear_Exist[30] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Rear_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if(view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if(Rear_PartName[renum] == "0")
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Front_LH_Result += Rear_PartName[renum] +"("+ Rear_ALC[renum]+")" + "없음!! ";
                                        }

                                        if(DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "있음!! ";
                                        }
                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                    else if(Cartype == "S1")
                    {
                        int renum = i + 3;
                        if (renum >= 5 && renum <= 8)
                        {

                        }
                        else if (renum >= 13 && renum <= 29)
                        {

                        }
                        else
                        {
                            DeepName = "R" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[0].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[0].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 3)
                                {
                                    if (Rear_Exist[3] != "0" || Rear_Exist[28] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 4)
                                {
                                    if (Rear_Exist[4] != "0" || Rear_Exist[29] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Rear_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "없음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "있음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "Q1")
                    {
                        int renum = i + 3;
                        if (renum >= 6 && renum <= 11)
                        {

                        }
                        else
                        {
                            DeepName = "R" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[0].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[0].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 3)
                                {
                                    if (Rear_Exist[3] != "0" || Rear_Exist[24] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 4)
                                {
                                    if (Rear_Exist[4] != "0" || Rear_Exist[25] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Rear_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "없음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Front_LH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "있음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                }

                if (Front_LH_Result != "")
                {
                    
                    Inspection1_State = false;
                }
                else
                {
                    Front_LH_Result = "OK";
                    
                    Inspection1_State = true;
                }
                Inspection1 = 1;
                return 1;
            }
            catch
            {
                Inspection1_State = false;
                Inspection1 = 1;
                return 0;
            }
        }

        private int FRONT_RH(string Cartype, CogRecordDisplay cogRecordDisplay, CogImage24PlanarColor _image,int _type)
        {
            try
            {
                bool Result = true;
                Front_RH_Result = "";
                int classnum = 0;
                int workspacenum = 0;
                if (Cartype == "C1")
                {
                    classnum = 10;
                    workspacenum = 1;
                }
                else if (Cartype == "Q1")
                {
                    classnum = 23;
                    workspacenum = 9;
                }
                else
                {
                    classnum = 9;
                    workspacenum = 17;
                }

                string DeepName = "";
                cogRecordDisplay.InteractiveGraphics.Clear();
                stream[1] = workspace[workspacenum].Streams["default"];
                ViDi2.VisionPro.Image image = new ViDi2.VisionPro.Image(_image);
                sample[1] = stream[1].Process(image);
                for (int i = 0; i < classnum; i++)
                {
                    if (Cartype == "C1")
                    {
                        int renum = i + 1;
                        if (renum == 2 || renum == 3 || renum == 4 || renum == 5 || renum == 6)
                        {

                        }
                        else
                        {
                            DeepName = "R" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[1].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[1].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 1)
                                {
                                    if (Rear_Exist[1] != "0" || Rear_Exist[27] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 6)
                                {
                                    if (Rear_Exist[6] != "0" || Rear_Exist[35] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Rear_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "없음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "있음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "S1")
                    {
                        int renum = i;
                        if (renum == 3 || renum == 4)
                        {

                        }
                        else
                        {
                            DeepName = "R" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[1].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[1].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                rstr = Rear_Exist[renum];
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "없음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "있음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "Q1")
                    {
                        int renum = i+1;
                        if (renum >= 3 && renum <= 5)
                        {

                        }
                        else if (renum >= 12 && renum <= 22)
                        {

                        }
                        else
                        {
                            DeepName = "R" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[1].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[1].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                rstr = Rear_Exist[renum];
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "없음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Rear_PartName[renum] == "0")
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Front_RH_Result += Rear_PartName[renum] + "(" + Rear_ALC[renum] + ")" + "있음!! ";
                                        }

                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                }
                if (Front_RH_Result != "")
                {
                    Inspection2_State = false;
                }
                else
                {
                    Front_RH_Result = "OK";
                    Inspection2_State = true;
                }
                Inspection2 = 1;
                return 1;
            }
            catch
            {
                Inspection2_State = false;
                Inspection2 = 1;
                return 0;
            }
        }

        private int CENTER(string Cartype, CogRecordDisplay cogRecordDisplay, CogImage24PlanarColor _image,int _type)
        {
            try
            {
                bool Result = true;
                Center_Result = "";
                int classnum = 0;
                int classnum2 = 0;
                int workspacenum = 0;
                int workspacenum2 = 0;
                if (Cartype == "C1")
                {
                    classnum = 29;
                    classnum2 = 2;
                    workspacenum = 4;
                    workspacenum2 = 5;
                }
                else if (Cartype == "Q1")
                {
                    classnum = 29;
                    workspacenum = 12;
                    //Q1_Center.Inputs["InputImage"].Value = _image;
                    //Q1_Center.Run();
                }
                else
                {
                    classnum = 28;
                    workspacenum = 20;
                    S1_F41.Inputs["InputImage"].Value = Center_Aquire_Image;
                    S1_F41.Run();
                }

                string DeepName = "";
                cogRecordDisplay.InteractiveGraphics.Clear();
                stream[2] = workspace[workspacenum].Streams["default"];
                ViDi2.VisionPro.Image image = new ViDi2.VisionPro.Image(_image);
                sample[2] = stream[2].Process(image);

                stream[5] = workspace[workspacenum2].Streams["default"];
                ViDi2.VisionPro.Image image2 = new ViDi2.VisionPro.Image(Center_Aquire_Image);
                sample[5] = stream[5].Process(image2);

                for (int i = 0; i < classnum; i++)
                {
                    if(Cartype == "C1")
                    {
                        int renum = i + 37;
                        if (renum == 55 || renum == 62)
                        {

                        }
                        else
                        {
                            double sr = 0;
                            CogRectangleAffine rect2 = new CogRectangleAffine();
                            IGreenView view2 = null;
                            if (renum == 50 || renum == 51)
                            {
                                sr = 0.6;
                                DeepName = "F" + renum.ToString();
                                Greenmarking = (ViDi2.IGreenMarking)sample[5].Markings[DeepName];
                                ICogRecord toolRecord2 = null;
                                toolRecord2 = new GreenToolRecord((IGreenMarking)Greenmarking, image2, DeepName);
                                view2 = (IGreenView)sample[5].Markings[DeepName].Views[0];
                                rect2 = (CogRectangleAffine)toolRecord2.SubRecords[0].Content;
                            }
                            else
                            {
                                sr = 0.8;
                            }
                            DeepName = "F" + renum.ToString();
                            Redmarking = (ViDi2.IRedMarking)sample[2].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new RedToolRecord((IRedMarking)Redmarking, image, DeepName);
                            IRedView view = (IRedView)sample[2].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.Score >= sr)
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 61)
                                {
                                    if (Front_Exist[61] != "0" || Front_Exist[62] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Front_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (renum == 50 || renum == 51)
                                    {
                                        if (view2.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    {
                                        if (view.Score < sr)
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (renum == 50 || renum == 51)
                                    {
                                        if (view2.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    { 
                                        if (view.Score >= sr)
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if(Cartype == "S1")
                    {
                        int renum = i + 36;
                        if (renum == 39 || renum == 55 || renum == 62)
                        {

                        }
                        else
                        {
                            DeepName = "F" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[2].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[2].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 42)
                                {
                                    if (Front_Exist[42] != "0" || Front_Exist[34] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 43)
                                {
                                    if (Front_Exist[43] != "0" || Front_Exist[6] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 47)
                                {
                                    if (Front_Exist[47] != "0" || Front_Exist[39] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 61)
                                {
                                    if (Front_Exist[61] != "0" || Front_Exist[62] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Front_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (renum == 41)
                                    {
                                        int recnt = (int)S1_F41.Outputs["Output"].Value;
                                        if(recnt == 0)
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (renum == 41)
                                    {
                                        int recnt = (int)S1_F41.Outputs["Output"].Value;
                                        if (recnt == 1)
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Center_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "Q1")
                    {
                        int renum = i + 35;

                        if (renum >= 47 && renum <= 55)
                        {

                        }
                        else
                        {
                            DeepName = "F" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[2].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[2].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;

                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 61)
                                {
                                    if (Front_Exist[61] != "0" || Front_Exist[66] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Front_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Front_PartName[renum] == "0")
                                        {
                                            Center_Result += Front_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                        }
                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Front_PartName[renum] == "0")
                                        {
                                            Center_Result += Front_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                        }
                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                        //else
                        //{
                        //    if (Q1_Center.RunStatus.Result == CogToolResultConstants.Accept)
                        //    {
                        //        CogBlobTool blobTool = Q1_Center.Tools["F"+ renum] as CogBlobTool;
                        //        CogRectangleAffine rect = blobTool.Region as CogRectangleAffine;
                        //        rect.Interactive = false;
                        //        if (_type == 1)
                        //        {
                        //            if (blobTool.Results.GetBlobs().Count > 0)
                        //            {
                        //                rect.Color = CogColorConstants.Green;
                        //                rect.LineWidthInScreenPixels = 2;
                        //                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                        //            }
                        //            else
                        //            {
                        //                rect.Color = CogColorConstants.Red;
                        //                rect.LineWidthInScreenPixels = 2;
                        //                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                        //            }
                        //        }
                        //        else
                        //        {
                        //            string rstr = "";
                        //            if (renum == 61)
                        //            {
                        //                if (Front_Exist[61] != "0" || Front_Exist[66] != "0")
                        //                {
                        //                    rstr = "1";
                        //                }
                        //                else
                        //                {
                        //                    rstr = "0";
                        //                }
                        //            }
                        //            else
                        //            {
                        //                rstr = Front_Exist[renum];
                        //            }
                        //            if (rstr != "0")
                        //            {
                        //                if (blobTool.Results.GetBlobs().Count == 0)
                        //                {
                        //                    rect.Color = CogColorConstants.Red;
                        //                    rect.LineWidthInScreenPixels = 2;
                        //                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                        //                    if (Front_PartName[renum] == "0")
                        //                    {
                        //                        Center_Result += Front_PartName[renum] + " 없음!! ";
                        //                    }
                        //                    else
                        //                    {
                        //                        Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                        //                    }
                        //                    if (DB_Ng_Num == "")
                        //                    {
                        //                        DB_Ng_Num = DeepName;
                        //                    }
                        //                    else
                        //                    {
                        //                        DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                        //                    }
                        //                    Result = false;
                        //                }
                        //            }
                        //            else
                        //            {
                        //                if (blobTool.Results.GetBlobs().Count > 0)
                        //                {
                        //                    rect.Color = CogColorConstants.Red;
                        //                    rect.LineWidthInScreenPixels = 2;
                        //                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                        //                    if (Front_PartName[renum] == "0")
                        //                    {
                        //                        Center_Result += Front_PartName[renum] + " 있음!! ";
                        //                    }
                        //                    else
                        //                    {
                        //                        Center_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                        //                    }
                        //                    if (DB_Ng_Num == "")
                        //                    {
                        //                        DB_Ng_Num = DeepName;
                        //                    }
                        //                    else
                        //                    {
                        //                        DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                        //                    }
                        //                    Result = false;
                        //                }
                        //            }
                        //        }
                        //    }
                        //}
                    }
                }
                if (Center_Result != "")
                {
                    Inspection3_State = false;
                }
                else
                {
                    Center_Result = "OK";
                    Inspection3_State = true;
                }
                
                Inspection3 = 1;
                return 1;
            }
            catch
            {
                Inspection3_State = false;
                Inspection3 = 1;
                return 0;
            }
        }

        private int REAR_LH(string Cartype, CogRecordDisplay cogRecordDisplay, CogImage24PlanarColor _image, int _type)
        {
            try
            {
                bool Result = true;
                Rear_LH_Result = "";
                int classnum = 0;
                int workspacenum = 0;
                int workspacenum2 = 0;
                if (Cartype == "C1")
                {
                    classnum = 32;
                    workspacenum = 2;
                    workspacenum2 = 6;
                }
                else if (Cartype == "Q1")
                {
                    classnum = 30;
                    workspacenum = 10;
                    workspacenum2 = 14;
                }
                else
                {
                    classnum = 66;
                    workspacenum = 18;
                    workspacenum2 = 22;
                }

                string DeepName = "";

                cogRecordDisplay.InteractiveGraphics.Clear();
                stream[3] = workspace[workspacenum].Streams["default"];
                ViDi2.VisionPro.Image image = new ViDi2.VisionPro.Image(_image);
                sample[3] = stream[3].Process(image);

                stream[6] = workspace[workspacenum2].Streams["default"];
                ViDi2.VisionPro.Image image2 = new ViDi2.VisionPro.Image(Front_LH_Aquire_Image);
                sample[6] = stream[6].Process(image2);

                for (int i = 0; i < classnum; i++)
                {
                    if (Cartype == "C1")
                    {
                        int renum = i;
                        if (renum >= 6 && renum <= 12)
                        {

                        }
                        else if (renum >= 16 && renum <= 23)
                        {

                        }
                        else
                        {
                            DeepName = "F" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[3].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[3].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            CogRectangleAffine rect2 = new CogRectangleAffine();
                            IGreenView view2 = null;
                            if (renum == 24 || renum == 25)
                            {
                                Greenmarking = (ViDi2.IGreenMarking)sample[6].Markings[DeepName];
                                ICogRecord toolRecord2 = null;
                                toolRecord2 = new GreenToolRecord((IGreenMarking)Greenmarking, image2, DeepName);
                                view2 = (IGreenView)sample[6].Markings[DeepName].Views[0];
                                rect2 = (CogRectangleAffine)toolRecord2.SubRecords[0].Content;
                            }
                            
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                rstr = Front_Exist[renum];
                                
                                if (rstr != "0")
                                {
                                    if (renum == 24 || renum == 25)
                                    {
                                        if (renum == 25 && ck_sis.Checked == false)
                                        { }
                                        else
                                        {
                                            if (view2.BestTag.Name == "ng")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 없음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (renum == 24 || renum == 25)
                                    {
                                        if (renum == 25 && ck_sis.Checked == false)
                                        { }
                                        else
                                        {
                                            if (view2.BestTag.Name == "ok")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 있음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "S1")
                    {
                        int renum = i;
                        if (renum >= 6 && renum <= 12)
                        {

                        }
                        else if (renum >= 16 && renum <= 22)
                        {

                        }
                        else if (renum >= 29 && renum <= 64)
                        {

                        }
                        else
                        {
                            string sss = lb_BodyNo.Text;
                            if (sss.Substring(0, 2) == "1E" && renum == 65)
                            {
                            }
                            else if (sss.Substring(0, 2) == "1Y" && renum == 13)
                            {

                            }
                            else
                            {
                                DeepName = "F" + renum.ToString();
                                Greenmarking = (ViDi2.IGreenMarking)sample[3].Markings[DeepName];
                                ICogRecord toolRecord = null;
                                toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                                IGreenView view = (IGreenView)sample[3].Markings[DeepName].Views[0];
                                CogRectangleAffine rect = new CogRectangleAffine();
                                rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;

                                IGreenView view2 = null;
                                CogRectangleAffine rect2 = new CogRectangleAffine();
                                if (renum == 24)
                                {
                                    Greenmarking = (ViDi2.IGreenMarking)sample[6].Markings[DeepName];
                                    ICogRecord toolRecord2 = null;
                                    toolRecord2 = new GreenToolRecord((IGreenMarking)Greenmarking, image2, DeepName);
                                    view2 = (IGreenView)sample[6].Markings[DeepName].Views[0];
                                    rect2 = (CogRectangleAffine)toolRecord2.SubRecords[0].Content;
                                }
                                if (_type == 1)
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Green;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                    }
                                    else
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                    }
                                }
                                else
                                {
                                    string rstr = "";
                                    rstr = Front_Exist[renum];

                                    if (rstr != "0")
                                    {
                                        if (renum == 24)
                                        {
                                            if (view2.BestTag.Name == "ng")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 없음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                        else
                                        {
                                            if (view.BestTag.Name == "ng")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 없음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (renum == 24)
                                        {
                                            if (view2.BestTag.Name == "ok")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 있음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                        else
                                        {
                                            if (view.BestTag.Name == "ok")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 있음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "Q1")
                    {
                        int renum = i;
                        if (renum >= 6 && renum <= 11)
                        {

                        }
                        else if (renum >= 16 && renum <= 23)
                        {

                        }
                        else
                        {
                            DeepName = "F" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[3].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[3].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;

                            IGreenView view2 = null;
                            CogRectangleAffine rect2 = new CogRectangleAffine();
                            if (renum == 24)
                            {
                                Greenmarking = (ViDi2.IGreenMarking)sample[6].Markings[DeepName];
                                ICogRecord toolRecord2 = null;
                                toolRecord2 = new GreenToolRecord((IGreenMarking)Greenmarking, image2, DeepName);
                                view2 = (IGreenView)sample[6].Markings[DeepName].Views[0];
                                rect2 = (CogRectangleAffine)toolRecord2.SubRecords[0].Content;
                            }

                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                rstr = Front_Exist[renum];

                                if (rstr != "0")
                                {
                                    if (renum == 24)
                                    {
                                        if (ck_sis.Checked == false)
                                        { }
                                        else
                                        {
                                            if (view2.BestTag.Name == "ng")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 없음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (renum == 24)
                                    {
                                        if (ck_sis.Checked == false)
                                        { }
                                        else
                                        {
                                            if (view2.BestTag.Name == "ok")
                                            {
                                                rect.Color = CogColorConstants.Red;
                                                rect.LineWidthInScreenPixels = 2;
                                                cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                                if (Front_PartName[renum] == "0")
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + " 있음!! ";
                                                }
                                                else
                                                {
                                                    Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                                }
                                                if (DB_Ng_Num == "")
                                                {
                                                    DB_Ng_Num = DeepName;
                                                }
                                                else
                                                {
                                                    DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                                }
                                                Result = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Rear_LH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Rear_LH_Result != "")
                {
                    Inspection4_State = false;
                }
                else
                {
                    Rear_LH_Result = "OK";
                    Inspection4_State = true;
                }
                Inspection4 = 1;
                return 1;
            }
            catch
            {
                Inspection4_State = false;
                Inspection4 = 1;
                return 0;
            }
        }

        private int REAR_RH(string Cartype, CogRecordDisplay cogRecordDisplay ,CogImage24PlanarColor _image,int _type)
        {
            try
            {
                bool Result = true;
                Rear_RH_Result = "";
                int classnum = 0;
                int workspacenum = 0;
                int workspacenum2 = 0;
                if (Cartype == "C1")
                {
                    classnum = 31;
                    workspacenum = 3;
                    workspacenum2 = 7;
                }
                else if (Cartype == "Q1")
                {
                    classnum = 29;
                    workspacenum = 11;
                    workspacenum2 = 15;
                }
                else
                {
                    classnum = 58;
                    workspacenum = 19;
                    workspacenum2 = 23;
                }

                string DeepName = "";
                cogRecordDisplay.InteractiveGraphics.Clear();
                stream[4] = workspace[workspacenum].Streams["default"];
                ViDi2.VisionPro.Image image = new ViDi2.VisionPro.Image(_image);
                sample[4] = stream[4].Process(image);

                stream[7] = workspace[workspacenum2].Streams["default"];
                ViDi2.VisionPro.Image image2 = new ViDi2.VisionPro.Image(Front_RH_Aquire_Image);
                sample[7] = stream[7].Process(image2);

                for (int i = 0; i < classnum; i++)
                {
                    if (Cartype == "C1")
                    {
                        int renum = i + 6;
                        if (renum >= 13 && renum <= 15)
                        {

                        }
                        else if (renum >= 24 && renum <= 31)
                        {

                        }
                        else
                        {
                            DeepName = "F" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[4].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[4].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;

                            IGreenView view2 = null;
                            CogRectangleAffine rect2 = new CogRectangleAffine();
                            if (renum == 11 || renum == 12 || renum == 22 || renum == 35 || renum == 36)
                            {
                                Greenmarking = (ViDi2.IGreenMarking)sample[7].Markings[DeepName];
                                ICogRecord toolRecord2 = null;
                                toolRecord2 = new GreenToolRecord((IGreenMarking)Greenmarking, image2, DeepName);
                                view2 = (IGreenView)sample[7].Markings[DeepName].Views[0];
                                rect2 = (CogRectangleAffine)toolRecord2.SubRecords[0].Content;
                            }

                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 9)
                                {
                                    if (Front_Exist[9] != "0" || Front_Exist[67] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 19)
                                {
                                    if (Front_Exist[19] != "0" || Front_Exist[66] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Front_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (renum == 11 || renum == 12 || renum == 22 || renum == 35 || renum == 36)
                                    {
                                        if (view2.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (renum == 11 || renum == 12 || renum == 22 || renum == 35 || renum == 36)
                                    {
                                        if (view2.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "S1")
                    {
                        int renum = i + 7;
                        if (renum == 9 || renum == 21 || renum == 34)
                        {

                        }
                        else if (renum >= 13 && renum <= 15)
                        {

                        }
                        else if (renum >= 23 && renum <= 29)
                        {

                        }
                        else if (renum >= 36 && renum <= 63)
                        {

                        }
                        else
                        {
                            string sss = lb_BodyNo.Text;
                            if (sss.Substring(0, 2) == "1E" && renum == 64)
                            {
                            }
                            else if (sss.Substring(0, 2) == "1Y" && renum == 22)
                            {

                            }
                            else
                            {
                                DeepName = "F" + renum.ToString();
                                Greenmarking = (ViDi2.IGreenMarking)sample[4].Markings[DeepName];
                                ICogRecord toolRecord = null;
                                toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                                IGreenView view = (IGreenView)sample[4].Markings[DeepName].Views[0];
                                CogRectangleAffine rect = new CogRectangleAffine();
                                rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                                if (_type == 1)
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Green;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                    }
                                    else
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                    }
                                }
                                else
                                {
                                    string rstr = "";
                                    if (renum == 20)
                                    {
                                        if (Front_Exist[20] != "0" || Front_Exist[21] != "0")
                                        {
                                            rstr = "1";
                                        }
                                        else
                                        {
                                            rstr = "0";
                                        }
                                    }
                                    else
                                    {
                                        rstr = Front_Exist[renum];
                                    }
                                    if (rstr != "0")
                                    {
                                        if (view.BestTag.Name == "ng")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + " 없음!! ";
                                            }
                                            else
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                    else
                                    {
                                        if (view.BestTag.Name == "ok")
                                        {
                                            rect.Color = CogColorConstants.Red;
                                            rect.LineWidthInScreenPixels = 2;
                                            cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                            if (Front_PartName[renum] == "0")
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + " 있음!! ";
                                            }
                                            else
                                            {
                                                Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                            }
                                            if (DB_Ng_Num == "")
                                            {
                                                DB_Ng_Num = DeepName;
                                            }
                                            else
                                            {
                                                DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                            }
                                            Result = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (Cartype == "Q1")
                    {
                        int renum = i + 6;
                        if (renum >= 11 && renum <= 15)
                        {

                        }
                        else if (renum >= 24 && renum <= 29)
                        {

                        }
                        else if (renum == 8)
                        {

                        }
                        else
                        {
                            DeepName = "F" + renum.ToString();
                            Greenmarking = (ViDi2.IGreenMarking)sample[4].Markings[DeepName];
                            ICogRecord toolRecord = null;
                            toolRecord = new GreenToolRecord((IGreenMarking)Greenmarking, image, DeepName);
                            IGreenView view = (IGreenView)sample[4].Markings[DeepName].Views[0];
                            CogRectangleAffine rect = new CogRectangleAffine();
                            rect = (CogRectangleAffine)toolRecord.SubRecords[0].Content;
                            if (_type == 1)
                            {
                                if (view.BestTag.Name == "ok")
                                {
                                    rect.Color = CogColorConstants.Green;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                                else
                                {
                                    rect.Color = CogColorConstants.Red;
                                    rect.LineWidthInScreenPixels = 2;
                                    cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                }
                            }
                            else
                            {
                                string rstr = "";
                                if (renum == 17)
                                {
                                    if (Front_Exist[17] != "0" || Front_Exist[65] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 31)
                                {
                                    if (Front_Exist[31] != "0" || Front_Exist[64] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else if (renum == 32)
                                {
                                    if (Front_Exist[32] != "0" || Front_Exist[64] != "0")
                                    {
                                        rstr = "1";
                                    }
                                    else
                                    {
                                        rstr = "0";
                                    }
                                }
                                else
                                {
                                    rstr = Front_Exist[renum];
                                }
                                if (rstr != "0")
                                {
                                    if (view.BestTag.Name == "ng")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Front_PartName[renum] == "0")
                                        {
                                            Rear_RH_Result += Front_PartName[renum] + " 없음!! ";
                                        }
                                        else
                                        {
                                            Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "없음!! ";
                                        }
                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                                else
                                {
                                    if (view.BestTag.Name == "ok")
                                    {
                                        rect.Color = CogColorConstants.Red;
                                        rect.LineWidthInScreenPixels = 2;
                                        cogRecordDisplay.InteractiveGraphics.Add(rect, "Test", false);
                                        if (Front_PartName[renum] == "0")
                                        {
                                            Rear_RH_Result += Front_PartName[renum] + " 있음!! ";
                                        }
                                        else
                                        {
                                            Rear_RH_Result += Front_PartName[renum] + "(" + Front_ALC[renum] + ")" + "있음!! ";
                                        }
                                        if (DB_Ng_Num == "")
                                        {
                                            DB_Ng_Num = DeepName;
                                        }
                                        else
                                        {
                                            DB_Ng_Num = DB_Ng_Num + "," + DeepName;
                                        }
                                        Result = false;
                                    }
                                }
                            }
                        }
                    }
                }
                if (Rear_RH_Result != "")
                {
                    Inspection5_State = false;
                }
                else
                {
                    Rear_RH_Result = "OK";
                    Inspection5_State = true;
                }
                Inspection5 = 1;
                return 1;
            }
            catch
            {
                Inspection5_State = false;
                Inspection5 = 1;
                return 0;
            }
        }
        #endregion

        private void ImageSave(CogImage24PlanarColor image, string car,int type)
        {
            #region 이미지 저장
            lock (LockImageSave)
            {
                //파일 구분을 위한 시간을 생성한다
                DateTime nowTime = DateTime.Now;

                try
                {
                    /*원본이미지 저장*/
                    string originalmageFileName = lb_BodyNo.Text +".jpg";

                    string OimageFolder = ProcessFunc.ImageForderCreate();
                    //string OimageFolder1 = ProcessFunc.ImageForderCreate1();
                    // 폴더 없으면 생성
                    if (!System.IO.Directory.Exists(OimageFolder))
                    {
                        System.IO.Directory.CreateDirectory(OimageFolder);
                    }
                    //if (!System.IO.Directory.Exists(OimageFolder1))
                    //{
                    //    System.IO.Directory.CreateDirectory(OimageFolder1);
                    //}
                    if (car == "C1")
                    {
                        OimageFolder = OimageFolder + "C1\\";
                        //OimageFolder1 = OimageFolder1 + "C1\\";
                    }
                    else if (car == "Q1")
                    {
                        OimageFolder = OimageFolder + "Q1\\";
                        //OimageFolder1 = OimageFolder1 + "Q1\\";
                    }
                    else if (car == "S1")
                    {
                        OimageFolder = OimageFolder + "S1\\";
                        //OimageFolder1 = OimageFolder1 + "S1\\";
                    }

                    if (type == 0)
                    {
                        OimageFolder = OimageFolder + "Center\\";
                        //OimageFolder1 = OimageFolder1 + "Center\\";
                    }
                    else if (type == 1)
                    {
                        OimageFolder = OimageFolder + "Front_LH\\";
                        //OimageFolder1 = OimageFolder1 + "Front_LH\\";
                    }
                    else if (type == 2)
                    {
                        OimageFolder = OimageFolder + "Front_RH\\";
                        //OimageFolder1 = OimageFolder1 + "Front_RH\\";
                    }
                    else if (type == 3)
                    {
                        OimageFolder = OimageFolder + "Rear_LH\\";
                        //OimageFolder1 = OimageFolder1 + "Rear_LH\\";
                    }
                    else if (type == 4)
                    {
                        OimageFolder = OimageFolder + "Rear_RH\\";
                        //OimageFolder1 = OimageFolder1 + "Rear_RH\\";
                    }
                    else if (type == 5)
                    {
                        OimageFolder = OimageFolder + "Center2\\";
                        //OimageFolder1 = OimageFolder1 + "Center2\\";
                    }
                    else if (type == 6)
                    {
                        OimageFolder = OimageFolder + "Rear_LH2\\";
                        //OimageFolder1 = OimageFolder1 + "Rear_LH2\\";
                    }
                    else if (type == 7)
                    {
                        OimageFolder = OimageFolder + "Rear_RH2\\";
                        //OimageFolder1 = OimageFolder1 + "Rear_RH2\\";
                    }

                    // 폴더 없으면 생성
                    if (!System.IO.Directory.Exists(OimageFolder))
                    {
                        System.IO.Directory.CreateDirectory(OimageFolder);
                    }
                    //if (!System.IO.Directory.Exists(OimageFolder1))
                    //{
                    //    System.IO.Directory.CreateDirectory(OimageFolder1);
                    //}
                    lock (LockImageSave)
                    {
                        Bitmap bmp = image.ToBitmap();
                        var encoderParam = new EncoderParameters(1);
                        long tt = 50;
                        encoderParam.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, tt);
                        bmp.Save(OimageFolder + originalmageFileName, GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg), encoderParam);
                        bmp.Dispose();
                        //bmp = image.ToBitmap();
                        //bmp.Save(OimageFolder1 + originalmageFileName, GetEncoder(System.Drawing.Imaging.ImageFormat.Jpeg), encoderParam);
                        //bmp.Dispose();
                    }
                }
                catch
                {
                }
            }
            #endregion
        }

        private void ResultImageSave(string car,bool result,string gh)
        {
            try
            {
                /*원본이미지 저장*/
                string originalmageFileName = lb_BodyNo.Text + ".jpg";

                string OimageFolder = ProcessFunc.ImageForderCreate();
                //string OimageFolder1 = ProcessFunc.ImageForderCreate1();
                // 폴더 없으면 생성
                if (!System.IO.Directory.Exists(OimageFolder))
                {
                    System.IO.Directory.CreateDirectory(OimageFolder);
                }

                //if (!System.IO.Directory.Exists(OimageFolder1))
                //{
                //    System.IO.Directory.CreateDirectory(OimageFolder1);
                //}

                if (car == "C1")
                {
                    OimageFolder = OimageFolder + "C1\\";
                    //OimageFolder1 = OimageFolder1 + "C1\\";
                }
                else if (car == "Q1")
                {
                    OimageFolder = OimageFolder + "Q1\\";
                    //OimageFolder1 = OimageFolder1 + "Q1\\";
                }
                else if (car == "S1")
                {
                    OimageFolder = OimageFolder + "S1\\";
                    //OimageFolder1 = OimageFolder1 + "S1\\";
                }
                
                OimageFolder = OimageFolder + "Result\\";
               //OimageFolder1 = OimageFolder1 + "Result\\";
                // 폴더 없으면 생성
                if (!System.IO.Directory.Exists(OimageFolder))
                {
                    System.IO.Directory.CreateDirectory(OimageFolder);
                }
                //if (!System.IO.Directory.Exists(OimageFolder1))
                //{
                //    System.IO.Directory.CreateDirectory(OimageFolder1);
                //}

                if (result == true)
                {
                    OimageFolder = OimageFolder + "OK\\";
                    // 폴더 없으면 생성
                    if (!System.IO.Directory.Exists(OimageFolder))
                    {
                        System.IO.Directory.CreateDirectory(OimageFolder);
                    }
                    //OimageFolder1 = OimageFolder1 + "OK\\";
                    //// 폴더 없으면 생성
                    //if (!System.IO.Directory.Exists(OimageFolder1))
                    //{
                    //    System.IO.Directory.CreateDirectory(OimageFolder1);
                    //}
                }
                else
                {
                    OimageFolder = OimageFolder + "NG\\";
                    // 폴더 없으면 생성
                    if (!System.IO.Directory.Exists(OimageFolder))
                    {
                        System.IO.Directory.CreateDirectory(OimageFolder);
                    }
                    //OimageFolder1 = OimageFolder1 + "NG\\";
                    //// 폴더 없으면 생성
                    //if (!System.IO.Directory.Exists(OimageFolder1))
                    //{
                    //    System.IO.Directory.CreateDirectory(OimageFolder1);
                    //}
                }

                // 주화면의 크기 정보 읽기
                Rectangle rect = Screen.PrimaryScreen.Bounds;
                // 2nd screen = Screen.AllScreens[1]

                // 픽셀 포맷 정보 얻기 (Optional)
                int bitsPerPixel = Screen.PrimaryScreen.BitsPerPixel;
                PixelFormat pixelFormat = PixelFormat.Format32bppArgb;
                if (bitsPerPixel <= 16)
                {
                    pixelFormat = PixelFormat.Format16bppRgb565;
                }
                if (bitsPerPixel == 24)
                {
                    pixelFormat = PixelFormat.Format24bppRgb;
                }

                // 화면 크기만큼의 Bitmap 생성
                Bitmap bmp = new Bitmap(rect.Width, rect.Height, pixelFormat);

                // Bitmap 이미지 변경을 위해 Graphics 객체 생성
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    // 화면을 그대로 카피해서 Bitmap 메모리에 저장
                    gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size);
                }

                // Bitmap 데이타를 파일로 저장
                bmp.Save(OimageFolder + originalmageFileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                bmp.Dispose();

                string ddd = DateTime.Now.ToString("yyyy-MM-dd");
                string imh = "/HDI/Trim/" + ddd + "/" + originalmageFileName;
                if (result == false)
                {
                    //Thread.Sleep(1000);
                    SendImage_ftp(OimageFolder + originalmageFileName,true);
                    Send_DB(lb_BodyNo.Text, DB_Ng_Num, imh, "NG",gh);
                }
                else
                {
                    //Thread.Sleep(1000);
                    SendImage_ftp(OimageFolder + originalmageFileName,true);
                    Send_DB(lb_BodyNo.Text, DB_Ng_Num, imh, "OK",gh);
                }
            }
            catch
            {
            }
        }

        private ImageCodecInfo GetEncoder(System.Drawing.Imaging.ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageDecoders();
            foreach (var codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        private void Main_Load(object sender, EventArgs e)
        {
            control = new ViDi2.Runtime.Local.Control();

            #region ViDi 로드
            if (control.Workspaces.Count > 0)
            {
                for (int i = 0; i < 15; i++)
                {
                    workspace[i].Close();
                }
            }

            timer1.Enabled = true;

            COMMON_MSGBOX m_pMessageFrm = new COMMON_MSGBOX();

            m_pMessageFrm.SetMessageBox("파일 로딩중...", 0);

            m_pMessageFrm.ShowDialog();

            #endregion

            Read_Data();
            Update_Data();

            DeleteFolder(_DeleteFoldor);
            DeleteFolder1(_DeleteFoldor);

            bool bResult = clsFTPHandling.Connect(_server_ftp, _port_ftp.ToString(), _id_ftp, _pw_ftp);
        }
        
        private int LoadTool(int namenum,int name_type)
        {
            try
            {
                string[] nametype = new string[8];
                nametype[0] = "FRONT_LH";
                nametype[1] = "FRONT_RH";
                nametype[2] = "REAR_LH";
                nametype[3] = "REAR_RH";
                nametype[4] = "CENTER";
                nametype[5] = "CENTER2";
                nametype[6] = "REAR_LH2";
                nametype[7] = "REAR_RH2";

                string[] name = new string[24];
                name[0] = "FRONT_LH_C1";
                name[1] = "FRONT_RH_C1";
                name[2] = "REAR_LH_C1";
                name[3] = "REAR_RH_C1";
                name[4] = "CENTER_C1";
                name[5] = "CENTER_C1_2";
                name[6] = "REAR_LH_C1_2";
                name[7] = "REAR_RH_C1_2";

                name[8] = "FRONT_LH_Q1";
                name[9] = "FRONT_RH_Q1";
                name[10] = "REAR_LH_Q1";
                name[11] = "REAR_RH_Q1";
                name[12] = "CENTER_Q1";
                name[13] = "CENTER_Q1_2";
                name[14] = "REAR_LH_Q1_2";
                name[15] = "REAR_RH_Q1_2";
                
                name[16] = "FRONT_LH_S1";
                name[17] = "FRONT_RH_S1";
                name[18] = "REAR_LH_S1";
                name[19] = "REAR_RH_S1";
                name[20] = "CENTER_S1";
                name[21] = "CENTER_S1_2";
                name[22] = "REAR_LH_S1_2";
                name[23] = "REAR_RH_S1_2";
                if (namenum < 8)
                {
                    //if (namenum == 4)
                    //{
                        string ViDistrDir = "D:\\Model\\C1\\" + nametype[name_type] + ".vrws";
                        workspace[namenum] = control.Workspaces.Add(name[namenum], ViDistrDir);
                    //}
                }
                else if (namenum < 16)
                {
                    //if (namenum == 16)
                    //{
                        string ViDistrDir = "D:\\Model\\Q1\\" + nametype[name_type] + ".vrws";
                    workspace[namenum] = control.Workspaces.Add(name[namenum], ViDistrDir);
                    //}
                }
                else if (namenum < 24)
                {
                    //if (namenum == 16)
                    //{
                        string ViDistrDir = "D:\\Model\\S1\\" + nametype[name_type] + ".vrws";
                    workspace[namenum] = control.Workspaces.Add(name[namenum], ViDistrDir);
                    //}
                }

                S1_F41 = CogSerializer.LoadObjectFromFile("D:\\Model\\S1\\" + "F41.vpp") as CogToolBlock;
                Q1_Center = CogSerializer.LoadObjectFromFile("D:\\Model\\Q1\\" + "Center.vpp") as CogToolBlock;
                return 1;
            }
            catch
            {
                return 0;
            }
        }
        
        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime nowTime = DateTime.Now; // 현재 시간 계산
            UpTimeSpan = nowTime - StartTime; // 현재시간-시작시간이 총 가동시간 (Uptime) 이다.

            Label_CurrentTime.Text = nowTime.ToString("yy.MM.dd HH:mm:ss");
            Label_OperatingTime.Text = UpTimeSpan.ToString(@"dd\.hh\:mm\:ss");

            #region 비젼파일로드
            if (ModelLoad_State == false)
            {
                if (LoadTool(ModelCount, ModelNameCount) == 1)
                {
                    ModelCount++;
                    ModelNameCount++;
                    if (ModelNameCount == 8)
                    {
                        ModelNameCount = 0;
                    }
                    if (ModelCount == 24)
                    {
                        ModelCount = 0;
                        ModelLoad = 1;
                        ModelLoad_State = true;
                        //timer1.Enabled = false;
                        fileSystemWatcher1.EnableRaisingEvents = true;
                    }
                }
                else
                {
                    ModelLoad = 1;
                    ModelLoad_State = true;
                    //timer1.Enabled = false;
                    MessageBox.Show("비젼파일 로딩 실패!! 확인 후 다시 시작 해 주세요");
                }
            }
            #endregion

            //#region 파일 삭제
            //DateTime _Date = DateTime.Now;
            //if (_Date.Hour == 24 && _Date.Minute == 0)
            //{
            //    if(_Date.Second >= 0 && _Date.Second < 1)
            //    {
            //        DeleteFolder(_DeleteFoldor);
            //        DeleteFolder1(_DeleteFoldor);
            //    }
            //}
            //#endregion

            //#region 결과 이미지 캡쳐 저장
            //if(Inspection1 == 1 && Inspection2 == 1 && Inspection3 == 1 && Inspection4 == 1 && Inspection5 == 1)
            //{
            //    Inspection1 = 0;
            //    Inspection2 = 0;
            //    Inspection3 = 0;
            //    Inspection4 = 0;
            //    Inspection5 = 0;
                
            //    string str = string.Format("{0:HH시mm분ss초}", DateTime.Now) + "," + lb_CarType.Text + "," + lb_BodyNo.Text + "," + Center_Result + "," + Front_LH_Result + "," + Front_RH_Result + ","
            //        + Rear_LH_Result + "," + Rear_RH_Result + ",";
            //    SaveDataLogFile(str,true);

            //    string gh = "";
            //    if (Front_LH_Result != "OK" || Front_RH_Result != "OK" || Center_Result != "OK" || Rear_LH_Result != "OK" || Rear_RH_Result != "OK")
            //    {
            //        #region 결과처리
            //        if (Front_LH_Result != "OK")
            //        {
            //            lb_Front_LH.Text = Front_LH_Result;
            //            lb_Front_LH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_LH.Text = Front_LH_Result;
            //            //    lb_Front_LH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Front_LH.Text = "OK";
            //            lb_Front_LH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_LH.Text = "OK";
            //            //    lb_Front_LH.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Front_RH_Result != "OK")
            //        {
            //            lb_Front_RH.Text = Front_RH_Result;
            //            lb_Front_RH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_RH.Text = Front_RH_Result;
            //            //    lb_Front_RH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Front_RH.Text = "OK";
            //            lb_Front_RH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_RH.Text = "OK";
            //            //    lb_Front_RH.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Center_Result != "OK")
            //        {
            //            lb_Center.Text = Center_Result;
            //            lb_Center.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Center.Text = Center_Result;
            //            //    lb_Center.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Center.Text = "OK";
            //            lb_Center.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Center.Text = "OK";
            //            //    lb_Center.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Rear_LH_Result != "OK")
            //        {
            //            lb_Rear_LH.Text = Rear_LH_Result;
            //            lb_Rear_LH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_LH.Text = Rear_LH_Result;
            //            //    lb_Rear_LH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Rear_LH.Text = "OK";
            //            lb_Rear_LH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_LH.Text = "OK";
            //            //    lb_Rear_LH.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Rear_RH_Result != "OK")
            //        {
            //            lb_Rear_RH.Text = Rear_RH_Result;
            //            lb_Rear_RH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_RH.Text = Rear_RH_Result;
            //            //    lb_Rear_RH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Rear_RH.Text = "OK";
            //            lb_Rear_RH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_RH.Text = "OK";
            //            //    lb_Rear_RH.BackColor = Color.Green;
            //            //}));
            //        }
            //        #endregion

            //        if (Front_LH_Result != "OK")
            //        {
            //            gh = Front_LH_Result +",";
            //        }
            //        if (Front_RH_Result != "OK")
            //        {
            //            gh = gh + Front_RH_Result + ",";
            //        }
            //        if (Center_Result != "OK")
            //        {
            //            gh = gh + Center_Result + ",";
            //        }
            //        if (Rear_LH_Result != "OK")
            //        {
            //            gh = gh + Rear_LH_Result + ",";
            //        }
            //        if (Rear_RH_Result != "OK")
            //        {
            //            gh = gh + Rear_RH_Result;
            //        }
            //        SaveDataLogFile(str, false);
            //        Buzzer(_NGTime);
            //        lb_Rear_LH.Refresh();
            //        lb_Rear_RH.Refresh();
            //        lb_Center.Refresh();
            //        lb_Front_LH.Refresh();
            //        lb_Front_RH.Refresh();
            //        Thread.Sleep(1000);
            //        ResultImageSave(lb_CarType.Text,false, gh);
                    
            //        DeleteFolder(_DeleteFoldor);
            //    }
            //    else
            //    {
            //        #region 결과처리
            //        if (Front_LH_Result != "OK")
            //        {
            //            lb_Front_LH.Text = Front_LH_Result;
            //            lb_Front_LH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_LH.Text = Front_LH_Result;
            //            //    lb_Front_LH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Front_LH.Text = "OK";
            //            lb_Front_LH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_LH.Text = "OK";
            //            //    lb_Front_LH.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Front_RH_Result != "OK")
            //        {
            //            lb_Front_RH.Text = Front_RH_Result;
            //            lb_Front_RH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_RH.Text = Front_RH_Result;
            //            //    lb_Front_RH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Front_RH.Text = "OK";
            //            lb_Front_RH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Front_RH.Text = "OK";
            //            //    lb_Front_RH.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Center_Result != "OK")
            //        {
            //            lb_Center.Text = Center_Result;
            //            lb_Center.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Center.Text = Center_Result;
            //            //    lb_Center.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Center.Text = "OK";
            //            lb_Center.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Center.Text = "OK";
            //            //    lb_Center.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Rear_LH_Result != "OK")
            //        {
            //            lb_Rear_LH.Text = Rear_LH_Result;
            //            lb_Rear_LH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_LH.Text = Rear_LH_Result;
            //            //    lb_Rear_LH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Rear_LH.Text = "OK";
            //            lb_Rear_LH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_LH.Text = "OK";
            //            //    lb_Rear_LH.BackColor = Color.Green;
            //            //}));
            //        }
            //        if (Rear_RH_Result != "OK")
            //        {
            //            lb_Rear_RH.Text = Rear_RH_Result;
            //            lb_Rear_RH.BackColor = Color.Red;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_RH.Text = Rear_RH_Result;
            //            //    lb_Rear_RH.BackColor = Color.Red;
            //            //}));
            //        }
            //        else
            //        {
            //            lb_Rear_RH.Text = "OK";
            //            lb_Rear_RH.BackColor = Color.Green;
            //            //this.Invoke(new MethodInvoker(delegate ()
            //            //{
            //            //    lb_Rear_RH.Text = "OK";
            //            //    lb_Rear_RH.BackColor = Color.Green;
            //            //}));
            //        }
            //        #endregion

            //        lb_Rear_LH.Refresh();
            //        lb_Rear_RH.Refresh();
            //        lb_Center.Refresh();
            //        lb_Front_LH.Refresh();
            //        lb_Front_RH.Refresh();
            //        Thread.Sleep(1000);
            //        ResultImageSave(lb_CarType.Text, true," ");

            //        DeleteFolder(_DeleteFoldor);
            //        //DeleteFolder1(_DeleteFoldor);
            //    }

            //    GC.Collect();
            //    GC.WaitForPendingFinalizers();

            //    Trigger_State = false;
            //}
            //#endregion

            #region 파이널 데이터
            //try
            //{
            //    Read_DB_Data();
            //    if (_DB_Start == "1")
            //    {
            //        Read_DB_Data();
            //        Thread.Sleep(1000);
            //        SendImage_ftp(_DB_path, false);
            //        Send_DB(_DB_Body, _DB_index, _DB_img, _DB_result, false);
            //        _DB_Start = "0";
            //        Save_DB_Data();
            //    }
            //}
            //catch
            //{

            //}
            #endregion
        }

        private void Buzzer(int delay=3000)
        {
            try
            {
                int selectID;
                bool result;

                selectID = 261;
                selectID = Convert.ToInt32("261", 16);
                result = usb_io_output(selectID, 0, 1, 0, 0, 0);
                result = usb_io_output(selectID, 0, 2, 0, 0, 0);
                Thread.Sleep(delay);
                result = usb_io_output(selectID, 0, -1, 0, 0, 0);
                result = usb_io_output(selectID, 0, -2, 0, 0, 0);
            }
            catch
            {

            }
        }

        private void Sound()
        {
            try
            {
                SoundPlayer player = new SoundPlayer("D:\\Model\\알람.wav");
                player.PlaySync();
            }
            catch
            {

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DB_cs = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                _server, _port, _database, _id, _pw);
            using (MySqlConnection conn = new MySqlConnection(DB_cs))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM tb_partorder_complete WHERE BODY_NO =" + "'" + textBox1.Text + "'", conn);
                    DataTable SearchDataTable = new DataTable();
                    using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    {
                        adaptor.Fill(SearchDataTable);
                    }
                    dataGridView1.DataSource = SearchDataTable;
                    conn.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(string.Format(ex.Message));
                }
            }
        }

        private int SelectDB_Partorder_Complete(string str)
        {
            for (int i = 0; i < 100; i++)
            {
                carrierindex[i] = "0";
                PartName[i] = "0";
                ALC[i] = "0";
                Front_Exist[i] = "0";
                Front_PartName[i] = "0";
                Front_ALC[i] = "0";
                Rear_Exist[i] = "0";
                Rear_PartName[i] = "0";
                Rear_ALC[i] = "0";
            }

            DB_cs = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                 _server, _port, _database, _id, _pw);
            using (MySqlConnection conn = new MySqlConnection(DB_cs))
            {
                try
                {
                    //dataGridView2.Rows.Clear();
                    //dataGridView3.Rows.Clear();
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM tb_partorder_complete WHERE BODY_NO =" + "'" + str + "'", conn);
                    DataTable SearchDataTable = new DataTable();
                    using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    {
                        adaptor.Fill(SearchDataTable);
                    }
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        dataGridView1.DataSource = SearchDataTable;
                    }));
                    
                    for (int i = 0; i < (dataGridView1.RowCount - 1); i++)
                    {
                        PartNo[i] = Convert.ToString(dataGridView1.Rows[i].Cells[6].Value);
                    }
                    int nrows = (dataGridView1.RowCount - 1);
                    for (int j = 0; j < nrows; j++)
                    {
                        cmd = new MySqlCommand("SELECT * FROM tb_partlist WHERE PartNo =" + "'" + PartNo[j] + "'", conn);
                        DataTable SearchDataTable1 = new DataTable();
                        using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                        {
                            adaptor.Fill(SearchDataTable1);
                        }
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            dataGridView1.DataSource = SearchDataTable1;
                        }));

                        string _car = "";
                        if(lb_CarType.Text == "C1")
                        {
                            _car = "CN7";
                        }
                        else if(lb_CarType.Text == "Q1")
                        {
                            _car = "QX";
                        }
                        else if (lb_CarType.Text == "S1")
                        {
                            _car = "SX2";
                        }
                        for (int i=0;i< (dataGridView1.Rows.Count-1);i++)
                        {
                            if (Convert.ToString(dataGridView1.Rows[i].Cells[6].Value) == _car)
                            {
                                carrierindex[j] = Convert.ToString(dataGridView1.Rows[i].Cells[15].Value);
                                PartName[j] = Convert.ToString(dataGridView1.Rows[i].Cells[2].Value);
                                ALC[j] = Convert.ToString(dataGridView1.Rows[i].Cells[7].Value);
                            }
                            
                        }
                    }
                    for (int j = 0; j < nrows; j++)
                    {
                        string[] nnum;
                        nnum = carrierindex[j].Split(',');
                        if (nnum.Length == 1)
                        {
                            string fr = nnum[0].Substring(0, 1);
                            int index = Convert.ToInt16(nnum[0].Substring(1, nnum[0].Length - 1));
                            if (fr == "F")
                            {
                                Front_Exist[index] = nnum[0];
                                Front_PartName[index] = PartName[j];
                                Front_ALC[index] = ALC[j];
                            }
                            else if(fr == "R")
                            {
                                Rear_Exist[index] = nnum[0];
                                Rear_PartName[index] = PartName[j];
                                Rear_ALC[index] = ALC[j];
                            }
                        }
                        else
                        {
                            string fr = nnum[0].Substring(0, 1);
                            int index = Convert.ToInt16(nnum[0].Substring(1, nnum[0].Length - 1));
                            if (fr == "F")
                            {
                                Front_Exist[index] = nnum[0];
                                Front_PartName[index] = PartName[j];
                                Front_ALC[index] = ALC[j];
                            }
                            else if (fr == "R")
                            {
                                Rear_Exist[index] = nnum[0];
                                Rear_PartName[index] = PartName[j];
                                Rear_ALC[index] = ALC[j];
                            }

                            fr = nnum[1].Substring(1, 1);
                            index = Convert.ToInt16(nnum[1].Substring(2, nnum[1].Length - 2));
                            if (fr == "F")
                            {
                                Front_Exist[index] = nnum[1];
                                Front_PartName[index] = PartName[j];
                                Front_ALC[index] = ALC[j];
                            }
                            else if (fr == "R")
                            {
                                Rear_Exist[index] = nnum[1];
                                Rear_PartName[index] = PartName[j];
                                Rear_ALC[index] = ALC[j];
                            }
                        }
                    }

                    string __car = "";
                    if (lb_CarType.Text == "C1")
                    {
                        __car = "CN7";
                    }
                    else if (lb_CarType.Text == "Q1")
                    {
                        __car = "QX";
                    }
                    else if (lb_CarType.Text == "S1")
                    {
                        __car = "SX2";
                    }

                    cmd = new MySqlCommand("SELECT * FROM tb_layout_display_31tr WHERE CarCode =" + "'" + __car + "'", conn);
                    DataTable SearchDataTable2 = new DataTable();
                    using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    {
                        adaptor.Fill(SearchDataTable2);
                    }
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        dataGridView1.DataSource = SearchDataTable2;
                    }));

                    for(int i=0;i<100;i++)
                    {
                        //if(Front_Exist[i] != "0")
                        //{
                            for(int t=0;t< dataGridView1.Rows.Count-1;t++)
                            {
                                if(Front_Exist[i] == Convert.ToString(dataGridView1.Rows[t].Cells[2].Value))
                                {
                                    Front_PartName[i] = Convert.ToString(dataGridView1.Rows[t].Cells[10].Value);
                                }
                            }
                        //}
                        //if (Rear_Exist[i] != "0")
                        //{
                            for (int t = 0; t < dataGridView1.Rows.Count - 1; t++)
                            {
                                if (Rear_Exist[i] == Convert.ToString(dataGridView1.Rows[t].Cells[2].Value))
                                {
                                    Rear_PartName[i] = Convert.ToString(dataGridView1.Rows[t].Cells[10].Value);
                                }
                            }
                        //}
                    }
                    conn.Close();
                    

                    return ProcessDefine._TRUE;
                }
                catch (System.Exception ex)
                {
                    return ProcessDefine._FALSE;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Save_Data();
                Read_Data();
                Update_Data();
            }
            catch (System.Exception ex)
            {

            }
        }

        public int Save_Data()
        {
            string strDir = "D:\\Model\\";

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            string strFileName = strDir + "Job.ini";

            //사이즈 셋팅
            ProcessFunc.WriteINI(strFileName, "Setting", "Server", txt_Server.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Port", txt_Port.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Database", txt_DB.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "id", txt_id.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "pw", txt_Password.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Folder", txt_DeleteFolder.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "NGTime", txt_NGTime.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Server_send", txt_Server_send.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Port_send", txt_Port_send.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "DB_send", txt_DB_send.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "ID_send", txt_ID_send.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Pass_send", txt_Pass_send.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Server_ftp", txt_Server_ftp.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Port_ftp", txt_Port_ftp.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "DB_ftp", txt_DB_ftp.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "ID_ftp", txt_ID_ftp.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Pass_ftp", txt_Pass_ftp.Text);
            ProcessFunc.WriteINI(strFileName, "Setting", "Finalpath", txt_Finalpath.Text);

            if (ck_sis.Checked == true)
            {
                ProcessFunc.WriteINI(strFileName, "Setting", "SiS", "1");
            }
            else
            {
                ProcessFunc.WriteINI(strFileName, "Setting", "SiS", "0");
            }
            return ProcessDefine._TRUE;
        }

        public int Read_Data()
        {
            string strDir = "D:\\Model\\";

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            string strFileName = strDir + "Job.ini";

            _server = ProcessFunc.ReadINI(strFileName, "Setting", "Server");
            int.TryParse(ProcessFunc.ReadINI(strFileName, "Setting", "Port"), out _port);
            _database = ProcessFunc.ReadINI(strFileName, "Setting", "Database");
            _id = ProcessFunc.ReadINI(strFileName, "Setting", "id");
            _pw = ProcessFunc.ReadINI(strFileName, "Setting", "pw");
            int.TryParse(ProcessFunc.ReadINI(strFileName, "Setting", "Folder"), out _DeleteFoldor);
            int.TryParse(ProcessFunc.ReadINI(strFileName, "Setting", "NGTime"), out _NGTime);
            int.TryParse(ProcessFunc.ReadINI(strFileName, "Setting", "SiS"), out _SiS);

            _server_send = ProcessFunc.ReadINI(strFileName, "Setting", "Server_send");
            int.TryParse(ProcessFunc.ReadINI(strFileName, "Setting", "Port_send"), out _port_send);
            _database_send = ProcessFunc.ReadINI(strFileName, "Setting", "DB_send");
            _id_send = ProcessFunc.ReadINI(strFileName, "Setting", "ID_send");
            _pw_send = ProcessFunc.ReadINI(strFileName, "Setting", "Pass_send");

            _server_ftp = ProcessFunc.ReadINI(strFileName, "Setting", "Server_ftp");
            int.TryParse(ProcessFunc.ReadINI(strFileName, "Setting", "Port_ftp"), out _port_ftp);
            _database_ftp = ProcessFunc.ReadINI(strFileName, "Setting", "DB_ftp");
            _id_ftp = ProcessFunc.ReadINI(strFileName, "Setting", "ID_ftp");
            _pw_ftp = ProcessFunc.ReadINI(strFileName, "Setting", "Pass_ftp");
            _Finalpath = ProcessFunc.ReadINI(strFileName, "Setting", "Finalpath");

            return ProcessDefine._TRUE;
        }

        public int Update_Data()
        {
            try
            {
                txt_Server.Text = _server;
                txt_Port.Text = _port.ToString();
                txt_DB.Text = _database;
                txt_id.Text = _id;
                txt_Password.Text = _pw;
                txt_DeleteFolder.Text = _DeleteFoldor.ToString();
                txt_NGTime.Text = _NGTime.ToString();

                txt_Server_send.Text = _server_send;
                txt_Port_send.Text = _port_send.ToString();
                txt_DB_send.Text = _database_send;
                txt_ID_send.Text = _id_send;
                txt_Pass_send.Text = _pw_send;

                txt_Server_ftp.Text = _server_ftp;
                txt_Port_ftp.Text = _port_ftp.ToString();
                txt_DB_ftp.Text = _database_ftp;
                txt_ID_ftp.Text = _id_ftp;
                txt_Pass_ftp.Text = _pw_ftp;
                txt_Finalpath.Text = _Finalpath;

                if (_SiS == 1)
                {
                    ck_sis.Checked = true;
                }
                else
                {
                    ck_sis.Checked = false;
                }
            }
            catch
            {
                return ProcessDefine._TRUE;
            }
            return ProcessDefine._TRUE;
        }

        public int Save_DB_Data()
        {
            string strDir = _Finalpath;

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            string strFileName = strDir + "DB.ini";

            //사이즈 셋팅
            ProcessFunc.WriteINI(strFileName, "DB", "Start", "0");
            ProcessFunc.WriteINI(strFileName, "DB", "Body", "");
            ProcessFunc.WriteINI(strFileName, "DB", "result", "");
            ProcessFunc.WriteINI(strFileName, "DB", "index", "");
            ProcessFunc.WriteINI(strFileName, "DB", "img", "");
            ProcessFunc.WriteINI(strFileName, "DB", "path", "");

            return ProcessDefine._TRUE;
        }

        public int Read_DB_Data()
        {
            string strDir = _Finalpath;

            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            string strFileName = strDir + "DB.ini";

            _DB_Start = ProcessFunc.ReadINI(strFileName, "DB", "Start");
            _DB_Body = ProcessFunc.ReadINI(strFileName, "DB", "Body");
            _DB_result = ProcessFunc.ReadINI(strFileName, "DB", "result");
            _DB_index = ProcessFunc.ReadINI(strFileName, "DB", "index");
            _DB_img = ProcessFunc.ReadINI(strFileName, "DB", "img");
            _DB_path = ProcessFunc.ReadINI(strFileName, "DB", "path");

            return ProcessDefine._TRUE;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //SelectDB_Partorder_Complete(textBox1.Text); 
            fileSystemWatcher1.EnableRaisingEvents = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DB_cs = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                _server, _port, _database, _id, _pw);
            using (MySqlConnection conn = new MySqlConnection(DB_cs))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM tb_partlist WHERE PartNo =" + "'" + textBox2.Text + "'", conn);
                    DataTable SearchDataTable = new DataTable();
                    using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    {
                        adaptor.Fill(SearchDataTable);
                    }
                    dataGridView1.DataSource = SearchDataTable;
                    conn.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(string.Format(ex.Message));
                }
            }
        }

        private void btn_OpenImage_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBox1.Items.Count != 0)
                {
                    listBox1.Items.Clear();
                    imagepath.Clear();
                }

                CommonOpenFileDialog fbd = new CommonOpenFileDialog();
                fbd.EnsureReadOnly = true;
                fbd.IsFolderPicker = true;
                fbd.AllowNonFileSystemItems = true;
                if (fbd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    txt_path.Text = fbd.FileName + "\\";
                    if (Directory.Exists(fbd.FileName + "\\"))
                    {
                        DirectoryInfo di = new DirectoryInfo(fbd.FileName + "\\");
                        var item = (from f in di.GetFiles()
                                    orderby f.LastWriteTime descending
                                    select f).ToArray();
                        foreach (var ii in item)
                        {
                            listBox1.Items.Add(ii.Name);
                            imagepath.Add(ii.Name);
                        }
                    }

                    SearchImage(0);
                    Number = 0;
                    listBox1.SetSelected(0, true);
                }
                listBox1.Sorted = false;
                fbd.Dispose();

                NgImagecnt = 0;
            }
            catch
            {

            }
        }

        public void SearchImage(int i)
        {
            cogRecordDisplay1.InteractiveGraphics.Clear(); 
            string imagename = txt_path.Text + imagepath[i];
            imagefileTool.Operator.Open(imagename, CogImageFileModeConstants.Read);
            cogRecordDisplay1.DrawingEnabled = false;
            imagefileTool.Run();
            cogRecordDisplay1.Fit(false);
            cogRecordDisplay1.Image = imagefileTool.OutputImage;

            Center_image = (CogImage24PlanarColor)imagefileTool.OutputImage;

            cogRecordDisplay1.DrawingEnabled = true;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            try
            {
                Number = Number - 1;
                if (Number < 0)
                {
                    MessageBox.Show("This is the first image");
                    Number = 0;
                    return;
                }
                else
                {
                    listBox1.SetSelected(Number, true);
                    SearchImage(Number);
                }
            }
            catch
            { }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            try
            {
                Number = Number + 1;
                if (Number > imagepath.Count - 1)
                {
                    MessageBox.Show("This is the last image");
                    Number = imagepath.Count - 1;
                    return;
                }
                else
                {
                    listBox1.SetSelected(Number, true);
                    SearchImage(Number);
                }
            }
            catch
            { }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                switch (comboBox2.Text)
                {
                    case "Center":
                        CENTER(comboBox1.Text, cogRecordDisplay1, Center_image,1);
                        break;
                    case "Front_LH":
                        FRONT_LH(comboBox1.Text, cogRecordDisplay1, Center_image,1);
                        break;
                    case "Front_RH":
                        FRONT_RH(comboBox1.Text, cogRecordDisplay1, Center_image,1);
                        break;
                    case "Rear_LH":
                        REAR_LH(comboBox1.Text, cogRecordDisplay1, Center_image,1);
                        break;
                    case "Rear_RH":
                        REAR_RH(comboBox1.Text, cogRecordDisplay1, Center_image,1);
                        break;
                }
            }
            catch
            { }
        }

        public static void DeleteFolder(int deleteDay = 60)
        {
            try
            {
                //년도 폴더 확인
                DirectoryInfo di = new DirectoryInfo("D:\\IMAGE\\");
                if (di.Exists)
                {
                    DirectoryInfo[] dirinfo = di.GetDirectories();
                    string IDate = DateTime.Today.AddDays(-deleteDay).ToString("yyyyMMdd");

                    foreach (DirectoryInfo dir in dirinfo)
                    {
                        if (dir.Name != IDate.Substring(0, 4))
                        {
                            if (IDate.CompareTo(dir.LastWriteTime.ToString("yyyyMMdd")) > 0)
                            {
                                dir.Attributes = FileAttributes.Normal;
                                dir.Delete(true);
                            }
                        }
                    }
                }

                //월별 폴더 확인
                string Day = DateTime.Now.ToString("yyyy-MM-dd");
                DirectoryInfo di_m = new DirectoryInfo("D:\\IMAGE\\" + Day.Substring(0, 4) + "\\");
                if (di_m.Exists)
                {
                    DirectoryInfo[] dirinfo = di_m.GetDirectories();
                    string IDate = DateTime.Today.AddDays(-deleteDay).ToString("yyyyMMdd");

                    foreach (DirectoryInfo dir in dirinfo)
                    {
                        if (IDate.CompareTo(dir.LastWriteTime.ToString("yyyyMMdd")) > 0)
                        {
                            dir.Attributes = FileAttributes.Normal;
                            dir.Delete(true);
                        }
                        else
                        {
                            //일별 폴더 확인
                            DirectoryInfo di_d = new DirectoryInfo("D:\\IMAGE\\" + Day.Substring(0, 4) + "\\" + dir.Name + "\\");
                            if (di_d.Exists)
                            {
                                DirectoryInfo[] dirinfo_d = di_d.GetDirectories();
                                foreach (DirectoryInfo dir_d in dirinfo_d)
                                {
                                    if (IDate.CompareTo(dir_d.LastWriteTime.ToString("yyyyMMdd")) > 0)
                                    {
                                        dir_d.Attributes = FileAttributes.Normal;
                                        dir_d.Delete(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }

        public static void DeleteFolder1(int deleteDay = 60)
        {
            try
            {
                //년도 폴더 확인
                DirectoryInfo di = new DirectoryInfo(@"\\90.90.90.90\vision\");
                if (di.Exists)
                {
                    DirectoryInfo[] dirinfo = di.GetDirectories();
                    string IDate = DateTime.Today.AddDays(-deleteDay).ToString("yyyyMMdd");

                    foreach (DirectoryInfo dir in dirinfo)
                    {
                        if (dir.Name != IDate.Substring(0, 4))
                        {
                            if (IDate.CompareTo(dir.LastWriteTime.ToString("yyyyMMdd")) > 0)
                            {
                                dir.Attributes = FileAttributes.Normal;
                                dir.Delete(true);
                            }
                        }
                    }
                }

                //월별 폴더 확인
                string Day = DateTime.Now.ToString("yyyy-MM-dd");
                DirectoryInfo di_m = new DirectoryInfo(@"\\90.90.90.90\vision\" + Day.Substring(0, 4) + "\\");
                if (di_m.Exists)
                {
                    DirectoryInfo[] dirinfo = di_m.GetDirectories();
                    string IDate = DateTime.Today.AddDays(-deleteDay).ToString("yyyyMMdd");

                    foreach (DirectoryInfo dir in dirinfo)
                    {
                        if (IDate.CompareTo(dir.LastWriteTime.ToString("yyyyMMdd")) > 0)
                        {
                            dir.Attributes = FileAttributes.Normal;
                            dir.Delete(true);
                        }
                        else
                        {
                            //일별 폴더 확인
                            DirectoryInfo di_d = new DirectoryInfo(@"\\90.90.90.90\vision\" + Day.Substring(0, 4) + "\\" + dir.Name + "\\");
                            if (di_d.Exists)
                            {
                                DirectoryInfo[] dirinfo_d = di_d.GetDirectories();
                                foreach (DirectoryInfo dir_d in dirinfo_d)
                                {
                                    if (IDate.CompareTo(dir_d.LastWriteTime.ToString("yyyyMMdd")) > 0)
                                    {
                                        dir_d.Attributes = FileAttributes.Normal;
                                        dir_d.Delete(true);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
            }
        }

        public int SaveDataLogFile(string strData,bool _state)
        {
            DateTime nowDate = DateTime.Now;

            string strDir = "D:\\DataLog\\";
            string strDir1 = @"\\90.90.90.90\vision\";
            if (_state == false)
            {
                strDir = "D:\\DataLog\\NG\\";
                strDir1 = @"\\90.90.90.90\vision\NG\";
            }

            if (!Directory.Exists(strDir))
            {
                Directory.CreateDirectory(strDir);
            }

            //if (!Directory.Exists(strDir1))
            //{
            //    Directory.CreateDirectory(strDir1);
            //}

            string strYear = nowDate.Year.ToString("0000");
            string strMonth = nowDate.Month.ToString("00");
            string strDay = nowDate.Day.ToString("00");

            string strDate = strYear + "_" + strMonth + "_" + strDay;
            string strFilePath = strDir + "\\" + strDate + ".csv";
            string strFilePath1 = strDir1 + "\\" + strDate + ".csv";

            if (!Directory.Exists(strDir))
            {
                Directory.CreateDirectory(strDir);
            }

            //if (!Directory.Exists(strDir1))
            //{
            //    Directory.CreateDirectory(strDir1);
            //}

            if (!File.Exists(strFilePath))
            {
                string rtr = "작업시간,차종,바디넘버,Center,Front_LH,Front_RH,Rear_LH,Rear_RH";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(strFilePath, true, System.Text.Encoding.GetEncoding("utf-8")))
                {
                    file.WriteLine(rtr);
                }
            }

            //if (!File.Exists(strFilePath1))
            //{
            //    string rtr = "작업시간,차종,바디넘버,Center,Front_LH,Front_RH,Rear_LH,Rear_RH";
            //    using (System.IO.StreamWriter file = new System.IO.StreamWriter(strFilePath1, true, System.Text.Encoding.GetEncoding("utf-8")))
            //    {
            //        file.WriteLine(rtr);
            //    }
            //}

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(strFilePath, true, System.Text.Encoding.GetEncoding("utf-8")))
            {
                file.WriteLine(strData);
            }

            //using (System.IO.StreamWriter file = new System.IO.StreamWriter(strFilePath1, true, System.Text.Encoding.GetEncoding("utf-8")))
            //{
            //    file.WriteLine(strData);
            //}

            return ProcessDefine._TRUE;
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }
        
        private void button6_Click(object sender, EventArgs e)
        {
            //timer1.Enabled = true;
            fileSystemWatcher1.EnableRaisingEvents = true;
            //Sound();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DB_cs = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                _server, _port, _database, _id, _pw);
            using (MySqlConnection conn = new MySqlConnection(DB_cs))
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM tb_layout_display_31tr WHERE CarCode =" + "'" + textBox3.Text + "'", conn);
                    DataTable SearchDataTable = new DataTable();
                    using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    {
                        adaptor.Fill(SearchDataTable);
                    }
                    dataGridView1.DataSource = SearchDataTable;
                    conn.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(string.Format(ex.Message));
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                Save_Data();
                Read_Data();
                Update_Data();
            }
            catch (System.Exception ex)
            {

            }
        }

        public int Aquire(CogRecordDisplay dis, int num)
        {
            if (this.InvokeRequired == true)
            {
                int Result = 0;
                this.Invoke(new MethodInvoker(delegate
                {
                    Result = Aquire(dis, num);
                }));
                return Result;
            }

            try
            {
                //dis.StopLiveDisplay();

                switch (num)
                {
                    case 0:
                        //Center_Aquire.Operator.Flush();
                        Center_Aquire.Run();
                        Center_Aquire_Image = (CogImage24PlanarColor)Center_Aquire.OutputImage.CopyBase(CogImageCopyModeConstants.CopyPixels);
                        dis.InteractiveGraphics.Clear();
                        dis.StaticGraphics.Clear();
                        dis.Image = (CogImage24PlanarColor)Center_Aquire_Image;
                        dis.Fit(false);
                        break;
                    case 1:
                        //Front_LH_Aquire.Operator.Flush();
                        Front_LH_Aquire.Run();
                        Front_LH_Aquire_Image = (CogImage24PlanarColor)Front_LH_Aquire.OutputImage.CopyBase(CogImageCopyModeConstants.CopyPixels);
                        dis.InteractiveGraphics.Clear();
                        dis.StaticGraphics.Clear();
                        dis.Image = (CogImage24PlanarColor)Front_LH_Aquire_Image;
                        dis.Fit(false);
                        break;
                    case 2:
                        //Front_RH_Aquire.Operator.Flush();
                        Front_RH_Aquire.Run();
                        Front_RH_Aquire_Image = (CogImage24PlanarColor)Front_RH_Aquire.OutputImage.CopyBase(CogImageCopyModeConstants.CopyPixels);
                        dis.InteractiveGraphics.Clear();
                        dis.StaticGraphics.Clear();
                        dis.Image = (CogImage24PlanarColor)Front_RH_Aquire_Image;
                        dis.Fit(false);
                        break;
                }
            }
            catch
            {
                //ProcessFunc.OnMessageDlg("이미지 획득 실패", (int)ProcessDefine.PROCESS_STATE._ERROR);
                return ProcessDefine._FALSE;
            }

            return ProcessDefine._TRUE;
        }
        private void button9_Click(object sender, EventArgs e)
        {
            Aquire(cogRecordDisplay_REAR_LH2,1);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Aquire(cogRecordDisplay_CENTER2, 0);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Aquire(cogRecordDisplay_REAR_RH2, 2);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            try
            {
                Save_Data();
                Read_Data();
                Update_Data();
            }
            catch (System.Exception ex)
            {

            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            try
            {
                Save_Data();
                Read_Data();
                Update_Data();
            }
            catch (System.Exception ex)
            {

            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            bool bResult = clsFTPHandling.Connect(_server_ftp, _port_ftp.ToString(), _id_ftp, _pw_ftp);
            
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string str = DateTime.Now.ToString("yyyy-MM-dd");
            string ssdd = "HDI/Trim/" + str;
            clsFTPHandling.MakeDirToFTPServer(ssdd);
            using (System.Windows.Forms.OpenFileDialog imgDlg = new System.Windows.Forms.OpenFileDialog())
            {

                imgDlg.InitialDirectory = Application.StartupPath;
                imgDlg.Filter = "모든 파일 (*.*)|*.*";

                DialogResult Result = imgDlg.ShowDialog();


                if (Result == DialogResult.OK)
                {

                    string strFilePath = imgDlg.FileName;
                    string strFileName = Path.GetFileName(strFilePath);



                    if (clsFTPHandling.Upload(strFilePath, strFileName,true) == false)
                    {

                    }

                    //clsCognexFunction.SaveColorImage(imgDlg.FileName, cogSetupDisplay.Image as CogImage24PlanarColor);

                }

            }
        }

        private void SendImage_ftp(string imagepath,bool gh)
        {
            string str = DateTime.Now.ToString("yyyy-MM-dd");
            string ssdd = "HDI/Trim/" + str;
            if(gh == true)
            {
                ssdd = "HDI/Trim/" + str;
            }
            else
            {
                ssdd = "HDI/Final/" + str;
            }
            clsFTPHandling.MakeDirToFTPServer(ssdd);
           
            string strFilePath = imagepath;
            string strFileName = Path.GetFileName(strFilePath);
            
            if (clsFTPHandling.Upload(strFilePath, strFileName, gh) == false)
            {

            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            DB_cs_send = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                _server_send, _port_send, _database_send, _id_send, _pw_send);
            using (MySqlConnection conn = new MySqlConnection(DB_cs_send))
            {
                try
                {
                    
                    conn.Open();
                    string body = lb_BodyNo.Text;
                    string img_path = @"\HDI\Trim\1.jpg";
                    MySqlCommand cmd = new MySqlCommand("INSERT INTO H2_VISION_INSPECTION (BODY_NO,CARRIER_INDEX,IMG_DIR) VALUES (" + body + "," + DB_Ng_Num + "," +img_path +")", conn);
                    //DataTable SearchDataTable = new DataTable();
                    //using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    //{
                    //    adaptor.Fill(SearchDataTable);
                    //}
                    //dataGridView1.DataSource = SearchDataTable;
                    conn.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(string.Format(ex.Message));
                }
            }
        }

        private void Send_DB(string body, string index,string img_path, string rstr, string gh)
        {
            DB_cs_send = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
                _server_send, _port_send, _database_send, _id_send, _pw_send);
            using (MySqlConnection conn = new MySqlConnection(DB_cs_send))
            {
                try
                {

                    conn.Open();
                    //string img_path = @"\HDI\Trim\1.jpg";
                    string sql = "";
                    sql = "INSERT INTO H2_VISION_INSPECTION (BODY_NO, RESULT, CARRIER_INDEX, REASON,IMG_DIR) VALUES ('" + body + "','" + rstr + "','" + index + "','" + gh + "','" + img_path + "');";
                    
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    //DataTable SearchDataTable = new DataTable();
                    //using (MySqlDataAdapter adaptor = new MySqlDataAdapter(cmd))
                    //{
                    //    adaptor.Fill(SearchDataTable);
                    //}
                    //dataGridView1.DataSource = SearchDataTable;
                    cmd.ExecuteNonQuery(); 
                    conn.Close();
                }
                catch (System.Exception ex)
                {
                    //MessageBox.Show(string.Format(ex.Message));
                }
            }
        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                Save_Data();
                Read_Data();
                Update_Data();
            }
            catch (System.Exception ex)
            {

            }
        }
    }
}
