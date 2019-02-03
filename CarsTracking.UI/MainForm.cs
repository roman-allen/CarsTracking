using System;
using System.Drawing;
using System.Windows.Forms;

namespace CarsTracking
{
    public partial class MainForm : Form
    {
        private VideoPlayer videoPlayer = null;
        private MotionDetector detector = null;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Construct video player
            videoPlayer = new VideoPlayer(@"Video\CarsDrivingUnderBridge.mp4");

            // Construct motion detector and initialize it with video configuration
            detector = new MotionDetector(videoPlayer.Width, videoPlayer.Height, videoPlayer.FrameRate);

            // Handle each frame read by VideoPlayer with MotionDetector
            videoPlayer.OnFrameAvailable += VideoPlayer_OnFrameAvailable;

            // Start playing video
            videoPlayer.Start();
        }

        private void VideoPlayer_OnFrameAvailable(Bitmap frame)
        {
            // Perform motion detection
            detector.ProcessFrame(frame);

            // Show frame with motion detection marks
            this.BeginInvoke((MethodInvoker) delegate () {
                pictureBox1.Image = frame;
            });
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop playing video/detection
            videoPlayer.Stop();
        }
    }
}
