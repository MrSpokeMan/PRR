using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace zad3
{
    delegate void ClientDelegate();

    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(() => run_n_clients(4 * 4));
            Console.ReadLine();
        }

        static void run_n_clients(int n)
        {
            Task[] tasks = new Task[n];
            for (int i = 0; i < n; i++)
            {
                tasks[i] = Task.Run(() => client_main());
            }
            Task.WaitAll(tasks);
        }

        // Convert Bitmap to grayscale 2D array
        static int[,] ImageTo2DArray(Bitmap image)
        {
            int width = image.Width;
            int height = image.Height;
            int[,] grayArray = new int[height, width];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = image.GetPixel(x, y);
                    grayArray[y, x] = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                }
            }
            return grayArray;
        }

        // Convert 2D array to byte array for transmission
        static byte[] ArrayToByteArray(int[,] array)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(ms);
                int height = array.GetLength(0);
                int width = array.GetLength(1);
                writer.Write(height);
                writer.Write(width);
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        writer.Write(array[y, x]);
                    }
                }
                return ms.ToArray();
            }
        }

        // Convert byte array to 2D array for processing
        static int[,] ByteArrayToArray(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                BinaryReader reader = new BinaryReader(ms);
                int height = reader.ReadInt32();
                int width = reader.ReadInt32();
                int[,] array = new int[height, width];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        array[y, x] = reader.ReadInt32();
                    }
                }
                return array;
            }
        }

        // Sobel filter on 2D array
        static int[,] SobelFilter(int[,] img)
        {
            int height = img.GetLength(0);
            int width = img.GetLength(1);
            int[,] processed = new int[height, width];
            int[,] sobel_x = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] sobel_y = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

            for (int i = 1; i < height - 1; i++)
            {
                for (int j = 1; j < width - 1; j++)
                {
                    int gx = 0, gy = 0;
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            gx += img[i + x, j + y] * sobel_x[x + 1, y + 1];
                            gy += img[i + x, j + y] * sobel_y[x + 1, y + 1];
                        }
                    }
                    processed[i, j] = Math.Min(255, (int)Math.Sqrt(gx * gx + gy * gy));
                }
            }
            return processed;
        }

        // Split 2D array into n*n parts
        static int[][,] SplitImageArray(int[,] img, int n)
        {
            int h = img.GetLength(0);
            int w = img.GetLength(1);
            int h_step = h / n;
            int w_step = w / n;
            int[][,] parts = new int[n * n][,];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    int[,] part = new int[h_step, w_step];
                    for (int y = 0; y < h_step; y++)
                    {
                        for (int x = 0; x < w_step; x++)
                        {
                            part[y, x] = img[i * h_step + y, j * w_step + x];
                        }
                    }
                    parts[i * n + j] = part;
                }
            }
            return parts;
        }

        // Sending and receiving arrays over network
        static void SendAll(NetworkStream stream, byte[] data)
        {
            stream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            stream.Write(data, 0, data.Length);
        }

        static byte[] RecvAll(NetworkStream stream)
        {
            byte[] length = new byte[4];
            stream.Read(length, 0, 4);
            int size = BitConverter.ToInt32(length, 0);
            byte[] data = new byte[size];
            int received = 0;
            while (received < size)
            {
                received += stream.Read(data, received, size - received);
            }
            return data;
        }

        static void client_main()
        {
            TcpClient client = new TcpClient("172.20.10.7", 12345);
            NetworkStream stream = client.GetStream();
            byte[] data = RecvAll(stream);
            int[,] fragment = ByteArrayToArray(data);
            int[,] processed_fragment = SobelFilter(fragment);
            SendAll(stream, ArrayToByteArray(processed_fragment));
            client.Close();
            Console.WriteLine("Client finished");
        }
    }
}
