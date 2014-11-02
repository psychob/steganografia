using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace zad2_2
{
 sealed class Decryptor
 {
  static string GetStringBlock(byte[] bytes)
  {
   char[] chars = new char[bytes.Length / sizeof(char)];
   System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
   return new string(chars);
  }

  static void AppendToList(List<bool> lst, byte b, int count)
  {
   for (int jt = count-1; jt >= 0; --jt)
    lst.Add((b & (1 << jt)) > 0);
  }

  static byte FromListByte(List<bool> lst)
  {
   byte ret = 0;

   for (int it = 0; it < 8; ++it)
   {
    ret <<= 1;
    ret |= (byte)((lst.FirstOrDefault() ? 1 : 0));

    if (lst.Count > 0)
     lst.RemoveAt(0);
   }

   return ret;
  }

  static ushort FromListUShort(List<bool> lst)
  {
   ushort ret = 0;

   for (int it = 0; it < 16; ++it)
   {
    ret <<= 1;
    ret |= (ushort)((lst.FirstOrDefault() ? 1 : 0));

    if (lst.Count > 0)
     lst.RemoveAt(0);
   }

   return ret;
  }

  private static byte CheckSum(byte[] table)
  {
   byte ret = 0;

   for (int it = 0; it < table.Length; ++it)
    ret = (byte)((ret << 1) ^ (table[it]));

   return ret;
  }

  public static string DecryptMessage(Bitmap bmp, int r, int g, int b, bool asci)
  {
   // na samym początku pobieramy pierwsze 24 bity

   List<bool> ret = new List<bool>();

   for (int y = 0; y < bmp.Height; y++)
    for (int x = 0; x < bmp.Width; ++x)
    {
     Color cpix = bmp.GetPixel(x, y);
     
     if (r > 0)
      AppendToList(ret, cpix.R, r);

     if (g > 0)
      AppendToList(ret, cpix.G, g);
     
     if (b > 0)
      AppendToList(ret, cpix.B, b);

     if (ret.Count >= 24)
      break;
    }

   // teraz wyciągamy crc i ilość znaków
   byte crc  = FromListByte(ret);
   ushort ct = FromListUShort(ret);

   int al_least = 24 + ct * 8;

   ret.Clear();

   for (int y = 0; y < bmp.Height && ret.Count < al_least; y++)
    for (int x = 0; x < bmp.Width && ret.Count < al_least; ++x)
    {
     Color cpix = bmp.GetPixel(x, y);

     if (r > 0)
      AppendToList(ret, cpix.R, r);

     if (g > 0)
      AppendToList(ret, cpix.G, g);

     if (b > 0)
      AppendToList(ret, cpix.B, b);
    }

   ret = ret.Skip(24).Take(al_least - 24).ToList();

   // konwersja na byte[]
   byte[] barr = new byte[ct];

   for (int it = 0; it < ct; ++it)
    barr[it] = FromListByte(ret);

   // sprawdzamy crc
   if (CheckSum(barr) != crc)
   {
    MessageBox.Show("Suma kontrolna się nie zgadza!");
    return "";
   }
   byte[] cs;

   if (asci)
    cs = Encoding.Convert(Encoding.ASCII, Encoding.Unicode, barr);
   else
    cs = Encoding.Convert(Encoding.Unicode, Encoding.Unicode, barr);

   return GetStringBlock(cs);
  }
 }
}
