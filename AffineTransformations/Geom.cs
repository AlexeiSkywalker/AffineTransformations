using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geometry
{
    enum Position{ LEFT, RIGHT, BEYOND, BEHIND, BETWEEN, ORIGIN, DESTINATION };
    //             СЛЕВА,СПРАВА,ВПЕРЕДИ,ПОЗАДИ, МЕЖДУ,   НАЧАЛО, КОНЕЦ
    class MyPoint
	{
		public double x, y; //Координаты

		public MyPoint(double _x = 0.0, double _y = 0.0)
		{
			x = _x; y = _y;
		}

		public MyPoint(MyPoint p)
		{
			x = p.x;
			y = p.y;
		}

        public static MyPoint operator +(MyPoint a, MyPoint b)
        {
            return new MyPoint(a.x + b.x, a.y + b.y);
        }

        public static MyPoint operator -(MyPoint a, MyPoint b)
        {
            return new MyPoint(a.x - b.x, a.y - b.y);
        }

        public static MyPoint operator *(double s, MyPoint b)
        {
            return new MyPoint(s * b.x, s * b.y);
        }

        public double length() //Возвращает длину текущего вектора
        {
            return Math.Sqrt(x * x + y * y);
        }

        public Position classify(MyPoint p0, MyPoint p1)//Говорит, где лежит точка(класса) относительно отрезка формируемого заданными точками 
        {
            MyPoint p2 = this;
            MyPoint a = p1 - p0;
            MyPoint b = p2 - p0;
            double sa = a.x * b.y - b.x * a.y;
            if (sa > 0.0)
                return Position.LEFT; //Слева от отрезка
            if (sa < 0.0)
                return Position.RIGHT; //Справа от отрезка
            if ((a.x * b.x < 0.0) || (a.y * b.y < 0.0))
                return Position.BEHIND; //На прямой со стороны p0
            if (a.length() < b.length())
                return Position.BEYOND; //На прямой со стороны p1
            if (p0 == p2)
                return Position.ORIGIN; //Совпадает с началом(p0)
            if (p1 == p2)
                return Position.DESTINATION; //Совпадает с концом(p1)
            return Position.BETWEEN; //Находится на прямой между p0 и p1
        }

        public Position classify(Edge e)//Тоже самое только принимает отрезок
        { 
            return classify(e.org, e.dest);
        }

        public MyPoint rotation(MyPoint r, int angle)
        {
            double angle_radian = angle * Math.PI / 180;
            double[,] rotmatrix = new double[2, 2] { { Math.Cos(angle_radian), Math.Sin(angle_radian) }, { Math.Sin(angle_radian), Math.Cos(angle_radian) } };

            x = (x - r.x) * rotmatrix[0, 0] - (y - r.y) * rotmatrix[0, 1] + r.x;
            y = (x - r.x) * rotmatrix[1, 0] - (y - r.y) * rotmatrix[1, 1] + r.y;
            
            return this;
        }

        public MyPoint move(double a, double b)
        {
            double[,] scalematrix = new double[3, 3] { { 1, 0, a }, { 0, 1, b }, { 0, 0, 1} };

            x = x * scalematrix[0, 0] + y * scalematrix[0, 1] + 1 * scalematrix[0, 2];
            y = y * scalematrix[1, 0] + y * scalematrix[1, 1] + 1 * scalematrix[1, 2];
            

            return this;
        }
    };

    enum Relation { COLLINEAR, PARALLEL, SKEW, SKEW_CROSS, SKEW_NO_CROSS };
    //              Коллинеарные, Параллельные, Линии пересекаются, Отрезки пересекаются, Отрезки не пересекаются
    class Edge
	{
		public MyPoint org; //Начальная точка
		public MyPoint dest; //Конечная точка
		public Edge(MyPoint _org, MyPoint _dest)
		{
			org = _org;
			dest = _dest;
		}
		public Edge()
		{
			org = new MyPoint(0, 0);
			dest = new MyPoint(1, 0);
		}

        //Поворот ребра на 90 градусов относительно его центра
        public Edge rotation()
        {
            MyPoint m = 0.5 * (org + dest);
            MyPoint v = dest - org;
            MyPoint n = new MyPoint(v.y, -v.x);
            org = m - 0.5 * n;
            dest = m + 0.5 * n;

            return this;
        }

        //Изменяет направление текущего ребра на обратное
        public Edge flip()
        {
            return rotation().rotation();
        }

        //Значение параметра t подставляется в параметрическое уравнение для этой линии P(t) = a +t(b - a)
        public MyPoint point(double t)
        {
           return new MyPoint(org + t * (dest - org));
        }

        //Скалярное произведение
        public double dotProduct(MyPoint p, MyPoint q)
        {
            return (p.x * q.x + p.y * q.y);
        }

        //Определяет отношение между текущими ребром и заданным.
        //В t - будет записано значение нужное для уравнения прямой a + t(b - a), где a и b точки(org dest) лежащие на прямой
        public Relation intersect(Edge e, out double t)
        {
            MyPoint a = org;
            MyPoint b = dest;
            MyPoint c = e.org;
            MyPoint d = e.dest;
            MyPoint n = new MyPoint((d - c).y, (c - d).x);
            double denom = dotProduct(n, b - a);
            if (denom == 0.0)
            {
                Position aclass = org.classify(e);
                t = -1.0;//Если коллинеарны или паралелльные, то параметр нельзя вычислить, потому что линии не пересекаются
                if ((aclass == Position.LEFT) || (aclass == Position.RIGHT))
                    return Relation.PARALLEL;
                else return Relation.COLLINEAR;
            }
            double num = dotProduct(n, a - c);
            t = -num / denom;
            return Relation.SKEW;
        }

        /*
         * Возвращает значение SKEW_CROSS (наклонены, и пересекаются), если и только если текущий отрезок прямой линии пересекает отрезок прямой линии е.
         * Если отрезки прямой линии пересекаются, то возвращается значение параметра t вдоль этого отрезка прямой линии, соответствующее точке пересечения.
         * В противном случае функция возвращает одно из следующих подходящих значений COLLINEAR (коллинеарны) , PARALLEL (параллельны) или SKEW_NO_CROSS (наклонены, но без пересечения).
         */
        public Relation cross(Edge e, out double t)
        {
            double s;
            t = -1.0;//Если коллинеарны или паралелльные, то параметр нельзя вычислить, потому что линии не пересекаются
            Relation crossType = e.intersect(this, out s);
            if ((crossType == Relation.COLLINEAR) || (crossType == Relation.PARALLEL))
                return crossType;
            if ((s < 0.0) || (s > 1.0))
                return Relation.SKEW_NO_CROSS;
            intersect(e, out t);
            if ((0.0 <= t) && (t <= 1.0))
                return Relation.SKEW_CROSS;
            else
                return Relation.SKEW_NO_CROSS;
        }

        public Edge rotation(MyPoint r, int angle)
        {
            double angle_radian = angle * Math.PI / 180;
            double[,] rotmatrix = new double[2, 2] { { Math.Cos(angle_radian), Math.Sin(angle_radian) }, { Math.Sin(angle_radian), Math.Cos(angle_radian) } };

            org.x = (org.x - r.x) * rotmatrix[0, 0] - (org.y - r.y) * rotmatrix[0, 1] + r.x;
            org.y = (org.x - r.x) * rotmatrix[1, 0] - (org.y - r.y) * rotmatrix[1, 1] + r.y;


            dest.x = (dest.x - r.x) * rotmatrix[0, 0] - (dest.y - r.y) * rotmatrix[0, 1] + r.x;
            dest.y = (dest.x - r.x) * rotmatrix[1, 0] - (dest.y - r.y) * rotmatrix[1, 1] + r.y;

            return this;
        }

        //Находит точку пересечения двух рёбер
        public static MyPoint crossPoint(Edge e, Edge f)
        {
            double t;
            MyPoint p = new MyPoint();
            if (e.cross(f, out t) == Relation.SKEW_CROSS)
                p = e.point(t);
            return p;
        }

        public Edge scale(double a, double b)
        {
            double[,] scalematrix = new double[2, 2] { { a, 0 }, { 0, b } };

            org.x = org.x * scalematrix[0, 0] + org.y * scalematrix[0, 1];
            org.y = org.x * scalematrix[1, 0] + org.y * scalematrix[1, 1];

            dest.x = dest.x * scalematrix[0, 0] + dest.y * scalematrix[0, 1];
            dest.y = dest.x * scalematrix[1, 0] + dest.y * scalematrix[1, 1];


            return this;
        }

        public Edge move(double a, double b)
        {
            double[,] scalematrix = new double[3, 3] { { 1, 0, a }, { 0, 1, b }, { 0, 0, 1 } };

            org.x = org.x * scalematrix[0, 0] + org.y * scalematrix[0, 1] + 1 * scalematrix[0, 2];
            org.y = org.x * scalematrix[1, 0] + org.y * scalematrix[1, 1] + 1 * scalematrix[1, 2];

            dest.x = dest.x * scalematrix[0, 0] + dest.y * scalematrix[0, 1] + 1 * scalematrix[0, 2];
            dest.y = dest.x * scalematrix[1, 0] + dest.y * scalematrix[1, 1] + 1 * scalematrix[1, 2];

            return this;
        }
    };


    enum Rotation { CLOCKWISE, COUNTER_CLOCKWISE };
    //          По часовой стрелке, Против часовой
    class Polygon
	{
		private Vertex _v; //Текущая(!!!) вершина
		private int _size; //Количество вершин в полигоне
        //Производит пересчёт всех вершин
		private void resize()
		{
			if (_v == null)
				_size = 0;
			else
			{
                Vertex v = _v.next();
                for (_size = 1; v != _v; ++_size, v = v.next()) ;
			}
		}

		public Polygon()
		{
			_v = null;
			_size = 0;
		}

		public Polygon(Polygon p)
		{

		}

		public Polygon(Vertex v)
		{
			_v = v;
			resize();
		}
		public Vertex v()
		{
			return _v;
		}

        //Образует точку из текущей вершины
		public MyPoint point()
		{
			return _v.point();
		}

        //Образует ребро из текущей вершины и следующей по часовой стрелке
        public Edge edge()
        {
            return new Edge(point(), _v.next().point()); 
        }

		public int size()
		{
			return _size;
		}

        //Следующая вершина по часовой стрелке
        public Vertex next()
        {
            return _v.next();
        }

        //Предыдущая вершина (против часовой стрелки)
        public Vertex prev()
        {
            return _v.prev();
        }

        //Возвращает соседнюю вершину в зависимости от направления
        public Vertex neighbor(Rotation rot)
        {
            return _v.neighbor(rot);
        }

        //Перемещает окно на последователя или предшественника текущей вершины 
        public Vertex advance(Rotation rot)
        {
            return _v = _v.neighbor(rot);
        }

        //Перемещает окно на заданную вершину
        //TODO: Сделать проверку, что такая вершина есть в полигоне
        public Vertex setV(Vertex v)
        {
            return _v = v;
        }

        //Преобразует точку в вершину, вставляет в полигон и делает текущей
        public Vertex insert(MyPoint p)
        {
            if (_size++ == 0)
                _v = new Vertex(p);
            else
                _v = _v.insert(new Vertex(p));
            return _v;
        }

        //Удаляет текущую вершину. Окно перемещается на предшественника или остается неопределенным
        public void Remove()
        {
            Vertex v = _v;
            _v = (--_size == 0) ? null : _v.prev();
            v.remove();
        }

        //Точка, входящая в состав выпуклого полигона
        public bool pointlnConvexPolygon(MyPoint s , Polygon p)
        {
            if (p.size() == 1)
                return (s == p.point());
            if (p.size() == 2)
            {
                Position c = s.classify(p.edge());
                return ((c == Position.BETWEEN) || (c == Position.ORIGIN)  || (c == Position.DESTINATION));
            }
            Vertex org = p.v();
            for (int i = 0; i < p.size(); i++, p.advance(Rotation.CLOCKWISE))
                if (s.classify(p.edge()) == Position.LEFT)
                {
                    p.setV(org);
                    return false;
                }
            return true;
        }

        //Проверка принаддежности для любого многоугольника
        public bool pointInPolygon(MyPoint a, Polygon p)
        {
            int parity = 0;
            for (int i = 0; i < p.size(); i++, p.advance(Rotation.CLOCKWISE))
            {
                Edge e = p.edge();
                switch (edgeType(a, e))
                {
                    case typeedge.TOUCHING:
                        return true;
                    case typeedge.CROSSING:
                        parity = 1 - parity;
                        break;
                }
            }
            return (parity == 1 ? true : false);
        }

        enum typeedge{ TOUCHING, CROSSING, INESSENTIAL };   // положение ребра
                                                    //     КАСАТЕЛbНОЕ, ПЕРЕСЕКАЮЩЕЕ, НЕСУЩЕСТВЕННОЕ
        typeedge edgeType(MyPoint a, Edge e)
        {
            MyPoint v = e.org;
            MyPoint w = e.dest;
            switch (a.classify(e))
            {
                case Position.LEFT:
                    return ((v.y < a.y) && (a.y <= w.y)) ? typeedge.CROSSING : typeedge.INESSENTIAL;
                case Position.RIGHT:
                    return ((w.y < a.y) && (a.y <= v.y)) ? typeedge.CROSSING : typeedge.INESSENTIAL;
                case Position.BETWEEN:
                case Position.ORIGIN:
                case Position.DESTINATION:
                    return typeedge.TOUCHING;
                default:
                    return typeedge.INESSENTIAL;
            }
        }

        public Polygon rotation(MyPoint r, int angle)
        {
            double angle_radian = angle * Math.PI / 180;
            double[,] rotmatrix = new double[2, 2] { { Math.Cos(angle_radian), Math.Sin(angle_radian) }, { Math.Sin(angle_radian), Math.Cos(angle_radian) } };

            for(int i = 0; i < size(); ++i)
            {
                _v.x = (_v.x - r.x) * rotmatrix[0, 0] - (_v.y - r.y) * rotmatrix[0, 1] + r.x;
                _v.y = (_v.x - r.x) * rotmatrix[1, 0] - (_v.y - r.y) * rotmatrix[1, 1] + r.y;

                _v = _v.next();
            }
            return this;
        }

        public Polygon scale(double a, double b)
        {
            double[,] scalematrix = new double[2, 2] { { a, 0 }, { 0, b } };

            for (int i = 0; i < size(); ++i)
            {
                _v.x = _v.x * scalematrix[0, 0] + _v.y * scalematrix[0, 1];
                _v.y = _v.x * scalematrix[1, 0] + _v.y * scalematrix[1, 1];

                _v = _v.next();
            }
            return this;
        }

        public Polygon move(double a, double b)
        {
            double[,] scalematrix = new double[3, 3] { { 1, 0, a }, { 0, 1, b }, { 0, 0, 1 } };

            for (int i = 0; i < size(); ++i)
            {
                _v.x = _v.x * scalematrix[0, 0] + _v.y * scalematrix[0, 1] + 1* scalematrix[0, 2];
                _v.y = _v.x * scalematrix[1, 0] + _v.y * scalematrix[1, 1] + 1 * scalematrix[1, 2];

                _v = _v.next();
            }
            return this;
        }
    };

	class Vertex : MyPoint {

		private Vertex _next, _prev;

		public Vertex(double x, double y):base(x, y)
		{
			_next = this;
			_prev = this;
		}
		public Vertex(MyPoint p):base(p)
		{
			_next = this;
		}

		public MyPoint point()
		{
			return (MyPoint)this;
		}

		public Vertex next() //Следующая вершина (по часовой стрелке)
		{
			return _next;
		}

		public Vertex prev() //Предыдущая вершина (против часовой стрелки)
		{
			return _next;
		}

        public Vertex neighbor(Rotation rotation)
        {
            return ((rotation == Rotation.CLOCKWISE) ? next() : prev());
        }

        public Vertex insert(Vertex v)
        {
            Vertex temp = _next;
            v._next = temp;
            v._prev = this;
            _next = v;
            temp._prev = v;
            return v;
        }

        public Vertex remove()
        {
            _prev._next = _next;
            _next._prev = _prev;
            _next = _prev = this;
            return this;
        }
    }


}
