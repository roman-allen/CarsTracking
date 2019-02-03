using System;
using System.Drawing;
using System.Threading;

namespace CarsTracking
{
    public delegate void FrameAvailable(Bitmap frame);

    public class VideoPlayer
    {
        private readonly Accord.Video.FFMPEG.VideoFileReader Video;
        private Thread PlaybackThread = null;
        private ManualResetEvent PlaybackThreadStop = new ManualResetEvent(false);
        private Object PlaybackThreadLock = new Object();

        public readonly int Width;
        public readonly int Height;
        public readonly float FrameRate;
        public event FrameAvailable OnFrameAvailable;

        public VideoPlayer(string videoFileName)
        {
            Video = new Accord.Video.FFMPEG.VideoFileReader();
            Video.Open(videoFileName);

            Width = Video.Width;
            Height = Video.Height;
            FrameRate = (float) Video.FrameRate.Value;
        }

        public void Start()
        {
            lock (PlaybackThreadLock)
            {
                if (PlaybackThread == null)
                {
                    PlaybackThreadStop.Reset();
                    PlaybackThread = new Thread(new ThreadStart(PlaybackThreadWorker));
                    PlaybackThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (PlaybackThreadLock)
            {
                if (PlaybackThread != null)
                {
                    PlaybackThreadStop.Set();
                }
            }
        }

        private void PlaybackThreadWorker()
        {
            try
            {
                int delay = (int)(1000 / Video.FrameRate);

                do
                {
                    Bitmap bitmap = Video.ReadVideoFrame();
                    if (bitmap == null) break;
                    try
                    {
                        OnFrameAvailable?.Invoke(bitmap);
                    } catch
                    {
                        // on any error -> stop the player
                        break;
                    }                    
                } while (!PlaybackThreadStop.WaitOne(delay));
            } finally
            {
                lock (PlaybackThreadLock)
                {
                    if (PlaybackThread != null)
                    {
                        PlaybackThread = null;
                        PlaybackThreadStop.Set();
                    }
                }
            }
        }
    }
}
