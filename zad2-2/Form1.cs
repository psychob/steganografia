using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace zad2_2
{
 public partial class Form1 : Form
 {
  public Form1()
  {
   InitializeComponent();
  }

  private Bitmap inImage, outImage;
  private long inFileSize;

  private void button5_Click(object sender, EventArgs e)
  {
   // ładujemy nasz plik
   OpenFileDialog ofd = new OpenFileDialog();

   ofd.CheckFileExists = true;
   ofd.Filter = "Bitmapy|*.bmp";
   ofd.Multiselect = false;
   ofd.Title = "Otwórz Bitmapę...";

   if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
    return;

   tbImageInput.Text = ofd.FileName;

   inImage = new Bitmap(tbImageInput.Text);
   outImage = inImage.Clone(new Rectangle(Point.Empty, inImage.Size), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

   pbInputImage.Image = inImage;
   pbOutImage.Image = outImage;

   // skalowanie obrazków

   pbInputImage.SizeMode = PictureBoxSizeMode.Zoom;
   pbOutImage.SizeMode = PictureBoxSizeMode.Zoom;

   tbOutFile.Text = Path.Combine(Path.GetDirectoryName(tbImageInput.Text), Path.GetFileNameWithoutExtension(tbImageInput.Text) + ".e.bmp");

   inFileSize = (new FileInfo(tbImageInput.Text)).Length;

   _addKeyInfoToOutput();
   _recalculateImageStats();
  }

  private void _addKeyInfoToOutput()
  {
   if (cbxAddKeyInfoToOutFile.Checked && tbImageInput.Text.Length > 0)
   {
    string path = Path.Combine(Path.GetDirectoryName(tbImageInput.Text), Path.GetFileNameWithoutExtension(tbImageInput.Text)) + ".e.";

    if (cbxCanalRed.Checked)
    {
     path += "R";
     path += (cbRCanal.SelectedIndex + 1).ToString();
    }
    else path += "R0";

    if (cbxCanalGreen.Checked)
    {
     path += "G";
     path += (cbGCanal.SelectedIndex + 1).ToString();
    }
    else
     path += "G0";

    if (cbxCanalBlue.Checked)
    {
     path += "B";
     path += (cbBCanal.SelectedIndex + 1).ToString();
    }
    else
     path += "B0";

    if (cbxOnlyASCI.Checked)
     path += "A";
    else
     path += "U";

    path += ".bmp";

    tbOutFile.Text = path;
   }
  }

  private void _recalculateImageStats()
  {
   if (inImage == null)
    return;

   lbImageSize.Text = _prettyPrint(inFileSize);
   lbImageResolution.Text = inImage.Width.ToString() + "x" + inImage.Height.ToString();
   lbImagePixelCount.Text = (inImage.Size.Height * inImage.Size.Width).ToString();

   // pojemność obrazka...
   lbImageCap.Text = _prettyPrint((inImage.Size.Height * inImage.Size.Width) * _getImagePixelCap() / 8);

   int charSize = cbxOnlyASCI.Checked ? 8 : 16;

   lbDataToSave.Text = _prettyPrint(tbText.TextLength * charSize / 8 + 3);

   int dtat = Math.Max((int)Math.Floor((inImage.Size.Height * inImage.Size.Width) * _getImagePixelCap() / (charSize * 1.0)) - 3, 0);

   if (dtat == 0)
   {
    tbText.Text = "";
    tbText.Enabled = false;
    lbCharsLeft.Text = 0.ToString();
   } else
   {
    if (tbText.TextLength > dtat)
    {
     tbText.Text = tbText.Text.Substring(0, dtat);
    }

    tbText.MaxLength = dtat;
    tbText.Enabled = true;

    lbCharsLeft.Text = (dtat - tbText.TextLength).ToString();
   }

   _drawPie(tbText.Text.Length * charSize + 24, (int)((inImage.Size.Height * inImage.Size.Width) * _getImagePixelCap()));
  }

  public long _getImagePixelCap()
  {
   long ret = 0;

   if (cbxCanalBlue.Checked)
    ret += cbBCanal.SelectedIndex + 1;

   if (cbxCanalGreen.Checked)
    ret += cbGCanal.SelectedIndex + 1;

   if (cbxCanalRed.Checked)
    ret += cbRCanal.SelectedIndex + 1;

   return ret;
  }

  long _drawPieTime;

  private void _drawPie(int used, int all)
  {
   if (_drawPieTime > DateTime.Now.Ticks)
    return;

   int i1 = used;
   int i2 = all - used;

   float total = i1 + i2;
   float deg1 = (i1 / total) * 360;
   float deg2 = (i2 / total) * 360;
   
   Pen p = new Pen(Color.Black, 2);

   Graphics g = panel1.CreateGraphics();

   System.Drawing.Size s = new System.Drawing.Size(8, 8);
   Rectangle rec = new Rectangle(Point.Empty, panel1.Size - s);
   
   Brush b1 = new SolidBrush(Color.Red);
   Brush b2 = new SolidBrush(Color.LimeGreen);
   
   g.Clear(Form1.DefaultBackColor);
   g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
   g.DrawPie(p, rec, 0, deg1);
   g.FillPie(b1, rec, 0, deg1);
   g.DrawPie(p, rec, deg1, deg2);
   g.FillPie(b2, rec, deg1, deg2);

   _drawPieTime = DateTime.Now.Ticks + 2000;
  }

  private string _prettyPrint(long nmb)
  {
   double db = nmb;
   string[] prv = 
   {
    "B",
    "KiB",
    "MiB",
    "GiB",
    "TiB"
   };

   int index = 0;

   while (db > 1024)
   {
    db /= 1024;
    index++;
   }

   if (index == 0)
    return db.ToString("F0") + " " + prv[index];
   else
    return db.ToString("F2") + " " + prv[index];
  }

  private void Form1_Load(object sender, EventArgs e)
  {
   cbRCanal.SelectedIndex = cbGCanal.SelectedIndex = cbBCanal.SelectedIndex = 0;
  }

  private void cbRCanal_SelectedIndexChanged(object sender, EventArgs e)
  {
   _recalculateImageStats();
   _addKeyInfoToOutput();
  }

  private void cbxCanalRed_CheckedChanged(object sender, EventArgs e)
  {
   if (cbxCanalRed.Checked)
    cbRCanal.Enabled = true;
   else
    cbRCanal.Enabled = false;

   _recalculateImageStats();
   _addKeyInfoToOutput();
  }

  private void cbxCanalGreen_CheckedChanged(object sender, EventArgs e)
  {
   if (cbxCanalGreen.Checked)
    cbGCanal.Enabled = true;
   else
    cbGCanal.Enabled = false;

   _recalculateImageStats();
   _addKeyInfoToOutput();
  }

  private void cbxCanalBlue_CheckedChanged(object sender, EventArgs e)
  {
   if (cbxCanalBlue.Checked)
    cbBCanal.Enabled = true;
   else
    cbBCanal.Enabled = false;

   _recalculateImageStats();
   _addKeyInfoToOutput();
  }

  private void tbText_TextChanged(object sender, EventArgs e)
  {
   _recalculateImageStats();
  }

  private void btnRefresh_Click(object sender, EventArgs e)
  {
   if (inImage == null)
    return;

   outImage = inImage.Clone(new Rectangle(Point.Empty, inImage.Size), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
   pbOutImage.Image = null;
   pbOutImage.SizeMode = PictureBoxSizeMode.Zoom;

   pbProgressBar.Visible = true;
   pbProgressBar.Value = 0;

   Encryptor.EncryptData(cbxCanalRed.Checked ? cbRCanal.SelectedIndex + 1 : 0,
                          cbxCanalGreen.Checked ? cbGCanal.SelectedIndex + 1 : 0,
                          cbxCanalBlue.Checked ? cbBCanal.SelectedIndex + 1 : 0,
                          cbxOnlyASCI.Checked, tbText.Text, inImage, outImage, this);

   pbProgressBar.Visible = false;

   pbOutImage.Image = outImage;
  }

  private void tbText_KeyDown(object sender, KeyEventArgs e)
  {
   TextBox txtBox = sender as TextBox;

   if (txtBox != null && txtBox.Multiline == true && e.Control == true && e.KeyCode == Keys.A)
    txtBox.SelectAll();
  }

  private void button3_Click(object sender, EventArgs e)
  {
   // zapisujemy obrazek
   if (inImage == null)
    return;

   btnRefresh_Click(sender, e);

   using (Bitmap blankImage = new Bitmap(outImage.Width, outImage.Height, PixelFormat.Format24bppRgb))
   {

    using (Graphics g = Graphics.FromImage(blankImage))
    {
     g.DrawImageUnscaledAndClipped(outImage, new Rectangle(Point.Empty, outImage.Size));
    }

    blankImage.Save(tbOutFile.Text, ImageFormat.Bmp);
   }

   //outImage.Save(tbOutFile.Text, System.Drawing.Imaging.ImageFormat.Bmp);
  }

  private void button4_Click(object sender, EventArgs e)
  {
   if (inImage == null)
    return;

   tbText.Text =  Decryptor.DecryptMessage(inImage, cbxCanalRed.Checked ? cbRCanal.SelectedIndex + 1 : 0,
                                           cbxCanalGreen.Checked ? cbGCanal.SelectedIndex + 1 : 0,
                                           cbxCanalBlue.Checked ? cbBCanal.SelectedIndex + 1 : 0,
                                           cbxOnlyASCI.Checked);
  }

  private void button7_Click(object sender, EventArgs e)
  {
   SaveFileDialog sfd = new SaveFileDialog();
   sfd.Filter = "Bitmapy|*.bmp";
   sfd.AddExtension = true;

   if (sfd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
    return;

   cbxAddKeyInfoToOutFile.Checked = false;

   tbOutFile.Text = sfd.FileName;
  }
 }
}
