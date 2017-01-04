using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        IntPtr h;
        int mOnceLenght = 30000000;
        string mCopyFilePath;
        int mCopyFileOffset;
        public Form1()
        {
            InitializeComponent();
            //h = this.Handle;
            //h = (IntPtr)Win32Api.FindWindow(null, "Form1");
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        void FindHandle()
        {
            Process[] process = Process.GetProcesses();//获取所有启动进程
            if (null != process)
            {
                for (int i = 0, size = process.Length; i < size; ++i)
                {
                    try
                    {
                        if (process[i].ProcessName.Equals("Foxmail"))
                        {
                            h = process[i].MainWindowHandle;
                            break;
                        }
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show("查找进程出错:" + ex.ToString());
                    }
                }
            }
        }




        private void BtnOnClick(Object sender, EventArgs e)
        {
            if (sender == this.button1)
            {
                try
                {
                    OpenFileDialog of = new OpenFileDialog(); of.ShowDialog();
                    this.textBox1.Text = of.FileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                } 
            }
            else if (sender == this.button2)
            {//运行
                if (!string.IsNullOrEmpty(this.textBox1.Text))
                {
                    try
                    {
                        Process.Start(this.textBox1.Text);
                        //Application.OpenURL(XmlPath + "SvnTool/bin/Release/SvnTool.exe");
                    }
                    catch (System.IO.FileNotFoundException ex1)
                    {//文件不存在
                        MessageBox.Show(ex1.Message);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            else if (sender == this.button3)
            {//复制
                if (!string.IsNullOrEmpty(this.textBox1.Text))
                {
                    try
                    {
                        FolderBrowserDialog dialog = new FolderBrowserDialog();
                        dialog.Description = "请选择文件路径";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            string foldPath = dialog.SelectedPath;
                            if (!string.IsNullOrEmpty(foldPath))
                            {
                                if (File.Exists(this.textBox1.Text))
                                {//选中了文件
                                    string fileName = Path.GetFileName(this.textBox1.Text);
                                    string newPath = Path.Combine(foldPath, fileName);
                                    /*if (File.Exists(newPath))
                                    {
                                        File.Delete(newPath);
                                    }
                                    File.Copy(this.textBox1.Text, newPath);*/
                                    byte[] bytes;
                                    FileStream mCopyFileStream = File.OpenRead(this.textBox1.Text);
                                    mCopyFilePath = this.textBox1.Text;
                                    Dictionary<string, string> dict = new Dictionary<string, string>();
                                    if (mCopyFileStream.Length <= mOnceLenght)
                                    {
                                        bytes = new byte[mCopyFileStream.Length];
                                        mCopyFileStream.Read(bytes, 0, (int)mCopyFileStream.Length);
                                    }
                                    else
                                    {
                                        bytes = new byte[mOnceLenght];
                                        mCopyFileStream.Read(bytes, mCopyFileOffset, mOnceLenght);
                                        dict.Add("state", "start");
                                        dict.Add("offset", mCopyFileOffset.ToString());
                                    }
                                    mCopyFileStream.Dispose();
                                    mCopyFileStream.Close();
                                    mCopyFileStream = null;
                                    string fileText = Convert.ToBase64String(bytes);
                                    dict.Add(newPath, fileText);
                                    string con = JsonHepler.JsonSerializerBySingleData(dict);
                                    if (!string.IsNullOrEmpty(con))
                                    {
                                        FindHandle();
                                        
                                        if (null != h)
                                        {
                                            /*IntPtr i = Marshal.StringToHGlobalAuto(con);
                                            Win32Api.PostMessage(h, Win32Api.UM_1, 0, i);*/
                                            Win32Api.CopyDataStruct sendData;
                                            sendData.dwData = (IntPtr)1;
                                            sendData.lpData = con;    //消息字符串
                                            sendData.cbData = System.Text.Encoding.UTF8.GetBytes(con).Length + 1;  //注意，这里的长度是按字节来算的
                                            Win32Api.SendMessage((int)h/*Win32Api.FindWindow(null, "Foxmail")*/, Win32Api.WM_COPYDATA, 0, ref sendData);
                                        }
                                        else
                                        {
                                            MessageBox.Show("未找到句柄");
                                        }
                                    }
                                    
                                }
                                else if (Directory.Exists(this.textBox1.Text))
                                {//选中了文件夹
                                    Dictionary<string, string> dict = new Dictionary<string, string>();
                                    GetAllFiles(this.textBox1.Text, foldPath, dict);
                                    string con = JsonHepler.JsonSerializerBySingleData(dict);
                                    if (!string.IsNullOrEmpty(con))
                                    {
                                        FindHandle();
                                        if (null != h)
                                        {
                                            /*IntPtr i = Marshal.StringToHGlobalAuto(con);
                                            Win32Api.PostMessage(h, Win32Api.UM_1, 0, i);*/
                                            Win32Api.CopyDataStruct sendData;
                                            sendData.dwData = (IntPtr)1;
                                            sendData.lpData = con;    //消息字符串
                                            sendData.cbData = System.Text.Encoding.UTF8.GetBytes(con).Length + 1;  //注意，这里的长度是按字节来算的
                                            Win32Api.SendMessage((int)h/*Win32Api.FindWindow(null, "Foxmail")*/, Win32Api.WM_COPYDATA, 0, ref sendData);
                                        }
                                        else
                                        {
                                            MessageBox.Show("未找到句柄");
                                        }
                                    }
                                    //CopyDirectory(this.textBox1.Text, foldPath);
                                }
                            }
                        }

                    }
                    catch (System.IO.FileNotFoundException ex1)
                    {//文件不存在
                        MessageBox.Show(ex1.Message);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        private void GetAllFiles(string srcdir, string desdir, Dictionary<string, string> dict)
        {
            string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);

            string desfolderdir = desdir + "\\" + folderName;

            if (desdir.LastIndexOf("\\") == (desdir.Length - 1))
            {
                desfolderdir = desdir + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(srcdir);

            foreach (string file in filenames)// 遍历所有的文件和目录
            {
                if (Directory.Exists(file))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {

                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    /*if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }*/

                    GetAllFiles(file, desfolderdir, dict);
                }

                else // 否则直接copy文件
                {
                    string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);

                    srcfileName = desfolderdir + "\\" + srcfileName;


                    /*if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }*/
                    byte[] bytes = File.ReadAllBytes(file);
                    string fileText = Convert.ToBase64String(bytes);
                    dict.Add(srcfileName, fileText);
                    
                    /*if (File.Exists(srcfileName))
                    {
                        File.Delete(srcfileName);
                    }
                    File.Copy(file, srcfileName);*/
                }
            }//foreach 
        }
        
        private void CopyDirectory(string srcdir, string desdir)
        {
            string folderName = srcdir.Substring(srcdir.LastIndexOf("\\") + 1);

            string desfolderdir = desdir + "\\" + folderName;

            if (desdir.LastIndexOf("\\") == (desdir.Length - 1))
            {
                desfolderdir = desdir + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(srcdir);

            foreach (string file in filenames)// 遍历所有的文件和目录
            {
                if (Directory.Exists(file))// 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {

                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }

                    CopyDirectory(file, desfolderdir);
                }

                else // 否则直接copy文件
                {
                    string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);

                    srcfileName = desfolderdir + "\\" + srcfileName;


                    if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }

                    if (File.Exists(srcfileName))
                    {
                        File.Delete(srcfileName);
                    }
                    File.Copy(file, srcfileName);
                }
            }//foreach 
        }//function end

        protected override void WndProc(ref Message m)
        {
            try
            {
                if (null == m)
                {
                    MessageBox.Show("msg为空");
                    return;
                }
                switch (m.Msg)
                {
                    case Win32Api.WM_COPYDATA:
                        Win32Api.CopyDataStruct cds = (Win32Api.CopyDataStruct)m.GetLParam(typeof(Win32Api.CopyDataStruct));
                        string str = cds.lpData.ToString();//Marshal.PtrToStringAuto(m.LParam);
                        if (str.Equals("back"))
                        {
                            Dictionary<string, string> dict = new Dictionary<string, string>();
                            byte[] bytes = null;
                            FileStream mCopyFileStream = File.OpenRead(mCopyFilePath);
                            if (mCopyFileOffset + mOnceLenght >= mCopyFileStream.Length)
                            {
                                bytes = new byte[mCopyFileStream.Length - mCopyFileOffset];
                                mCopyFileStream.Read(bytes, mCopyFileOffset, bytes.Length);
                                dict.Add("state", "end");
                                dict.Add("offset", mCopyFileOffset.ToString());
                                mCopyFileOffset = 0;
                            }
                            else
                            {
                                bytes = new byte[mOnceLenght];
                                mCopyFileStream.Read(bytes, mCopyFileOffset, mOnceLenght);
                                dict.Add("state", "update");
                                dict.Add("offset", mCopyFileOffset.ToString());
                                mCopyFileOffset += mOnceLenght;
                            }
                            mCopyFileStream.Dispose();
                            mCopyFileStream.Close();
                            mCopyFileStream = null;
                            string fileText = Convert.ToBase64String(bytes);

                            dict.Add(mCopyFilePath, fileText);
                            string con = JsonHepler.JsonSerializerBySingleData(dict);
                            if (!string.IsNullOrEmpty(con))
                            {
                                FindHandle();
                                if (null != h)
                                {
                                    Win32Api.CopyDataStruct sendData;
                                    sendData.dwData = (IntPtr)1;
                                    sendData.lpData = con;    //消息字符串
                                    sendData.cbData = System.Text.Encoding.UTF8.GetBytes(con).Length + 1;  //注意，这里的长度是按字节来算的
                                    Win32Api.SendMessage(Win32Api.FindWindow(null, "Foxmail"), Win32Api.WM_COPYDATA, 0, ref sendData);
                                }
                                else
                                {
                                    MessageBox.Show("未找到句柄");
                                }
                            }
                        }
                        else
                        {
                            Dictionary<string, string> dict = JsonHepler.JsonDeserializeBySingleData<Dictionary<string, string>>(str);
                            if (null != dict)
                            {
                                string state = string.Empty;
                                int offset = 0;
                                if (dict.ContainsKey("state"))
                                {
                                    state = dict["state"];
                                }
                                if (dict.ContainsKey("offset"))
                                {
                                    offset = int.Parse(dict["offset"]);
                                }
                                foreach (KeyValuePair<string, string> kvp in dict)
                                {
                                    if (kvp.Key.Equals("state")) continue;
                                    if (kvp.Key.Equals("offset")) continue;
                                    string dirPath = Path.GetDirectoryName(kvp.Key);
                                    if (!Directory.Exists(dirPath))
                                    {
                                        Directory.CreateDirectory(dirPath);
                                    }
                                    try
                                    {
                                        FileStream aFile = new FileStream(kvp.Key, FileMode.OpenOrCreate);
                                        byte[] bytes = Convert.FromBase64String(kvp.Value);
                                        if (null != bytes)
                                        {
                                            if (string.IsNullOrEmpty(state))
                                            {
                                                aFile.Write(bytes, 0, bytes.Length);
                                            }
                                            else
                                            {
                                                aFile.Write(bytes, offset, bytes.Length);
                                            }

                                        }
                                        if (string.IsNullOrEmpty(state))
                                        {
                                            aFile.Close();
                                        }
                                        else if (state.Equals("end"))
                                        {
                                            aFile.Close();
                                        }
                                        else
                                        {
                                            Win32Api.CopyDataStruct sendData;
                                            sendData.dwData = (IntPtr)1;
                                            sendData.lpData = "back";    //消息字符串
                                            sendData.cbData = System.Text.Encoding.UTF8.GetBytes("back").Length + 1;  //注意，这里的长度是按字节来算的
                                            Win32Api.SendMessage(Win32Api.FindWindow(null, "MyTool"), Win32Api.WM_COPYDATA, 0, ref sendData);
                                        }

                                    }
                                    catch (System.Exception ex)
                                    {
                                        MessageBox.Show(ex.Message);
                                    }

                                }
                            }
                        }
                        
                        //MessageBox.Show("收到了消息");
                        break;
                    default:
                        base.WndProc(ref m);
                        break;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("接收消息出错" + ex.ToString());
            }
            
        }

    }

    public class Win32Api
    {
        //WM_COPYDATA消息所要求的数据结构
public struct CopyDataStruct
{
    public IntPtr dwData;
    public int cbData;
 
    [MarshalAs(UnmanagedType.LPStr)]
 
    public string lpData;
}
 
public const int WM_COPYDATA = 0x004A;
 
//通过窗口的标题来查找窗口的句柄 
[DllImport("User32.dll", EntryPoint = "FindWindow")]
public static extern int FindWindow(string lpClassName, string lpWindowName);
 
//在DLL库中的发送消息函数
[DllImport("User32.dll", EntryPoint = "SendMessage")]
public static extern int SendMessage
    (
    int hWnd,                         // 目标窗口的句柄  
    int Msg,                          // 在这里是WM_COPYDATA
    int wParam,                       // 第一个消息参数
    ref  CopyDataStruct lParam        // 第二个消息参数
   );
 
    #region msg
        public const int USER = 0x0400;
        //public const int UM_1 = USER + 1;
        #endregion
        #region api
        [DllImport("user32.dll")]
        public static extern void PostMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
        #endregion
    }


}
