﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurfaceStoreageFormat
{
    public class AllClass
    {
        //Блок 1 Заголовок
        public class Header
        {
            public byte Ver { get; set; } //Версия
            public string Author { get; set; } //Автор
            public string  NameSur { get; set; } //Имя поверхности
            public DateTime TimeSur { get; set; } //Дата публикаций
            public Header(byte ver, string author, string namesur, DateTime timeSur)
            {
                Ver = ver;
                Author = author;
                NameSur = namesur;
                TimeSur = timeSur;
            }
            // Метод для записи данных заголовка в массив байтов
            public byte[] Record()
            {
                List<byte> data = new List<byte>
        {
            Ver, // Добавляем версию формата файла
        };
                // Добавляем данные автора
                MakeBloks.AppendRecord(data, Author);
                // Добавляем данные имени поверхности
                MakeBloks.AppendRecord(data, NameSur);
                // Добавляем время и дату публикации в формате 8 байт 
                data.AddRange(BitConverter.GetBytes(TimeSur.ToBinary()));
                return data.ToArray();
            }
        }

        // Класс треугольника для сохранения в файле
        public class Treangle
        {
            public byte[] Indexes { get; set; }
            public byte ControlFlag { get; set; }

            public Treangle(byte controlFlag, byte[] indexes)
            {
                ControlFlag = controlFlag;
                Indexes = indexes;
            }
            public byte[] Record(bool exFormat)
            {
                List<byte> data = new List<byte>
        {
            ControlFlag // Добавляем контрольный бит
        };
                if (exFormat)
                {
                    // Индексы по 2 байта
                    foreach (var index in Indexes)
                        data.AddRange(BitConverter.GetBytes((UInt16)index));
                }
                else
                    // Индексы по 1 байту
                    data.AddRange(Indexes);
                return data.ToArray();
            }
        }

        //Треугольники кластера
        public class TreangleCluster
        {
            public byte IndexTrCl { get; set; } //индекс записи в таблице покрытий
            public List<Treangle> Triangles { get; set; }
            public bool RecTrg => Triangles.Count > 256; // Формат записи: true, если больше 256 треугольников (используем 2 байта)

            public TreangleCluster(byte indexTr)
            {
                IndexTrCl = indexTr;
                Triangles = new List<Treangle>();
            }


            // Добавляем треугольники в таблицу
            public void AddTriangle(byte controlFlag, byte index1, byte index2, byte index3)
            {
                byte[] indexes = new byte[] { index1, index2, index3 };
                Treangle triangle = new Treangle(controlFlag, indexes);
                Triangles.Add(triangle);
            }
            // Запись данных таблицы треугольников
            public byte[] Record()
            {
                List<byte> data = new List<byte>();
                data.Add(IndexTrCl);
                // Определяем, нужно ли использовать 2 байта для индексов
                bool exFormat = RecTrg;
                // Добавляем количество треугольников (2 байта)
                UInt16 triangleCount = (UInt16)Triangles.Count;
                data.AddRange(BitConverter.GetBytes(triangleCount));

                // Записываем треугольники
                foreach (var triangle in Triangles)
                {
                    data.AddRange(triangle.Record(exFormat));
                }
                return data.ToArray();
            }
        }

        //Глобальная таблица треугольников
        public class GlobalTriangleTable
        {
            public byte IndexPok { get; set; } // Индекс записи в таблице покрытий
          
            public List<TriangleRecord> TriangleRecords { get; set; }

            public GlobalTriangleTable(byte indexPok)
            {
                IndexPok = indexPok;
                TriangleRecords = new List<TriangleRecord>();
            }

            public void AddTriangle(Vertex vertex1, Vertex vertex2, Vertex vertex3)
            {
                TriangleRecord triangle = new TriangleRecord(vertex1, vertex2, vertex3);
                TriangleRecords.Add(triangle);
            }

            public byte[] Record()
            {
                List<byte> data = new List<byte>
        {
            IndexPok // Добавляем индекс записи в таблице покрытий
        };
                // Количество треугольников (2 байта)
                UInt16 triangleCount = (UInt16)TriangleRecords.Count;
                data.AddRange(BitConverter.GetBytes(triangleCount));

                // Записываем треугольники
                foreach (var triangle in TriangleRecords)
                {
                    data.AddRange(triangle.Record());
                }

                return data.ToArray();
            }
    }
        public class TriangleRecord
        {
            public Vertex Vertex1 { get; set; }
            public Vertex Vertex2 { get; set; }
            public Vertex Vertex3 { get; set; }

            public TriangleRecord( Vertex vertex1, Vertex vertex2, Vertex vertex3)
            {
                Vertex1 = vertex1;
                Vertex2 = vertex2;
                Vertex3 = vertex3;
            }
            public byte[] Record()
            {
                List<byte> data = new List<byte>();
                // Добавляем данные для каждой вершины
                data.AddRange(Vertex1.Record());
                data.AddRange(Vertex2.Record());
                data.AddRange(Vertex3.Record());

                return data.ToArray();
            }
        }
        //Вершина треугольника
        public class Vertex
        {
            public byte[] ClusterIndex { get; set; } //  номер кластера 4 байта
            public byte[] AbsolutePosition { get; set; } //абсолютный номер вершины в файле  4 байта
            public byte[] AdditionalInfo { get; set; } //  доп информ (например, индекс отметок или идентификатор вершины)

            public Vertex(byte[] clusterIndex, byte[] absolutePosition, byte[] additionalInfo)
            {
                ClusterIndex = NormalizeTo4Bytes(clusterIndex);
                AbsolutePosition = NormalizeTo4Bytes(absolutePosition);
                AdditionalInfo = NormalizeTo4Bytes(additionalInfo);
            }

        public byte[] Record()
            {
                List<byte> data = new List<byte>();
                data.AddRange(ClusterIndex);
                data.AddRange(AbsolutePosition);
                data.AddRange(AdditionalInfo);

                return data.ToArray();
            }
        }

        // Класс отметки для сохранения в файле
        public class Otmetka
        {
            public UInt16 Filepos { get; set; }
            public UInt16 X { get; set; }
            public UInt16 Y { get; set; }
            public float PZ { get; set; } //Высота

        }
        //Покрытия
        public class Pokr
        {
            private Color col; // Цвет покрытия
            private string namePokr; // Имя покрытия

            public Color Col
            {
                get { return col; }
                private set { col = value; }
            }

            public string NamePokr
            {
                get { return namePokr; }
                private set { namePokr = value; }
            }

            public Pokr(Color col, string name)
            {
                this.col = col;
                this.namePokr = name;
            }

            public List<byte> Record()
            {
                List<byte> data = new List<byte>();
                byte[] color = { Col.Red, Col.Green, Col.Blue, Col.Alpha };
                data.AddRange(color);
                MakeBloks.AppendRecord(data, NamePokr);
                return data;
            }
        }

        //Таблица покрытий
        public class TablePokr
        {
            public byte CountPokr { get; set; } //кол-во покрытий
            public List<Pokr> CoverList { get; set; } = new List<Pokr>();
            public byte[] SquareCounts { get; set; } // Массив байтов, представляющий количество квадратов

            public TablePokr(int countPokr,List<Pokr> covers, byte[] squareCounts)
            {
                CountPokr = (byte)countPokr; 
                CoverList = covers;
                SquareCounts = NormalizeTo4Bytes(squareCounts);
            }
         
            public byte[] Record()
            {
                List<byte> data = new List<byte>();
                // Добавляем количество записей
                data.Add(CountPokr);
                // Добавляем записи всех покрытий
                foreach (var cover in CoverList)
                    data.AddRange(cover.Record());
                // Добавляем количество квадратов
                data.AddRange(SquareCounts);

                return data.ToArray();
            }
        }

        // Кластер  хранения данных
        public class Claster
        {
            public UInt32 CountCluster { get; set; }
            public Index2D pos;
            public List<Piket> Piks1;  // Таблица отметок одновысотных
            public List<Piket> Piks2;  // Таблица отметок двухвысотных однобайтовых перепадов
            public List<Piket> Piks3;  // Таблица отметок двухвысотных двухбатовых перепадов
          //  public List<Treangle> Trgs;  // Таблица треугольников локальных

            public Claster()
            {
                Piks1 = new List<Piket>();
                Piks2 = new List<Piket>();
                Piks3 = new List<Piket>();
            }
            // получение блок байт кластера
            public byte[] Record()
            {
                List<byte> data = new List<byte>();
                data.AddRange(BitConverter.GetBytes(CountCluster));
                // Добавляем координаты кластера (Q.X и Q.Y) в заголовок
                data.AddRange(BitConverter.GetBytes(pos.X)); // 4 байта для X
                data.AddRange(BitConverter.GetBytes(pos.Y)); // 4 байта для Y

                // Записываем все отметки Piks1
                data.AddRange(BitConverter.GetBytes((UInt16)Piks1.Count)); // Добавляем 2 байта на кол-во
                foreach (var piket in Piks1)
                    data.AddRange(piket.Record());
                // Записываем все отметки Piks2
                data.AddRange(BitConverter.GetBytes((UInt16)Piks2.Count)); // Добавляем 2 байта на кол-во
                foreach (var piket in Piks2)
                    data.AddRange(piket.Record());
                // Записываем все отметки Piks3
                data.AddRange(BitConverter.GetBytes((UInt16)Piks3.Count)); // Добавляем 2 байта на кол-во
                foreach (var piket in Piks3)
                    data.AddRange(piket.Record());
                return data.ToArray();
            }
        }
    

        // класс поверхности 
        public class Surface
        {

            public byte CountPokr { get; set; } // кол-во покрытий
            public string NameSurface { get; set; }             // Имя поверхонрсти
            public List<Pokr> TablePork;     // Таблица покрытий 
            public Surface(string name, byte countPokr, List<Pokr> tablePork)
            {
                CountPokr = countPokr;
                NameSurface = name;
                TablePork = tablePork;

            }
        }
            //// Метод для расчета размера байтового представления таблицы покрытий
            //public static int GetByteSize( TablePokr table)
            //{
            //    int size = 1; // для хранения количества покрытий
            //    foreach (var cover in table.CoverList)
            //    {
            //        size += 4; // для хранения цвета (4 байта)
            //        size += cover.NamePokr.Length + 1; // для хранения имени покрытия и его длины
            //    }
            //    size += table.SquareCounts.Length; // для хранения массива количества квадратов
            //    return size;
            //}
        
        //сохраняем блоки данных в файле
        public static bool SaveToFile(string fn,byte[] data )
        {
            if (!System.IO.File.Exists(fn))
            {
                System.IO.File.WriteAllBytes(fn,data);
                return System.IO.File.Exists(fn);
            }
            return false;
        }
        //читаем массив блоков
        public static byte[] ReadFile(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                return System.IO.File.ReadAllBytes(fileName);
            }
            return null;
        }
        //Нормализация массива до 4 байт
        public static byte[] NormalizeTo4Bytes(byte[] input)
        {
            if (input.Length == 4) return input;

            byte[] normalized = new byte[4];
            for (int i = 0; i < Math.Min(input.Length, 4); i++)
            {
                normalized[i] = input[i];
            }
            return normalized;
        }

        public class descrH
        {
            public float H; //высота отметки в метрах
            public string type; //тип отметки
            public descrH(float h) 
            {
                H = h;
            }
            public descrH() { }
        }

        public class Piket
        {
            public double X { get; set; }
            public double Y { get; set; }
            public Index2D ClusterIndex { get; set; }  // Индекс кластера

            public descrH H1, H2;

            //public Piket(double x, double y)
            //{
            //    X = x;
            //    Y = y;
            //}
            //public Piket(double x, double y, float h1Value)
            //{
            //    X = x;
            //    Y = y;
            //    h1 = new descrH { H1 = h1Value, type = "h1" };
            //}
            public Piket(double x, double y, descrH h1, descrH h2 = null)
            { 
                X = x;
                Y = y;
                H1 = h1; //  new descrH { H1 = h1Value, type = "" };
                H2 = h2; // new descrH { H1 = h2Value, type = "" };

                    // Вычисляем индекс кластера при создании пикета
                ClusterIndex = new Index2D((int)X / 32, (int)Y / 32);
            }

            public byte[] Record()
            {
                ////Преобразуем глобальные координаты (в мм.) плана в локальные с условием что размеры квадрата 32х32
                List<byte> data = new List<byte>();


                //сохраняем координаты кластера
                data.AddRange(BitConverter.GetBytes(ClusterIndex.X));
                data.AddRange(BitConverter.GetBytes(ClusterIndex.Y));
             
                byte[] localCoords = GlobalXYToLocal();
                data.AddRange(BitConverter.GetBytes(BitConverter.ToUInt16(localCoords, 0))); // X
                data.AddRange(BitConverter.GetBytes(BitConverter.ToUInt16(localCoords, 2))); // Y
                data.AddRange(BitConverter.GetBytes(H2 == null ? H1.H: Math.Min(H1.H, H2.H)));

                // Анализ перепада между h1 и h2 (если h2 существует)
                if (H1 != null && H2 != null)
                {
                    int dz =(int) Math.Abs((H2.H - H1.H)*1000); // Вычисляем перепад
                    // Проверяем, если перепад может быть выражен в одном или двух байтах
                    if (dz <= 255)
                        data.Add((byte)dz); // Добавляем 1 байт
                    else if (dz <= 65535)
                        data.AddRange(BitConverter.GetBytes((UInt16)dz)); // Добавляем 2 байта
                }
                // Возвращаем сформированную запись в виде массива байт
                return data.ToArray();
            }

            //преобразование глобальных координат в локальные
            public byte[] GlobalXYToLocal()
            {

                int indexX = (int)X / 32;
                int indexY = (int)Y / 32;

                // Локал координаты внутри квадрата
                int localXmm = (int)((X - (indexX * 32)) * 1000);
                int localYmm = (int)((Y - (indexY * 32)) * 1000);

                /// Разбиваем координаты на байты
                byte[] localXBytes = BitConverter.GetBytes(localXmm);
                byte[] localYBytes = BitConverter.GetBytes(localYmm);

                // Возвращаем массив из 4 байтов: младшие два байта для X, старшие два байта для Y
                return new byte[] {
                  localXBytes[0], localXBytes[1],
                  localYBytes[0], localYBytes[1]
                             };
            }
            //координаты локальные в глобальные
            public Piket(Index2D q, byte[] byteRecord)
            {
                int Xmm = BitConverter.ToUInt16(byteRecord, 0);
                int Ymm = BitConverter.ToUInt16(byteRecord, 2);
                float Z = BitConverter.ToSingle(byteRecord, 4);
                // Расчитываем глобальные координаты в метрах
                X = q.X * 32 + Xmm / 1000.0;
                Y = q.Y * 32 + Ymm / 1000.0;
                H1 = new descrH(Z);
                if (byteRecord.Length == 9) 
                {
                    byte dZ = byteRecord[8];
                }
                if (byteRecord.Length == 10)
                {
                    UInt16 dZ = BitConverter.ToUInt16(byteRecord, 8);
                }

            }
        }
        public struct Index2D
        {
            public int X { get; }
            public int Y { get; }

            public Index2D(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        public struct Color
        {
            public byte Red { get; set; }
            public byte Green { get; set; }
            public byte Blue { get; set; }
            public byte Alpha { get; set; }

            public Color(byte red, byte green, byte blue, byte alpha = 255)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }

        }

        //lля добавления строковых данных в байтовый массив
        public static class MakeBloks
        {
            public static void AppendRecord(List<byte> dest, string sAdd)
            {
                dest.Add((byte)sAdd.Length);
                dest.AddRange(Encoding.ASCII.GetBytes(sAdd));
            }
        }

    }
}

