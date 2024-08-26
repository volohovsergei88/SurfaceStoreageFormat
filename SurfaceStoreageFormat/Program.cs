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
        public static Piket ParsePiket(byte[] data, ref int offset)
        {
            //Читаем координаты X Y
            UInt16 x = BitConverter.ToUInt16(data, offset);
            offset += 2;
            UInt16 y = BitConverter.ToUInt16(data, offset);
            offset += 2;

            Piket piket = new Piket(x, y);

            // Читаем первую высоту отметки
            float h1Value = BitConverter.ToSingle(data, offset);
            offset += 4;
            piket.h1 = new descrH { H1 = h1Value, type = "h1" };

            // Проверяем наличие h2
            if (offset < data.Length)
            {
                //Определяем тип h2 для определ разницы высот (9 || 10)
                if (data.Length - offset >= 1 && data.Length - offset <= 2)
                {
                    // Имеем 1 или 2 байта второй высоты
                    double dz;
                    if (data.Length - offset == 1)
                    {
                        // разница по высоте на 1 байт
                        dz = data[offset];
                        offset += 1;
                    }
                    else
                    {
                        // разница по высоте значит 2 байта
                        dz = BitConverter.ToUInt16(data, offset);
                        offset += 2;
                    }

                    // Рассчитываем значение h2
                    float h2Value = h1Value + (float)dz;
                    piket.h2 = new descrH { H1 = h2Value, type = "h2" };
                }
            }

            return piket;
        }

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

        //преобразование глобальных координат в локальные
        public static byte[] GlobalXYToLocal(Piket p)
        {

            int indexX = (int)p.X / 32;
            int indexY = (int)p.Y / 32;

            // Локал координаты внутри квадрата
            int localXmm = (int)((p.X - (indexX * 32)) * 1000);
            int localYmm = (int)((p.Y - (indexY * 32)) * 1000);

            /// Разбиваем координаты на байты
            byte[] localXBytes = BitConverter.GetBytes(localXmm);
            byte[] localYBytes = BitConverter.GetBytes(localYmm);

            // Возвращаем массив из 4 байтов: младшие два байта для X, старшие два байта для Y
            return new byte[] {
                  localXBytes[0], localXBytes[1],
                  localYBytes[0], localYBytes[1]
                             };
        }

        public static Piket LocalXYToGlobal(Index2D q, int[] byteRecord)
        {
            // Извлекаем байты для X и Y
            byte[] localXBytes = new byte[4];
            byte[] localYBytes = new byte[4];

            // Заполняем младшие 2 байта для X и Y из byteRecord
            localXBytes[0] = (byte)byteRecord[0];
            localXBytes[1] = (byte)byteRecord[1];

            localYBytes[0] = (byte)byteRecord[2];
            localYBytes[1] = (byte)byteRecord[3];

            // Конвертируем байты обратно в целые числа
            int localXmm = BitConverter.ToInt32(localXBytes, 0);
            int localYmm = BitConverter.ToInt32(localYBytes, 0);

            // Расчитываем глобальные координаты в миллиметрах
            int globalXmm = q.X * 32000 + localXmm;
            int globalYmm = q.Y * 32000 + localYmm;

            // Конвертируем в метры
            double globalX = globalXmm / 1000.0;
            double globalY = globalYmm / 1000.0;

            // Создаем piket с глобальными координатами
            return new Piket((UInt16)globalX, (UInt16)globalY);
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
            new Pokr(new Color(204, 204, 255,255), "Асфальт"),
            new Pokr(new Color(204, 204, 255,255), "Газон")
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
                new Piket((UInt16)100.67, (UInt16)200.789, 101.78f),
                new Piket((UInt16)150.123, (UInt16)250.654, 95.74f)
             });
            // Добавляем кластер в список кластеров
            clusters.Add(cl1);
            UInt16 countPiks1 = (UInt16)cl1.Piks1.Count;
            allData.AddRange(BitConverter.GetBytes(countPiks1));
            foreach (var p in cl1.Piks1)
                allData.AddRange(p.Record());

            //9 байт
            cl1.Piks2.Add( new Piket((UInt16)101.8, (UInt16)122.3, 101.25f, 101.40f));
            UInt16 countPiks2 = (UInt16)cl1.Piks2.Count;
            allData.AddRange(BitConverter.GetBytes(countPiks2));
            foreach (var p in cl1.Piks2)
                allData.AddRange(p.Record());

            //10 байт
            cl1.Piks3.Add(new Piket((UInt16)99.4, (UInt16)81.2, 77.5f, 121.0f));
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
          

            static void ReadFileData(byte[] fileData)
            {
                
                // Парсинг заголовка
                int offset = 0;
                Header header = ParceHeader(fileData, ref offset);
                
                // Парсинг таблицы покрытий
                TablePokr coverTable = ParcePokr(fileData, ref offset);
                //Парсинг отметок и треугольников
                Piket pk = ParsePiket(fileData, ref offset);
                //Парсинг треугольников в разных кластерах
                GlobalTriangleTable glTr = ParceGlTrg(fileData, ref offset);

                
            }


        }
    }
}



