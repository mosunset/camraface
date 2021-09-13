using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace opencv
{
    public partial class Form1 : Form
    {
        private readonly Timer timer = new();
        private VideoCapture vcap;
        private Mat mat;
        private Bitmap bmp;
        private Rect rt;
        private int framecount = 0, facecount = 0, inc = 0;
        private readonly int wi = 640;
        private readonly int he = 480;
        private readonly String model = "haarcascade_frontalface_default.xml";
        public Form1()
        {
            InitializeComponent();
            MaximizeBox = false;
            timer.Tick += new EventHandler(Videoplay);
            wi = pictureBox1.Width;
            he = pictureBox1.Height;
            label22.Text = wi.ToString();
            label21.Text = he.ToString();
            label23.Text = model;
        }

        private void Videoplay(object sender, EventArgs e)
        {
            ++framecount;
            mat = new Mat();
            if (vcap.Read(mat))
            {
                if (inc == 0)
                {
                    Cv2.Flip(mat, mat, FlipMode.Y);
                }
                else
                {
                    label5.Text = framecount.ToString();
                }

                if (mat.IsContinuous())
                {
                    if (checkBox2.Checked)
                    {
                        try
                        {
                            using (CascadeClassifier cascadeClassifier = new CascadeClassifier(".\\learnedmodels\\" + model))
                            {
                                foreach (OpenCvSharp.Rect rect in cascadeClassifier.DetectMultiScale(mat))
                                {
                                    ++facecount;
                                    rt = new OpenCvSharp.Rect(rect.X, rect.Y, rect.Width, rect.Height);
                                    Cv2.Rectangle(mat, rt, new Scalar(230.0, 230.0, 100.0), 4);
                                }
                            }
                        }
                        catch
                        {
                            checkBox2.Checked = false;
                            int num = (int)MessageBox.Show("多分モデル読み込めてない", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                        }
                    }
                    if (checkBox1.Checked)
                    {
                        rt = new OpenCvSharp.Rect(trackBar1.Value, trackBar2.Value, trackBar3.Value, trackBar4.Value);
                        Cv2.Rectangle(mat, rt, new Scalar(92.0, 153.0, 65.0), 2);
                    }
                    bmp = new Bitmap(mat.ToBitmap(), new System.Drawing.Size(wi, he));
                    pictureBox1.Image = bmp;
                    if (inc == 0)
                    {
                        if (checkBox3.Checked)
                        {
                            Coderead(bmp);
                        }
                    }
                    else if (checkBox3.Checked)
                    {
                        checkBox3.Checked = false;
                        int num = (int)MessageBox.Show("コードリーダー\nカメラの場合のみ選択できます", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    }
                    label15.Text = facecount.ToString();
                    facecount = 0;
                    mat?.Dispose();
                }
            }
            if (framecount == vcap.FrameCount)
            {
                label2.ForeColor = Color.Red;
                label2.Text = "表示終了";
                timer.Enabled = false;
            }
            else
            {
                label2.Text = "表示中";
            }
        }
        public void Lab(int cl)
        {
            try
            {
                if (cl == 0)
                {

                    vcap = new VideoCapture(Int16.Parse(textBox2.Text));
                    timer.Interval = 33;
                    timer.Enabled = true;
                }
                else
                {
                    vcap = new VideoCapture(textBox1.Text);
                    timer.Interval = (int)(990 / vcap.Fps);
                    timer.Enabled = true;
                }
                label1.Text = vcap.FrameCount.ToString();
                label3.Text = vcap.FrameWidth.ToString();
                label4.Text = vcap.FrameHeight.ToString();
                trackBar1.Maximum = vcap.FrameWidth;
                trackBar2.Maximum = vcap.FrameHeight;
                trackBar3.Maximum = vcap.FrameWidth;
                trackBar4.Maximum = vcap.FrameHeight;
                if (!vcap.IsOpened())
                {
                    MessageBox.Show("メディアを読み込めません", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch
            {
                MessageBox.Show("なんかおかしい\nパスかカメラの設定", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        private void Button1_Click(object sender, EventArgs e)
        {
            Clear();
            inc = 0;
            Lab(inc);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            Clear();
            inc = 1;
            Lab(inc);
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            label13.Text = trackBar1.Value.ToString();
        }

        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
            label12.Text = trackBar2.Value.ToString();
        }

        private void TrackBar3_Scroll(object sender, EventArgs e)
        {
            label11.Text = trackBar3.Value.ToString();
        }

        private void TrackBar4_Scroll(object sender, EventArgs e)
        {
            label10.Text = trackBar4.Value.ToString();
        }
        private void Button4_Click(object sender, EventArgs e)
        {
            B4reset();
        }
        public void B4reset()
        {
            try
            {
                trackBar1.Value = 100;
                label13.Text = "100";
                trackBar2.Value = 100;
                label12.Text = "100";
                trackBar3.Value = 100;
                label11.Text = "100";
                trackBar4.Value = 100;
                label10.Text = "100";
            }
            catch { }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new();
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            ofd.Filter = "MP4かJPEG|*.mp4;*.jpg;*.jpeg|すべてのファイル(*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.Title = "開くファイルを選択してください";
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = ofd.FileName;
            }
            else
            {
                Console.WriteLine("キャンセルされました");
            }
            ofd.Dispose();
        }


        public void Coderead(Bitmap f)
        {
            String ret;
            ZXing.BarcodeReader reader = new();
            ZXing.Result result = reader.Decode(f);
            if (result != null)
            {
                ret = result.Text;
                bool resu = Regex.IsMatch(ret, @"http.*://.*?\.");
                if (resu)
                {
                    richTextBox1.Text = ret;
                }
                else
                {
                    try
                    {
                        long c = Int64.Parse(ret);
                        richTextBox1.Text = "https://www.google.com/search?q=" + ret;
                    }
                    catch
                    {
                        richTextBox1.Text = ret;
                    }
                }
                label24.Text = result.BarcodeFormat.ToString();
            }
        }

        private void Linkclick(object sender, LinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(@"C:\Program Files\Google\Chrome\Application\chrome.exe", e.LinkText);
            }
            catch
            {
                MessageBox.Show("なんかブラウザ開けんのやけど\nなんで?", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Clear()
        {
            timer.Enabled = false;
            framecount = 0;
            facecount = 0;
            label1.Text = "0";
            label3.Text = "0";
            label4.Text = "0";
            label5.Text = "0";
            label2.ForeColor = SystemColors.ControlText;
            label2.Text = "No Image";
            label15.Text = "0";
            mat?.Dispose();
            bmp?.Dispose();
            vcap?.Dispose();
            B4reset();
            pictureBox1.Image = null;
        }

        private void Formclose(object sender, FormClosingEventArgs e)
        {
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            DialogResult result = MessageBox.Show("ウィンドウを閉じますか？", "アプリケーション終了", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                timer.Enabled = false;
                timer?.Dispose();
                mat?.Dispose();
                bmp?.Dispose();
                vcap?.Dispose();
                Clear();
                Dispose();
                Environment.Exit(0);
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
