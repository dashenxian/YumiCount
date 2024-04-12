using System;
using System.Text;
using OpenCvSharp;

namespace 玉米粒计数
{
    public partial class Form1 : Form
    {
        string fileFilter = "*.*|*.bmp;*.jpg;*.jpeg;*.tiff;*.tiff;*.png";
        string image_path = "";

        DateTime dt1 = DateTime.Now;
        DateTime dt2 = DateTime.Now;

        Mat image;
        Mat result_image;

        StringBuilder sb = new StringBuilder();
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = fileFilter;
            if (ofd.ShowDialog() != DialogResult.OK) return;

            pictureBox1.Image = null;
            pictureBox2.Image = null;
            textBox1.Text = "";

            image_path = ofd.FileName;
            pictureBox1.Image = new Bitmap(image_path);
            image = new Mat(image_path);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (image_path == "")
            {
                return;
            }
            textBox1.Text = "检测中，请稍等……";
            pictureBox2.Image = null;
            Application.DoEvents();

            result_image = image.Clone();

            //二值化操作
            Mat grayimg = new Mat();
            Cv2.CvtColor(image, grayimg, ColorConversionCodes.BGR2GRAY);
            Mat BinaryImg = new Mat();
            Cv2.Threshold(grayimg, BinaryImg, 240, 255, ThresholdTypes.Binary);
            //Cv2.ImShow("二值化", BinaryImg);

            //腐蚀
            Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(15, 15));
            Mat morhImage = new Mat();
            Cv2.Dilate(BinaryImg, morhImage, kernel, null, 2);
            //Cv2.ImShow("morphology", morhImage);

            //距离变换：用于二值化图像中的每一个非零点距自己最近的零点的距离，距离变换图像上越亮的点，代表了这一点距离零点的距离越远
            Mat dist = new Mat();
            Cv2.BitwiseNot(morhImage, morhImage);
            /*
            OpenCV中，函数distanceTransform()用于计算图像中每一个非零点像素与其最近的零点像素之间的距离，
            输出的是保存每一个非零点与最近零点的距离信息，图像上越亮的点，代表了离零点的距离越远。
            用途：
            可以根据距离变换的这个性质，经过简单的运算，用于细化字符的轮廓和查找物体质心（中心）。
            */
            /*
            距离变换的处理图像通常都是二值图像，而二值图像其实就是把图像分为两部分，即背景和物体两部分，物体通常又称为前景目标。
            通常我们把前景目标的灰度值设为255（即白色），背景的灰度值设为0（即黑色）。
            所以定义中的非零像素点即为前景目标，零像素点即为背景。
            所以图像中前景目标中的像素点距离背景越远，那么距离就越大，如果我们用这个距离值替换像素值，那么新生成的图像中这个点越亮。
            */
            //User：用户自定义
            //L1：  曼哈顿距离
            //L2：  欧式距离
            //C：   棋盘距离
            Cv2.DistanceTransform(morhImage, dist, DistanceTypes.L1, DistanceTransformMasks.Mask3);
            Cv2.Normalize(dist, dist, 0, 1.0, NormTypes.MinMax); //范围在0~1之间
            //Cv2.ImShow("distance", dist);

            //形态学处理
            Mat MorphImg = new Mat();
            dist.ConvertTo(MorphImg, MatType.CV_8U);
            Cv2.Threshold(MorphImg, MorphImg, 0.99, 255, ThresholdTypes.Binary);  //上图像素值在0~1之间
            kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(7, 3), new OpenCvSharp.Point(-1, -1));
            Cv2.MorphologyEx(MorphImg, MorphImg, MorphTypes.Open, kernel);  //开操作
                                                                            //Cv2.ImShow("t-distance", MorphImg);

            //找到种子的轮廓区域
            OpenCvSharp.Point[][] contours;
            HierarchyIndex[] hierarchly;
            Cv2.FindContours(MorphImg, out contours, out hierarchly, RetrievalModes.External, ContourApproximationModes.ApproxSimple, new OpenCvSharp.Point(0, 0));
            Mat markers = Mat.Zeros(image.Size(), MatType.CV_8UC3);
            int x, y, w, h;
            Rect rect;
            for (int i = 0; i < contours.Length; i++)
            {
                // Cv2.DrawContours(markers, contours, i, Scalar.RandomColor(), 2, LineTypes.Link8, hierarchly);
                rect = Cv2.BoundingRect(contours[i]);
                x = rect.X;
                y = rect.Y;
                w = rect.Width;
                h = rect.Height;
                Cv2.Circle(result_image, x + w / 2, y + h / 2, 20, new Scalar(0, 0, 255), -1);
                if (i >= 9)
                {
                    Cv2.PutText(result_image, (i + 1).ToString(), new OpenCvSharp.Point(x + w / 2 - 18, y + h / 2 + 8), HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 0), 2);
                }
                else
                {
                    Cv2.PutText(result_image, (i + 1).ToString(), new OpenCvSharp.Point(x + w / 2 - 8, y + h / 2 + 8), HersheyFonts.HersheySimplex, 0.8, new Scalar(0, 255, 0), 2);
                }
            }

            textBox1.Text = "number of corns: " + contours.Length;
            pictureBox2.Image = new Bitmap(result_image.ToMemoryStream());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //test
            image_path = "test_img/1.png";
            image = new Mat(image_path);
            pictureBox1.Image = new Bitmap(image_path);
        }
    }
}
