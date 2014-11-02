using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace zad2_2
{
 sealed class Encryptor
 {
  static byte[] GetBlockString(string str)
  {
   byte[] bytes = new byte[str.Length * sizeof(char)];
   System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
   return bytes;
  }

  static List<bool> GetBinaryStringASCI(string str)
  {
   List<bool> ret = new List<bool>();

   // na samym początku konwersja
   byte[] tmp = Encoding.Convert(Encoding.Unicode, Encoding.ASCII, GetBlockString(str));

   // suma kontrola
   AppendList(ret, CheckSum(tmp));
   AppendList(ret, (ushort)tmp.Length);

   // dodajemy do listy
   for (int it = 0; it < tmp.Length; ++it)
    AppendList(ret, tmp[it]);

   return ret;
  }

  static List<bool> GetBinaryStringUTF16(string str)
  {
   List<bool> ret = new List<bool>();

   // na samym początku konwersja
   byte[] tmp = Encoding.Convert(Encoding.Unicode, Encoding.Unicode, GetBlockString(str));

   // suma kontrola
   AppendList(ret, CheckSum(tmp));
   AppendList(ret, (ushort)tmp.Length);

   // dodajemy do listy
   for (int it = 0; it < tmp.Length; ++it)
    AppendList(ret, tmp[it]);

   return ret;
  }

  private static byte CheckSum(byte[] table)
  {
   byte ret = 0;

   for (int it = 0; it < table.Length; ++it)
    ret = (byte)((ret << 1) ^ (table[it]));

   return ret;
  }

  private static void AppendList(List<bool> ret, byte b)
  {
   for (int jt = 7; jt >= 0; --jt)
    ret.Add((b & (1 << jt)) > 0);
  }

  private static void AppendList(List<bool> ret, ushort b)
  {
   for (int jt = 15; jt >= 0; --jt)
    ret.Add((b & (1 << jt)) > 0);
  }

  private static byte GetPart(List<bool> lst, int count)
  {
   byte ret = 0;

   for (int it = 0; it < count; ++it)
   {
    ret <<= 1;
    ret |= (byte)((lst.FirstOrDefault() ? 1 : 0));

    if ( lst.Count > 0 )
     lst.RemoveAt(0);
   }

   return ret;
  }

  public static void EncryptData(int r, int g, int b, bool asci, string whatEncrypt, Bitmap inBmp, Bitmap outBmp, Form1 f1)
  {
   if (r == 0 && g == 0 && b == 0)
    return;

   List<bool> data;

   if (asci)
    data = GetBinaryStringASCI(whatEncrypt);
   else
    data = GetBinaryStringUTF16(whatEncrypt);

   f1.pbProgressBar.Maximum = data.Count;

   int used = 0;

   // zapisujemy dane do pliku

   byte clearR = (byte)~((1 << r) - 1);
   byte clearG = (byte)~((1 << g) - 1);
   byte clearB = (byte)~((1 << b) - 1);


   for ( int y = 0; y < inBmp.Height && data.Count > 0; ++y)
    for (int x = 0; x < inBmp.Width && data.Count > 0; ++x)
    {
     Color cpix = inBmp.GetPixel(x, y);
     byte R = cpix.R, G = cpix.G, B = cpix.B;
     int bf = data.Count;

     if (r > 0)
      R = (byte)((R & clearR) | GetPart(data, r));

     if (g > 0)
      G = (byte)((G & clearG) | GetPart(data, g));

     if (b > 0)
      B = (byte)((B & clearB) | GetPart(data, b));

     cpix = Color.FromArgb(R, G, B);
     outBmp.SetPixel(x, y, cpix);

     used += bf - data.Count;

     f1.pbProgressBar.Value = used;
//     Application.DoEvents();
    }
  }
 }
}
