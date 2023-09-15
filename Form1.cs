using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace yobikomi_pack
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /*
                struct songheader_t
                {
                    u32 magic;
                    u16 val1;
                    u16 val2;
                    u32 addr_start;
                    u32 addr_stop;
                };

                struct song_t
                {
                    songheader_t header;
                    padding[0x400 - 16];
                    u8 songdata[header.addr_stop - header.addr_start + 1];
                };

                struct globalheader
                {
                    u32 magic;
                    u32 val1;
                    u32 val2;
                    u16 u1;
                    u16 u2;
                    u32 songaddr[2];
                    padding[0x40 - 24];
    
            song_t song1@songaddr[0];
            song_t song2@songaddr[1];
        };

                globalheader data@0;*/
        class songheader
        {
            public byte[] magic={0x55,0xaa,0x01,0x00};
            public byte[] reserverd = {0x00,0x80,0xff,0xff };

            public UInt32 start;
            public UInt32 stop;
        }

        public static void _Pack(string f1,string f2,string output)
        {
            try
            {

                if (!System.BitConverter.IsLittleEndian)
                {
                    throw new Exception("Environment exception(Endian)");
                }
                byte[] ch1 = System.IO.File.ReadAllBytes(f1);
                byte[] ch2 = System.IO.File.ReadAllBytes(f2);
                byte[][] chs = new byte[][] { ch1, ch2 };
                System.IO.MemoryStream ms_gheader = new System.IO.MemoryStream();
                ms_gheader.Write(new byte[] {
                    0xaa,0x55,0x01,0x00,
                    0x00,0x01,0x64,0x00,
                    0x00,0x00,0x00,0x00,
                    0x01,0x00,0x02,0x00
                });



                int arignment = 1024;
                int ch1_length = (ch1.Length % arignment == 0) ? ch1.Length : ((ch1.Length + arignment - 1) / arignment * arignment);
                int ch2_length = (ch2.Length % arignment == 0) ? ch2.Length : ((ch2.Length + arignment - 1) / arignment * arignment);




                int[] ch_padded_length = { ch1_length, ch2_length };

                int offsetof_ch1 = 0x400;
                int offsetof_ch2 = 0x400 /* footer padding */ + 0x400 /* global header */ + (ch1_length + 0x400);

                songheader[] songheaders = new songheader[2];
                songheaders[0] = new songheader();
                songheaders[0].start = (UInt32)offsetof_ch1 + 0x400;
                songheaders[0].stop = (UInt32)(songheaders[0].start + ch_padded_length[0] - 1);

                songheaders[1] = new songheader();
                songheaders[1].start = (UInt32)offsetof_ch2 + 0x400;
                songheaders[1].stop = (UInt32)(songheaders[1].start + ch_padded_length[1] - 1);


                byte[] pad = new byte[16 * 3 + 8];
                Array.Fill<byte>(pad, 0xff);



                ms_gheader.Write(System.BitConverter.GetBytes((int)offsetof_ch1));
                ms_gheader.Write(System.BitConverter.GetBytes((int)offsetof_ch2));

                ms_gheader.Write(pad);

                for (int x = 0; x < 0x400 - 80; x++)
                {
                    ms_gheader.WriteByte(0x00);
                }
                System.IO.MemoryStream song = new System.IO.MemoryStream();
                for (int n = 0; n < 2; n++)
                {
                    song.Write(songheaders[n].magic);
                    song.Write(songheaders[n].reserverd);
                    song.Write(System.BitConverter.GetBytes((int)songheaders[n].start));
                    song.Write(System.BitConverter.GetBytes((int)songheaders[n].stop));

                    for (int x = 0; x < 0x400 - 16; x++)
                    {
                        song.WriteByte(0xff);
                    }

                    song.Write(chs[n]);

                    for (int p = 0; p < ch_padded_length[n] - chs[n].Length; p++)
                    {
                        song.WriteByte(0x80);
                    }
                    for (int x = 0; x < 0x400; x++)
                    {
                        song.WriteByte(0xff);
                    }
                }

                ms_gheader.Write(song.ToArray());


                while (ms_gheader.Length < 1024 * 1024 * 4)
                {
                    ms_gheader.WriteByte(0xff);
                }
                System.IO.File.WriteAllBytes(output, ms_gheader.ToArray());
                Console.WriteLine("packed to " + output);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

    
        }
        public static void Unpack(string s)
        {
            byte[] bs = System.IO.File.ReadAllBytes(s);
            UInt32 off1 = System.BitConverter.ToUInt32(bs.Skip(16).Take(4).ToArray());
            UInt32 off2 = System.BitConverter.ToUInt32(bs.Skip(20).Take(4).ToArray());


            byte[] song1 = bs.Skip((int)off1).Take((int)(off2-off1)).ToArray();
            byte[] song2 = bs.Skip((int)off2).ToArray();


            UInt32 start1 = System.BitConverter.ToUInt32(song1.Skip(8).Take(4).ToArray());
            UInt32 stop1 = System.BitConverter.ToUInt32(song1.Skip(12).Take(4).ToArray());
            song1 = song1.Skip(0x400).Take((int)(stop1 - start1 + 1)).ToArray();


            UInt32 start2 = System.BitConverter.ToUInt32(song2.Skip(8).Take(4).ToArray());
            UInt32 stop2 = System.BitConverter.ToUInt32(song2.Skip(12).Take(4).ToArray());
            song2 = song2.Skip(0x400).Take((int)(stop2 - start2 + 1)).ToArray();

            System.IO.File.WriteAllBytes(s + ".1.songblock", song1);
            System.IO.File.WriteAllBytes(s + ".2.songblock", song2);

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if(dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxUnpackIn.Text = dlg.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Unpack(textBoxUnpackIn.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxSong1.Text = dlg.FileName;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxSong2.Text = dlg.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            if(dlg.ShowDialog() == DialogResult.OK)
            {
                _Pack(textBoxSong1.Text,textBoxSong2.Text,dlg.FileName);
            }
        }
    }
}
