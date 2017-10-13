using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Geometry;

namespace AffineTransformations
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            EdgeIsBeingDrawn = false;
            PoligonIsBeingDrawn = false;
            EdgeCount = 0;
            PoligonVertexesCount = 0;
            CurrPoligon = new List<Point>();
			CurrPoligon2 = new Polygon();
            PoligonHasDrawn = false;
            EdgeHasDrawn = false;
			PointIsBeingChosen = false;
			EdgesIsBeingCrossed = false;
			InitialImg = new Bitmap(730, 510);
            Clear();
        }

        // Координаты текужего отрезка
        private Point EdgeFirst;
        private Point EdgeSecond;

        

        // Флаги выбора
        private bool EdgeIsBeingDrawn;
        private bool PoligonIsBeingDrawn;
        private bool PointIsBeingChosen;
        private bool EdgesIsBeingCrossed;
        private bool pointIn;

        // Флаги нарисованных фигур
        private bool PoligonHasDrawn;
        private bool EdgeHasDrawn;

        // Счётчики текущего числа выбранных вершин 
        private int EdgeCount;
        private int PoligonVertexesCount;

        // Текущие рёбра        
		private Edge CurrEdge;
		private Edge CurrEdge2;

        // Точка вращения
        private Point RotatePoint;

        // Текущие полигоны
        private List<Point> CurrPoligon;
		private Polygon CurrPoligon2;
        
		
        private Image InitialImg;
        private Bitmap CurrentImage;

        private void DrawEdge_Click(object sender, EventArgs e)
        {
            Clear();
            pictureBox1.Cursor = Cursors.Cross;
            StatusLabel.Text = "Выберете две точки";
            EdgeIsBeingDrawn = true;
        }

        private void DrawCross(Point center, int wight)
        {
			Graphics gr = pictureBox1.CreateGraphics();
			gr.DrawLine(Pens.Red, new Point(center.X + wight, center.Y), new Point(center.X - wight, center.Y));
            gr.DrawLine(Pens.Red, new Point(center.X, center.Y + wight), new Point(center.X, center.Y - wight));

        }
        private void DrawEdge(Point p)
        {
            if (EdgeCount == 0)
                CurrentImage = new Bitmap(pictureBox1.Image);

            EdgeCount++;
            if (EdgeCount == 2)
            {
                pictureBox1.Image = CurrentImage;
                EdgeSecond = p;
                CurrEdge = new Edge(new MyPoint(EdgeFirst.X, EdgeFirst.Y), new MyPoint(EdgeSecond.X, EdgeSecond.Y));
                Graphics gr = Graphics.FromImage(CurrentImage);
                //Graphics gr = pictureBox1.CreateGraphics();
                gr.DrawLine(Pens.Black, EdgeFirst, EdgeSecond);
                EdgeIsBeingDrawn = false;
                EdgeCount = 0;
                StatusLabel.Text = "Действие не выбрано";
                pictureBox1.Cursor = Cursors.Arrow;
                EdgeHasDrawn = true;
                PoligonHasDrawn = false;
            }
            else
            {
                EdgeFirst = p;
                DrawCross(p, 10);
            }
        }

        private void DrawPoligon(Point p)
        {
            if (PoligonVertexesCount == 0)
            {

                for (int i = 0; i < CurrPoligon2.size(); ++i)
                    CurrPoligon2.Remove();
                CurrPoligon.Clear();
                CurrentImage = new Bitmap(pictureBox1.Image);
            }
            PoligonVertexesCount++;
            if (PoligonVertexesCount == numericUpDown1.Value)
            {
                pictureBox1.Image = CurrentImage;
                CurrPoligon2.insert(new MyPoint(p.X, p.Y));
                CurrPoligon.Add(p);
                Graphics gr = Graphics.FromImage(CurrentImage);
                gr.DrawLines(Pens.Black, CurrPoligon.ToArray());
                gr.DrawLine(Pens.Black, CurrPoligon.First(), CurrPoligon.Last());
                PoligonIsBeingDrawn = false;
                PoligonVertexesCount = 0;
                StatusLabel.Text = "Действие не выбрано";
                pictureBox1.Cursor = Cursors.Arrow;
                EdgeHasDrawn = false;
                PoligonHasDrawn = true;
            }
            else
            {
                CurrPoligon2.insert(new MyPoint(p.X, p.Y));
                CurrPoligon.Add(p);
                DrawCross(p, 10);
            }
        }

        private void CrossEdges()
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs mArgs = (MouseEventArgs)e;
            Point p = new Point(mArgs.X, mArgs.Y);
            if (EdgeIsBeingDrawn)
                DrawEdge(p);
            if (PoligonIsBeingDrawn)
                DrawPoligon(p);
			if (PointIsBeingChosen)
			{
				RotatePoint = new Point(mArgs.X, mArgs.Y);
				RotationAroundPoint();
				pictureBox1.Cursor = Cursors.Arrow;
				PointIsBeingChosen = false;
			}
			else if (EdgesIsBeingCrossed)
			{
				if (EdgeCount == 0)
					CurrentImage = new Bitmap(pictureBox1.Image);
				EdgeCount++;
				if (EdgeCount == 2)
				{
					pictureBox1.Image = CurrentImage;
					EdgeSecond = p;
					CurrEdge2 = new Edge(new MyPoint(EdgeFirst.X, EdgeFirst.Y), new MyPoint(EdgeSecond.X, EdgeSecond.Y));
					DrawEdge(p);
					var cp = Edge.crossPoint(CurrEdge, CurrEdge2);
					if (cp.x != 0 && cp.y != 0)
					{
						DrawCross(new Point((int)cp.x, (int)cp.y), 10);
						Graphics gr = Graphics.FromImage(pictureBox1.Image);
						gr.DrawLine(Pens.Red, new Point((int)cp.x + 10, (int)cp.y), new Point((int)cp.x - 10, (int)cp.y));
						gr.DrawLine(Pens.Red, new Point((int)cp.x, (int)cp.y + 10), new Point((int)cp.x, (int)cp.y - 10));
					}

					EdgeIsBeingDrawn = false;
					EdgeCount = 0;
					StatusLabel.Text = "Действие не выбрано";
					pictureBox1.Cursor = Cursors.Arrow;
					EdgeHasDrawn = true;
					PoligonHasDrawn = false;
					EdgesIsBeingCrossed = false;
				}
				else
				{
					EdgeFirst = p;
					DrawCross(p, 10);
				}
			}
			else if (pointIn)
			{
				DrawCross(p, 10);
				MessageBox.Show(CurrPoligon2.pointInPolygon(new MyPoint(p.X, p.Y), CurrPoligon2)? "true": "false");
				pointIn = false;
			}
            
        }

        private void DrawPoligon_Click(object sender, EventArgs e)
        {
            Clear();
            pictureBox1.Cursor = Cursors.Cross;
            StatusLabel.Text = "Выберете вершины многоугольника";
            PoligonIsBeingDrawn = true;
        }
        private void Clear()
        {
            pictureBox1.Image = InitialImg;
        }

        private void ClearForm_Click(object sender, EventArgs e)
        {
            Clear();
        }        

        private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private void Rotation_Click(object sender, EventArgs e)
        {
            double angle = (double)numericUpDown3.Value;
            double cos = Math.Cos(DegreeToRadian(angle));
            double sin = Math.Sin(DegreeToRadian(angle));
            Matrix m1 = new Matrix((float)cos, (float)sin,
                -(float)sin, (float)cos, 0, 0);
            Graphics gr = pictureBox1.CreateGraphics();
            gr.Transform = m1;
            if (PoligonHasDrawn)
            {
                gr.DrawLines(Pens.Blue, CurrPoligon.ToArray());
                gr.DrawLine(Pens.Blue, CurrPoligon.First(), CurrPoligon.Last());
            }
            if (EdgeHasDrawn)
                gr.DrawLine(Pens.Blue, EdgeFirst, EdgeSecond);
        }

        private void Rotate90Degrees_Click(object sender, EventArgs e)
        {
            if (!EdgeHasDrawn)
                return;
            double angle = 90.0;
            Point a = new Point((EdgeFirst.X + EdgeSecond.X) / 2,
                (EdgeFirst.Y + EdgeSecond.Y) / 2);
            double cos = Math.Cos(DegreeToRadian(angle));
            double sin = Math.Sin(DegreeToRadian(angle));
            Matrix m = new Matrix((float)cos, (float)sin,
                -(float)sin, (float)cos, (float)(-a.X*cos + a.Y*sin + a.X),
                (float)(-a.X * sin - a.Y * cos + a.Y));
            Graphics gr = pictureBox1.CreateGraphics();
            gr.Transform = m;
            gr.DrawLine(Pens.Blue, EdgeFirst, EdgeSecond);
        }

        private void Scaling_Click(object sender, EventArgs e)
        {
            Matrix m = new Matrix((float)numericUpDown4.Value, 0, 0, (float)numericUpDown5.Value, 0, 0);
            Graphics gr = pictureBox1.CreateGraphics();
            gr.Transform = m;
            if (PoligonHasDrawn)
            {
                gr.DrawLines(Pens.Blue, CurrPoligon.ToArray());
                gr.DrawLine(Pens.Blue, CurrPoligon.First(), CurrPoligon.Last());
            }
            if (EdgeHasDrawn)
                gr.DrawLine(Pens.Blue, EdgeFirst, EdgeSecond);
        }

        private void Translation_Click(object sender, EventArgs e)
        {
            Matrix m = new Matrix(1, 0, 0, 1, (float)numericUpDown6.Value, (float)numericUpDown7.Value);
            Graphics gr = pictureBox1.CreateGraphics();
            gr.Transform = m;
            if (PoligonHasDrawn)
            {
                gr.DrawLines(Pens.Blue, CurrPoligon.ToArray());
                gr.DrawLine(Pens.Blue, CurrPoligon.First(), CurrPoligon.Last());
            }
            if (EdgeHasDrawn)
                gr.DrawLine(Pens.Blue, EdgeFirst, EdgeSecond);
        }

       
		private void RotationAroundPoint()
		{
			double angle = (double)numericUpDown2.Value;
			double cos = Math.Cos(DegreeToRadian(angle));
			double sin = Math.Sin(DegreeToRadian(angle));
			Matrix m = new Matrix();
			m.RotateAt((float)angle, RotatePoint, MatrixOrder.Append);				
			Graphics gr = pictureBox1.CreateGraphics();
			gr.Transform = m;
			if (PoligonHasDrawn)
			{
				gr.DrawLines(Pens.Blue, CurrPoligon.ToArray());
				gr.DrawLine(Pens.Blue, CurrPoligon.First(), CurrPoligon.Last());
			}
			if (EdgeHasDrawn)
				gr.DrawLine(Pens.Blue, EdgeFirst, EdgeSecond);
		}

        

		private void button9_Click(object sender, EventArgs e)
		{
			EdgesIsBeingCrossed = true;
			pictureBox1.Cursor = Cursors.Cross;
		}

		private void button10_Click(object sender, EventArgs e)
		{
			pointIn = true;
			pictureBox1.Cursor = Cursors.Cross;
		}

        private void RotationAroundPoint_Click(object sender, EventArgs e)
        {
            PointIsBeingChosen = true;
            pictureBox1.Cursor = Cursors.Cross;
        }
    }
}
