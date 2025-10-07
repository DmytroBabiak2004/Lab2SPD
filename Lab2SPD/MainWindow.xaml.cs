using System;
using System.Linq;
using System.Text;
using System.Windows;

namespace Lab2SPD
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private const uint INIT_A = 0x67452301;
        private const uint INIT_B = 0xEFCDAB89;
        private const uint INIT_C = 0x98BADCFE;
        private const uint INIT_D = 0x10325476;

        private static readonly int[] SHIFT_AMTS =
        {
            7, 12, 17, 22,
            5, 9, 14, 20,
            4, 11, 16, 23,
            6, 10, 15, 21
        };

        private static readonly uint[] K = Enumerable.Range(0, 64)
            .Select(i => (uint)(Math.Abs(Math.Sin(i + 1)) * Math.Pow(2, 32)))
            .ToArray();

        private static uint LeftRotate(uint x, int amount)
        {
            return (x << amount) | (x >> (32 - amount));
        }

        private static byte[] Md5Algorithm(byte[] inputBytes)
        {
            int messageLength = inputBytes.Length;
            int numberOfBlocks = (int)Math.Ceiling((messageLength + 9) / 64.0);
            byte[] paddedMessage = new byte[numberOfBlocks * 64];
            Array.Copy(inputBytes, paddedMessage, messageLength);

            paddedMessage[messageLength] = 0x80;
            ulong messageBitLength = (ulong)messageLength * 8;
            BitConverter.GetBytes(messageBitLength).CopyTo(paddedMessage, paddedMessage.Length - 8);

            uint a = INIT_A, b = INIT_B, c = INIT_C, d = INIT_D;

            for (int i = 0; i < numberOfBlocks; i++)
            {
                uint[] M = new uint[16];
                for (int j = 0; j < 16; j++)
                    M[j] = BitConverter.ToUInt32(paddedMessage, (i * 64) + j * 4);

                uint originalA = a, originalB = b, originalC = c, originalD = d;

                for (int j = 0; j < 64; j++)
                {
                    uint f, g;
                    if (j < 16)
                    {
                        f = (b & c) | (~b & d);
                        g = (uint)j;
                    }
                    else if (j < 32)
                    {
                        f = (d & b) | (~d & c);
                        g = (uint)((5 * j + 1) % 16);
                    }
                    else if (j < 48)
                    {
                        f = b ^ c ^ d;
                        g = (uint)((3 * j + 5) % 16);
                    }
                    else
                    {
                        f = c ^ (b | ~d);
                        g = (uint)((7 * j) % 16);
                    }

                    uint temp = d;
                    d = c;
                    c = b;
                    b = b + LeftRotate(a + f + K[j] + M[g],
                        SHIFT_AMTS[j % 4 + (j / 16) * 4]);
                    a = temp;
                }

                a += originalA;
                b += originalB;
                c += originalC;
                d += originalD;
            }

            byte[] result = new byte[16];
            Array.Copy(BitConverter.GetBytes(a), 0, result, 0, 4);
            Array.Copy(BitConverter.GetBytes(b), 0, result, 4, 4);
            Array.Copy(BitConverter.GetBytes(c), 0, result, 8, 4);
            Array.Copy(BitConverter.GetBytes(d), 0, result, 12, 4);

            return result;
        }

        private static string ToHexString(byte[] byteArray)
        {
            return string.Concat(byteArray.Select(b => b.ToString("x2")));
        }

        public static string MD5(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return ToHexString(Md5Algorithm(inputBytes));
        }

        private void GenerateMD5_Click(object sender, RoutedEventArgs e)
        {
            string input = InputTextBox.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Введи текст для хешування!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultTextBox.Text = MD5(input);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
