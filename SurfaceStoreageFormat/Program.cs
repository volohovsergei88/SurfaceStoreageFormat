using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static SurfaceStoreageFormat.AllClass;

namespace SurfaceStoreageFormat
{
    class Program
    {
     
        // Метод для парсинга заголовка из массива байтов
        public static Header ParceHeader(byte[] data, ref int offset)
        {
            // Чтение версии
            byte ver = data[offset++];
            // Чтение имени автора
            string author = ReadString(data, ref offset);
            // Чтение имени поверхности
            string nameSur = ReadString(data, ref offset);
            // Чтение даты публикации (8 байт)
            long dateTimeBinary = BitConverter.ToInt64(data, offset);
            DateTime timeSur = DateTime.FromBinary(dateTimeBinary);
            offset += 8;
            // Возвращаем объект Header 
            return new Header(ver, author, nameSur, timeSur);
        }
        public static TablePokr ParcePokr(byte[] d, ref int offset)
        {
            byte countPokr = d[offset++];

            List<Pokr> tablePork = new List<Pokr>();
            for (int i = 0; i < countPokr; i++)
                tablePork.Add(ParsePokr(d, ref offset));

            byte[] squareCounts = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                squareCounts[i] = d[offset++];
            }

            return new TablePokr(countPokr, tablePork, squareCounts);
        }

        public static string ReadString(byte[] d, ref int offset)
        {

            int lenght = d[offset++];
            string result = Encoding.ASCII.GetString(d, offset, lenght);
            offset += lenght; //увеличиваем offset на длину прочитанной строки
            return result;
        }
        public static Pokr ParsePokr(byte[] data, ref int offset)
        {
            byte red = data[offset++];
            byte green = data[offset++];
            byte blue = data[offset++];
            byte alpha = data[offset++];
            Color col = new Color(red, green, blue, alpha);
            string name = ReadString(data, ref offset);

            return new Pokr(col, name);
        }
        //Парсим отметки 8,9,10 байт
        public static List<Piket> ParsePikets(byte[] data, ref int offset)
        {
            List<Piket> pikets = new List<Piket>();

            // Чтение количества 8-байтовых пикетов
            UInt16 count8BytePikets = BitConverter.ToUInt16(data, offset);
            offset += 2;

            for (int i = 0; i < count8BytePikets; i++)
            {
                // Чтение координат и высоты для 8-байтовых пикетов
                UInt16 x = BitConverter.ToUInt16(data, offset);
                offset += 2;
                UInt16 y = BitConverter.ToUInt16(data, offset);
                offset += 2;
                float pz = BitConverter.ToSingle(data, offset);
                offset += 4;
                byte[] rec8 = new byte[8] ;
                Array.Copy(data, offset, rec8, 0, 8);
                Index2D clusterIndex = new Index2D(x, y);
                // Преобразование локальных координат в глобальные
                Piket piket = new Piket(clusterIndex, rec8);
             

                pikets.Add(piket);
            }

            // Чтение количества 9-байтовых пикетов
            UInt16 count9BytePikets = BitConverter.ToUInt16(data, offset);
            offset += 2;

            for (int i = 0; i < count9BytePikets; i++)
            {
                // Чтение координат и высоты для 9-байтовых пикетов
                UInt16 x = BitConverter.ToUInt16(data, offset);
                offset += 2;
                UInt16 y = BitConverter.ToUInt16(data, offset);
                offset += 2;
                float pz = BitConverter.ToSingle(data, offset);
                offset += 4;
                byte dz = data[offset++];
                byte[] rec9 = new byte[9];
                Array.Copy(data, offset, rec9, 0, 9);
                Index2D clusterIndex = new Index2D(x, y);
                // Преобразование локальных координат в глобальные
                Piket piket = new Piket(clusterIndex, rec9);
                piket.H1 = new descrH { H = pz, type = "h1" };
                piket.H2 = new descrH { H = pz + dz, type = "h2" };

                pikets.Add(piket);
            }

            // Чтение количества 10-байтовых пикетов
            UInt16 count10BytePikets = BitConverter.ToUInt16(data, offset);
            offset += 2;

            for (int i = 0; i < count10BytePikets; i++)
            {
                // Чтение координат и высоты для 10-байтовых пикетов
                UInt16 x = BitConverter.ToUInt16(data, offset);
                offset += 2;
                UInt16 y = BitConverter.ToUInt16(data, offset);
                offset += 2;
                float pz = BitConverter.ToSingle(data, offset);
                offset += 4;
                UInt16 dz = BitConverter.ToUInt16(data, offset);
                offset += 2;
                byte[] rec10 = new byte[10];
                Array.Copy(data, offset, rec10, 0, 10);
                Index2D clusterIndex = new Index2D(x, y);
                // Преобразование локальных координат в глобальные
                Piket pik =new Piket(clusterIndex, rec10);
                pik.H1 = new descrH { H = pz, type = "h1" };
                pik.H2 = new descrH { H = pz + dz, type = "h2" };

                pikets.Add(pik);
            }

            return pikets;
        }
        //public static Piket ParsePiket(byte[] data, ref int offset)
        //{
        //    //Читаем координаты X Y
        //    UInt16 x = BitConverter.ToUInt16(data, offset);
        //    offset += 2;
        //    UInt16 y = BitConverter.ToUInt16(data, offset);
        //    offset += 2;

        //    Piket piket = new Piket(x, y);

        //    // Читаем первую высоту отметки
        //    float h1Value = BitConverter.ToSingle(data, offset);
        //    offset += 4;
        //    piket.h1 = new descrH { H1 = h1Value, type = "h1" };

        //    // Проверяем наличие h2
        //    if (offset < data.Length)
        //    {
        //        //Определяем тип h2 для определ разницы высот (9 || 10)
        //        if (data.Length - offset >= 1 && data.Length - offset <= 2)
        //        {
        //            // Имеем 1 или 2 байта второй высоты
        //            double dz;
        //            if (data.Length - offset == 1)
        //            {
        //                // разница по высоте на 1 байт
        //                dz = data[offset];
        //                offset += 1;
        //            }
        //            else
        //            {
        //                // разница по высоте значит 2 байта
        //                dz = BitConverter.ToUInt16(data, offset);
        //                offset += 2;
        //            }

        //            // Рассчитываем значение h2
        //            float h2Value = h1Value + (float)dz;
        //            piket.h2 = new descrH { H1 = h2Value, type = "h2" };
        //        }
        //    }

        //    return piket;
        //}

        //Парсим треугольники в разных кластерах
        public static GlobalTriangleTable ParceGlTrg(byte[] data, ref int offset)
        {
            byte index = data[offset++];
            UInt16 triangleCount = BitConverter.ToUInt16(data, offset);
            offset += 2;
            GlobalTriangleTable globalTable = new GlobalTriangleTable(index);
            for (int i = 0; i < triangleCount; i++)
            {
                Vertex vertex1 = ParseVertex(data, ref offset);
                Vertex vertex2 = ParseVertex(data, ref offset);
                Vertex vertex3 = ParseVertex(data, ref offset);

                globalTable.AddTriangle(vertex1, vertex2, vertex3);
            }

            return globalTable;

        }
        
        //Парсим вершины треугольника 4 байта
        public static Vertex ParseVertex(byte[] data, ref int offset)
        {
            byte[] clusterIndex = ReadBytes(data, ref offset, 4);
            byte[] absolutePosition = ReadBytes(data, ref offset, 4);
            byte[] additionalInfo = ReadBytes(data, ref offset, 4);

            return new Vertex(clusterIndex, absolutePosition, additionalInfo);
        }
        public static byte[] ReadBytes(byte[] data, ref int offset, int count)
        {
            byte[] result = new byte[count];
            Array.Copy(data, offset, result, 0, count);
            offset += count;
            return result;
        }



        //Рассчитываем размер массива байта всех блоков
        public static int CalculateFileSize(Header header, TablePokr coverTable, List<Claster> clusters,
             TreangleCluster cl,GlobalTriangleTable globalTrgTable)
        {
            int fileSize = 0;

            // Размер заголовка
            fileSize += header.Record().Length;

            // Размер таблицы покрытий
            fileSize += coverTable.Record().Length;
            // Размер всех кластеров отметок
            foreach (var cluster in clusters)
                fileSize += cluster.Record().Length;
            //Размер треугольников кластера
                fileSize += cl.Record().Length; 
            // Размер таблицы глобальных треугольников
            fileSize += globalTrgTable.Record().Length;

            return fileSize;
        }

      
        static void Main(string[] args)
        {
            List<byte> allData = new List<byte>();
            //Заголовок
            Header header = new Header(1, "Иван", "Дорога", DateTime.Parse("12.03.2024"));
            // Записываем данные в массив байтов
            byte[] headerData = header.Record();
            allData.AddRange(headerData);

            //Покрытия
            List<Pokr> tablePork = new List<Pokr>
        {
            new Pokr(new Color(204, 204, 255, 255), "Асфальт"),
            new Pokr(new Color(204, 204, 255, 255), "Газон")
        };
            // Массив количества квадратов
            byte[] squareCounts = { 5 };

            var coverTable = new TablePokr(tablePork.Count, tablePork, squareCounts);
            byte[] coverTableBytes = coverTable.Record();

            allData.AddRange(coverTableBytes);

            //кластер отметок
            List<Claster> clusters = new List<Claster>();
            //8байт

            // Создаем кластер
            Claster cl1 = new Claster();
            // Добавляем отметки в первую таблицу (Piks1)
            cl1.Piks1.AddRange(new List<Piket>
            {
                new Piket(100.67, 200.789, new descrH(101.78f), null),
                new Piket(150.123, 250.654, new descrH(95.74f), null)
             });
            // Добавляем кластер в список кластеров
            clusters.Add(cl1);

            UInt16 countPiks1 = (UInt16)cl1.Piks1.Count;
            allData.AddRange(BitConverter.GetBytes(countPiks1));
            foreach (var p in cl1.Piks1)
                allData.AddRange(p.Record());

            //9 байт
            cl1.Piks2.Add( new Piket(101.8,122.3, new descrH(101.25f), new descrH(101.40f)));
            UInt16 countPiks2 = (UInt16)cl1.Piks2.Count;
            allData.AddRange(BitConverter.GetBytes(countPiks2));
            foreach (var p in cl1.Piks2)
                allData.AddRange(p.Record());

            //10 байт
            cl1.Piks3.Add(new Piket(99.4, 81.2, new descrH(77.5f), new descrH(121.0f)));
            UInt16 countPiks3 = (UInt16)cl1.Piks2.Count;
            allData.AddRange(BitConverter.GetBytes(countPiks3));
            foreach (var p in cl1.Piks3)
                allData.AddRange(p.Record());
            

            //Таблица треугольников кластера
            TreangleCluster clr = new TreangleCluster(1);
            // Добавляем 5 треугольников в кластер
            clr.AddTriangle(1, 1, 2, 3);
            clr.AddTriangle(1, 4, 5, 6);
            clr.AddTriangle(1, 7, 8, 9);
            clr.AddTriangle(1, 10, 11, 12);
            clr.AddTriangle(1, 13, 14, 15);

           
            byte[] recordedData = clr.Record();
            allData.AddRange(recordedData);

            //Таблица треугольников разных кластеров 
            GlobalTriangleTable glTrg = new GlobalTriangleTable(1);
            var vertex1 = new Vertex(new byte[] { 1, 0 }, new byte[] { 0, 1, 2 }, new byte[] { 0, 0, 0 });
            var vertex2 = new Vertex(new byte[] { 1 }, new byte[] { 0, 1, 0, 8 }, new byte[] { 0 });
            var vertex3 = new Vertex(new byte[] { 1, 0, 0, 0 }, new byte[] { 3, 0, 1, 9 }, new byte[] { 0, 0, 1, 5 });

            glTrg.AddTriangle(vertex1, vertex2, vertex3);
            byte[] globalTrg = glTrg.Record(); //39 байт
            allData.AddRange(globalTrg);

            //Сохраняем массив байтов в файл
            string fileName = "surface.dat";
            SaveToFile(fileName, allData.ToArray());

            //чтение
           
            int fileSize = CalculateFileSize(header, coverTable, clusters, clr, glTrg); //размер массива байт
            byte[] fileData = new byte[fileSize];
           // byte [] fileRead = ReadFile(fileName);
           fileData=  ReadFile(fileName);
            // Вызов метода для чтения и парсинга данных
            SurfaceData parsedData =SurfaceData.ReadFileData(fileData);

            //Console.WriteLine(parsedData.Header.Author);


        }
    }
}



